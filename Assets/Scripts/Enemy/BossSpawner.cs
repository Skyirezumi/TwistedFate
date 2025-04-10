using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int numberOfEnemiesToSpawn = 3;
    [SerializeField] private float spawnRadius = 3f;
    [SerializeField] private bool hasSpawnedEnemies = false;
    [SerializeField] private GameObject spawnEffectPrefab;
    
    [Header("Health Threshold")]
    [SerializeField] private float healthThresholdPercent = 0.5f; // 50% health
    
    private EnemyHealth bossHealth;
    
    private void Awake()
    {
        // Get the boss health component
        bossHealth = GetComponent<EnemyHealth>();
        
        if (bossHealth == null)
        {
            Debug.LogError("BossSpawner requires an EnemyHealth component on the same GameObject!");
        }
    }
    
    private void Start()
    {
        if (bossHealth != null)
        {
            // Subscribe to the health changed event
            bossHealth.OnHealthChanged += CheckHealthThreshold;
        }
    }
    
    private void OnDestroy()
    {
        if (bossHealth != null)
        {
            // Unsubscribe when destroyed to prevent memory leaks
            bossHealth.OnHealthChanged -= CheckHealthThreshold;
        }
    }
    
    private void CheckHealthThreshold(int currentHealth, int maxHealth)
    {
        // Calculate current health percentage
        float healthPercentage = (float)currentHealth / maxHealth;
        
        // Check if health is below threshold and enemies haven't been spawned yet
        if (healthPercentage <= healthThresholdPercent && !hasSpawnedEnemies)
        {
            SpawnEnemies();
            hasSpawnedEnemies = true;
        }
    }
    
    private void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
        {
            // Calculate random position within spawn radius
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 spawnPosition = transform.position + new Vector3(randomDirection.x, randomDirection.y, 0) * spawnRadius;
            
            // Create spawn effect if available
            if (spawnEffectPrefab != null)
            {
                Instantiate(spawnEffectPrefab, spawnPosition, Quaternion.identity);
            }
            
            // Spawn the enemy
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }
    
    // For debugging purposes
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
} 