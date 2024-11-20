using UnityEngine;

public class PongBall : MonoBehaviour
{
  PongNetworkManager networkManager;

  // Server's screen dimensions
  private const float SERVER_WIDTH = 800f;
  private const float SERVER_HEIGHT = 600f;

  void Start()
  {
    networkManager = PongNetworkManager.Instance;
  }

  void Update()
  {
    if (networkManager == null) return;

    var gameState = networkManager.GetGameState();
    if (gameState != null && gameState.ball != null)
    {
      // Convert server coordinates to Unity coordinates
      float unityX = (gameState.ball.x - SERVER_WIDTH / 2) / (SERVER_WIDTH / 2) * 8; // 8 is Unity's width
      float unityY = (gameState.ball.y - SERVER_HEIGHT / 2) / (SERVER_HEIGHT / 2) * 4; // 4 is Unity's height

      transform.position = new Vector3(
          unityX,
          unityY,
          transform.position.z
      );
    }
  }
}
