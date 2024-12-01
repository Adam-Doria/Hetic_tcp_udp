using UnityEngine;
using System.Collections.Generic;

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
        // Get inputs from the server manager
        List<float> inputs;
        lock (ServerMan.TeamInputs) {
            inputs = new List<float>(ServerMan.TeamInputs[Paddle.Player]);
            ServerMan.TeamInputs[Paddle.Player].Clear();
        }

        float aggregatedInput = 0f;
        if (inputs.Count > 0) {
            foreach (float input in inputs) {
                aggregatedInput += input;
            }
            aggregatedInput /= inputs.Count; // Average the inputs
        }

        // Move the paddle based on the aggregated input
        float direction = aggregatedInput;

        Vector3 newPos = Paddle.transform.position + (Vector3.up * Paddle.Speed * direction * Time.deltaTime);
        newPos.y = Mathf.Clamp(newPos.y, Paddle.MinY, Paddle.MaxY);

        Paddle.transform.position = newPos;

        // Send the updated paddle position to clients
        PaddleState state = new PaddleState {
            Position = Paddle.transform.position
        };

        string json = JsonUtility.ToJson(state);
        string message = $"PADDLE_{Paddle.Player}|{json}";

        ServerMan.BroadcastUDPMessage(message);
        NextUpdateTimeout = Time.time + 0.03f;
    }
}
}
