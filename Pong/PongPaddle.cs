using UnityEngine;
using UnityEngine.InputSystem;

public enum PongPlayer
{
  PlayerLeft = 1,
  PlayerRight = 2
}

public class PongPaddle : MonoBehaviour
{
  public PongPlayer Player = PongPlayer.PlayerLeft;
  public float Speed = 5;
  public float MinY = -4;
  public float MaxY = 4;

  // Server's screen dimensions
  private const float SERVER_WIDTH = 800f;
  private const float SERVER_HEIGHT = 600f;

  PongInput inputActions;
  InputAction PlayerAction;
  PongNetworkManager networkManager;

  void Start()
  {
    inputActions = new PongInput();
    switch (Player)
    {
      case PongPlayer.PlayerLeft:
        PlayerAction = inputActions.Pong.Player1;
        break;
      case PongPlayer.PlayerRight:
        PlayerAction = inputActions.Pong.Player2;
        break;
    }

    PlayerAction.Enable();
    networkManager = PongNetworkManager.Instance;

    if (networkManager == null)
    {
      Debug.LogError("NetworkManager not found in scene!");
    }
  }

  void Update()
  {
    if (networkManager == null || !networkManager.IsConnected()) return;

    var gameState = networkManager.GetGameState();
    if (gameState != null && gameState.paddles != null)
    {
      string paddleKey = ((int)Player).ToString();

      if (gameState.paddles.ContainsKey(paddleKey))
      {
        if ((int)Player == networkManager.playerId)
        {
          // Handle local player input
          float direction = PlayerAction.ReadValue<float>();
          Vector3 newPos = transform.position + (Vector3.up * Speed * direction * Time.deltaTime);
          newPos.y = Mathf.Clamp(newPos.y, MinY, MaxY);
          transform.position = newPos;

          // Send update to server
          float serverY = (newPos.y / 4 * (SERVER_HEIGHT / 2)) + SERVER_HEIGHT / 2;
          networkManager.SendPaddleUpdate(serverY);
        }
        else
        {
          // Update opponent paddle position from server
          float unityY = (gameState.paddles[paddleKey].y - SERVER_HEIGHT / 2) / (SERVER_HEIGHT / 2) * 4;
          transform.position = new Vector3(
            transform.position.x,
            unityY,
            transform.position.z
          );
        }
      }
    }
  }

  void OnDisable()
  {
    PlayerAction.Disable();
  }
}
