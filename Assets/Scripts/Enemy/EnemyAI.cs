using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float roamChangeDirFloat = 2f;
    [SerializeField] private float attackRange = 0f;
    [SerializeField] private MonoBehaviour enemyType;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private bool stopMovingWhileAttacking = false;

    private bool canAttack = true;

    private enum State {
        Roaming, 
        Attacking
    }

    private Vector2 roamPosition;
    private float timeRoaming = 0f;
    
    private State state;
    private EnemyPathfinding enemyPathfinding;

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
        
        // Visual effect
        GameObject stunEffect = CreateStunEffect();
        
        // Store original state
        State previousState = state;
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Remove stun
        isStunned = false;
        
        // Restore previous state or check if player is in attack range
        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) < attackRange) {
            state = State.Attacking;
        } else {
            state = previousState;
        }
        
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
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        state = State.Roaming;
    }

    private void Start() {
        roamPosition = GetRoamingPosition();
    }

    private void Update() {
        // Skip AI updates if stunned
        if (isStunned)
        {
            return;
        }
        
        MovementStateControl();
    }

    private void MovementStateControl() {
        switch (state)
        {
            default:
            case State.Roaming:
                Roaming();
            break;

            case State.Attacking:
                Attacking();
            break;
        }
    }

    private void Roaming() {
        timeRoaming += Time.deltaTime;

        enemyPathfinding.MoveTo(roamPosition);

        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) < attackRange) {
            state = State.Attacking;
        }

        if (timeRoaming > roamChangeDirFloat) {
            roamPosition = GetRoamingPosition();
        }
    }

    private void Attacking() {
        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) > attackRange)
        {
            state = State.Roaming;
        }

        if (attackRange != 0 && canAttack) {

            canAttack = false;
            (enemyType as IEnemy).Attack();

            if (stopMovingWhileAttacking) {
                enemyPathfinding.StopMoving();
            } else {
                enemyPathfinding.MoveTo(roamPosition);
            }

            StartCoroutine(AttackCooldownRoutine());
        }
    }

    private IEnumerator AttackCooldownRoutine() {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private Vector2 GetRoamingPosition() {
        timeRoaming = 0f;
        
        if (PlayAreaManager.Instance != null) {
            // Get a random direction but ensure it stays in play area
            Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            
            // Calculate a position that's within the play area
            Vector2 currentPos = transform.position;
            Vector2 targetPos = currentPos + randomDir;
            
            // Check if this position is within play area, if not, adjust
            if (!PlayAreaManager.Instance.IsInPlayArea(targetPos)) {
                // Get a position toward the center of the play area
                Vector2 centerDir = PlayAreaManager.Instance.Center - (Vector2)currentPos;
                
                return centerDir.normalized;
            }
            
            return randomDir;
        }
        
        // Default behavior if no PlayAreaManager
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }
}
