using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public UDPService UDP;
    public int ListenPort = 25000;

    private int nextPlayerId = 1; 
    public Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();

    public Dictionary<PongPlayer, TeamInputData> TeamInputs = new Dictionary<PongPlayer, TeamInputData>();
    public PlayerCountDisplay PlayerCountDisplay;

    void Awake()
    {
        if (!Globals.IsServer)
        {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        UDP.Listen(ListenPort);

        TeamInputs[PongPlayer.PlayerLeft] = new TeamInputData();
        TeamInputs[PongPlayer.PlayerRight] = new TeamInputData();

        UDP.OnMessageReceived += (string message, IPEndPoint sender) =>
        {
            string addr = sender.Address.ToString() + ":" + sender.Port;
            string[] tokens = message.Split('|');

            switch (tokens[0])
            {
                case "coucou":
                    HandleNewPlayer(addr, tokens, sender);
                    break;
                case "INPUT":
                    HandlePlayerInput(tokens);
                    break;
                case "DISCONNECT":
                    HandlePlayerDisconnect(addr);
                    break;
            }
        };
    }

    void Update()
    {
    }

    private void HandleNewPlayer(string addr, string[] tokens, IPEndPoint sender)
    {
        if (GetPlayerIdByAddress(addr) == -1)
        {
            int playerId = nextPlayerId++;
            PongPlayer assignedTeam = PongPlayer.PlayerLeft;
            if (tokens.Length > 1 && tokens[1] == "False") {
                assignedTeam = PongPlayer.PlayerRight;
            }

            Players[playerId] = new PlayerData
            {
                PlayerId = playerId,
                Address = addr,
                Team = assignedTeam,
                EndPoint = sender
            };

            // Send welcome with playerId and team
            UDP.SendUDPMessage($"welcome|{playerId}|{assignedTeam}", sender);
            UpdatePlayerCounts();
        }
    }

    private void HandlePlayerInput(string[] tokens)
    {
        // INPUT|playerId|inputValue
        if (tokens.Length >= 3)
        {
            int playerId;
            if (int.TryParse(tokens[1], out playerId) && Players.ContainsKey(playerId))
            {
                float input;
                if (float.TryParse(tokens[2], out input))
                {
                    PongPlayer playerTeam = Players[playerId].Team;
                    lock (TeamInputs)
                    {
                        TeamInputs[playerTeam].AddInput(input);
                    }
                }
            }
        }
    }

    private void HandlePlayerDisconnect(string addr)
    {
        int disconnectingPlayerId = GetPlayerIdByAddress(addr);
        if (disconnectingPlayerId != -1 && Players.ContainsKey(disconnectingPlayerId))
        {
            Players.Remove(disconnectingPlayerId);
            UpdatePlayerCounts();
        }
    }

    private int GetPlayerIdByAddress(string address)
    {
        foreach (var kvp in Players)
        {
            if (kvp.Value.Address == address)
                return kvp.Key;
        }
        return -1; 
    }

    private void UpdatePlayerCounts()
    {
        int leftCount = 0, rightCount = 0;
        foreach (var player in Players.Values)
        {
            if (player.Team == PongPlayer.PlayerLeft) leftCount++;
            else if (player.Team == PongPlayer.PlayerRight) rightCount++;
        }

        BroadcastUDPMessage($"PLAYER_COUNT|{leftCount}|{rightCount}");
        if (PlayerCountDisplay != null)
        {
            PlayerCountDisplay.UpdatePlayerCounts(leftCount, rightCount);
        }
    }

    public void BroadcastUDPMessage(string message)
    {
        foreach (var player in Players.Values)
        {
            if (player.EndPoint != null)
            {
                UDP.SendUDPMessage(message, player.EndPoint);
            }
        }
    }
}

public class PlayerData
{
    public int PlayerId;
    public string Address;
    public PongPlayer Team;
    public IPEndPoint EndPoint;
}
