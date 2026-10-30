/*
 //ClaudeCode 
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MilkCaps.Wager
{
    /// <summary>
    /// uGUI + TextMeshPro view/controller for WagerModel.
    /// Attach to any GameObject (e.g. the wager panel) and assign the references in the Inspector.
    /// </summary>
    public class WagerSelector : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField, Tooltip("Pogs the player can wager. Negative values are treated as 0.")]
        private int availableInventory = 10;

        [Header("UI References")]
        [SerializeField] private TMP_Text inventoryLabel;
        [SerializeField] private TMP_Text wagerLabel;
        [SerializeField] private TMP_Text feedbackLabel; // optional but recommended
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button confirmButton;

        /// <summary>Raised when a valid wager is confirmed.</summary>
        public event Action<int> WagerConfirmed;

        public bool IsConfirmed => _model != null && _model.IsConfirmed;
        public int CurrentWager => _model != null ? _model.Wager : 0;

        /// <summary>The confirmed wager, or 0 if not confirmed yet.</summary>
        public int ConfirmedWager => _model != null ? _model.ConfirmedWager : 0;

        private WagerModel _model;
        private bool _referencesValid;

        private void Awake()
        {
            _referencesValid = ValidateReferences();
            if (!_referencesValid) enabled = false;
        }

        private void OnEnable()
        {
            if (!_referencesValid) return;
            minusButton.onClick.AddListener(OnMinusClicked);
            plusButton.onClick.AddListener(OnPlusClicked);
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void OnDisable()
        {
            if (!_referencesValid) return;
            minusButton.onClick.RemoveListener(OnMinusClicked);
            plusButton.onClick.RemoveListener(OnPlusClicked);
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }

        private void Start()
        {
            if (_model == null) Initialize(availableInventory);
        }

        private void OnDestroy()
        {
            if (_model != null) _model.Confirmed -= HandleModelConfirmed;
        }

        /// <summary>
        /// Starts (or restarts) the selector. Also re-opens it after a confirmation.
        /// Other code may call this when the player enters the arena.
        /// </summary>
        public void Initialize(int inventory)
        {
            if (!_referencesValid) return;

            availableInventory = Math.Max(0, inventory);

            if (_model == null)
            {
                //Original Code: _model = new WagerModel(availableInventory);
                //Fixed Code:
                _model = GetComponent<WagerModel>();
                _model.ResetBet(availableInventory);

                _model.Confirmed += HandleModelConfirmed;
            }
            else
            {
                _model.ResetBet(availableInventory);
            }

            Refresh();
            SetFeedback(_model.CanConfirm ? "Choose your wager." : "No Pogs available to wager.");
        }

        private void OnMinusClicked()
        {
            if (_model == null) return;
            _model.TryDecrease();
            Refresh();
        }

        private void OnPlusClicked()
        {
            if (_model == null) return;
            _model.TryIncrease();
            Refresh();
        }

        private void OnConfirmClicked()
        {
            if (_model == null) return;

            if (_model.IsConfirmed)
            {
                SetFeedback("Wager already confirmed.");
            }
            else if (!_model.TryConfirm())
            {
                SetFeedback(_model.AvailableInventory < 1
                    ? "Confirmation rejected: you have no Pogs."
                    : "Confirmation rejected: wager must be at least 1.");
            }
            // On success, HandleModelConfirmed sets the feedback and raises the event.
            Refresh();
        }

        private void HandleModelConfirmed(int wager)
        {
            SetFeedback($"Wager confirmed: {wager} Pog{(wager == 1 ? "" : "s")}.");
            WagerConfirmed?.Invoke(wager);
        }

        /// <summary>Single place where labels and interactability are synced to the model.</summary>
        private void Refresh()
        {
            inventoryLabel.text = $"Available Pogs: {_model.AvailableInventory}";
            wagerLabel.text = $"Wager: {_model.Wager}";
            minusButton.interactable = _model.CanDecrease;
            plusButton.interactable = _model.CanIncrease;
            confirmButton.interactable = _model.CanConfirm;
        }

        private void SetFeedback(string message)
        {
            if (feedbackLabel != null) feedbackLabel.text = message;
        }

        private bool ValidateReferences()
        {
            bool ok = true;
            ok &= Require(inventoryLabel, nameof(inventoryLabel));
            ok &= Require(wagerLabel, nameof(wagerLabel));
            ok &= Require(minusButton, nameof(minusButton));
            ok &= Require(plusButton, nameof(plusButton));
            ok &= Require(confirmButton, nameof(confirmButton));
            if (feedbackLabel == null)
                Debug.LogWarning($"[{nameof(WagerSelector)}] 'feedbackLabel' is not assigned; feedback text will not be shown.", this);
            return ok;
        }

        private bool Require(UnityEngine.Object reference, string fieldName)
        {
            if (reference != null) return true;
            Debug.LogError($"[{nameof(WagerSelector)}] '{fieldName}' is not assigned. Assign it in the Inspector. Component disabled.", this);
            return false;
        }

        private void OnValidate()
        {
            if (availableInventory < 0) availableInventory = 0;
        }
    }
}*/
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PogGameManager;

