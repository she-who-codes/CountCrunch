using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class AppleGameManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject applePrefab;
    [SerializeField] private Transform[] quadrantCenters = new Transform[4];

    [Header("UI Setup")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Game Over Screen Setup")]
    [SerializeField] private GameObject gameOverPanel;        // Drag your UI Pop-up Panel here
    [SerializeField] private TextMeshProUGUI finalScoreText;   // Drag the Text component inside the panel here
    [SerializeField] private float exitInputDelay = 2.0f;      // Cooldown time in seconds before A works

    [Header("Audio Setup")]
    [SerializeField] private AudioSource backgroundMusic;     // Drag your AudioSource component here

    [Header("Settings")]
    [SerializeField] private float minSpawnDelay = 0.5f;
    [SerializeField] private float maxSpawnDelay = 2.0f;
    [SerializeField] private float minAppleLifetime = 2.0f;
    [SerializeField] private float maxAppleLifetime = 5.0f;
    [SerializeField] private float gameDuration = 45f;

    [Header("Score Settings")]
    [SerializeField] private int currentScore = 0;

    // Cooldown management flags
    private bool isGameOverScreenReady = false;
    private float gameOverTime;

    private List<string> availableButtons = new List<string> { "A", "B", "X", "Y" };
    private List<int> availableQuadrants = new List<int> { 0, 1, 2, 3 };
    private List<Apples> activeApples = new List<Apples>();

    private float nextSpawnTime;
    private float timeRemaining;
    private bool isGameActive = true;

    void Start()
    {
        timeRemaining = gameDuration;

        // Ensure the game over panel is safely hidden when starting the round
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateScoreUI();
        UpdateTimerUI();
        CalculateNextSpawnTime();
    }

    void Update()
    {
        if (!isGameActive)
        {
            // If the match ended, check if the cooldown timer has run its course
            if (!isGameOverScreenReady && Time.unscaledTime >= gameOverTime + exitInputDelay)
            {
                isGameOverScreenReady = true;
                
            }
            return;
        }

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            EndGame();
        }
        UpdateTimerUI();

        if (isGameActive && Time.time >= nextSpawnTime && availableButtons.Count > 0 && availableQuadrants.Count > 0)
        {
            SpawnApple();
            CalculateNextSpawnTime();
        }
    }

    private void CalculateNextSpawnTime()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    private void SpawnApple()
    {
        int buttonIndex = Random.Range(0, availableButtons.Count);
        string chosenButton = availableButtons[buttonIndex];
        availableButtons.RemoveAt(buttonIndex);

        int quadIndex = Random.Range(0, availableQuadrants.Count);
        int chosenQuad = availableQuadrants[quadIndex];
        availableQuadrants.RemoveAt(quadIndex);

        bool isGood = Random.value > 0.5f;
        float lifetime = Random.Range(minAppleLifetime, maxAppleLifetime);

        Vector3 spawnPosition = quadrantCenters[chosenQuad].position;
        GameObject appleObj = Instantiate(applePrefab, spawnPosition, Quaternion.identity);

        Apples appleScript = appleObj.GetComponent<Apples>();
        appleScript.Initialize(this, chosenButton, chosenQuad, isGood, lifetime);

        activeApples.Add(appleScript);
    }

    public void ReturnResources(string button, int quadrant, Apples apple)
    {
        if (!availableButtons.Contains(button)) availableButtons.Add(button);
        if (!availableQuadrants.Contains(quadrant)) availableQuadrants.Add(quadrant);
        activeApples.Remove(apple);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Points: {currentScore}";
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = $"Time: {Mathf.CeilToInt(timeRemaining)}s";
        }
    }

    private void EndGame()
    {
        isGameActive = false;
        gameOverTime = Time.unscaledTime; // Mark the exact timestamp the screen popped up

        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }

        List<Apples> applesToClear = new List<Apples>(activeApples);
        foreach (Apples apple in applesToClear)
        {
            if (apple != null)
            {
                apple.Despawn();
            }
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"{currentScore}";
        }
    }
    private void ExitMiniGame()
    {
        SceneManager.LoadScene("halloweenfestival");
    }
    #region Input System Package Callbacks
    public void OnButtonA(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isGameActive)
            {
                // Normal gameplay interaction
                CheckInputTrigger("A");
            }
            else if (isGameOverScreenReady)
            {
                // Match is over and delay has passed: proceed back to the main game
                ExitMiniGame();
            }
        }
    }
    public void OnButtonB(InputAction.CallbackContext context) { if (isGameActive && context.started) CheckInputTrigger("B"); }
    public void OnButtonX(InputAction.CallbackContext context) { if (isGameActive && context.started) CheckInputTrigger("X"); }
    public void OnButtonY(InputAction.CallbackContext context) { if (isGameActive && context.started) CheckInputTrigger("Y"); }

    private void CheckInputTrigger(string buttonPressed)
    {
        Apples targetApple = activeApples.Find(a => a.assignedButton == buttonPressed);

        if (targetApple != null)
        {
            if (targetApple.isGoodApple)
            {
                currentScore += 1;
            }
            else
            {
                currentScore -= 1;
            }

            UpdateScoreUI();
            targetApple.Despawn();
        }
    }
    #endregion
}
