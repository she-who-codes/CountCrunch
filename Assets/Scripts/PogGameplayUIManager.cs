using System.Collections;
using TMPro; // Use TMPro if using TextMeshPro components
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class GameplayUIManager : MonoBehaviour
{
    [Header("UI Text Displays")]
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI npcScoreText;
    [SerializeField] private TextMeshProUGUI statusStateText;

    [Header("Visual Styling Tweak")]
    [SerializeField] private Color playerColor = Color.cyan;
    [SerializeField] private Color npcColor = Color.red;
    [SerializeField] private Color alertColor = Color.yellow;

    private void OnEnable()
    {
        // Bind functions directly to our GameManager action streams
        PogGameManager.OnStateChanged += UpdateStatusBanner;
        PogGameManager.OnScoresUpdated += RefreshScoreboardDisplays;
    }

    private void OnDisable()
    {
        // Always unbind events when disabled or leaving the scene to prevent memory leaks
        PogGameManager.OnStateChanged -= UpdateStatusBanner;
        PogGameManager.OnScoresUpdated -= RefreshScoreboardDisplays;
    }

    /// <summary>
    /// Listens to score change events and updates the display counts instantly
    /// </summary>
    private void RefreshScoreboardDisplays(int currentSubScore, int currentNpcScore)
    {
        if (playerScoreText != null) playerScoreText.text = $"Caps Taken: {currentSubScore}";
        if (npcScoreText != null) npcScoreText.text = $"Caps Taken: {currentNpcScore}";
    }

    /// <summary>
    /// Listens to the core state machine transitions and applies dynamic mobile banners
    /// </summary>
    private void UpdateStatusBanner(PogGameManager.GameState activeMatchState)
    {
        if (statusStateText == null) return;

        Debug.Log("====================================================> " + activeMatchState);
        switch (activeMatchState)
        {
            case PogGameManager.GameState.Init:
                statusStateText.text = "SETTING UP TABLE...";
                statusStateText.color = alertColor;
                break;

            case PogGameManager.GameState.PlayerTurn:
                statusStateText.text = "👉 YOUR TURN (SWIPE TO SLAM!)";
                statusStateText.color = playerColor;
                break;

            case PogGameManager.GameState.NPCTurn:
                statusStateText.text = "🤖 NPC IS AIMING...";
                statusStateText.color = npcColor;
                break;

            case PogGameManager.GameState.Slamming:
                statusStateText.text = "💥 SLAMMER DEPLOYED!";
                statusStateText.color = alertColor;
                break;

            case PogGameManager.GameState.Evaluation:
                statusStateText.text = "🔍 COUNTING FLIPPED POGS...";
                statusStateText.color = Color.white;
                break;

            case PogGameManager.GameState.GameOver:
                statusStateText.text = "MATCH OVER";
                statusStateText.color = Color.green;
                break;
        }
    }
}
