using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ConnectionUI : MonoBehaviour
{
    public TMP_InputField inputFieldIP;       
    public TMP_InputField inputFieldPort;     
    public Button connectButton;         

    void Start()
    {
        connectButton.onClick.AddListener(OnConnectButtonClicked);
    }

    void OnConnectButtonClicked()
    {
        string ip = inputFieldIP.text;
        string portText = inputFieldPort.text;

        if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(portText))
        {
            Debug.LogError("Veuillez remplir tous les champs !");
            return;
        }

        if (!int.TryParse(portText, out int port))
        {
            Debug.LogError("Le port doit être un nombre valide !");
            return;
        }

        PongNetworkManager.Instance.ConfigureAndConnect(ip, port);

    Debug.Log("HIDE?");
        // Wait for the connection to be established
    StartCoroutine(WaitForConnection());
    Debug.Log("HIDE2 ");
    }
    IEnumerator WaitForConnection()
    {

        Debug.Log("Waiting for connection...");
 Debug.Log("HIDE4 ");
 
        while (!PongNetworkManager.Instance.IsConnected())
        {
        yield return null; // Wait until the next frame
        }
        Debug.Log("HIDE3 ");
        Debug.Log("Connected! Starting the game...");
        HideUI();
    }
    void HideUI()
    {
        Debug.Log("Hiding UI...");
        gameObject.SetActive(false); 

    }
}