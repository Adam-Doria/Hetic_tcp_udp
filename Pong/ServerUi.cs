using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ServerUI : MonoBehaviour
{
    public TMP_InputField ipInput;
    public TMP_InputField portInput;

    public void StartGame()
    {
        if (ipInput.text == "" || portInput.text == "")
        {
            return;
        }
        SetServerIP(ipInput.text);
        SetServerPort(int.Parse(portInput.text));
        SceneManager.LoadScene("Pong");
    }

    public void SetServerIP(string ip)
    {
        Globals.ServerIP = ip;
    }

    public void SetServerPort(int port)
    {
        Globals.ServerPort = port;
    }
}
