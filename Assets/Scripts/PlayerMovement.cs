using UnityEngine;
using UnityEngine.InputSystem;
using Spine.Unity; // Required for Spine SkeletonAnimation

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float acceleration = 50f;

    [Header("Jump Physics")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float doubleJumpForce = 10f;
    [SerializeField] private float gravityScale = 3f;
    [SerializeField] private float fallMultiplier = 1.5f;

    [Header("Ground Pound")]
    [SerializeField] private float groundPoundForce = 25f;

    [Header("Combat")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Spine Animation Settings")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    [Header("Spine Animation Names")]
    [SpineAnimation(dataField: "skeletonAnimation")][SerializeField] private string idleAnim = "rest";
    [SpineAnimation(dataField: "skeletonAnimation")][SerializeField] private string walkAnim = "walk";
    [SpineAnimation(dataField: "skeletonAnimation")][SerializeField] private string runAnim = "run";
    [SpineAnimation(dataField: "skeletonAnimation")][SerializeField] private string jumpAnim = "Jump2";
    [SpineAnimation(dataField: "skeletonAnimation")][SerializeField] private string fallAnim = "jump_hold";
    [SpineAnimation(dataField: "skeletonAnimation")][SerializeField] private string shootAnim = "shoot2";
    [SpineAnimation(dataField: "skeletonAnimation")][SerializeField] private string groundPoundAnim = "pound";

    // Internal State
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool canDoubleJump;
    private bool isRunning;
    private bool isGroundPounding;
    private Vector3 initialScale;
    private string currentAnimation = "";

    // Generated Input Action Class
    private PlayerControls controls;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new PlayerControls();
        rb.gravityScale = gravityScale;
        initialScale = transform.localScale;

        // Auto-grab SkeletonAnimation if not assigned manually in inspector
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
        }
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.Jump.started += OnJumpPressed;
        controls.Player.Run.started += _ => isRunning = true;
        controls.Player.Run.canceled += _ => isRunning = false;
        controls.Player.Shoot.started += OnShootPressed;
        controls.Player.GroundPound.started += OnGroundPoundPressed;
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        moveInput = controls.Player.Move.ReadValue<Vector2>();
        CheckGrounded();

        if (moveInput.x != 0 && !isGroundPounding)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x) * initialScale.x, initialScale.y, initialScale.z);
        }

        // Handle structural locomotion animations (Idle, Walk, Run, Jump, Fall)
        UpdateLocomotionAnimations();
    }

    private void FixedUpdate()
    {
        HandleHorizontalMovement();
        ApplyBetterJumpPhysics();
        HandleGroundPoundPhysics();
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer);

        if (isGrounded)
        {
            canDoubleJump = true;
            if (!wasGrounded && isGroundPounding)
            {
                isGroundPounding = false;
            }
        }
        else 
        {
            // Airborne tracking state:
            // If our vertical velocity drops below -1f, we are officially falling!
            if (rb.linearVelocity.y < -1f)
            {
               SetAnimation(fallAnim, true, 0); // Plays your Spine fall loop
            }
        }
    }

    private void HandleHorizontalMovement()
    {
        if (isGroundPounding) return;

        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        float targetVelocityX = Mathf.Abs(moveInput.x) > 0.1f ? moveInput.x * targetSpeed : 0f;
        float newVelocityX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelocityX, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
    }

    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        if (isGroundPounding) return;

        if (isGrounded)
        {
            ExecuteJump(jumpForce);
        }
        else if (canDoubleJump)
        {
            ExecuteJump(doubleJumpForce);
            canDoubleJump = false;
        }
    }

    private void ExecuteJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);

        // Force the current animation string tracker to "jump" 
        // This stops other methods from immediately rewriting it on Track 0
        currentAnimation = jumpAnim;
        skeletonAnimation.AnimationState.SetAnimation(0, jumpAnim, false);
    }
   /* private void ExecuteJump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        SetAnimation(jumpAnim, false, 0); // Play jump once on Track 0
    }*/

    private void ApplyBetterJumpPhysics()
    {
        if (isGroundPounding) return;

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * fallMultiplier * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !controls.Player.Jump.IsPressed())
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier * 1.2f) * Time.fixedDeltaTime;
        }
    }

    private void OnGroundPoundPressed(InputAction.CallbackContext context)
    {
        if (!isGrounded && !isGroundPounding)
        {
            isGroundPounding = true;
            rb.linearVelocity = Vector2.zero;
            SetAnimation(groundPoundAnim, true, 0); // Loop slam animation down
        }
    }

    private void HandleGroundPoundPhysics()
    {
        if (isGroundPounding)
        {
            rb.linearVelocity = new Vector2(0f, -groundPoundForce);
        }
    }

    private void OnShootPressed(InputAction.CallbackContext context)
    {
        if (projectilePrefab && shootPoint)
        {
            GameObject bullet = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            float direction = transform.localScale.x;
            if (bullet.TryGetComponent(out Projectile2D proj))
            {
                proj.Launch(direction);
            }

            // Play shooting overlay animation on Track 1 so the legs can still run/walk independently!
            if (!string.IsNullOrEmpty(shootAnim))
            {
                var shootTrack = skeletonAnimation.AnimationState.SetAnimation(1, shootAnim, false);
                skeletonAnimation.AnimationState.AddEmptyAnimation(1, 0.2f, 0f); // Clear Track 1 when done shooting
            }
        }
    }

    private void UpdateLocomotionAnimations()
    {
        // Ground Pound state takes full tracking precedence
        if (isGroundPounding) return;

        if (isGrounded)
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.2f)
            {
                SetAnimation(isRunning ? runAnim : walkAnim, true, 0);
            }
            else
            {
                SetAnimation(idleAnim, true, 0);
            }
        }
        else
        {
            // Falling state handles the drift down after a jump finishes scaling up
            if (rb.linearVelocity.y < -2f)
            {
                SetAnimation(fallAnim, true, 0);
            }
            else if (rb.linearVelocity.y > 0.1f)
            {
                // 3. Keeps the jump animation locked in as long as the player is rising
                SetAnimation(jumpAnim, false, 0);
            }
        }
    }

    // Safeguard helper method to ensure animations aren't restarting every frame
    private void SetAnimation(string animName, bool loop, int trackIndex)
    {
        if (skeletonAnimation == null || string.IsNullOrEmpty(animName)) return;

        // Track state only interrupts if changing to a brand new animation sequence
        if (trackIndex == 0 && currentAnimation == animName) return;

        skeletonAnimation.AnimationState.SetAnimation(trackIndex, animName, loop);

        if (trackIndex == 0)
        {
            currentAnimation = animName;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
    }
}
