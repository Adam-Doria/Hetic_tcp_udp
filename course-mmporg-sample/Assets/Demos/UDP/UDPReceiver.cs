using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class UDPReceiver : MonoBehaviour
{
    public int ListenPort = 25000;
    UdpClient udp;
    IPEndPoint localEP;

    public delegate void UDPMessageReceive(string message, IPEndPoint sender);

    private UDPMessageReceive OnMessageReceive;

    public bool Listen(UDPMessageReceive handler) {
        if (udp != null) {
            Debug.LogWarning("Socket already initialized! Close it first.");
            return false;
        }

        try
        {
            // Local End-Point
            localEP = new IPEndPoint(IPAddress.Any, ListenPort);
            
            
            // Create the listener
            udp = new UdpClient();
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.ExclusiveAddressUse = false;
            udp.Client.Bind(localEP);

            Debug.Log("Server listening on port: " + ListenPort);

            OnMessageReceive = handler;
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Error creating UDP listener on port: " + ListenPort + ": " + ex.Message);
            CloseUDP();
            return false;
        }
    }

    public void Close() {
        CloseUDP();
    }

    public bool IsListening {
        get {
            return (udp != null);
        }
    }

    void OnDisable() {
        CloseUDP();
    }

    void Update() {
        ReceiveUDP();
    }


    private void ReceiveUDP() {
        if (udp == null) { return; }

        while (udp.Available > 0)
		{
            IPEndPoint sourceEP = new IPEndPoint(IPAddress.Any, 0);
			byte[] data = udp.Receive(ref sourceEP);

			try
			{
				ParseString(data, sourceEP);
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning("Error receiving UDP message: " + ex.Message);
			}
		}
    }

    private void ParseString(byte[] bytes, IPEndPoint sender) {
        string message = System.Text.Encoding.UTF8.GetString(bytes);
        OnMessageReceive.Invoke(message, sender);
    }

    private void CloseUDP() {
        if (udp != null) {
            udp.Close();
            udp = null;
        }
        OnMessageReceive = null;
    }

    public void SendUDPMessage(string message, IPEndPoint destination) {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
        SendUDPBytes(bytes, destination);
    }

    private void SendUDPBytes(byte[] bytes, IPEndPoint destination) {
        if (udp == null) { 
            Debug.LogWarning("Trying to send a message on socket that is not yet open");
            return; 
        }

        try {
            udp.Send(bytes, bytes.Length, destination);
            
        } catch (SocketException e)
        {
            Debug.LogWarning(e.Message);
        }
    }
}
