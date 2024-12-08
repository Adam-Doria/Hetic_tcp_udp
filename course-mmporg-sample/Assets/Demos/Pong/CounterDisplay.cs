using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCountDisplay : MonoBehaviour
{
    public TextMeshProUGUI LeftPlayerCounter;
    public TextMeshProUGUI RightPlayerCounter;

    void Start()
    {
        // Initialiser les textes
        UpdatePlayerCounts(0, 0);
    }

    public void UpdatePlayerCounts(int leftTeamCount, int rightTeamCount)
    {
        LeftPlayerCounter.text = $"Joueurs : {leftTeamCount}";
        RightPlayerCounter.text = $"Joueurs : {rightTeamCount}";
    }
}