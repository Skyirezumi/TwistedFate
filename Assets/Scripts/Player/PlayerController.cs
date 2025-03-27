using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public bool FacingLeft { get; set; }

    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float dashSpeed = 4f;
    [SerializeField] private float dashDuration = 0.2f;

    [SerializeField] private float dashCooldown = 1f;
    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;

    private SpriteRenderer spriteRenderer;

    [SerializeField] private TrailRenderer trailRenderer;
    
    [Header("Card Throwing")]
    [SerializeField] private CardThrower cardThrower;

    private bool isDashing = false;
    private bool isShooting = false;
    
    // Direction states
    private enum FacingDirection
    {
        Front,
        Back,
        Left,
        Right
    }
    
    private FacingDirection currentDirection = FacingDirection.Front;
    private string currentAnimation;

    private Animator animator;
    // Start is called before the first frame update

    private void Awake()
    {
        Instance = this;
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    void Start()
    {
        playerControls.Combat.Dash.performed += _ => Dash();
        playerControls.Combat.ThrowCard.performed += _ => ThrowCard();
        PlayAnimation("idle_front");
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * movementSpeed * Time.fixedDeltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerInput();
        DeterminePlayerDirection();
        AdjustPlayerFacing();
        UpdateAnimationState();
        
        // Debug
        if (movement.magnitude < 0.1f && currentAnimation != null && currentAnimation.StartsWith("run_"))
        {
            Debug.LogWarning("Should stop running but animation is: " + currentAnimation);
        }
    }

    private void PlayerInput()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();
        // Restore these in case pickups rely on them
        animator.SetFloat("MoveX", movement.x);
        animator.SetFloat("MoveY", movement.y);
    }
    
    private void DeterminePlayerDirection()
    {
        // Determine the dominant direction for animation purposes
        if (movement.magnitude < 0.1f)
        {
            // Don't change direction when idle
            return;
        }
        
        // Determine the most dominant direction (up, down, left, right)
        float absX = Mathf.Abs(movement.x);
        float absY = Mathf.Abs(movement.y);
        
        if (absX > absY)
        {
            // Horizontal movement dominates
            if (movement.x > 0)
            {
                // Moving right
                currentDirection = FacingDirection.Right;
            }
            else
            {
                // Moving left
                currentDirection = FacingDirection.Left;
            }
        }
        else
        {
            if (movement.y > 0)
                currentDirection = FacingDirection.Back;
            else
                currentDirection = FacingDirection.Front;
        }
    }

    private void AdjustPlayerFacing()
    {
        // Only update flip state when not running or at the start of run
        bool isMoving = movement.magnitude > 0.1f;
        bool isShootingAnimation = currentAnimation != null && currentAnimation.Contains("shoot");
        
        // If moving, flip based on movement direction
        if (isMoving && movement.x != 0)
        {
            spriteRenderer.flipX = (movement.x > 0);
            FacingLeft = (movement.x < 0);
        }
        // If not moving, use mouse position to determine facing
        else if (!isMoving || (currentAnimation == null) || (!currentAnimation.StartsWith("run_") && !isShootingAnimation))
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
            
            if (mousePosition.x < playerScreenPoint.x)
            {
                spriteRenderer.flipX = false;
                FacingLeft = true;
            }
            else
            {
                spriteRenderer.flipX = true;
                FacingLeft = false;
            }
        }
    }
    
    private void UpdateAnimationState()
    {
        bool isMoving = movement.magnitude > 0.1f;
        
        // Force stop running animation when player stops moving
        if (!isMoving)
        {
            if (currentAnimation != null && currentAnimation.StartsWith("run_"))
            {
                Debug.Log("Forcing idle because movement stopped: " + movement.magnitude);
                PlayAnimation(GetIdleAnimationName());
                return;
            }
        }
        
        // Determine what animation to play based on state
        string animationName = GetAnimationName();
        
        // Only change animation if different from current
        if (animationName != currentAnimation)
        {
            PlayAnimation(animationName);
            Debug.Log("Playing animation: " + animationName);
        }
    }

    private string GetAnimationName()
    {
        bool isMoving = movement.magnitude > 0.1f;
        
        // Determine base animation name
        string baseName = "";
        
        // If running and shooting, use the combined animation
        if (isMoving && isShooting)
        {
            baseName = "running_shoot_";
            Debug.Log("Should be playing running_shoot animation");
        }
        // If just shooting
        else if (isShooting)
        {
            baseName = "shoot_";
        }
        // If running
        else if (isMoving)
        {
            baseName = "run_";
        }
        // Idle state
        else
        {
            return GetIdleAnimationName();
        }
        
        // Add direction suffix
        switch (currentDirection)
        {
            case FacingDirection.Front:
                return baseName + "front";
            case FacingDirection.Back:
                return baseName + "back";
            case FacingDirection.Left:
                return baseName + "left";
            case FacingDirection.Right:
                return baseName + "right";
        }
        
        return "idle_front"; // Default
    }
    
    private string GetIdleAnimationName()
    {
        // Special case for idle back, which has a different name
        if (currentDirection == FacingDirection.Back)
            return "idleback";
            
        // Regular idle animations
        switch (currentDirection)
        {
            case FacingDirection.Front:
                return "idle_front";
            case FacingDirection.Left:
                return "idle_left";
            case FacingDirection.Right:
                return "idle_right";
            default:
                return "idle_front";
        }
    }
    
    private void PlayAnimation(string animationName)
    {
        // Play the animation using the proper state path format
        // The format should be "Base Layer.StateName" instead of just "StateName"
        animator.Play("Base Layer." + animationName, 0);
        currentAnimation = animationName;
    }

    private void Dash()
    {
        if (isDashing) return;
        isDashing = true;
        movementSpeed *= dashSpeed;
        trailRenderer.emitting = true;
        StartCoroutine(EndDashRoutine());
    }

    private IEnumerator EndDashRoutine()
    {
        yield return new WaitForSeconds(dashDuration);
        trailRenderer.emitting = false;
        movementSpeed /= dashSpeed;
        yield return new WaitForSeconds(dashCooldown);
        isDashing = false;
    }
    
    private void ThrowCard()
    {
        if (cardThrower != null)
        {
            cardThrower.TriggerThrowCard();
            StartCoroutine(ShootAnimationRoutine());
        }
        else
        {
            Debug.LogWarning("CardThrower component not assigned to PlayerController!");
        }
    }
    
    private IEnumerator ShootAnimationRoutine()
    {
        // Set shooting animation state
        isShooting = true;
        
        // Wait for animation to complete
        yield return new WaitForSeconds(0.3f); // Adjust timing based on animation length
        
        // Reset shooting state
        isShooting = false;
    }
}
