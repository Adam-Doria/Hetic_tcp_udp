using UnityEngine;
using UnityEngine.InputSystem;
using System.Net;

public class InputSenderClient : MonoBehaviour
{
    UDPService UDP;
    IPEndPoint ServerEndpoint;
    PongInput inputActions;
    InputAction PlayerAction;

    void Start()
    {
        UDP = FindObjectOfType<UDPService>();
        ServerEndpoint = new IPEndPoint(IPAddress.Parse("172.31.16.148"), 25000); // Replace with your server IP and port

        inputActions = new PongInput();
        PlayerAction = inputActions.Pong.Player1;
        PlayerAction.Enable();
    }

    void Update()
    {
        float direction = PlayerAction.ReadValue<float>();

        // Send input to server
        string message = $"INPUT|{Globals.LocalPlayer}|{direction}";
        UDP.SendUDPMessage(message, ServerEndpoint);
    }

    void OnDisable()
    {
        PlayerAction.Disable();
    }
}
