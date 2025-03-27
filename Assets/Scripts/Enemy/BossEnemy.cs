using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private float musicTriggerRange = 15f;
    [SerializeField] private AudioClip bossTheme;
    [SerializeField] private bool activateMusicOnStart = true;
    [SerializeField] private bool showDebugRange = true;
    
    private bool playerInRange = false;
    private EnemyAI enemyAI;
    private Transform playerTransform;
    
    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        if (enemyAI == null)
        {
            Debug.LogError("BossEnemy script requires an EnemyAI component!");
        }
    }
    
    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
        else
        {
            Debug.LogError("Player not found in scene! BossEnemy script requires PlayerController.Instance");
        }
    }
    
    private void Update()
    {
        if (playerTransform == null || AudioManager.Instance == null || bossTheme == null) return;
        
        // Check if player is in range
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool isInRange = distanceToPlayer <= musicTriggerRange;
        
        // If player enters the range
        if (isInRange && !playerInRange)
        {
            playerInRange = true;
            AudioManager.Instance.PlayBossTheme(bossTheme);
        }
        // If player exits the range
        else if (!isInRange && playerInRange)
        {
            playerInRange = false;
            AudioManager.Instance.ReturnToNormalMusic();
        }
    }
    
    private void OnDrawGizmos()
    {
        if (showDebugRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, musicTriggerRange);
        }
    }
} 