using System.Net;
using UnityEngine;

public class BallSyncClient : MonoBehaviour
{
    UDPService UDP;

    void Awake() {
      if (Globals.IsServer) {
        enabled = false;
      }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UDP = FindFirstObjectByType<UDPService>();

        UDP.OnMessageReceived += (string message, IPEndPoint sender) => {

            if (!message.StartsWith("UPDATE")) { return; }

            string[] tokens = message.Split('|');
            string json = tokens[1];

            BallState state = JsonUtility.FromJson<BallState>(json);
            
            transform.position = state.Position;

        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
