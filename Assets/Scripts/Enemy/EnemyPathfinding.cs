using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool stayInPlayArea = true;  // Added option to constrain to play area

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Knockback knockback;
    private SpriteRenderer spriteRenderer;
    private Vector2 nextPosition;

    // Stun functionality
    
    private bool isStunned = false;
    private Coroutine stunCoroutine;
    
    public void ApplyStun(float duration)
    {
        // Stop existing stun coroutine if active
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        
        // Start new stun
        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }
    
    private IEnumerator StunRoutine(float duration)
    {
        // Apply stun
        isStunned = true;
        
        // Create stun visual effect
        GameObject stunEffect = CreateStunEffect();
        
        // Store original speed
        float originalSpeed = moveSpeed;
        
        // Set speed to 0 to stop movement
        moveSpeed = 0f;
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Remove stun
        isStunned = false;
        
        // Restore speed
        moveSpeed = originalSpeed;
        
        // Clean up stun effect
        if (stunEffect != null)
        {
            Destroy(stunEffect);
        }
        
        stunCoroutine = null;
    }
    
    private GameObject CreateStunEffect()
    {
        // Create a visual indicator for the stun effect
        GameObject stunObject = new GameObject("StunEffect");
        stunObject.transform.parent = transform;
        stunObject.transform.localPosition = new Vector3(0, 0.5f, 0); // Position above the enemy
        
        // Add a sprite renderer to show stars or other stun indicator
        SpriteRenderer renderer = stunObject.AddComponent<SpriteRenderer>();
        
        // Create a simple particle effect for stun
        ParticleSystem particleSystem = stunObject.AddComponent<ParticleSystem>();
        
        // Configure basic particle system to look like stars or similar
        var main = particleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(Color.yellow, Color.white);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Emission
        var emission = particleSystem.emission;
        emission.rateOverTime = 8;
        
        // Shape
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;
        
        particleSystem.Play();
        
        return stunObject;
    }

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockback = GetComponent<Knockback>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        if (knockback.GettingKnockedBack) { return; }

        // Calculate the next position
        nextPosition = rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime);
        
        // If stay in play area is enabled, clamp the position
        if (stayInPlayArea && PlayAreaManager.Instance != null) {
            nextPosition = PlayAreaManager.Instance.ClampToPlayArea(nextPosition);
            
            // If we're at the boundary and trying to move outside, redirect movement
            if (!PlayAreaManager.Instance.IsInPlayArea(rb.position + moveDir.normalized * 0.1f)) {
                // If we're heading toward the player but are at the boundary, recalculate direction
                if (IsMovingTowardPlayer()) {
                    // Find a direction that keeps us in bounds while still trying to chase the player
                    moveDir = CalculateValidDirection();
                    nextPosition = rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime);
                }
            }
        }
        
        rb.MovePosition(nextPosition);

        if (moveDir.x < 0) {
            spriteRenderer.flipX = true;
        } else if (moveDir.x > 0) {
            spriteRenderer.flipX = false;
        }
    }

    // Check if the enemy is moving toward the player
    private bool IsMovingTowardPlayer() {
        if (PlayerController.Instance == null) return false;
        
        Vector2 dirToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
        return Vector2.Dot(dirToPlayer, moveDir) > 0.5f; // If dot product is positive, we're generally moving toward the player
    }
    
    // Calculate a valid direction that keeps the enemy in bounds
    private Vector2 CalculateValidDirection() {
        if (PlayAreaManager.Instance == null || PlayerController.Instance == null) return Vector2.zero;
        
        Vector2 dirToPlayer = (PlayerController.Instance.transform.position - transform.position).normalized;
        
        // Try multiple directions, prioritizing directions closer to the player
        Vector2[] testDirections = new Vector2[] {
            dirToPlayer,                           // Direct to player
            new Vector2(dirToPlayer.x, 0),         // Horizontal component only
            new Vector2(0, dirToPlayer.y),         // Vertical component only
            new Vector2(-dirToPlayer.x, dirToPlayer.y), // Perpendicular 1
            new Vector2(dirToPlayer.x, -dirToPlayer.y), // Perpendicular 2
            -dirToPlayer                           // Away from player (last resort)
        };
        
        foreach (Vector2 dir in testDirections) {
            if (dir.magnitude < 0.1f) continue;  // Skip zero vectors
            
            Vector2 testPos = (Vector2)transform.position + dir.normalized * 0.2f;
            if (PlayAreaManager.Instance.IsInPlayArea(testPos)) {
                return dir.normalized;
            }
        }
        
        // If all else fails, move away from the boundary
        Vector2 awayCenterDir = ((Vector2)transform.position - PlayAreaManager.Instance.Center).normalized;
        
        return -awayCenterDir; // Move toward center of play area
    }

    public void MoveTo(Vector2 targetPosition) {
        moveDir = targetPosition;
    }

    public void StopMoving() {
        moveDir = Vector3.zero;
    }

    // Modify the Update method to check for stun
    private void Update()
    {
        // Skip movement if stunned
        if (isStunned)
        {
            return;
        }
        
        // Existing movement code...
    }
}
