using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CatapultController : MonoBehaviour
{
    [Header("Controls & Input")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Aim Math Settings (Invisible)")]
    [SerializeField] private float aimRotateSpeed = 45f;
    [SerializeField] private float minAngle = -32f;
    [SerializeField] private float maxAngle = 20f;

    [Header("Physics Launch Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float launchForce = 15f;
    [SerializeField] private float launchAngleOffset = 65f;

    [Header("Visual Arm Animation")]
    [SerializeField] private Transform catapultArm;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float visualLaunchAngle = -90f;    // Set to -90 to match your art hierarchy
    [SerializeField] private float armRotateToTargetSpeed = 700f;
    [SerializeField] private float armResetSpeed = 180f;

    [Header("Trajectory Visuals")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private int dotCount = 15;
    [SerializeField] private float dotSpacing = 0.1f;
    [SerializeField] private float dotMinScale = 0.2f;
    [SerializeField] private float dotMaxScale = 0.8f;

    [Header("Straight Arrow Visual Guide")]
    [SerializeField] private GameObject arrowIndicatorObject;
    [SerializeField] private float arrowVisualLength = 3f; // How long the arrow stretches into the scene


    private float currentAimAngle = 0f;
    private Vector2 aimInput;
    private InputAction aimAction;
    private InputAction launchAction;
    private GameObject[] trajectoryDots;
    private float projectileMass;
    private bool isLaunching = false;
    private float visualRestAngle;
    private Vector3 staticLaunchPosition; // Caches the safe rest position for flawless spawning

    private void Awake()
    {
        var actionMap = playerInput.actions.FindActionMap("CatapultGame");
        aimAction = actionMap.FindAction("Aim");
        launchAction = actionMap.FindAction("Launch");

        Rigidbody2D projRb = projectilePrefab.GetComponent<Rigidbody2D>();
        projectileMass = (projRb != null) ? projRb.mass : 1f;

        // Cache the default rest angle from your starting scene layout
        visualRestAngle = catapultArm.localRotation.eulerAngles.z;
        if (visualRestAngle > 180f) visualRestAngle -= 360f;

        // CRITICAL: Cache the launch point position while the arm is at rest.
        // This ensures the object spawns exactly where the trajectory dots start.
        staticLaunchPosition = launchPoint.position;

        //PrepareTrajectoryDots();
    }

    private void OnEnable()
    {
        aimAction.Enable();
        launchAction.Enable();
        launchAction.performed += OnLaunchPerformed;
    }

    private void OnDisable()
    {
        DisableControls();
    }

    public void DisableControls()
    {
        aimAction.Disable();
        launchAction.Disable();
        launchAction.performed -= OnLaunchPerformed;
        ToggleTrajectoryVisibility(false);
    }

    private void Update()
    {
        if (!isLaunching)
        {
            ProcessAimInput();
            UpdateTrajectoryPath();
        }
    }

    private void ProcessAimInput()
    {
        aimInput = aimAction.ReadValue<Vector2>();

        if (aimInput.y != 0)
        {
            currentAimAngle += aimInput.y * aimRotateSpeed * Time.deltaTime;
            currentAimAngle = Mathf.Clamp(currentAimAngle, minAngle, maxAngle);
        }
    }

    private void OnLaunchPerformed(InputAction.CallbackContext context)
    {
        if (isLaunching) return;
        StartCoroutine(AnimateAndLaunchSequence());
    }

    private IEnumerator AnimateAndLaunchSequence()
    {
        isLaunching = true;
        ToggleTrajectoryVisibility(false);

        // 1. Force the arm to always rotate forward to your required visual mark (-90)
        while (Mathf.Abs(Mathf.DeltaAngle(catapultArm.localRotation.eulerAngles.z, visualLaunchAngle)) > 1f)
        {
            float step = armRotateToTargetSpeed * Time.deltaTime;
            float newAngle = Mathf.MoveTowardsAngle(catapultArm.localRotation.eulerAngles.z, visualLaunchAngle, step);
            catapultArm.localRotation = Quaternion.Euler(0, 0, newAngle);
            yield return null;
        }

        catapultArm.localRotation = Quaternion.Euler(0, 0, visualLaunchAngle);

        // 2. Launch the object using our cached stationary position and clean math vectors
        LaunchObject();

        // 3. Brief visual frame extension freeze
        yield return new WaitForSeconds(0.12f);

        // 4. Return the catapult arm smoothly back to its rest posture
        while (Mathf.Abs(Mathf.DeltaAngle(catapultArm.localRotation.eulerAngles.z, visualRestAngle)) > 1f)
        {
            float step = armResetSpeed * Time.deltaTime;
            float newAngle = Mathf.MoveTowardsAngle(catapultArm.localRotation.eulerAngles.z, visualRestAngle, step);
            catapultArm.localRotation = Quaternion.Euler(0, 0, newAngle);
            yield return null;
        }

        catapultArm.localRotation = Quaternion.Euler(0, 0, visualRestAngle);

        if (CatapultGameManager.Instance != null)
        {
            isLaunching = false;
            ToggleTrajectoryVisibility(true);
        }
    }

    private Vector2 GetModifiedLaunchVector()
    {
        float finalPhysicsAngle = currentAimAngle + launchAngleOffset;
        float radians = finalPhysicsAngle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    private void LaunchObject()
    {
        GameObject projectile = Instantiate(projectilePrefab, launchPoint.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // Apply the trajectory force directly using pristine math coordinates
            Vector2 directionalForce = GetModifiedLaunchVector() * launchForce;
            rb.AddForce(directionalForce, ForceMode2D.Impulse);
        }

        CatapultGameManager.Instance.UseAmmo();
    }

   

    
    private void ToggleTrajectoryVisibility(bool visible)
    {
        if (arrowIndicatorObject != null)
        {
            arrowIndicatorObject.SetActive(visible);
        }
    }

    private void UpdateTrajectoryPath()
    {
        if (arrowIndicatorObject == null) return;

        // Matches the exact physics launch angle calculation flawlessly
        float finalVisualAngle = currentAimAngle + launchAngleOffset;
        arrowIndicatorObject.transform.localRotation = Quaternion.Euler(0f, 0f, finalVisualAngle);
    }
    /*
     private void PrepareTrajectoryDots()
    {
        trajectoryDots = new GameObject[dotCount];
        for (int i = 0; i < dotCount; i++)
        {
            trajectoryDots[i] = Instantiate(dotPrefab, transform);
            float t = (float)i / (dotCount - 1);
            float scaleFactor = Mathf.Lerp(dotMaxScale, dotMinScale, t);
            trajectoryDots[i].transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }
        ToggleTrajectoryVisibility(true);
    }
    private void ToggleTrajectoryVisibility(bool visible)
    {
        if (trajectoryDots == null) return;
        foreach (var dot in trajectoryDots)
        {
            if (dot != null) dot.SetActive(visible);
        }
    }
    private void UpdateTrajectoryPath()
    {
        // Fix: Force dots to always project from the safe static launch position
        Vector2 startingPosition = staticLaunchPosition;
        Vector2 initialVelocity = (GetModifiedLaunchVector() * launchForce) / projectileMass;
        Vector2 gravityEffect = Physics2D.gravity;

        for (int i = 0; i < dotCount; i++)
        {
            float timeStep = i * dotSpacing;
            float xPos = startingPosition.x + (initialVelocity.x * timeStep) + (0.5f * gravityEffect.x * timeStep * timeStep);
            float yPos = startingPosition.y + (initialVelocity.y * timeStep) + (0.5f * gravityEffect.y * timeStep * timeStep);
            trajectoryDots[i].transform.position = new Vector3(xPos, yPos, 0f);
        }
    }*/
}
