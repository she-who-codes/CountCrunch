using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class TransformGridManager : MonoBehaviour
{
    [Header("Input Dependencies")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Card Anchors")]
    [Tooltip("Assign exactly 12 transforms ordered left-to-right, top-to-bottom.")]
    [SerializeField] private Transform[] gridPositions;
   
    [Header("Game Over Screen Setup")]
    [SerializeField] private GameObject gameOverPanel;        // Drag your UI Pop-up Panel here
    [SerializeField] private TextMeshProUGUI finalScoreText;   // Drag the Text component inside the panel here
    [SerializeField] private float exitInputDelay = 2.0f;      // Cooldown time in seconds before A works
    private bool isGameActive = true;

    [Header("Audio Setup")]
    [SerializeField] private AudioSource backgroundMusic;     // Drag your AudioSource component here

    [Header("Assets")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Sprite[] availableImages; // Set up your pool of 9 images here
    [SerializeField] private float misMatchDelay = 1f;
    [SerializeField] private TextMeshProUGUI CurrentScoreText;
    private MemoryCard3D[] instantiatedCards = new MemoryCard3D[12];
    private int currentIndex = 0;

    // Matching States
    private MemoryCard3D firstSelectedCard;
    private MemoryCard3D secondSelectedCard;
    private bool isProcessingMatch = false;

    // Metrics
    private int totalTaps = 0;
    private int matchesFound = 0;
    private const int TotalPairs = 6;

    // Input bindings
    private InputAction navigateAction;
    private InputAction selectAction;
    private bool inputThrottled = false;

    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        // Safely extract actions compiled inside the PlayerInput Component Map
        navigateAction = playerInput.actions["Navigate"];
        selectAction = playerInput.actions["Select"];
    }

    private void OnEnable()
    {
        // Event binding matches both Gamepad buttons and Keyboard stroke executions automatically
        selectAction.performed += OnSelectPerformed;
    }

    private void OnDisable()
    {
        selectAction.performed -= OnSelectPerformed;
    }


    private void Start()
    {
        if (gridPositions.Length != 12)
        {
            Debug.LogError("Please assign exactly 12 layout marker positions inside the Inspector array!");
            return;
        }

        GenerateCardsOnPositions();
        UpdateSelectionVisuals();
    }

    private void Update()
    {
        HandleGridNavigation();
    }

    private void GenerateCardsOnPositions()
    {
        // 1. Pick 6 random card images without repeats
        List<int> poolIndexes = new List<int>();
        for (int i = 0; i < availableImages.Length; i++) poolIndexes.Add(i);

        List<int> selectedIds = new List<int>();
        for (int i = 0; i < TotalPairs; i++)
        {
            int randomIndex = Random.Range(0, poolIndexes.Count);
            selectedIds.Add(poolIndexes[randomIndex]);
            poolIndexes.RemoveAt(randomIndex);
        }

        // 2. Build explicit pairs
        List<int> trackingDeck = new List<int>();
        foreach (int id in selectedIds)
        {
            trackingDeck.Add(id);
            trackingDeck.Add(id);
        }

        // 3. Perform standard array shuffle
        for (int i = 0; i < trackingDeck.Count; i++)
        {
            int temp = trackingDeck[i];
            int rand = Random.Range(i, trackingDeck.Count);
            trackingDeck[i] = trackingDeck[rand];
            trackingDeck[rand] = temp;
        }

        // 4. Instantiation across fixed position references
        for (int i = 0; i < gridPositions.Length; i++)
        {
            GameObject spawnedCard = Instantiate(cardPrefab, gridPositions[i].position, gridPositions[i].rotation, gridPositions[i]);
            MemoryCard3D cardComp = spawnedCard.GetComponent<MemoryCard3D>();

            int targetImageId = trackingDeck[i];
            cardComp.Initialize(targetImageId, availableImages[targetImageId]);

            instantiatedCards[i] = cardComp;
        }
    }
    private int FindNextValidCardIndex(int startIndex, int stepX, int stepY)
    {
        // Convert the 1D index into structural 2D grid coordinates (4x3 space)
        int startX = startIndex % 4;
        int startY = startIndex / 4;

        int checkX = startX;
        int checkY = startY;

        while (true)
        {
            checkX += stepX;
            checkY += stepY;

            // Break if the search bounds fall off the physical grid structure edges
            if (checkX < 0 || checkX >= 4 || checkY < 0 || checkY >= 3)
            {
                break;
            }

            int candidateIndex = checkY * 4 + checkX;

            // Return immediately if this step lands on an active, unmatched gameplay card
            if (!instantiatedCards[candidateIndex].IsMatched)
            {
                return candidateIndex;
            }
        }

        // Fallback: If pushing that direction hits only completed matches or a wall, don't move selection
        return startIndex;
    }
    /*private void HandleGridNavigation()
    {
        if (matchesFound >= TotalPairs) return;

        Vector2 stickInput = navigateAction.ReadValue<Vector2>();

        if (stickInput.magnitude < 0.5f)
        {
            inputThrottled = false;
            return;
        }

        if (inputThrottled) return;

        // Determine input direction
        int stepX = 0;
        int stepY = 0;

        if (Mathf.Abs(stickInput.x) > Mathf.Abs(stickInput.y))
        {
            stepX = (stickInput.x > 0) ? 1 : -1;
        }
        else
        {
            stepY = (stickInput.y > 0) ? -1 : 1; // -1 steps up a row in our 1D grid layout array
        }

        // Try to find the next valid unmatched card position in that input direction
        int nextIndex = FindNextValidCardIndex(currentIndex, stepX, stepY);

        if (nextIndex != currentIndex)
        {
            instantiatedCards[currentIndex].SetSelectionState(false);
            currentIndex = nextIndex;
            UpdateSelectionVisuals();
        }

        inputThrottled = true;
    }*/
    
    private void HandleGridNavigation()
    {
        Vector2 stickInput = navigateAction.ReadValue<Vector2>();

        if (stickInput.magnitude < 0.5f)
        {
            inputThrottled = false;
            return;
        }

        if (inputThrottled) return;

        // Deselect historical card position
        instantiatedCards[currentIndex].SetSelectionState(false);

        // Grid Math for a flat 4x3 mapping space inside a 1D Array (Length 12)
        // Row 0: 0,  1,  2,  3
        // Row 1: 4,  5,  6,  7
        // Row 2: 8,  9, 10, 11
        if (Mathf.Abs(stickInput.x) > Mathf.Abs(stickInput.y))
        {
            // Horizontal Navigation
            if (stickInput.x > 0)
                currentIndex = (currentIndex % 4 == 3) ? currentIndex : currentIndex + 1; // Block right edge
            else
                currentIndex = (currentIndex % 4 == 0) ? currentIndex : currentIndex - 1; // Block left edge
        }
        else
        {
            // Vertical Navigation
            if (stickInput.y > 0)
                currentIndex = (currentIndex - 4 < 0) ? currentIndex : currentIndex - 4; // Block upper edge
            else
                currentIndex = (currentIndex + 4 >= 12) ? currentIndex : currentIndex + 4; // Block lower edge
        }

        UpdateSelectionVisuals();
        inputThrottled = true;
    }

    private void UpdateSelectionVisuals()
    {
        if (instantiatedCards[currentIndex] != null) //&& !instantiatedCards[currentIndex].IsMatched)
        {
            instantiatedCards[currentIndex].SetSelectionState(true);
        }
    }


    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        if (isProcessingMatch) return;
        if (isGameActive)
        {
            MemoryCard3D currentCard = instantiatedCards[currentIndex];
            if (currentCard.IsFlipped || currentCard.IsMatched) return;

            totalTaps++;
            CurrentScoreText.text = $"Moves: {totalTaps}"; 

            currentCard.FlipCard(true);

            if (firstSelectedCard == null)
            {
                firstSelectedCard = currentCard;
            }
            else
            {
                secondSelectedCard = currentCard;
                StartCoroutine(CheckMatchRoutine());
            }
        }
        else 
        {
            // Match is over and delay has passed: proceed back to the main game
            ExitMiniGame();
        }

       
    }
    private IEnumerator CheckMatchRoutine()
    {
        isProcessingMatch = true;

        if (firstSelectedCard.CardID == secondSelectedCard.CardID)
        {
            firstSelectedCard.MarkAsMatched();
            secondSelectedCard.MarkAsMatched();
            matchesFound++;

            if (matchesFound >= TotalPairs)
            {
                EndGame();
            }
            else
            {
                // Auto-advance selection pointer if the card the cursor was on just became matched
                if (instantiatedCards[currentIndex].IsMatched)
                {
                    AutoMoveToNearestValidCard();
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(misMatchDelay);
            firstSelectedCard.FlipCard(false);
            secondSelectedCard.FlipCard(false);
        }

        firstSelectedCard = null;
        secondSelectedCard = null;
        isProcessingMatch = false;
    }
     

    private void AutoMoveToNearestValidCard()
    {
        // Scan the entire board linearly for the closest remaining active card asset
        for (int i = 0; i < instantiatedCards.Length; i++)
        {
            if (!instantiatedCards[i].IsMatched)
            {
                currentIndex = i;
                UpdateSelectionVisuals();
                return;
            }
        }
    }
    private void EndGame()
    {
        isGameActive = false;

        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }

        StartCoroutine(DelayPopUp());
        

        if (finalScoreText != null)
        {
            int score = 40 - totalTaps;
            finalScoreText.text = $"{score}";
        }


    }

    IEnumerator DelayPopUp() 
    {
        yield return new WaitForSeconds(1.5f);
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    private void ExitMiniGame()
    {
        SceneManager.LoadScene("halloweenfestival");
    }
   

    private void OnDestroy()
    {
        selectAction.performed -= OnSelectPerformed;
        navigateAction.Disable();
        selectAction.Disable();
    }
}
