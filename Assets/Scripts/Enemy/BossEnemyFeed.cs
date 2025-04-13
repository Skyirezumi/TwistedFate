using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemyFeed : MonoBehaviour
{
    [Header("Feeding Settings")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private string targetEnemyTag = "CactusEnemy";
    [SerializeField] private float healthGainAmount = 10f;
    [SerializeField] private float feedCooldown = 1.5f;
    [SerializeField] private bool showDetectionRadius = true;
    [SerializeField] private Color detectionRadiusColor = Color.green;
    
    [Header("Particle Effects")]
    [SerializeField] private GameObject eatEffectPrefab; // Particle system for eating effect
    [SerializeField] private float eatEffectDuration = 0.5f;
    
    [Header("State Management")]
    [SerializeField] private float healthThresholdToFeed = 0.7f; // Start feeding when below 70% health
    [SerializeField] private float returnToFightThreshold = 0.9f; // Return to fighting when above 90% health
    
    // Components
    private EnemyHealth bossHealth;
    private EnemyPathfinding enemyPathfinding;
    private EnemyAI enemyAI;
    private Collider2D bossCollider;
    
    // State tracking
    private bool isFeeding = false;
    private bool canFeed = true;
    private Transform currentTarget;
    private Transform originalTarget;
    
    // Start is called before the first frame update
    void Start()
    {
        // Get required components
        bossHealth = GetComponent<EnemyHealth>();
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        enemyAI = GetComponent<EnemyAI>();
        bossCollider = GetComponent<Collider2D>();
        
        if (bossHealth == null || enemyPathfinding == null || enemyAI == null || bossCollider == null)
        {
            Debug.LogError("BossEnemyFeed requires EnemyHealth, EnemyPathfinding, EnemyAI, and Collider2D components on the same GameObject!");
            enabled = false;
            return;
        }
        
        // Subscribe to health changed event
        bossHealth.OnHealthChanged += CheckHealthThreshold;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (bossHealth != null)
        {
            bossHealth.OnHealthChanged -= CheckHealthThreshold;
        }
    }
    
    // Check health threshold to determine if we should start feeding
    private void CheckHealthThreshold(int currentHealth, int maxHealth)
    {
        float healthPercentage = (float)currentHealth / maxHealth;
        
        // Start feeding mode if health is below threshold
        if (healthPercentage <= healthThresholdToFeed && !isFeeding)
        {
            StartFeedingMode();
        }
        // Return to normal mode if health is above return threshold
        else if (healthPercentage >= returnToFightThreshold && isFeeding)
        {
            StopFeedingMode();
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!isFeeding) return;
        
        // If no target or target was destroyed, find a new one
        if (currentTarget == null)
        {
            FindClosestFoodTarget();
        }
        
        // If we have a target, move toward it
        if (currentTarget != null)
        {
            // Move toward food target
            Vector2 direction = (currentTarget.position - transform.position).normalized;
            
            // Instead of directly controlling movement, inform the EnemyAI about our food target
            // This way the boss can still attack the player while moving toward the food
            if (enemyAI != null)
            {
                // Only override movement if we're in feeding mode
                enemyPathfinding.MoveTo(direction);
            }
            
            // Check if we're close enough to eat the target
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
            if (distanceToTarget <= 1.5f && canFeed) // 1.5 units is close enough to eat
            {
                EatEnemy(currentTarget.gameObject);
            }
        }
        else
        {
            // No food targets available, return to normal behavior
            StopFeedingMode();
        }
    }
    
    private void EatEnemy(GameObject enemy)
    {
        if (!canFeed) return;
        
        // Apply health boost to boss
        if (bossHealth != null)
        {
            bossHealth.GainHealth((int)healthGainAmount);
        }
        
        // Play eat effect/animation
        PlayEatEffect(enemy.transform.position);
        
        // Destroy the eaten enemy
        Destroy(enemy);
        
        // Reset current target
        currentTarget = null;
        
        // Set cooldown
        StartCoroutine(FeedCooldownRoutine());
    }
    
    private IEnumerator FeedCooldownRoutine()
    {
        canFeed = false;
        yield return new WaitForSeconds(feedCooldown);
        canFeed = true;
    }
    
    private void StartFeedingMode()
    {
        isFeeding = true;
        
        // NOTE: We no longer disable the EnemyAI component
        // This allows the boss to continue attacking while feeding
        
        // Find initial food target
        FindClosestFoodTarget();
    }
    
    private void StopFeedingMode()
    {
        isFeeding = false;
        currentTarget = null;
        
        // NOTE: No need to re-enable EnemyAI since we never disabled it
    }
    
    private void FindClosestFoodTarget()
    {
        // Find all potential food targets
        GameObject[] foodTargets = GameObject.FindGameObjectsWithTag(targetEnemyTag);
        
        // No targets found
        if (foodTargets.Length == 0)
        {
            currentTarget = null;
            return;
        }
        
        // Find the closest target
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;
        
        foreach (GameObject target in foodTargets)
        {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            
            if (distance < closestDistance && distance <= detectionRadius)
            {
                closestDistance = distance;
                closestTarget = target.transform;
            }
        }
        
        currentTarget = closestTarget;
    }
    
    private void PlayEatEffect(Vector3 position)
    {
        if (eatEffectPrefab != null)
        {
            // Create the particle effect
            GameObject effect = Instantiate(eatEffectPrefab, position, Quaternion.identity);
            
            // Destroy the effect after its duration
            Destroy(effect, eatEffectDuration);
        }
        
        // Play a sound effect if available
        // AudioManager.Instance?.PlaySound("BossEat");
    }
    
    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        if (showDetectionRadius)
        {
            Gizmos.color = detectionRadiusColor;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
} 