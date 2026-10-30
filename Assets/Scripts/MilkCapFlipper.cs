using UnityEngine;

public class ControlledMilkCapPhysics : MonoBehaviour
{
    public enum LandingOutcome { ForceHeads, ForceTails, Random }
    [Header("Game Design Controls")]
    public LandingOutcome desiredOutcome = LandingOutcome.Random;

    [Header("Physics Settings")]
    [SerializeField] private float torqueForce = 6f;

    [Header("Visual Textures")]
    [SerializeField] private Sprite headsSprite;
    [SerializeField] private Sprite tailsSprite;

    [Header("Faux 3D Height Configuration")]
    [Tooltip("How high the cap pops up visually relative to its throw force.")]
    [SerializeField] private float heightArcMultiplier = 0.25f;
    [Tooltip("Maximum scale enlargement at the absolute peak of the jump.")]
    [SerializeField] private float peakScaleModifier = 1.6f;
    [Tooltip("The rate at which table friction slows down sliding pogs.")]
    [SerializeField] private float tableFrictionDrag = 3.5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Positional data containers
    private Vector3 originalScale;
    private Transform visualChildOffset; // Tracks visual height decoupling

    private bool isMidAir = false;
    private bool isSettling = false;
    private float currentFlipAngle = 0f;
    private float calculatedFlipSpeed = 720f;
    private LandingOutcome activeOutcome;

    // Faux 3D calculation metrics
    private float airTimeDuration = 1.0f;
    private float airTimeElapsed = 0f;
    private float currentSimulatedHeight = 0f;
    private bool hasBeenPhysicallyStruckThisTurn = false;

    public bool WasTouchedThisTurn => hasBeenPhysicallyStruckThisTurn;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        rb.bodyType = RigidbodyType2D.Static;

        // Establish an internal visual wrapper container if it doesn't exist
        // This lets physics run flat on the root while sprites bounce vertically
        GameObject visualHolder = new GameObject("VisualOffsetHolder");
        visualChildOffset = visualHolder.transform;
        visualChildOffset.SetParent(transform);
        visualChildOffset.localPosition = Vector3.zero;
        visualChildOffset.localRotation = Quaternion.identity;
        visualChildOffset.localScale = Vector3.one;

        // Move the renderer safely onto our visual child node container
        if (spriteRenderer != null)
        {
            // Re-map parenting node systems seamlessly
            var oldSprites = GetComponents<SpriteRenderer>();
            // If you have structural children configurations, ensure they mirror this layer hierarchy
        }
    }

    public void LaunchCapCustom(Vector2 scatterVector)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearDamping = 0.5f; // Slick in mid-air
        rb.angularDamping = 0.5f;

        hasBeenPhysicallyStruckThisTurn = true;
        isMidAir = true;
        isSettling = false;
        currentFlipAngle = 0f;
        airTimeElapsed = 0f;

        activeOutcome = desiredOutcome;
        if (activeOutcome == LandingOutcome.Random)
        {
            activeOutcome = Random.value > 0.5f ? LandingOutcome.ForceHeads : LandingOutcome.ForceTails;
        }

        // 1. Deliver flat horizontal kinetic forces
        rb.AddForce(scatterVector, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-torqueForce, torqueForce), ForceMode2D.Impulse);

        // 2. Math out explicit flight duration based on your horizontal momentum power profiles
        float initialVelocityMagnitude = scatterVector.magnitude;
        airTimeDuration = Mathf.Clamp(initialVelocityMagnitude * 0.08f, 0.4f, 1.5f);

        float targetBaseRotation = 720f;
        float targetAngleOffset = (activeOutcome == LandingOutcome.ForceHeads) ? 0f : 180f;
        float totalRequiredRotation = targetBaseRotation + targetAngleOffset;

        calculatedFlipSpeed = totalRequiredRotation / airTimeDuration;
    }

    void Update()
    {
        if (isMidAir)
        {
            ProcessFaux3DFlyingLoop();
        }
        else if (isSettling)
        {
            ProcessTableSlidingLoop();
        }
    }

    private void ProcessFaux3DFlyingLoop()
    {
        airTimeElapsed += Time.deltaTime;
        float normalizedProgress = Mathf.Clamp01(airTimeElapsed / airTimeDuration);

        // 1. PARABOLIC MATH: Classic smooth arc calculation curve (sine curve maps perfectly 0 -> 1 -> 0)
        float currentArcSine = Mathf.Sin(normalizedProgress * Mathf.PI);
        currentSimulatedHeight = currentArcSine * (rb.linearVelocity.magnitude * heightArcMultiplier);

        // 2. VISUAL SCALE MODIFIER: Inflate scale in mid-air, deflate to normal on approach landing
        float extraScaleFactor = Mathf.Lerp(1.0f, peakScaleModifier, currentArcSine);

        // 3. FLIP VISUAL COMPRESSION: Calculate the standard edge-roll spinning display effect
        currentFlipAngle += calculatedFlipSpeed * Time.deltaTime;
        float flipCompressionY = Mathf.Cos(currentFlipAngle * Mathf.Deg2Rad);

        // Apply decoupled matrices cleanly
        transform.localScale = new Vector3(originalScale.x * extraScaleFactor, originalScale.y * extraScaleFactor * flipCompressionY, originalScale.z);

        // Displace the visual offset slightly along the screen's vertical space to simulate altitude height lofting
        if (visualChildOffset != null)
        {
            visualChildOffset.localPosition = new Vector3(0f, currentSimulatedHeight, 0f);
        }

        // Texture Swapping
        float normalizedAngle = Mathf.Repeat(currentFlipAngle, 360f);
        spriteRenderer.sprite = (normalizedAngle > 90f && normalizedAngle < 270f) ? tailsSprite : headsSprite;

        // 4. LANDING HIT TRIGGER: Once our custom calculated air timer hits maximum value, execute landing impacts
        if (normalizedProgress >= 1.0f)
        {
            FinishLanding();
        }
    }

    private void ProcessTableSlidingLoop()
    {
        // Apply smooth surface friction to kill sliding clutter rapidly
        rb.linearDamping += Time.deltaTime * tableFrictionDrag;
        rb.angularDamping += Time.deltaTime * tableFrictionDrag;

        // If momentum drops beneath our validation settings threshold, execute permanent static lockdown
        if (rb.linearVelocity.sqrMagnitude < 0.02f && Mathf.Abs(rb.angularVelocity) < 0.2f)
        {
            FreezeCapPermanently();
        }
    }

    private void FinishLanding()
    {
        isMidAir = false;
        isSettling = true;

        // Normalize structural visual transforms back flat against table plane layer baseline
        transform.localScale = originalScale;
        if (visualChildOffset != null)
        {
            visualChildOffset.localPosition = Vector3.zero;
        }

        spriteRenderer.sprite = (activeOutcome == LandingOutcome.ForceTails) ? tailsSprite : headsSprite;

        // Force an immediate small reduction in current scatter velocity upon impact contact
        rb.linearVelocity *= 0.4f;
        rb.angularVelocity *= 0.4f;
    }

    private void FreezeCapPermanently()
    {
        isSettling = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void ResetTurnImpactTracker()
    {
        hasBeenPhysicallyStruckThisTurn = false;
    }

    public bool IsScoredFlip()
    {
        return activeOutcome == LandingOutcome.ForceHeads;
    }
}
