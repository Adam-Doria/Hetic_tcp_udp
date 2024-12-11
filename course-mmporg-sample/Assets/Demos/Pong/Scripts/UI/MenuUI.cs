using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuUI : MonoBehaviour
{
    public TMP_InputField ServerIPInput;
    public void SetRole(bool isServer) {
        Globals.IsServer = isServer;
    }

    public void StartGame() {
           if (!Globals.IsServer && ServerIPInput != null && !string.IsNullOrEmpty(ServerIPInput.text)) {
            Globals.ServerIP = ServerIPInput.text;
        } else if (!Globals.IsServer && (ServerIPInput == null || string.IsNullOrEmpty(ServerIPInput.text))) {
            Globals.ServerIP = "127.0.0.1";
        }

        SceneManager.LoadScene("Pong");
    }

    public void setTeamChoice(bool teamChoice){//True -> Left team | False -> Right team
        Globals.teamChoice = teamChoice;
    }
}
