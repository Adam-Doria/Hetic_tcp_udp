using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    public void SetRole(bool isServer) {
        Globals.IsServer = isServer;
    }

    public void StartGame() {
        SceneManager.LoadScene("Pong");
    }

    public void setTeamChoice(bool teamChoice){//True -> Left team | False -> Right team
        Globals.teamChoice = teamChoice;
    }
}
