using UnityEngine;

public class PaddleSyncServer : MonoBehaviour
{
    public PongPaddle Paddle;
    ServerManager ServerMan;
    float NextUpdateTimeout = -1;

    void Awake()
    {
        if (!Globals.IsServer)
        {
            enabled = false;
        }
    }

    void Start()
    {
        ServerMan = FindObjectOfType<ServerManager>();
    }

    void Update()
    {
        if (Time.time > NextUpdateTimeout)
        {
            float aggregatedInput = 0f;

            lock (ServerMan.TeamInputs)
            {
                var teamData = ServerMan.TeamInputs[Paddle.Player];
                aggregatedInput = teamData.GetAverage();
                teamData.Reset(); // Clear for next cycle
            }

            // Move the paddle based on aggregated input
            Vector3 newPos = Paddle.transform.position + (Vector3.up * Paddle.Speed * aggregatedInput * 0.03f);
            newPos.y = Mathf.Clamp(newPos.y, Paddle.MinY, Paddle.MaxY);
            Paddle.transform.position = newPos;

            // Broadcast position to clients
            PaddleState state = new PaddleState { Position = Paddle.transform.position };
            string json = JsonUtility.ToJson(state);
            string message = $"PADDLE_{Paddle.Player}|{json}";
            ServerMan.BroadcastUDPMessage(message);

            // Update every 0.03 seconds
            NextUpdateTimeout = Time.time + 0.03f;
        }
    }
}
