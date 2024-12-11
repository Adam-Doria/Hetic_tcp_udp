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
            // Format: PADDLE_<Team>|x|y|z

            if (tokens.Length == 4 && tokens[0].StartsWith("PADDLE"))
            {
                float x = float.Parse(tokens[1]);
                float y = float.Parse(tokens[2]);
                float z = float.Parse(tokens[3]);
                Paddle.transform.position = new Vector3(x, y, z);
            }
        };
    }
}
