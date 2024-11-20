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
    private bool isRunning = false;
    public int playerId;
    private GameState gameState;
    public string playerName = "Player";
    private bool isConnected = false;
  


    public static PongNetworkManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Configures the manager with server IP, port, and player name, and initializes the connection
    public void ConfigureAndConnect(string ip, int port)
    {
        if (isRunning)
        {
            Debug.LogWarning("Manager is already configured and running.");
            return;
        }

        Debug.Log($"Setting up connection to {ip}:{port}");

        try
        {
            client = new UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            // Start the listener thread
            isRunning = true;
            listenerThread = new Thread(new ThreadStart(ListenForMessages));
            listenerThread.IsBackground = true;
            listenerThread.Start();

            // Send a connect message to the server
            SendConnectMessage();
            Debug.Log("Connection successfully initialized.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error while configuring the connection: {e.Message}");
            isRunning = false;
        }
    }

    // Sends a connect message to the server
    private void SendConnectMessage()
    {
        if (client == null || serverEndPoint == null) return;

        Debug.Log("Sending connection message...");
        try
        {
            var connectMessage = new { type = "connect", name = playerName };
            string jsonMessage = JsonConvert.SerializeObject(connectMessage);
            byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
            client.Send(data, data.Length, serverEndPoint);
            Debug.Log("Connection message sent.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error sending connection message: {e.Message}");
        }
    }

    // Listens for incoming messages from the server
    private void ListenForMessages()
    {
        Debug.Log("Listener thread started...");
        while (isRunning)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref remoteEP);
                string message = Encoding.UTF8.GetString(data);
                // Debug.Log($"Received message: {message}");

                var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);

                if (response["type"].ToString() == "init")
                {
                    Debug.Log("Received init message from server");
                    playerId = int.Parse(response["player_id"].ToString());
                    gameState = JsonConvert.DeserializeObject<GameState>(response["game_state"].ToString());
                    isConnected = true;
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

    // Sends an update for the paddle's position to the server
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
            Debug.LogError($"Error sending paddle update: {e.Message}");
        }
    }

    // Returns the current game state
    public GameState GetGameState()
    {
        return gameState;
    }

    public bool IsConnected()
    {
        return isConnected;
    }



    // Cleans up the network connection and stops the listener thread
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
}
