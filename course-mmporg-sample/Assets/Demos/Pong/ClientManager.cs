using System.Net;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public UDPService UDP;
    public string ServerIP = "127.0.0.1";//192.168.77.65
    public int ServerPort = 25000;

    private float NextCoucouTimeout = -1;
    private IPEndPoint ServerEndpoint;
    public PlayerCountDisplay PlayerCountDisplay;
 
    //public bool leftOrRight = true;
    

    void Awake() {
        // Desactiver mon objet si je ne suis pas le client
        if (Globals.IsServer) {
            gameObject.SetActive(false);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UDP.InitClient();

        ServerEndpoint = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);
            
        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {
            Debug.Log("[CLIENT] Message received from " + sender.Address.ToString() + ":" + sender.Port + " => " + message);

            if (message.StartsWith("welcome")) {
                string[] tokens = message.Split('|');
                if (tokens.Length > 1) {
                    string teamStr = tokens[1];
                    PongPlayer assignedTeam;
                    if (System.Enum.TryParse(teamStr, out assignedTeam)) {
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
                        PlayerCountDisplay.UpdatePlayerCounts(leftTeamCount, rightTeamCount);
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
    

    // Update is called once per frame
    void Update()
    {
        if (Time.time > NextCoucouTimeout) {
            UDP.SendUDPMessage("coucou|"+Globals.teamChoice, ServerEndpoint);
            NextCoucouTimeout = Time.time + 0.5f;
        }
    }
    
    void OnApplicationQuit() {
        UDP.SendUDPMessage("DISCONNECT", ServerEndpoint);
    }
}