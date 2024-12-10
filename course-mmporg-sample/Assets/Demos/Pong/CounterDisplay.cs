using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCountDisplay : MonoBehaviour
{
    public TextMeshProUGUI LeftPlayerCounter;
    public TextMeshProUGUI RightPlayerCounter;
    public TextMeshProUGUI LeftPlayerScore;
    public TextMeshProUGUI RightPlayerScore;

    void Start()
    {
        // Initialiser les textes
        UpdatePlayerCounts(0, 0);
        UpdatePlayerScores(0, 0);
    }

    public void UpdatePlayerCounts(int leftTeamCount, int rightTeamCount)
    {
        LeftPlayerCounter.text = $"Joueurs : {leftTeamCount}";
        RightPlayerCounter.text = $"Joueurs : {rightTeamCount}";
    }

        public void UpdatePlayerScores(int leftTeamScore, int rightTeamScore)
    {
        LeftPlayerScore.text = $"Score : {leftTeamScore}";
        RightPlayerScore.text = $"Score : {rightTeamScore}";
    }
}