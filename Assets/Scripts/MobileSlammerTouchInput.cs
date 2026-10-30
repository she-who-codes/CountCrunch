using UnityEngine;
using static PogGameManager;

public class MobileSlammerTouchInput : MonoBehaviour
{
    [Header("Input Sensitivity")]
    [Tooltip("Maximum pixel distance for a full-power swipe vector.")]
    [SerializeField] private float maxSwipeDragDistance = 250f;
    [Tooltip("Global force multiplier scaling touch input into physics units.")]
    [SerializeField] private float launchForceMultiplier = 20f;

    [Header("Visual Indicators (Optional)")]
    [SerializeField] private LineRenderer aimingLineIndicator;

    private PogGameManager gameManager;
    private SlammerEntity controlledPlayerSlammer;

    private Vector2 touchStartPos;
    private Vector2 currentTouchPos;
    private bool isInputActive = false;
    private bool isDragging = false;

    private void Awake()
    {
        if (aimingLineIndicator != null)
        {
            aimingLineIndicator.positionCount = 2;
            aimingLineIndicator.enabled = false;
        }
    }

    /// <summary>
    /// Injected by the GameManager when the Player Turn starts.
    /// </summary>
    public void ConfigurePlayerInput(PogGameManager manager, SlammerEntity spawnedSlammer)
    {
        gameManager = manager;
        controlledPlayerSlammer = spawnedSlammer;
    }

    /// <summary>
    /// Opens or closes the interactivity window based on active Turn states.
    /// </summary>
    public void SetInputLockout(bool isLocked)
    {
        isInputActive = !isLocked;
        if (isLocked)
        {
            CancelCurrentDrag();
        }
    }

    private void Update()
    {
        if (!isInputActive || controlledPlayerSlammer == null) return;

        HandleMobileGestures();
    }

    private void HandleMobileGestures()
    {
        // Stand-in mapping tracking both mouse drags and single-finger mobile touch layout inputs natively
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            isDragging = true;

            if (aimingLineIndicator != null) aimingLineIndicator.enabled = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            currentTouchPos = Input.mousePosition;
            UpdateAimingVisuals();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            ExecuteSwipeLaunch();
        }
    }

    private void UpdateAimingVisuals()
    {
        if (aimingLineIndicator == null) return;

        Vector2 dragDelta = touchStartPos - currentTouchPos;
        float dragPercentage = Mathf.Clamp01(dragDelta.magnitude / maxSwipeDragDistance);

        Vector2 aimDirection = dragDelta.normalized;
        Vector3 slammerWorldPos = controlledPlayerSlammer.transform.position;

        // Draw a line out from the slammer showing the projection path vector
        aimingLineIndicator.SetPosition(0, slammerWorldPos);
        aimingLineIndicator.SetPosition(1, slammerWorldPos + (Vector3)(aimDirection * (dragPercentage * 2.5f)));
    }

    private void ExecuteSwipeLaunch()
    {
        isDragging = false;
        if (aimingLineIndicator != null) aimingLineIndicator.enabled = false;

        Vector2 dragDelta = touchStartPos - currentTouchPos;
        float dragDist = dragDelta.magnitude;

        // Guard clause: Prevent accidental micro-clicks from triggering a weak throw
        if (dragDist < 20f) return;

        float normalizedPower = Mathf.Clamp01(dragDist / maxSwipeDragDistance);
        float absoluteStrikeForce = normalizedPower * launchForceMultiplier;
        Vector2 finalSwipeVector = dragDelta.normalized;

        //Debug.Log($"🎯 Player UI Input: Swipe launched with {normalizedPower * 100f}% raw power baseline.");

        // Lock out the input immediately so they cannot pull back again mid-flight
        SetInputLockout(true);

        // Deliver the slammer down toward the stack
        controlledPlayerSlammer.ThrowSlammer(finalSwipeVector, absoluteStrikeForce);

        // Tell the GameManager that the pieces are flying to spin up the physics rest tracking loop

        if (gameManager != null)
        {
            gameManager.UpdateState(GameState.Slamming);
        }
    }

    private void CancelCurrentDrag()
    {
        isDragging = false;
        if (aimingLineIndicator != null) aimingLineIndicator.enabled = false;
    }
}
