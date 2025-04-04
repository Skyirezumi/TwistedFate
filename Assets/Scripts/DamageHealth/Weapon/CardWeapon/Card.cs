using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [Header("Basic Card Settings")]
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected float lifetime = 5f;
    [SerializeField] protected int damageAmount = 1;
    [SerializeField] protected TrailRenderer trailRenderer;
    
    protected SpriteRenderer spriteRenderer;
    protected DamageSource damageSource;
    protected Vector2 direction;
    protected Rigidbody2D rb;
    
    // Card type and properties
    public enum CardType { Red, Green, Blue }
    protected CardType cardType;
    protected Color cardColor;
    protected GameObject effectPrefab;
    protected CardStats stats;
    protected AudioClip[] collisionSounds;
    
    [SerializeField] protected bool useCollision = true;
    
    protected Transform currentTarget;
    protected bool isCriticalHit = false;
    
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageSource = GetComponent<DamageSource>();
        rb = GetComponent<Rigidbody2D>();
        
        // Ensure we have a collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider)
        {
            collider.isTrigger = true; // Make sure it's a trigger
        }
        
        if (!trailRenderer)
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }
    }
    
    public void Initialize(CardType type, CardStats cardStats, GameObject impactEffect, Color color, AudioClip[] cardCollisionSounds = null)
    {
        cardType = type;
        stats = cardStats;
        effectPrefab = impactEffect;
        cardColor = color;
        collisionSounds = cardCollisionSounds;
        
        // Apply color to sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.color = cardColor;
            Debug.Log($"Initialized card of type {type} with color {color}");
        }
        else
        {
            Debug.LogError("Card sprite renderer is null! Make sure it's attached to the prefab.");
        }
        
        // Safety check for stats
        if (stats == null)
        {
            Debug.LogError("Card stats are null! Using default values.");
            stats = new CardStats();
        }
    }
    
    protected virtual void Start()
    {
        // Initialize
        Destroy(gameObject, lifetime);
    }
    
    protected virtual void Update()
    {
        // Debug movement
        if (stats == null)
        {
            Debug.LogError("Card stats are null in Update!");
            return;
        }
        
        // Check if we have a valid direction
        if (direction == Vector2.zero)
        {
            Debug.LogWarning("Card direction is zero! Setting default direction up.");
            direction = Vector2.up;
        }
        
        // Apply homing if stat is greater than 0
        if (stats.homingStrength.GetValue() > 0f)
        {
            ApplyHoming();
        }
        
        // Move in the set direction at constant speed - using up vector (Y axis)
        float speed = stats.speed.GetValue();
        if (speed <= 0) speed = 10f; // Fallback speed
        
        transform.Translate(Vector2.up * speed * Time.deltaTime);
        
        // Debug movement
        Debug.DrawRay(transform.position, transform.up * 0.5f, Color.red);
    }
    
    public virtual void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public virtual float GetSpeed()
    {
        return speed;
    }
    
    public virtual void Launch(Vector2 dir)
    {
        // Store the direction (normalized in the CardThrower)
        direction = dir;
        
        if (direction == Vector2.zero)
        {
            Debug.LogError("Launching card with zero direction! Setting default up direction.");
            direction = Vector2.up;
        }
        else
        {
            Debug.Log($"Launching card with direction: {direction}");
        }
        
        // Set rotation to point Y axis in the movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    // What happens when the card hits something
    protected virtual void OnHit(GameObject target)
    {
        // Play collision sound
        PlayCollisionSound();
        
        // Apply special effect based on card type
        ApplySpecialEffect(target);
        Destroy(gameObject);
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore collisions with the player and with the throw point
        if (other.gameObject.GetComponent<PlayerController>() == null &&
            other.transform != transform.parent)
        {
            OnHit(other.gameObject);
            Debug.Log($"Card hit trigger: {other.gameObject.name}");
        }
    }
    
    // Add OnCollisionEnter2D as backup if trigger doesn't work
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // No layer check - just ensure it's not the player or throw point
        if (collision.gameObject.GetComponent<PlayerController>() == null &&
            collision.transform != transform.parent)
        {
            OnHit(collision.gameObject);
            Debug.Log($"Card hit collision: {collision.gameObject.name}");
        }
    }
    
    public virtual void ApplySpecialEffect(GameObject target)
    {
        // Get values from stats
        float radius = stats.explosionRadius.GetValue();
        int damage = GetDamage();
        
        // Apply card type specific effects
        switch (cardType)
        {
            case CardType.Red:
                ApplyRedCardEffect(target, radius, damage);
                break;
            case CardType.Green:
                ApplyGreenCardEffect(target, radius, damage);
                break;
            case CardType.Blue:
                ApplyBlueCardEffect(target, radius, damage);
                break;
        }
        
        // Create impact effect
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            ParticleSystem particleSystem = effect.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                // Scale based on explosion radius
                effect.transform.localScale = Vector3.one * radius;
                
                // Adjust particle speed based on radius - larger explosions have faster particles
                var main = particleSystem.main;
                float speedMultiplier = Mathf.Clamp(radius * 0.8f, 0.5f, 3f);
                main.startSpeedMultiplier = main.startSpeed.constant * speedMultiplier;
                
                // Ensure the particle system plays
                particleSystem.Play();
                
                // Auto-destroy after playing
                float lifetime = main.duration + main.startLifetime.constantMax;
                Destroy(effect, lifetime);
            }
        }
    }
    
    protected virtual void ApplyRedCardEffect(GameObject target, float radius, int damage)
    {
        // Red card - standard damage with possible poison
        ApplyDamageInRadius(transform.position, radius, damage);
        
        // Check if poison upgrade is active
        CardThrower cardThrower = FindCardThrower();
        if (cardThrower != null && cardThrower.HasRedPoisonUpgrade())
        {
            // Apply poison to all targets in radius
            ApplyPoisonInRadius(transform.position, radius, 
                cardThrower.GetRedPoisonDamagePerSecond(), 
                cardThrower.GetRedPoisonDuration());
        }
    }
    
    protected virtual void ApplyGreenCardEffect(GameObject target, float radius, int damage)
    {
        // Green card - larger area damage (radius already affected by upgrade in CardThrower)
        ApplyDamageInRadius(transform.position, radius, damage);
    }
    
    protected virtual void ApplyBlueCardEffect(GameObject target, float radius, int damage)
    {
        // Blue card - standard damage with possible stun
        ApplyDamageInRadius(transform.position, radius, damage);
        
        // Check if stun upgrade is active
        CardThrower cardThrower = FindCardThrower();
        if (cardThrower != null && cardThrower.HasBlueStunUpgrade())
        {
            // Apply stun to all targets in radius
            ApplyStunInRadius(transform.position, radius, cardThrower.GetBlueStunDuration());
        }
    }
    
    protected virtual void ApplyPoisonInRadius(Vector2 position, float radius, float damagePerSecond, float duration)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, radius);
        foreach (Collider2D hitCollider in hitColliders)
        {
            // Apply poison to any enemy
            EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Create or get poison effect controller
                PoisonEffect poisonEffect = enemyHealth.gameObject.GetComponent<PoisonEffect>();
                if (poisonEffect == null)
                {
                    poisonEffect = enemyHealth.gameObject.AddComponent<PoisonEffect>();
                }
                
                // Apply poison
                poisonEffect.ApplyPoison(damagePerSecond, duration);
                
                Debug.Log($"Applied poison to {hitCollider.name}: {damagePerSecond} DPS for {duration}s");
            }
        }
    }
    
    protected virtual void ApplyStunInRadius(Vector2 position, float radius, float duration)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, radius);
        foreach (Collider2D hitCollider in hitColliders)
        {
            // Apply stun to any enemy with pathfinding
            EnemyPathfinding pathfinding = hitCollider.GetComponent<EnemyPathfinding>();
            if (pathfinding != null)
            {
                pathfinding.ApplyStun(duration);
                Debug.Log($"Applied stun to {hitCollider.name} for {duration}s");
            }
            
            // Also try to affect any enemy AI
            EnemyAI enemyAI = hitCollider.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.ApplyStun(duration);
                Debug.Log($"Applied stun to AI {hitCollider.name} for {duration}s");
            }
        }
    }
    
    protected virtual CardThrower FindCardThrower()
    {
        // Try to find on player first
        if (PlayerController.Instance != null)
        {
            CardThrower thrower = PlayerController.Instance.GetComponent<CardThrower>();
            if (thrower != null)
            {
                return thrower;
            }
        }
        
        // As fallback, look for any CardThrower in scene
        CardThrower[] throwers = Object.FindObjectsOfType<CardThrower>();
        if (throwers.Length > 0)
        {
            return throwers[0];
        }
        
        return null;
    }
    
    protected virtual void ApplyHoming()
    {
        // Find target if we don't have one
        if (currentTarget == null)
        {
            FindNearestTarget();
        }
        
        if (currentTarget != null)
        {
            // Calculate direction to target
            Vector2 directionToTarget = (currentTarget.position - transform.position).normalized;
            
            // Lerp our current direction toward target direction
            float homingStrength = stats.homingStrength.GetValue() * Time.deltaTime;
            direction = Vector2.Lerp(direction.normalized, directionToTarget, homingStrength);
            
            // Update rotation to match new direction (now accounting for Y-axis orientation)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    protected virtual void FindNearestTarget()
    {
        // Find all enemies within range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 10f, LayerMask.GetMask("Enemy"));
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;
        
        foreach (Collider2D col in colliders)
        {
            float distance = Vector2.Distance(transform.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = col.transform;
            }
        }
        
        currentTarget = closestTarget;
    }
    
    public virtual int GetDamage()
    {
        // Check for critical hit
        isCriticalHit = Random.value < stats.criticalChance.GetValue();
        int damage = Mathf.RoundToInt(stats.damage.GetValue());
        
        if (isCriticalHit)
        {
            damage *= 2;
        }
        
        return damage;
    }
    
    // Apply damage in radius
    protected virtual void ApplyDamageInRadius(Vector2 position, float radius, int damage)
    {
        // Debug sphere to visualize the damage radius
        Debug.DrawRay(position, Vector3.up * radius, Color.red, 1.0f);
        Debug.DrawRay(position, Vector3.right * radius, Color.red, 1.0f);
        Debug.DrawRay(position, Vector3.down * radius, Color.red, 1.0f);
        Debug.DrawRay(position, Vector3.left * radius, Color.red, 1.0f);
        
        // Check all colliders in radius (not just enemies in case layer is wrong)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius);
        Debug.Log($"Found {colliders.Length} colliders in splash radius of {radius}");
        
        int hitCount = 0;
        foreach (Collider2D col in colliders)
        {
            // Try to apply damage to anything with health
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                hitCount++;
                Debug.Log($"Applied {damage} splash damage to {col.gameObject.name}");
            }
        }
        
        if (hitCount == 0)
        {
            Debug.LogWarning("No enemies found in splash radius. Ensure enemies have EnemyHealth component and colliders.");
        }
    }
    
    // New method to play collision sound
    protected virtual void PlayCollisionSound()
    {
        if (collisionSounds != null && collisionSounds.Length > 0)
        {
            // Check for null entries
            bool hasValidSounds = false;
            foreach (AudioClip clip in collisionSounds)
            {
                if (clip != null)
                {
                    hasValidSounds = true;
                    break;
                }
            }
            
            if (hasValidSounds)
            {
                // Get a random sound that isn't null
                AudioClip soundToPlay = null;
                while (soundToPlay == null && hasValidSounds)
                {
                    int randomIndex = Random.Range(0, collisionSounds.Length);
                    soundToPlay = collisionSounds[randomIndex];
                    if (soundToPlay == null)
                    {
                        continue; // Try again if we got a null clip
                    }
                }
                
                if (soundToPlay != null)
                {
                    // EXTREME LOUD - create a temporary AudioSource for maximum volume
                    GameObject tempAudio = new GameObject("TempAudioSource");
                    tempAudio.transform.position = transform.position;
                    AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
                    tempSource.clip = soundToPlay;
                    tempSource.spatialBlend = 1.0f; // 3D sound
                    tempSource.volume = 6.0f; // Double the previous volume (3.0 -> 6.0)
                    tempSource.PlayOneShot(soundToPlay, 6.0f); // Double the previous volume (3.0 -> 6.0)
                    
                    Debug.Log($"Playing card collision sound: {soundToPlay.name} at EXTREME volume 6.0");
                    
                    // Destroy the temporary object after the sound finishes
                    Destroy(tempAudio, soundToPlay.length + 0.1f);
                }
                else
                {
                    Debug.LogWarning("All card collision sounds are null!");
                }
            }
            else
            {
                Debug.LogWarning("Card collision sounds array contains only null entries!");
            }
        }
        else
        {
            Debug.LogWarning("No card collision sounds assigned or passed to card!");
        }
    }
}
