using System.Net;
using UnityEngine;

public class PaddleSyncClient : MonoBehaviour
{
    public PongPaddle Paddle; 
    UDPService UDP;

    void Awake()
    {
        if (Globals.IsServer)
        {
            enabled = false;
        }
    }

    void Start()
    {
        UDP = FindObjectOfType<UDPService>();

        if (UDP == null)
        {
            Debug.LogError("UDPService not found in the scene.");
            return;
        }

        UDP.OnMessageReceived += (string message, IPEndPoint sender) =>
        {
            if (!message.StartsWith("PADDLE")) return;

            string[] tokens = message.Split('|');
            // Format: PADDLE_<Team>|x|y|z

            if (tokens.Length == 4 && tokens[0].StartsWith("PADDLE"))
            {
                string team = tokens[0].Split('_')[1];

                if ((Paddle.Player == PongPlayer.PlayerLeft && team != "PlayerLeft") ||
                    (Paddle.Player == PongPlayer.PlayerRight && team != "PlayerRight"))
                {
                    // Message is not for this paddle
                    return;
                }

                if (float.TryParse(tokens[1], out float x) &&
                    float.TryParse(tokens[2], out float y) &&
                    float.TryParse(tokens[3], out float z))
                {
                    // Update paddle position
                    Paddle.transform.position = new Vector3(x, y, z);
                }
            }
        };
    }
}
