import random
import math

SCREEN_WIDTH = 800
SCREEN_HEIGHT = 600
PADDLE_WIDTH = 20
PADDLE_HEIGHT = 100
BALL_SIZE = 10
PADDLE_SPEED = 5
BALL_SPEED = 7
BOT_NAME = "BOT"


class Paddle:
    def __init__(self, x, is_left=True):
        self.x = x
        self.y = SCREEN_HEIGHT // 2
        self.width = PADDLE_WIDTH
        self.height = PADDLE_HEIGHT
        self.score = 0
        self.is_left = is_left

    def move(self, y_pos):
        self.y = max(self.height // 2, min(y_pos, SCREEN_HEIGHT - self.height // 2))


class Ball:
    def __init__(self):
        self.reset()

    def reset(self):
        self.x = SCREEN_WIDTH // 2
        self.y = SCREEN_HEIGHT // 2
        angle = random.uniform(-math.pi / 4, math.pi / 4)
        if random.random() < 0.5:
            angle += math.pi
        self.dx = BALL_SPEED * math.cos(angle)
        self.dy = BALL_SPEED * math.sin(angle)


class GameState:
    def __init__(self):
        self.ball = Ball()
        self.paddles = {}
        self.bot_active = False

    def add_player(self, player_id, name):
        is_left = len(self.paddles) == 0
        x = PADDLE_WIDTH if is_left else SCREEN_WIDTH - PADDLE_WIDTH
        self.paddles[player_id] = {"paddle": Paddle(x, is_left), "name": name}

    def activate_bot(self):
        if 2 not in self.paddles:
            self.add_player(2, BOT_NAME)
        self.bot_active = True

    def deactivate_bot(self):
        if BOT_NAME in [p["name"] for p in self.paddles.values()]:
            del self.paddles[2]
        self.bot_active = False

    def update_paddle(self, player_id, y_pos):
        if player_id in self.paddles:
            self.paddles[player_id]["paddle"].move(y_pos)

    def update(self):
        # Update ball position
        self.ball.x += self.ball.dx
        self.ball.y += self.ball.dy

        # Bot AI
        if self.bot_active:
            bot_paddle = self.paddles[2]["paddle"]
            if self.ball.y > bot_paddle.y + PADDLE_HEIGHT // 4:
                bot_paddle.move(bot_paddle.y + PADDLE_SPEED)
            elif self.ball.y < bot_paddle.y - PADDLE_HEIGHT // 4:
                bot_paddle.move(bot_paddle.y - PADDLE_SPEED)

        # Ball collision with top and bottom
        if self.ball.y <= 0 or self.ball.y >= SCREEN_HEIGHT:
            self.ball.dy *= -1

        # Ball collision with paddles
        for paddle_info in self.paddles.values():
            paddle = paddle_info["paddle"]
            if (self.ball.x <= paddle.x + PADDLE_WIDTH and paddle.is_left) or (
                self.ball.x >= paddle.x - PADDLE_WIDTH and not paddle.is_left
            ):
                if (
                    paddle.y - PADDLE_HEIGHT // 2
                    <= self.ball.y
                    <= paddle.y + PADDLE_HEIGHT // 2
                ):
                    self.ball.dx *= -1
                    break

        # Score points
        if self.ball.x <= 0:
            self.paddles[2]["paddle"].score += 1
            self.ball.reset()
        elif self.ball.x >= SCREEN_WIDTH:
            self.paddles[1]["paddle"].score += 1
            self.ball.reset()

    def to_dict(self):
        return {
            "ball": {"x": self.ball.x, "y": self.ball.y},
            "paddles": {
                pid: {
                    "x": p["paddle"].x,
                    "y": p["paddle"].y,
                    "score": p["paddle"].score,
                    "name": p["name"],
                }
                for pid, p in self.paddles.items()
            },
        }