public class WagerSelector : MonoBehaviour
{
    [Header("Scene Systems")]
    [SerializeField] private PogGameManager pogGameManager;

    [Header("Inventory Settings")]
    [Tooltip("Initial number of Pogs in the player's inventory.")]
    [SerializeField] private int initialInventory = 10;

    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI inventoryText;
    [SerializeField] private TextMeshProUGUI wagerText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("UI Button References")]
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button confirmButton;

    // Runtime tracking properties
    public int AvailableInventory { get; private set; }
    public int CurrentWager { get; private set; }
    public bool IsConfirmed { get; private set; }

    // Public C# event to let other systems listen for successful wagers
    public static event Action<int> OnWagerConfirmed;

    private void Awake()
    {
        ValidateReferences();
    }

    private void Start()
    {
        InitializeSelection();
        SetupButtonListeners();
    }

    /// <summary>
    /// Checks that all required Inspector references are assigned.
    /// </summary>
    private void ValidateReferences()
    {
        if (inventoryText == null) Debug.LogError("WagerSelector: Inventory Text reference is missing!", this);
        if (wagerText == null) Debug.LogError("WagerSelector: Wager Text reference is missing!", this);
        if (feedbackText == null) Debug.LogError("WagerSelector: Feedback Text reference is missing!", this);
        if (minusButton == null) Debug.LogError("WagerSelector: Minus Button reference is missing!", this);
        if (plusButton == null) Debug.LogError("WagerSelector: Plus Button reference is missing!", this);
        if (confirmButton == null) Debug.LogError("WagerSelector: Confirm Button reference is missing!", this);
    }

    /// <summary>
    /// Configures the base state based on initial inventory.
    /// </summary>
    private void InitializeSelection()
    {
        IsConfirmed = false;

        // Requirement 1: Clamp negative inventory values to zero
        AvailableInventory = Mathf.Max(0, initialInventory);

        // Requirement 2: Handle wager initial state boundaries
        if (AvailableInventory >= 1)
        {
            CurrentWager = 1;
        }
        else
        {
            CurrentWager = 0;
        }

        UpdateUI();
    }

    private void SetupButtonListeners()
    {
        // Explicitly clear listeners first to safeguard against double invocation
        minusButton.onClick.RemoveAllListeners();
        plusButton.onClick.RemoveAllListeners();
        confirmButton.onClick.RemoveAllListeners();

        minusButton.onClick.AddListener(DecreaseWager);
        plusButton.onClick.AddListener(IncreaseWager);
        confirmButton.onClick.AddListener(ConfirmWager);
    }

    /// <summary>
    /// Increments the wager safely within the dynamic bounds.
    /// </summary>
    public void IncreaseWager()
    {
        if (IsConfirmed || CurrentWager >= AvailableInventory) return;

        CurrentWager++;
        UpdateUI();
    }

    /// <summary>
    /// Decrements the wager safely within the dynamic bounds.
    /// </summary>
    public void DecreaseWager()
    {
        if (IsConfirmed || CurrentWager <= 0) return;

        CurrentWager--;
        UpdateUI();
    }

    /// <summary>
    /// Synchronizes internal math with the UI labels and button interactability states.
    /// </summary>
    private void UpdateUI()
    {
        // Update display text elements
        if (inventoryText != null) inventoryText.text = $"Inventory: {AvailableInventory}";
        if (wagerText != null) wagerText.text = $"{CurrentWager}";

        if (IsConfirmed)
        {
            // Block all input actions post-confirmation
            minusButton.interactable = false;
            plusButton.interactable = false;
            confirmButton.interactable = false;
            return;
        }

        // Requirement 4: Set button interactability explicitly based on dynamic state
        minusButton.interactable = (CurrentWager > 0);
        plusButton.interactable = (CurrentWager < AvailableInventory);

        // Requirement 2 & 4: Confirm button requires a wager of at least 1 and within inventory limits
        confirmButton.interactable = (CurrentWager >= 1 && CurrentWager <= AvailableInventory);
    }

    /// <summary>
    /// Finalizes the wager validation workflow and broadcasts state.
    /// </summary>
    public void ConfirmWager()
    {
        // Fail-safe duplicate click check
        if (IsConfirmed) return;

        // Requirement 5: Secondary dynamic check to validate values explicitly
        if (CurrentWager < 1 || CurrentWager > AvailableInventory)
        {
            if (feedbackText != null)
            {
                feedbackText.text = "<color=red>Invalid wager selected!</color>";
            }
            return;
        }

        // Lock state to satisfy Requirement 10 (No repeated confirmations)
        IsConfirmed = true;
        UpdateUI();

        if (feedbackText != null)
        {
            feedbackText.text = $"<color=green>Wager of {CurrentWager} Pogs Confirmed!</color>";
        }

        // Requirement 5: Expose data to decoupled subscribers
        OnWagerConfirmed?.Invoke(CurrentWager);

        pogGameManager.UpdateState(GameState.Init);
        gameObject.SetActive(false);
        Debug.Log($"WagerSelector Event: Successfully confirmed wager of {CurrentWager} Pogs.");
    }
}
