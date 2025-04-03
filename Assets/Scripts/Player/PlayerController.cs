using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public bool FacingLeft { get; set; }
    public bool IsDead { get { return isDead; } }

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

    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepRate = 0.5f;
    [SerializeField] private float footstepVolume = 0.4f;
    private float footstepTimer = 0f;
    private AudioSource audioSource;
    private bool isPlayingFootstep = false;
    private Coroutine currentFootstepCoroutine;
    private List<GameObject> activeFootstepSounds = new List<GameObject>();

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

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    // Start is called before the first frame update

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        
        // Get or add AudioSource for footsteps
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.volume = footstepVolume;
        }
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
        if (isDead) return;
        
        // Calculate the next position
        Vector2 nextPosition = rb.position + movement * movementSpeed * Time.fixedDeltaTime;
        
        // Constrain to play area if PlayAreaManager exists
        if (PlayAreaManager.Instance != null)
        {
            // Get current position and direction to center
            Vector2 positionToCenter = rb.position - (Vector2)PlayAreaManager.Instance.Center;
            float distanceToCenter = positionToCenter.magnitude;
            
            // If we're at the boundary and trying to move outward, cancel outward movement
            if (distanceToCenter >= PlayAreaManager.Instance.Radius * 0.98f) // Allow a small buffer
            {
                // Calculate the normalized direction from center
                Vector2 directionFromCenter = positionToCenter.normalized;
                
                // Get the dot product between movement and direction from center
                float movingOutward = Vector2.Dot(movement.normalized, directionFromCenter);
                
                // If trying to move outward, project movement along the tangent only
                if (movingOutward > 0)
                {
                    // Project movement vector onto tangent of the circle
                    Vector2 tangent = new Vector2(-directionFromCenter.y, directionFromCenter.x);
                    Vector2 tangentialMovement = Vector2.Dot(movement, tangent) * tangent;
                    nextPosition = rb.position + tangentialMovement * movementSpeed * Time.fixedDeltaTime;
                }
            }
            
            // Final safety clamp
            nextPosition = PlayAreaManager.Instance.ClampToPlayArea(nextPosition);
        }
        
        // Apply the movement
        rb.MovePosition(nextPosition);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerInput();
        DeterminePlayerDirection();
        AdjustPlayerFacing();
        UpdateAnimationState();
        UpdateFootsteps();
        
        // Debug
        if (movement.magnitude < 0.1f && currentAnimation != null && currentAnimation.StartsWith("run_"))
        {
            Debug.LogWarning("Should stop running but animation is: " + currentAnimation);
        }
    }

    private void PlayerInput()
    {
        if (isDead) return;
        
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
            // Keep the current direction when idle
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
            // Vertical movement dominates
            if (movement.y > 0)
            {
                // Moving up
                currentDirection = FacingDirection.Back;
            }
            else
            {
                // Moving down
                currentDirection = FacingDirection.Front;
            }
        }
    }

    private void AdjustPlayerFacing()
    {
        // Only update facing direction when not running or at the start of run
        bool isMoving = movement.magnitude > 0.1f;
        bool isShootingAnimation = currentAnimation != null && currentAnimation.Contains("shoot");
        
        // If moving, set facing direction based on movement
        if (isMoving && movement.x != 0)
        {
            FacingLeft = (movement.x < 0);
        }
        // If not moving, use mouse position to determine facing
        else if (!isMoving || (currentAnimation == null) || (!currentAnimation.StartsWith("run_") && !isShootingAnimation))
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
            
            FacingLeft = (mousePosition.x < playerScreenPoint.x);
        }
    }
    
    private void UpdateAnimationState()
    {
        bool isMoving = movement.magnitude > 0.1f;
        
        // Force stop running animation when player stops moving
        if (!isMoving)
        {
            if (currentAnimation != null && (currentAnimation.StartsWith("run_") || currentAnimation.StartsWith("running_shoot_")))
            {
                Debug.Log($"Stopping running animation. Current: {currentAnimation}, Movement: {movement}, IsMoving: {isMoving}");
                string idleAnimation = GetIdleAnimationName();
                PlayAnimation(idleAnimation);
                return;
            }
        }
        
        // Determine what animation to play based on state
        string animationName = GetAnimationName();
        
        // Only change animation if different from current
        if (animationName != currentAnimation)
        {
            Debug.Log($"Changing animation from {currentAnimation} to {animationName}. Movement: {movement}, IsMoving: {isMoving}");
            PlayAnimation(animationName);
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
        // Regular idle animations for all directions
        switch (currentDirection)
        {
            case FacingDirection.Front:
                return "Idle_front";
            case FacingDirection.Back:
                return "idle_back";
            case FacingDirection.Left:
                return "idle_left";
            case FacingDirection.Right:
                return "idle_right";
            default:
                return "Idle_front";
        }
    }
    
    private void PlayAnimation(string animationName)
    {
        // Play the animation directly without the Base Layer prefix
        animator.Play(animationName, 0);
        currentAnimation = animationName;

        // Adjust animation speed based on movement speed
        if (animationName.StartsWith("run_") || animationName.StartsWith("running_shoot_"))
        {
            // Set animation speed to match movement speed
            animator.speed = movementSpeed / 10f; // 10f is the base movement speed
        }
        else
        {
            // Reset to normal speed for other animations
            animator.speed = 1f;
        }
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

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        SetDeadState(true);
        
        // Find and show the death screen
        DeathScreen deathScreen = FindObjectOfType<DeathScreen>();
        if (deathScreen != null)
        {
            deathScreen.ShowDeathScreen();
        }
        else
        {
            Debug.LogError("DeathScreen not found in the scene!");
        }
    }
    
    // Public method to set the dead state - can be called from PlayerHealth
    public void SetDeadState(bool state)
    {
        isDead = state;
        
        // If dead, zero out movement to stop immediately
        if (isDead)
        {
            movement = Vector2.zero;
            
            // Stop any ongoing animations
            if (animator != null)
            {
                // Set trigger for death or play idle animation
                PlayAnimation(GetIdleAnimationName());
            }
        }
    }
    
    // STAT UPGRADE METHODS
    
    // Increase the player's maximum health
    public void IncreaseMaxHealth(float amount)
    {
        float oldMaxHealth = maxHealth;
        maxHealth += amount;
        currentHealth += amount;
        Debug.Log($"Player: Max health increased from {oldMaxHealth} to {maxHealth} (added {amount})");
    }
    
    // Increase the player's movement speed
    public void IncreaseMovementSpeed(float amount)
    {
        float oldSpeed = movementSpeed;
        movementSpeed += amount;
        Debug.Log($"Player: Movement speed increased from {oldSpeed} to {movementSpeed} (added {amount})");
    }
    
    // Increase the dash power/distance
    public void IncreaseDashPower(float amount)
    {
        float oldDashPower = dashSpeed;
        dashSpeed += amount;
        Debug.Log($"Player: Dash power increased from {oldDashPower} to {dashSpeed} (added {amount})");
    }
    
    // Add this method to test player death
    public void TestDeath()
    {
        TakeDamage(maxHealth);
    }

    // Add public getter methods to expose current values
    public float GetCurrentMovementSpeed()
    {
        return movementSpeed;
    }

    public float GetCurrentDashPower()
    {
        return dashSpeed;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    // New method to handle footstep sounds
    private void UpdateFootsteps()
    {
        if (isDead) return;
        
        // If moving and on the ground
        if (movement.magnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            
            // Play footstep sound at regular intervals while moving
            if (footstepTimer <= 0 && !isPlayingFootstep)
            {
                currentFootstepCoroutine = StartCoroutine(PlayFootstepWithCooldown());
                // Reset timer - make it slightly longer than the footstep sound length
                footstepTimer = footstepRate;
            }
        }
        else
        {
            // Reset timer when not moving
            footstepTimer = 0;
            
            // Stop any playing footstep coroutine when player stops moving
            if (isPlayingFootstep && currentFootstepCoroutine != null)
            {
                StopCoroutine(currentFootstepCoroutine);
                isPlayingFootstep = false;
                currentFootstepCoroutine = null;
            }
            
            // Quickly fade out any active footstep sounds
            FadeOutAllFootsteps();
        }
    }

    // Play a random footstep sound with cooldown to prevent overlapping
    private IEnumerator PlayFootstepWithCooldown()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) yield break;
        
        isPlayingFootstep = true;
        
        // Select a random footstep sound
        int randomIndex = Random.Range(0, footstepSounds.Length);
        AudioClip footstepSound = footstepSounds[randomIndex];
        
        if (footstepSound != null)
        {
            // Create a temporary AudioSource that we can stop immediately if needed
            GameObject tempAudio = new GameObject("TempFootstepSound");
            tempAudio.transform.position = transform.position;
            tempAudio.transform.parent = transform;
            
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = footstepSound;
            tempSource.volume = footstepVolume;
            tempSource.spatialBlend = 1.0f;
            tempSource.Play();
            
            // Keep track of active sounds so we can fade them out if needed
            activeFootstepSounds.Add(tempAudio);
            
            // Clean up the audio when done
            float soundDuration = footstepSound.length * 0.8f;
            yield return new WaitForSeconds(soundDuration);
            
            // Remove from active sounds list and destroy
            activeFootstepSounds.Remove(tempAudio);
            Destroy(tempAudio);
        }
        
        isPlayingFootstep = false;
        currentFootstepCoroutine = null;
    }
    
    // Quick fade out of all active footstep sounds
    private void FadeOutAllFootsteps()
    {
        if (activeFootstepSounds.Count == 0) return;
        
        // Start fade out on all active footstep sounds
        foreach (GameObject soundObj in activeFootstepSounds.ToArray())
        {
            if (soundObj != null)
            {
                StartCoroutine(QuickFadeOut(soundObj));
            }
        }
        
        // Clear the list since we're handling them all
        activeFootstepSounds.Clear();
    }
    
    // Coroutine for a very quick fade-out and destroy
    private IEnumerator QuickFadeOut(GameObject soundObj)
    {
        AudioSource source = soundObj.GetComponent<AudioSource>();
        if (source == null)
        {
            Destroy(soundObj);
            yield break;
        }
        
        // Very quick fade-out (0.1 seconds)
        float fadeTime = 0.1f;
        float startVolume = source.volume;
        float timer = 0;
        
        while (timer < fadeTime && soundObj != null)
        {
            timer += Time.deltaTime;
            if (source != null)
            {
                source.volume = Mathf.Lerp(startVolume, 0, timer / fadeTime);
            }
            yield return null;
        }
        
        // Destroy after fade-out
        if (soundObj != null)
        {
            Destroy(soundObj);
        }
    }
}
