import socket
import threading
import json
import pygame
import sys
import time

SCREEN_WIDTH = 800
SCREEN_HEIGHT = 600
PADDLE_WIDTH = 20
PADDLE_HEIGHT = 100
BALL_SIZE = 10
WHITE = (255, 255, 255)
BLACK = (0, 0, 0)


class GameClient:
    def __init__(self):
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.server_address = ("127.0.0.1", 9876)
        self.player_id = None
        self.game_state = None
        self.connected = False

        # Initialize Pygame
        pygame.init()
        self.screen = pygame.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
        pygame.display.set_caption("Ping Pong")
        self.clock = pygame.time.Clock()

    def listen_for_messages(self):
        print("Started listening for messages...")
        last_ping = time.time()

        while True:
            try:
                current_time = time.time()

                # Send ping every 2 seconds
                if current_time - last_ping >= 2:
                    ping_message = {"type": "ping"}
                    self.socket.sendto(
                        json.dumps(ping_message).encode(), self.server_address
                    )
                    last_ping = current_time

                data, _ = self.socket.recvfrom(2048)
                message = json.loads(data.decode())
                print(f"Received message: {message['type']}")

                if message["type"] == "init":
                    print(f"Initialized as player {message['player_id']}")
                    self.player_id = message["player_id"]
                    self.game_state = message["game_state"]
                    self.connected = True
                elif message["type"] == "game_state":
                    self.game_state = message["game_state"]
            except Exception as e:
                print(f"Error in listener: {e}")
                time.sleep(0.1)

    def send_paddle_update(self, y_pos):
        if self.connected:
            message = {"type": "paddle_update", "y_pos": y_pos}
            try:
                self.socket.sendto(json.dumps(message).encode(), self.server_address)
            except Exception as e:
                print(f"Error sending paddle update: {e}")

    def connect(self, player_name):
        print(f"Attempting to connect as {player_name}...")
        message = {"type": "connect", "name": player_name}
        try:
            self.socket.sendto(json.dumps(message).encode(), self.server_address)
            print("Connection message sent")
            # Start listener thread after sending connect message
            threading.Thread(target=self.listen_for_messages, daemon=True).start()
        except Exception as e:
            print(f"Error connecting: {e}")

    def draw(self):
        self.screen.fill(BLACK)

        if self.game_state:
            try:
                # Draw ball
                ball = self.game_state["ball"]
                pygame.draw.circle(
                    self.screen, WHITE, (int(ball["x"]), int(ball["y"])), BALL_SIZE
                )

                # Draw paddles and scores
                for pid, paddle in self.game_state["paddles"].items():
                    pygame.draw.rect(
                        self.screen,
                        WHITE,
                        (
                            paddle["x"] - PADDLE_WIDTH // 2,
                            paddle["y"] - PADDLE_HEIGHT // 2,
                            PADDLE_WIDTH,
                            PADDLE_HEIGHT,
                        ),
                    )

                    # Draw scores
                    font = pygame.font.Font(None, 36)
                    score_text = f"{paddle['name']}: {paddle['score']}"
                    text = font.render(score_text, True, WHITE)
                    x_pos = 100 if int(pid) == 1 else SCREEN_WIDTH - 200
                    self.screen.blit(text, (x_pos, 20))

                pygame.display.flip()
            except Exception as e:
                print(f"Error drawing: {e}")

    def run(self):
        player_name = input("Enter your name: ")
        self.connect(player_name)

        while True:
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    pygame.quit()
                    sys.exit()

            # Handle paddle movement
            keys = pygame.key.get_pressed()
            if self.player_id and self.game_state:
                try:
                    paddle = self.game_state["paddles"][str(self.player_id)]
                    y_pos = paddle["y"]

                    if keys[pygame.K_UP]:
                        y_pos -= 5
                    if keys[pygame.K_DOWN]:
                        y_pos += 5

                    self.send_paddle_update(y_pos)
                except Exception as e:
                    print(f"Error updating paddle: {e}")

            self.draw()
            self.clock.tick(60)


if __name__ == "__main__":
    client = GameClient()
    client.run()
