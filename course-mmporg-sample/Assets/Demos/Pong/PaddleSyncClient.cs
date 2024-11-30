using System.Net;
using UnityEngine;

public class PaddleSyncClient : MonoBehaviour
{
    public PongPaddle Paddle; // Assigné dans l'éditeur Unity
    UDPService UDP;

    void Awake() {
        if (Globals.IsServer) {
            enabled = false;
        }
    }

    void Start() {
        UDP = FindFirstObjectByType<UDPService>();

        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            if (!message.StartsWith("PADDLE")) { return; }

            string[] tokens = message.Split('|');
            string paddleId = tokens[0];
            string json = tokens[1];

            // Vérifiez que le message concerne ce paddle
            if (paddleId != $"PADDLE_{Paddle.Player}") { return; }

            PaddleState state = JsonUtility.FromJson<PaddleState>(json);

            Paddle.transform.position = state.Position;
        };
    }
}
