using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngryEnemyAI : MonoBehaviour
{
    [Header("Angry Enemy Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float aggroRange = 15f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private MonoBehaviour enemyType;
    [SerializeField] private bool showAggroRange = true;
    [SerializeField] private Color aggroRangeColor = Color.red;
    
    private bool canAttack = true;
    private EnemyPathfinding enemyPathfinding;
    private Transform playerTransform;
    private Vector2 targetPosition;
    private enum State { Chasing, Attacking }
    private State currentState;

    private void Awake()
    {
        // Get components
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        
        // Default state is chasing
        currentState = State.Chasing;
    }

    private void Start()
    {
        // Find player
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
        else
        {
            Debug.LogError("Player not found! AngryEnemyAI requires PlayerController.Instance");
        }
    }

    private void Update()
    {
        // Try to get player reference if it was null
        if (playerTransform == null && PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
        
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // State management
        if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attacking;
        }
        else
        {
            currentState = State.Chasing;
        }
        
        // Handle states
        switch (currentState)
        {
            case State.Chasing:
                ChasePlayer();
                break;
                
            case State.Attacking:
                AttackPlayer();
                break;
        }
    }
    
    private void ChasePlayer()
    {
        if (playerTransform == null) return;
        
        // Always move directly toward the player
        targetPosition = (Vector2)playerTransform.position - (Vector2)transform.position;
        enemyPathfinding.MoveTo(targetPosition.normalized);
    }
    
    private void AttackPlayer()
    {
        if (canAttack && enemyType != null)
        {
            // Stop moving while attacking
            enemyPathfinding.StopMoving();
            
            // Attack
            canAttack = false;
            
            // Use interface if it's available
            if (enemyType is IEnemy enemy)
            {
                enemy.Attack();
            }
            
            // Start attack cooldown
            StartCoroutine(AttackCooldownRoutine());
        }
    }
    
    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        if (showAggroRange)
        {
            Gizmos.color = aggroRangeColor;
            Gizmos.DrawWireSphere(transform.position, aggroRange);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
} 