using UnityEngine;

public class BallSyncServer : MonoBehaviour
{
    ServerManager ServerMan;
    float NextUpdateTimeout = -1;
    PongBall Ball;

    void Awake() {
        if (!Globals.IsServer) {
            enabled = false;
        }
    }

    void Start()
    {
        ServerMan = FindObjectOfType<ServerManager>();
        Ball = FindObjectOfType<PongBall>();
    }

    void Update()
    {  
        if (Time.time > NextUpdateTimeout) {
            Vector3 position = transform.position;
            Vector3 direction = Ball != null ? Ball.GetDirection() : Vector3.right;
            float speed = Ball != null ? Ball.Speed : 1f;
            string message = $"UPDATE|{position.x}|{position.y}|{position.z}|{direction.x}|{direction.y}|{direction.z}|{speed}";

            ServerMan.BroadcastUDPMessage(message);
            NextUpdateTimeout = Time.time + 1.0f; 
        }
    }
}