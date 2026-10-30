using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CatapultGameManager : MonoBehaviour
{
    public static CatapultGameManager Instance { get; private set; }

    [Header("Puzzle Generation Settings")]
    [SerializeField] private Transform puzzleSpawnOrigin;
   // [SerializeField] private List<GameObject> puzzleLayoutPrefabs; // Pre-made structures of haybales/pumpkins

    [Header("Gameplay Settings")]
    [SerializeField] private CatapultController catapult;
    [SerializeField] private int baseAmmoCount = 3;


    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI currAmmoText;
    [SerializeField] private TextMeshProUGUI finalScoreText;   // Drag the Text component inside the panel here

    [Header("Audio Setup")]
    [SerializeField] private AudioSource backgroundMusic;


    private int currentAmmo;
    private int totalPumpkinsInPuzzle;
    private int smashedPumpkinsCount;
    private bool isGameOver;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // GenerateRandomPuzzle();
        currentAmmo = baseAmmoCount + currentAmmo;
    }
    /*
       private void GenerateRandomPuzzle()
       {
           if (puzzleLayoutPrefabs == null || puzzleLayoutPrefabs.Count == 0) return;

           // Clean old puzzles if reloading within the same scene lifecycle
           foreach (Transform child in puzzleSpawnOrigin) Destroy(child.gameObject);

           // Select and spawn a random layout structure
           int randomIndex = Random.Range(0, puzzleLayoutPrefabs.Count);
           GameObject selectedLayout = Instantiate(puzzleLayoutPrefabs[randomIndex], puzzleSpawnOrigin.position, Quaternion.identity, puzzleSpawnOrigin);

           // Dynamically find and calculate targets
           PumpkinDestructible[] pumpkins = selectedLayout.GetComponentsInChildren<PumpkinDestructible>();
           totalPumpkinsInPuzzle = pumpkins.Length;
           smashedPumpkinsCount = 0;
           isGameOver = false;

           // Dynamic difficulty setting for ammo counts based on layout scaling/complexity
           currentAmmo = baseAmmoCount + Mathf.FloorToInt(totalPumpkinsInPuzzle / 2f);

           UpdateUI();
       }

       private void GenerateRandomPuzzle()
       {
           if (puzzleGenerator == null) return;

           // Execute procedural code matrix calculations
           puzzleGenerator.CleanAndGenerate();

           // Dynamically find, track, and score the newly spawned pumpkin objects
           PumpkinDestructible[] pumpkins = puzzleGenerator.GetComponentsInChildren<PumpkinDestructible>();
           totalPumpkinsInPuzzle = pumpkins.Length;
           smashedPumpkinsCount = 0;
           isGameOver = false;

           // Fail-safe protection: if generation rolled extremely empty, restart calculation loop
           if (totalPumpkinsInPuzzle == 0)
           {
               GenerateRandomPuzzle();
               return;
           }

           currentAmmo = baseAmmoCount + Mathf.FloorToInt(totalPumpkinsInPuzzle / 2f);
           UpdateUI();
       }
       */
    public void OnPumpkinSmashed()
    {
        if (isGameOver) return;

        smashedPumpkinsCount++;
        UpdateUI();

        if (smashedPumpkinsCount >= totalPumpkinsInPuzzle)
        {
            EndGame(true);
        }
    }

    public void UseAmmo()
    {
        if (CatapultGameManager.Instance.isGameOver == false)
        {
            currentAmmo--;
            UpdateUI();

            if (currentAmmo <= 0 && smashedPumpkinsCount < totalPumpkinsInPuzzle)
            {
                // Give physics objects a moment to settle down before ending the game
                StartCoroutine(CheckForDelayedEndGame());
            }
        }
        else
        {
            // Match is over and delay has passed: proceed back to the main game
            ExitMiniGame();
        }
    }

    private IEnumerator CheckForDelayedEndGame()
    {
        yield return new WaitForSeconds(3.5f); // Wait for the final projectile trajectory impact
        //if (smashedPumpkinsCount < totalPumpkinsInPuzzle && !isGameOver)
        //{
            EndGame(false);
        //}
    }

    private void UpdateUI()
    {
        // Wire your TextMeshPro assets or UI elements here
        currAmmoText.text = $"{currentAmmo}";
        // Debug.Log($"Ammo Left: {currentAmmo} | Pumpkins Smashed: {smashedPumpkinsCount}/{totalPumpkinsInPuzzle}");
    }

    private void EndGame(bool won)
    {
        isGameOver = true;
        catapult.DisableControls();

        int finalScore = Mathf.Clamp((smashedPumpkinsCount * 100) + (currentAmmo * 250), 0, int.MaxValue);
         Debug.Log($"Game Over! Won: {won}. Score: {finalScore} .  Smashed: {smashedPumpkinsCount}  .  Current ammo: {currentAmmo}");
        // Trigger your overlay Scene Additive Final UI canvas here
        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }


        if (finalScoreText != null)
        {
            finalScoreText.text = $"{finalScore}";
        }


        StartCoroutine(DelayPopUp());

    }
    IEnumerator DelayPopUp()
    {
        yield return new WaitForSeconds(1.5f);
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    public void ExitMiniGame()
    {
        SceneManager.LoadScene("halloweenfestival");
    }
}

