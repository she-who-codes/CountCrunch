using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PogGameManager : MonoBehaviour
{
    // The core states governing our turn-based Pogs match
    public enum GameState
    {
        Init,
        PlayerTurn,
        NPCTurn,
        Slamming,
        Evaluation,
        GameOver
    }

    [Header("Input Framework Hook")]
    [SerializeField] private MobileSlammerTouchInput playerTouchInput;

    [Header("Current Match State")]
    [SerializeField] private GameState currentState;

    [Header("Scene Systems")]
    [SerializeField] private MilkCapStackManager stackManager;
   

    // The single, optimized source-of-truth cache list for all active game pieces
    private System.Collections.Generic.List<ControlledMilkCapPhysics> activeCapsOnTable = new();
    public int RemainingPogs => activeCapsOnTable.Count;


    [Header("AI Hook")]
    [SerializeField] private NPCController npcController;

    [Header("Match Analytics")]
    [SerializeField] private int playerScore = 0;
    [SerializeField] private int npcScore = 0;

    [Header("Restack Configuration")]
    [SerializeField] private Vector2 stackCenterPosition = Vector2.zero;
    [Tooltip("Small physical offset spacing per pog in the stack to prevent clipping errors.")]
    [SerializeField] private float verticalStackOffset = 0.05f;

    [Header("Slammer Spawning Configurations")]
    [Tooltip("The base physical Slammer Prefab container containing Rigidbody2D, Collider2D, and SlammerEntity components.")]
    [SerializeField] private GameObject slammerPrefab;

    [Space]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform npcSpawnPoint;

    [Header("Active Equipment Data Profiles")]
    [SerializeField] private SlammerData activePlayerSlammerData;
    [SerializeField] private SlammerData activeNpcSlammerData;

    [Header("Sprint 8: Game Over UI Panels")]
    [Tooltip("The UI Panel GameObject containing your 'Victory/Defeat' text and 'Play Again' button.")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverWinnerText; 


    // Runtime tracked instances
    private SlammerEntity runtimePlayerSlammer;
    private SlammerEntity runtimeNpcSlammer;

    [SerializeField] private int totalPogsInStack = 10; // Setup placeholder
    //private int remainingPogs;

    // Actions allow UI managers or other scripts to listen to state changes cleanly
    public static event Action<GameState> OnStateChanged;
    public static event Action<int, int> OnScoresUpdated;

    private Coroutine physicsTrackingCoroutine;
    private Coroutine turnTransitionCoroutine;

    [Header("Current Match State")] 
    public GameState CurrentState => currentState;
   

    // NEW: Streamlined header tracking property
    private GameState activeThrower;
    public GameState ActiveThrower => activeThrower;

    private void Start()
    {
       //Don't Intiate until Wager is made
        //UpdateState(GameState.Init);
    }

    /// <summary>
    /// Core function to handle all state logic transitions safely
    /// </summary>
    public void UpdateState(GameState newState)
    {
        if (physicsTrackingCoroutine != null)
        {
            StopCoroutine(physicsTrackingCoroutine);
            physicsTrackingCoroutine = null;
        }
        if (turnTransitionCoroutine != null)
        {
            StopCoroutine(turnTransitionCoroutine);
            turnTransitionCoroutine = null;
        }
        currentState = newState;
        Debug.Log("----------------------------------> " + newState);
        switch (newState)
        {
            case GameState.Init:
                HandleInit();
                break;
            case GameState.PlayerTurn:
                HandlePlayerTurn();
                break;
            case GameState.NPCTurn:
                HandleNPCTurn();
                break;
            case GameState.Slamming:
                HandleSlamming();
                break;
            case GameState.Evaluation:
                HandleEvaluation();
                break;
            case GameState.GameOver:
                HandleGameOver();
                break;
        }

        // Notify any listening UI or audio elements
        OnStateChanged?.Invoke(newState);
    }
    

    /// <summary>
    /// Populates initial list cache directly from the instantiated stack manager objects.
    /// </summary>
    public void InitializeActiveTableCaps(System.Collections.Generic.List<ControlledMilkCapPhysics> initialStack)
    {
        activeCapsOnTable.Clear();
        activeCapsOnTable.AddRange(initialStack);
    }

    /// <summary>
    /// Handles the initialization state, building the stack and launching the match loop.
    /// </summary>
    private void HandleInit()
    {
        

        //Debug.Log("🎲 Game Manager: Initializing match elements...");
        playerScore = 0;
        npcScore = 0;
        OnScoresUpdated?.Invoke(playerScore, npcScore);

        // 1. Clear out any old runtime slammers if restarting the scene/match
        if (runtimePlayerSlammer != null) Destroy(runtimePlayerSlammer.gameObject);
        if (runtimeNpcSlammer != null) Destroy(runtimeNpcSlammer.gameObject);

        // 2. DYNAMICALLY SPAWN SLAMMERS
        if (slammerPrefab != null)
        {
            // Spawn Player Slammer
            GameObject pSlamObj = Instantiate(slammerPrefab, playerSpawnPoint.position, Quaternion.identity);
            pSlamObj.name = "Player_Slammer_Runtime";
            runtimePlayerSlammer = pSlamObj.GetComponent<SlammerEntity>();

            // Inject the player's active progression gear data profile
            runtimePlayerSlammer.SetInitialSpawnPoint(playerSpawnPoint.position);
            runtimePlayerSlammer.ApplyCustomSlammerData(activePlayerSlammerData);

            // Spawn NPC Slammer
            GameObject nSlamObj = Instantiate(slammerPrefab, npcSpawnPoint.position, Quaternion.identity);
            nSlamObj.name = "NPC_Slammer_Runtime";
            runtimeNpcSlammer = nSlamObj.GetComponent<SlammerEntity>();

            // Inject the NPC's gear data profile
            runtimeNpcSlammer.SetInitialSpawnPoint(npcSpawnPoint.position);
            runtimeNpcSlammer.ApplyCustomSlammerData(activeNpcSlammerData);
        }
        else
        {
            Debug.LogError("🚨 PogGameManager: Slammer Prefab is completely missing from the inspector fields!");
        }

        // 3. AUTOMATIC DEPENDENCY INJECTION INTO NPC SCRIPT
        if (npcController != null)
        {
            // Seamlessly populate all pertinent fields on the NPC without manual scene assignment
            npcController.ConfigureNPCReferences(this, runtimeNpcSlammer);
        }
      

        // ADDED FOR SPRINT 6 PLAYER LOOP:
        if (playerTouchInput != null)
        {
            playerTouchInput.ConfigurePlayerInput(this, runtimePlayerSlammer);
            playerTouchInput.SetInputLockout(true); // Keep frozen during Init countdown
        }
        // 4. Build Milkcap Column
        if (stackManager != null)
        {
            List<ControlledMilkCapPhysics> newlyBuiltStack = stackManager.BuildInitialStack();
            InitializeActiveTableCaps(newlyBuiltStack);
        }

        // 5. Coin Toss
        GameState startingPlayer = (UnityEngine.Random.value > 0.5f) ? GameState.PlayerTurn : GameState.NPCTurn;
        StartCoroutine(DelayedStateTransition(startingPlayer, 1.5f));
    }

    private void HandlePlayerTurn()
    {
        activeThrower = GameState.PlayerTurn;
       // Debug.Log("🎯 Game Manager: Player's Turn. Opening Touch Swipe Interactivity Canvas layer.");

        if (playerTouchInput != null)
        {
            playerTouchInput.SetInputLockout(false); // OPEN CONTROL VALVE
        }
        
    }

    private void HandleNPCTurn()
    {
        activeThrower = GameState.NPCTurn;

        //Debug.Log("🤖 Game Manager: NPC's Turn. Passing cached target stack to AI Controller.");
        if (playerTouchInput != null)
        {
            playerTouchInput.SetInputLockout(true); // CLOSE CONTROL VALVE
        }

       
        // 1. Safety Guard: Check if the AI controller is hooked up in the inspector fields
        if (npcController != null)
        {
            // 2. Clear out any dead null references from previous loops to protect the AI's math
            for (int i = activeCapsOnTable.Count - 1; i >= 0; i--)
            {
                if (activeCapsOnTable[i] == null)
                {
                    activeCapsOnTable.RemoveAt(i);
                }
            }

            // 3. Check if there are actually any targets left to pass
            if (activeCapsOnTable.Count > 0)
            {
                // Pass our direct, optimized list reference straight to the NPC script
                npcController.ExecuteAITurn(activeCapsOnTable);
            }
            else
            {
                // Fallback win condition route if evaluation somehow missed a zero count
                UpdateState(GameState.GameOver);
            }
        }
        else
        {
            Debug.LogError("🚨 PogGameManager: NPCController reference is missing in the inspector! Automatically forfeiting to Player turn.");
            // Graceful mobile recovery fallback to keep the game from locking up if you forget to bind it
            UpdateState(GameState.PlayerTurn);
        }
    }
    private void HandleSlamming()
    {
        //Debug.Log("💥 Game Manager: Slammer deployed!");
        if (playerTouchInput != null) playerTouchInput.SetInputLockout(true);

        // No more inline ternary operator required here!
        physicsTrackingCoroutine = StartCoroutine(WaitForPhysicsToStop(activeThrower));
        
    }
    private void HandleEvaluation()
    {
        //Debug.Log($"🔍 Game Manager: Evaluating board state for active thrower: {activeThrower}");

        int flippedThisTurnCount = 0;

        for (int i = activeCapsOnTable.Count - 1; i >= 0; i--)
        {
            ControlledMilkCapPhysics cap = activeCapsOnTable[i];
            if (cap == null)
            {
                activeCapsOnTable.RemoveAt(i);
                continue;
            }

            if (!cap.WasTouchedThisTurn) continue;

            if (cap.IsScoredFlip())
            {
                flippedThisTurnCount++;
                activeCapsOnTable.RemoveAt(i);
                Destroy(cap.gameObject);
            }
        }

        // Award scores cleanly using our optimized header tracking parameter
        if (flippedThisTurnCount > 0)
        {
            if (activeThrower == GameState.PlayerTurn) playerScore += flippedThisTurnCount;
            else npcScore += flippedThisTurnCount;

            OnScoresUpdated?.Invoke(playerScore, npcScore);
        }

        // Check Win Condition or toggle strict turn passing
        if (activeCapsOnTable.Count <= 0)
        {
            Debug.Log("🔍 Evaluation: No milkcaps remain. Preparing Game Over sequence.");

            // Route to Game Over using our clean, paced transition system
            turnTransitionCoroutine = StartCoroutine(DelayedTurnTransition(0.8f, true));
        }
        else
        {
            // Clean up the table layouts and reset positions
            RestackRemainingPogs();

            // Route to Next Turn using our clean, paced transition system
            turnTransitionCoroutine = StartCoroutine(DelayedTurnTransition(0.6f, false));
        }
    }

    /// <summary>
    /// Paces transitions out of Evaluation to allow UI animations and text to render.
    /// </summary>
    private IEnumerator DelayedTurnTransition(float displayDelay, bool isMatchOver)
    {
        // Let the "Counting Flipped Pogs" text or score animations play out legibly
        yield return new WaitForSeconds(displayDelay);

        if (isMatchOver)
        {
            Debug.Log("🏆 Transition: Transitioning cleanly to Game Over state.");
            UpdateState(GameState.GameOver);
        }
        else
        {
            // Execute standard alternate turn toggle logic
            GameState nextTurn = (activeThrower == GameState.PlayerTurn) ? GameState.NPCTurn : GameState.PlayerTurn;

            Debug.Log($"🔄 Transition: Shifting turn state forward to {nextTurn}");
            UpdateState(nextTurn);
        }

        // Clear our tracking pointer safely since execution is complete
        turnTransitionCoroutine = null;
    }

    /*private void RestackRemainingPogs()
    {
        for (int i = 0; i < activeCapsOnTable.Count; i++)
        {
            if (activeCapsOnTable[i] != null)
            {
                // Reset the tracker so it must be physically struck again next turn to count
                activeCapsOnTable[i].ResetTurnImpactTracker();
            }
        }
        
        if (stackManager != null) stackManager.RestackRemainingPogs(activeCapsOnTable);
        
        // Reset our dynamically instantiated slammers cleanly
        if (runtimePlayerSlammer != null) runtimePlayerSlammer.ResetPosition();
        if (runtimeNpcSlammer != null) runtimeNpcSlammer.ResetPosition();
    }*/
    private void RestackRemainingPogs()
    {
        if (stackManager != null)
        {
            stackManager.RestackRemainingPogs(activeCapsOnTable);
        }

        // Clear tracking lists on the spawned slammers
        if (runtimePlayerSlammer != null)
        {
            runtimePlayerSlammer.ResetPosition();
            runtimePlayerSlammer.ResetSlammerHitGuard(); // CLEAR HIT GUARD
        }

        if (runtimeNpcSlammer != null)
        {
            runtimeNpcSlammer.ResetPosition();
            runtimeNpcSlammer.ResetSlammerHitGuard(); // CLEAR HIT GUARD
        }
    }
    private void HandleGameOver()
    {
        Debug.Log("🏆 Game Manager: Game Over State Activated.");

        // 1. Brick any human inputs immediately so they can't swipe behind the panel
        if (playerTouchInput != null)
        {
            playerTouchInput.SetInputLockout(true);
        }

        // 2. Determine the victor based on final score tracking metrics
        if (gameOverWinnerText != null)
        {
            if (playerScore > npcScore)
            {
                gameOverWinnerText.text = $"🏆 VICTORY!\nYou collected {playerScore} Pogs!";
                gameOverWinnerText.color = Color.green;
            }
            else if (npcScore > playerScore)
            {
                gameOverWinnerText.text = $"🤖 DEFEAT...\nNPC won with {npcScore} Pogs.";
                gameOverWinnerText.color = Color.red;
            }
            else
            {
                gameOverWinnerText.text = $"🤝 IT'S A TIE!\nBoth scored {playerScore} Pogs.";
                gameOverWinnerText.color = Color.yellow;
            }
        }

        // 3. Pop the UI display overlay screen into the viewport
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Public UI Hook function mapped directly to your 'Play Again' Canvas Button.
    /// </summary>
    public void RestartMatchLoop()
    {
        Debug.Log("🔄 UI Action: Resetting match arena for a fresh game.");

        // 1. Collapse the victory display layer panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // 2. Wipe clean the playing field table using your optimized manager asset
        if (stackManager != null)
        {
            stackManager.ClearTable();
        }

        // 3. Route back to Init state to recreate the stack and roll a fresh coin toss
        UpdateState(GameState.Init);
    }

    // Helper coroutine for basic visual delays or mock turns during testing
    private IEnumerator DelayedStateTransition(GameState targetState, float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateState(targetState);
    }


    [Header("Physics Settings")]
    [Tooltip("The lower this is, the more completely stopped the pogs must be.")]
    [SerializeField] private float velocityThreshold = 0.05f;
    [Tooltip("How many seconds the board must remain perfectly still before evaluating.")]
    [SerializeField] private float stabilizationDuration = 0.4f;

    /*private IEnumerator WaitForPhysicsToStop(GameState throwerState)
    {
        // 1. Gather all Rigidbodies on the board dynamically 
        // (Assumes your Pogs and Slammer have a Rigidbody2D component attached)
        Rigidbody2D[] allBodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);

        float stableTimer = 0f;
        bool allStopped = false;

        // Small initial safety buffer to let physics objects actually start moving/flying
        yield return new WaitForSeconds(0.2f);

        while (!allStopped)
        {
            bool dynamicMovementDetected = false;

            foreach (Rigidbody2D body in allBodies)
            {
                if (body == null) continue;

                // sqrMagnitude is significantly cheaper on mobile processors than .magnitude
                float linearVelocitySE = body.linearVelocity.sqrMagnitude;
                float angularVelocitySE = body.angularVelocity * body.angularVelocity;

                // Check if anything is still sliding or spinning rapidly
                if (linearVelocitySE > (velocityThreshold * velocityThreshold) || angularVelocitySE > (velocityThreshold * velocityThreshold))
                {
                    dynamicMovementDetected = true;
                    break; // No need to check the rest this frame, something is still moving
                }
            }

            if (!dynamicMovementDetected)
            {
                // Everything looks still, increment our stabilization window timer
                stableTimer += Time.deltaTime;
                if (stableTimer >= stabilizationDuration)
                {
                    allStopped = true;
                }
            }
            else
            {
                // Reset timer if even one pog wobbles or shifts position
                stableTimer = 0f;
            }

            yield return null; // Wait for the next physics frame updates
        }

        Debug.Log("💤 Game Manager: Board settled. Transitioning to Evaluation.");

        // Pass control over to the evaluation state
        UpdateState(GameState.Evaluation);
    }*/
    private IEnumerator WaitForPhysicsToStop(GameState throwerState)
    {
        Debug.Log("WaitForPhysicsToStop");
        Rigidbody2D[] allBodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);

        float stableTimer = 0f;
        bool allStopped = false;
        float absoluteTimeoutTimer = 0f;

        // CRITICAL BUFFER: Wait half a second so forces can propagate 
        // and send things sliding before checking if the board is still!
        yield return new WaitForSeconds(0.5f);

        while (!allStopped)
        {
            bool dynamicMovementDetected = false;
            absoluteTimeoutTimer += Time.deltaTime;

            if (absoluteTimeoutTimer > 9.0f)
            {
                Debug.LogWarning("⚠️ Timeout: Physics took too long, forcing execution.");
                break;
            }

            foreach (Rigidbody2D body in allBodies)
            {
                if (body == null) continue;
                if (body.bodyType == RigidbodyType2D.Static) continue;

                float linearVelocitySE = body.linearVelocity.sqrMagnitude;
                float angularVelocitySE = body.angularVelocity * body.angularVelocity;

                if (linearVelocitySE > (velocityThreshold * velocityThreshold) || angularVelocitySE > (velocityThreshold * velocityThreshold))
                {
                    dynamicMovementDetected = true;
                    break;
                }
            }

            if (!dynamicMovementDetected)
            {
                stableTimer += Time.deltaTime;
                if (stableTimer >= stabilizationDuration)
                {
                    allStopped = true;
                }
            }
            else
            {
                stableTimer = 0f;
            }

            yield return null;
        }
        Debug.Log("eval");
        UpdateState(GameState.Evaluation);
    }



}
