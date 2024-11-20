import socket
import json
import time
from test_py.game_state import GameState

# Store connected clients and their paddles
clients = {}
game_state = GameState()  # Create single game state instance


def remove_client(client_address):
    """Handle client disconnection"""
    if client_address in clients:
        player_id = clients[client_address]["id"]
        player_name = clients[client_address]["name"]
        print(f"Player {player_id} ({player_name}) disconnected")

        # Remove from clients dict
        del clients[client_address]

        # Remove from game state
        if player_id in game_state.paddles:
            del game_state.paddles[player_id]

        # If only one player remains, activate bot
        if len(clients) == 1:
            game_state.activate_bot()
            print("Bot activated due to player disconnection")

        # If no players remain, reset game state
        elif len(clients) == 0:
            game_state.__init__()
            print("Game state reset - no players remaining")


def start_server():
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    server_address = ("127.0.0.1", 9876)
    server_socket.bind(server_address)
    server_socket.settimeout(0.016)  # 60 FPS timeout

    print(f"Server started on {server_address[0]}:{server_address[1]}")

    last_update = time.time()
    last_ping = {}  # Track last message time for each client

    while True:
        try:
            current_time = time.time()

            # Check for disconnected clients (no messages for 5 seconds)
            disconnected = []
            for client in list(clients.keys()):
                if current_time - last_ping.get(client, 0) > 5:
                    disconnected.append(client)

            # Handle disconnections
            for client in disconnected:
                remove_client(client)

            try:
                # Receive message from a client
                data, client_address = server_socket.recvfrom(2048)
                last_ping[client_address] = current_time

                try:
                    # Decode the JSON message
                    message = json.loads(data.decode())
                    msg_type = message["type"]
                    print(f"Received {msg_type} from {client_address}")

                    # Handle new connections
                    if msg_type == "connect":
                        if client_address not in clients and len(clients) < 2:
                            # Assign player_id based on available IDs
                            available_ids = set([1, 2]) - set(
                                c["id"] for c in clients.values()
                            )
                            if available_ids:
                                player_id = min(available_ids)
                                clients[client_address] = {
                                    "id": player_id,
                                    "name": message["name"],
                                }
                                game_state.add_player(player_id, message["name"])

                                # Handle bot
                                if len(clients) == 1:
                                    game_state.activate_bot()
                                elif len(clients) == 2:
                                    game_state.deactivate_bot()

                                # Send initial game state
                                response = {
                                    "type": "init",
                                    "player_id": player_id,
                                    "game_state": game_state.to_dict(),
                                }
                                server_socket.sendto(
                                    json.dumps(response).encode(), client_address
                                )

                    # Handle paddle updates
                    elif msg_type == "paddle_update":
                        if client_address in clients:
                            player_id = clients[client_address]["id"]
                            game_state.update_paddle(player_id, message["y_pos"])

                    # Handle ping messages
                    elif msg_type == "ping":
                        last_ping[client_address] = current_time

                except json.JSONDecodeError:
                    print(f"Invalid JSON from {client_address}")
                except KeyError as e:
                    print(f"Missing key in message from {client_address}: {e}")

            except socket.timeout:
                pass  # No messages received this frame
            except ConnectionResetError:
                if client_address in clients:
                    remove_client(client_address)
            except Exception as e:
                print(f"Error receiving data: {e}")

            # Update game state
            if current_time - last_update >= 0.016:  # 60 FPS
                if len(game_state.paddles) > 0:  # Only update if there are players
                    game_state.update()
                last_update = current_time

                # Broadcast game state to all clients
                if clients:  # Only broadcast if there are clients
                    state_update = {
                        "type": "game_state",
                        "game_state": game_state.to_dict(),
                    }
                    disconnected = []
                    for client in list(clients.keys()):
                        try:
                            server_socket.sendto(
                                json.dumps(state_update).encode(), client
                            )
                        except Exception as e:
                            print(f"Error sending to {client}: {e}")
                            disconnected.append(client)

                    # Remove disconnected clients
                    for client in disconnected:
                        remove_client(client)

        except Exception as e:
            print(f"Server loop error: {e}")
            time.sleep(0.1)  # Add small delay on error


if __name__ == "__main__":
    start_server()
