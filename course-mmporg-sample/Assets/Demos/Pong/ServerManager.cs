using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public UDPService UDP;
    public int ListenPort = 25000;

    public Dictionary<string, IPEndPoint> Clients = new Dictionary<string, IPEndPoint>(); 
    public Dictionary<string, PongPlayer> PlayerTeams = new Dictionary<string, PongPlayer>();
    private PongPlayer nextTeam = PongPlayer.PlayerLeft;
    public Dictionary<PongPlayer, List<float>> TeamInputs = new Dictionary<PongPlayer, List<float>>();


    void Awake() {
        // Desactiver mon objet si je ne suis pas le serveur
        if (!Globals.IsServer) {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        UDP.Listen(ListenPort);

        // En gros le serveur va jouer le rôle de gestionnaire des inputs, c'est lui qui va faire la pondération sur le nombre de joueur et voir les comman
        TeamInputs[PongPlayer.PlayerLeft] = new List<float>();
        TeamInputs[PongPlayer.PlayerRight] = new List<float>();

        UDP.OnMessageReceived +=  
            (string message, IPEndPoint sender) => {
                Debug.Log("[SERVER] Message received from " + 
                    sender.Address.ToString() + ":" + sender.Port 
                    + " =>" + message);

                string addr = sender.Address.ToString() + ":" + sender.Port;
                string[] tokens = message.Split('|');
                switch (tokens[0]) {
                    case "coucou":
                    // Add the client to the Clients dictionary
                    if (!Clients.ContainsKey(addr)) {
                        Clients.Add(addr, sender);

                        // Assign team alternately
                        PongPlayer assignedTeam = nextTeam;
                        PlayerTeams.Add(addr, assignedTeam);

                        // Alternate the next team
                        nextTeam = (nextTeam == PongPlayer.PlayerLeft) ? PongPlayer.PlayerRight : PongPlayer.PlayerLeft;

                        Debug.Log("Assigned player " + addr + " to team " + assignedTeam);

                        // Send welcome message with team assignment
                        UDP.SendUDPMessage("welcome|" + assignedTeam.ToString(), sender);
                    }

                    // Display the number of players in each team
                    int leftTeamCount = 0;
                    int rightTeamCount = 0;
                    foreach (var team in PlayerTeams.Values) {
                        if (team == PongPlayer.PlayerLeft) leftTeamCount++;
                        else if (team == PongPlayer.PlayerRight) rightTeamCount++;
                    }
                    Debug.Log("Left Team Players: " + leftTeamCount);
                    Debug.Log("Right Team Players: " + rightTeamCount);

                    break;

                    case "INPUT":
                        if (tokens.Length >= 3) {
                        PongPlayer playerTeam;
                        if (System.Enum.TryParse(tokens[1], out playerTeam)) {
                            float input;
                            if (float.TryParse(tokens[2], out input)) {
                                lock (TeamInputs) {
                                    TeamInputs[playerTeam].Add(input);
                                }
                            }
                        }
                    }
                     break;
                }
                //@todo : do something with the message that has arrived! 
            };
    }

    public void BroadcastUDPMessage(string message) {
        foreach (KeyValuePair<string, IPEndPoint> client in Clients) {
            UDP.SendUDPMessage(message, client.Value);
        }
    }
}
