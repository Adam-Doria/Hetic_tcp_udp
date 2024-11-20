using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class ClientUDP : MonoBehaviour
{
    private UdpClient udpClient;
    private IPEndPoint serverEndpoint;
    private IPEndPoint clientEndpoint;
    public string serverIP = "192.168.2.65";
    public int serverPort = 9876;
    public int clientPort = 8000;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    serverEndpoint = new IPEndPoint(IPAddress.Parse(serverIP), 
    serverPort); 
    clientEndpoint = new IPEndPoint(IPAddress.Any,clientPort );

    udpClient = new UdpClient();

    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); 
    udpClient.Client.Bind(clientEndpoint);

    Debug.Log($"Client UDP initialisé. Envoi au serveur {serverIP}:{serverPort} depuis le port {clientPort}.");

    }

    // Update is called once per frame
    void Update()
    {
    }


}