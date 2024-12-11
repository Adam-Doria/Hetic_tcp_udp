using System.Net;
using UnityEngine;

public class PaddleSyncClient : MonoBehaviour
{
    public PongPaddle Paddle; 
    UDPService UDP;

    void Awake() {
        if (Globals.IsServer) {
            enabled = false;
        }
    }

    void Start() {
        UDP = FindObjectOfType<UDPService>();

        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            if (!message.StartsWith("PADDLE")) { return; }

            string[] tokens = message.Split('|');
            if (tokens.Length < 2) return;

            string paddleId = tokens[0];
            string json = tokens[1];

            if (paddleId != $"PADDLE_{Paddle.Player}") return;

            PaddleState state = JsonUtility.FromJson<PaddleState>(json);
            Paddle.transform.position = state.Position;
        };
    }
}
