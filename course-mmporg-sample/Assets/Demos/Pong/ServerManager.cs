using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public UDPService UDP;
    public int ListenPort = 25000;

    public Dictionary<string, IPEndPoint> Clients = new Dictionary<string, IPEndPoint>(); 
    public Dictionary<string, PongPlayer> PlayerTeams = new Dictionary<string, PongPlayer>();
    //private PongPlayer nextTeam = PongPlayer.PlayerLeft;
    public Dictionary<PongPlayer, List<float>> TeamInputs = new Dictionary<PongPlayer, List<float>>();
    public PlayerCountDisplay PlayerCountDisplay;


    void Awake() {
        // Desactiver mon objet si je ne suis pas le serveur
        if (!Globals.IsServer) {
            gameObject.SetActive(false);
        }
    }

    void Start()
    {
        UDP.Listen(ListenPort);

    
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
                    if (!Clients.ContainsKey(addr)) {
                        Clients.Add(addr, sender);

                        PongPlayer assignedTeam = (tokens[1]=="True")? PongPlayer.PlayerLeft : PongPlayer.PlayerRight;
                        PlayerTeams.Add(addr, assignedTeam);

                        //nextTeam = (nextTeam == PongPlayer.PlayerLeft) ? PongPlayer.PlayerRight : PongPlayer.PlayerLeft;

                        Debug.Log("Assigned player " + addr + " to team " + assignedTeam);

                        UDP.SendUDPMessage("welcome|" + assignedTeam.ToString(), sender);

                        // Recalculer le nombre de joueurs
                        int leftTeamCount = 0;
                        int rightTeamCount = 0;
                        foreach (var team in PlayerTeams.Values) {
                            if (team == PongPlayer.PlayerLeft) leftTeamCount++;
                            else if (team == PongPlayer.PlayerRight) rightTeamCount++;
                        }

                        // Envoyer à tous
                        string playerCountMessage = $"PLAYER_COUNT|{leftTeamCount}|{rightTeamCount}";
                        BroadcastUDPMessage(playerCountMessage);

                        if (PlayerCountDisplay != null) {
                            PlayerCountDisplay.UpdatePlayerCounts(leftTeamCount, rightTeamCount);
                        }
                    }
                break;
                // gérer les déplacements au seins d'une équipe
                    // En gros le serveur va jouer le rôle de gestionnaire des inputs, c'est lui qui va faire la pondération sur le nombre de joueur et voir les commandes réalisées
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
                     //gérer les cas de déconnection=>MAJ du nombre de joueurs
                    case "DISCONNECT":
                    if (Clients.ContainsKey(addr)) {
                        Clients.Remove(addr);
                        if (PlayerTeams.ContainsKey(addr)) {
                            PlayerTeams.Remove(addr);
                        }

                        int leftCount = 0, rightCount = 0;
                        foreach (var t in PlayerTeams.Values) {
                            if (t == PongPlayer.PlayerLeft) leftCount++;
                            else if (t == PongPlayer.PlayerRight) rightCount++;
                        }

                        BroadcastUDPMessage($"PLAYER_COUNT|{leftCount}|{rightCount}");
                        if (PlayerCountDisplay != null) {
                            PlayerCountDisplay.UpdatePlayerCounts(leftCount, rightCount);
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
