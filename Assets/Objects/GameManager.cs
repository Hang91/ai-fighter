using UnityEngine;
using TMPro; // Required for the winner text

/// <summary>
/// Manages the game state. Counts players on each team and subscribes
/// to their death events to determine a winner.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI winnerText;

    private int team1PlayerCount = 0;
    private int team2PlayerCount = 0;
    private bool gameIsOver = false;

    void Start()
    {
        // Make sure winner text is hidden at the start
        if(winnerText != null)
        {
            winnerText.gameObject.SetActive(false);
        }

        // Find all players in the scene
        PlayerHealth[] allPlayers = FindObjectsOfType<PlayerHealth>();

        foreach (PlayerHealth player in allPlayers)
        {
            // Count players for each team
            if (player.teamID == 1)
            {
                team1PlayerCount++;
            }
            else if (player.teamID == 2)
            {
                team2PlayerCount++;
            }

            // Subscribe to this player's death event
            player.OnPlayerDied += HandlePlayerDeath;
        }

        Debug.Log($"Game Start! Team 1: {team1PlayerCount} players. Team 2: {team2PlayerCount} players.");
    }

    /// <summary>
    /// This method is called by the OnPlayerDied event from any PlayerHealth script.
    /// </summary>
    /// <param name="teamID">The team of the player who just died.</param>
    private void HandlePlayerDeath(int teamID)
    {
        if (gameIsOver) return; // Don't do anything if the game is already over

        // Decrement the correct team count
        if (teamID == 1)
        {
            team1PlayerCount--;
            Debug.Log($"A player from Team 1 died. {team1PlayerCount} remaining.");
        }
        else if (teamID == 2)
        {
            team2PlayerCount--;
            Debug.Log($"A player from Team 2 died. {team2PlayerCount} remaining.");
        }

        // Check for a winner
        if (team1PlayerCount <= 0)
        {
            EndGame("TEAM 2 WINS!");
        }
        else if (team2PlayerCount <= 0)
        {
            EndGame("TEAM 1 WINS!");
        }
    }

    private void EndGame(string winnerMessage)
    {
        gameIsOver = true;
        Debug.Log("Game Over! " + winnerMessage);

        // Show the winner text on screen
        if (winnerText != null)
        {
            winnerText.text = winnerMessage;
            winnerText.gameObject.SetActive(true);
        }

        // Optional: Stop the game
        // Time.timeScale = 0f; // Uncomment this line to freeze the game
    }
}
