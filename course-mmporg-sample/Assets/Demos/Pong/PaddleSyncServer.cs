using UnityEngine;

public class PaddleSyncServer : MonoBehaviour
{
    public PongPaddle Paddle;
    ServerManager ServerMan;
    float NextUpdateTimeout = -1;

    void Awake() {
        if (!Globals.IsServer) {
            enabled = false;
        }
    }

    void Start() {
        ServerMan = FindFirstObjectByType<ServerManager>();
    }

    void Update() {
        if (Time.time > NextUpdateTimeout) {
            PaddleState state = new PaddleState {
                Position = Paddle.transform.position
            };

            string json = JsonUtility.ToJson(state);

            // Ajout de l'identité du paddle
            string message = $"PADDLE_{Paddle.Player}|{json}";

            ServerMan.BroadcastUDPMessage(message);
            NextUpdateTimeout = Time.time + 0.03f;
        }
    }
}
