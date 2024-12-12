using System.Net;
using UnityEngine;
using System.Net;

public class ClientManager : MonoBehaviour
{
    public UDPService UDP;
    public string ServerIP = "172.31.16.148";
    public int ServerPort = 25000;

    private float NextCoucouTimeout = -1f;
    private float NextInputTimeout = -1f;
    private IPEndPoint ServerEndpoint;
    public PlayerCountDisplay PlayerCountDisplay;

    void Awake()
    {
        if (Globals.IsServer) {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        UDP.InitClient();
        ServerEndpoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);

        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            Debug.Log("[CLIENT] Message received: " + message);

            if (message.StartsWith("welcome")) {
                string[] tokens = message.Split('|');
                if (tokens.Length > 2) {
                    int playerId;
                    if (int.TryParse(tokens[1], out playerId)) {
                        Globals.LocalPlayerId = playerId;
                    }

                    PongPlayer assignedTeam;
                    if (System.Enum.TryParse(tokens[2], out assignedTeam)) {
                        Globals.LocalPlayer = assignedTeam;
                        Debug.Log("Assigned to team " + assignedTeam);
                    }
                }
            } else if (message.StartsWith("PLAYER_COUNT")) {
                string[] tokens = message.Split('|');
                if (tokens.Length >= 3) {
                    int leftTeamCount;
                    int rightTeamCount;
                    if (int.TryParse(tokens[1], out leftTeamCount) && int.TryParse(tokens[2], out rightTeamCount)) {
                        Debug.Log($"[CLIENT] Updating player counts - Left: {leftTeamCount}, Right: {rightTeamCount}");
                        if (PlayerCountDisplay != null) {
                            PlayerCountDisplay.UpdatePlayerCounts(leftTeamCount, rightTeamCount);
                        }
                    }
                }
            } else if (message.StartsWith("SCORE_UPDATE")) {
                 string[] tokens = message.Split('|');
                if (tokens.Length >= 3) {
                    int leftTeamScore;
                    int rightTeamScore;
                    if (int.TryParse(tokens[1], out leftTeamScore) && int.TryParse(tokens[2], out rightTeamScore)) {
                        Debug.Log($"[CLIENT] Updating player Scores - Left: {leftTeamScore}, Right: {rightTeamScore}");
                        PlayerCountDisplay.UpdatePlayerScores(leftTeamScore, rightTeamScore);
                    }
                }
            }else if (message.StartsWith("GAME_OVER")) {
                 string[] tokens = message.Split('|');
                    if (tokens.Length > 1) {
                        string winner = tokens[1];
                        Debug.Log($"[CLIENT] winner: {winner}");
                        PongBallState newState = (winner == "PlayerLeft") ? PongBallState.PlayerLeftWin : PongBallState.PlayerRightWin;
                        PongBall ball = FindObjectOfType<PongBall>(); 
                        if (ball != null) {
                            ball.SetState(newState);
                        }
                    }
            }
        };
    }
    

    void Update()
    {
        if (Time.time > NextCoucouTimeout) {
            UDP.SendUDPMessage("coucou|" + Globals.teamChoice, ServerEndpoint);
            NextCoucouTimeout = Time.time + 0.5f;
        }

        if (Time.time > NextInputTimeout && Globals.LocalPlayer != PongPlayer.None && Globals.LocalPlayerId != -1) {
            float input = Input.GetAxis("Vertical");
            // Send the unique playerId assigned by server, not just the team number
            string message = $"INPUT|{Globals.LocalPlayerId}|{input}";
            UDP.SendUDPMessage(message, ServerEndpoint);
            NextInputTimeout = Time.time + 0.05f;
        }
    }
    
    void OnApplicationQuit() {
        UDP.SendUDPMessage("DISCONNECT", ServerEndpoint);
    }
}
