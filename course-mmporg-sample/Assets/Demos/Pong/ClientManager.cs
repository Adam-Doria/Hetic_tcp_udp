using System.Net;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public UDPService UDP;
    public string ServerIP = "127.0.0.1";
    public int ServerPort = 25000;

    private float NextCoucouTimeout = -1;
    private IPEndPoint ServerEndpoint;

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
            }
        };  
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > NextCoucouTimeout) {
            UDP.SendUDPMessage("coucou", ServerEndpoint);
            NextCoucouTimeout = Time.time + 0.5f;
        }
    }
}
