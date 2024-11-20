using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;

public class GameState
{
    public Ball ball;
    public Dictionary<string, Paddle> paddles;

    public class Ball
    {
        public float x;
        public float y;
    }

    public class Paddle
    {
        public float x;
        public float y;
        public int score;
        public string name;
    }
}

public class PongNetworkManager : MonoBehaviour
{
    private UdpClient client;
    private IPEndPoint serverEndPoint;
    private Thread listenerThread;
    private bool isRunning = true;
    public int playerId;
    private GameState gameState;
    public string playerName = "Player";

    public static PongNetworkManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeNetwork();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeNetwork()
    {
        Debug.Log("Initializing network connection...");
        try
        {
            client = new UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Parse("192.168.2.65"), 9876);

            // Start listener thread
            listenerThread = new Thread(new ThreadStart(ListenForMessages));
            listenerThread.IsBackground = true;
            listenerThread.Start();

            // Send connect message
            SendConnectMessage();
            Debug.Log("Network initialization complete");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize network: {e.Message}");
        }
    }

    void SendConnectMessage()
    {
        Debug.Log("Sending connect message...");
        try
        {
            var connectMessage = new { type = "connect", name = playerName };
            string jsonMessage = JsonConvert.SerializeObject(connectMessage);
            byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
            client.Send(data, data.Length, serverEndPoint);
            Debug.Log("Connect message sent successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to send connect message: {e.Message}");
        }
    }

    void ListenForMessages()
    {
        Debug.Log("Started listening for messages...");
        while (isRunning)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref remoteEP);
                string message = Encoding.UTF8.GetString(data);
                Debug.Log($"Received message: {message}");

                var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);

                if (response["type"].ToString() == "init")
                {
                    playerId = int.Parse(response["player_id"].ToString());
                    gameState = JsonConvert.DeserializeObject<GameState>(response["game_state"].ToString());
                    Debug.Log($"Initialized as player {playerId}");
                }
                else if (response["type"].ToString() == "game_state")
                {
                    gameState = JsonConvert.DeserializeObject<GameState>(response["game_state"].ToString());
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Network error: {e.Message}");
                Thread.Sleep(100);
            }
        }
    }

    public void SendPaddleUpdate(float yPos)
    {
        if (client == null) return;

        try
        {
            var updateMessage = new { type = "paddle_update", y_pos = yPos };
            string jsonMessage = JsonConvert.SerializeObject(updateMessage);
            byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
            client.Send(data, data.Length, serverEndPoint);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to send paddle update: {e.Message}");
        }
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    void OnDestroy()
    {
        Debug.Log("Cleaning up network connection...");
        isRunning = false;
        if (client != null)
        {
            client.Close();
        }
        if (listenerThread != null)
        {
            listenerThread.Join();
        }
    }

    // Add this to check if we're properly connected
    public bool IsConnected()
    {
        return gameState != null && playerId != 0;
    }
}
