using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection; // Add this namespace for reflection

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
    protected float damageModifier = 1f; // Add field for damage modifier
    
    // Fan shot splitting properties
    protected bool shouldSplit = false;
    protected bool hasSplit = false;
    protected float splitTime = 1.0f; // 1 second before splitting
    protected float timeSinceSpawn = 0f;
    protected float splitAngle = 15f;
    protected float splitDamageMultiplier = 0.7f;
    protected float splitSizeMultiplier = 0.5f; // Smaller cards (50% size)
    protected Vector3 startPosition;
    
    // Vampire properties
    protected bool hasVampire = false;
    protected float vampireHealPercent = 0.2f; // Heal 20% of damage dealt
    [SerializeField] protected GameObject vampireEffectPrefab; // Assign in inspector or default one will be created
    
    // Homing precision properties
    protected bool hasHoming = false;
    protected float homingStrength = 0.05f; // Reduced from 0.1f to 0.05f for very subtle tracking
    protected float homingRange = 5f;
    protected Transform currentTarget;
    protected bool isCriticalHit = false;
    
    // Chain lightning properties
    protected bool hasChainLightning = false;
    protected float chainLightningRange = 4f;
    protected float chainLightningDamageMultiplier = 0.6f;
    
    [SerializeField] protected bool useCollision = true;
    
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
    
    public void Initialize(CardType type, CardStats cardStats, GameObject impactEffect, Color color, AudioClip[] cardCollisionSounds = null, float damageModifier = 1f, bool shouldSplit = false, float splitAngle = 15f, float splitDamageMultiplier = 0.7f, bool hasVampire = false, float vampireHealPercent = 0.2f, bool hasHoming = false, float homingStrength = 0.05f, float homingRange = 5f, bool hasChainLightning = false, float chainLightningRange = 4f, float chainLightningDamageMultiplier = 0.6f)
    {
        cardType = type;
        stats = cardStats;
        effectPrefab = impactEffect;
        cardColor = color;
        collisionSounds = cardCollisionSounds;
        this.damageModifier = damageModifier; // Store the damage modifier
        
        // Initialize fan shot properties
        this.shouldSplit = shouldSplit;
        this.splitAngle = splitAngle;
        this.splitDamageMultiplier = splitDamageMultiplier;
        hasSplit = false;
        startPosition = transform.position;
        
        // Initialize vampire
        this.hasVampire = hasVampire;
        this.vampireHealPercent = vampireHealPercent;
        
        // Initialize homing precision
        this.hasHoming = hasHoming;
        this.homingStrength = homingStrength;
        this.homingRange = homingRange;
        
        // Initialize chain lightning
        this.hasChainLightning = hasChainLightning;
        this.chainLightningRange = chainLightningRange;
        this.chainLightningDamageMultiplier = chainLightningDamageMultiplier;
        
        // Apply color to sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.color = cardColor;
            Debug.Log($"Initialized card of type {type} with color {color}, damage modifier: {damageModifier}, shouldSplit: {shouldSplit}, hasVampire: {hasVampire}, hasHoming: {hasHoming}, hasChainLightning: {hasChainLightning}");
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
        
        // Record starting position
        startPosition = transform.position;
        
        // Debug log for split settings
        if (shouldSplit)
        {
            Debug.Log($"<color=yellow>Card {cardType} will split in {splitTime} seconds</color>");
        }
        
        // Find initial target if homing is enabled
        if (hasHoming)
        {
            FindNearestTarget();
        }
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
        
        // Apply homing if enabled
        if (hasHoming)
        {
            ApplyHoming();
        }
        
        // Move in the set direction at constant speed - using up vector (Y axis)
        float speed = stats.speed.GetValue();
        if (speed <= 0) speed = 10f; // Fallback speed
        
        transform.Translate(Vector2.up * speed * Time.deltaTime);
        
        // Fan shot splitting logic
        if (shouldSplit && !hasSplit)
        {
            timeSinceSpawn += Time.deltaTime;
            
            // Make the card flash intensely 
            if (spriteRenderer != null && timeSinceSpawn > 0.5f)
            {
                float flash = Mathf.Sin(timeSinceSpawn * 20f) * 0.5f + 0.5f;
                spriteRenderer.color = Color.Lerp(cardColor, Color.white, flash);
            }
            
            if (timeSinceSpawn >= splitTime)
            {
                CreateRealSplitCards();
                
                // Hide the original card immediately
                spriteRenderer.enabled = false;
                if (trailRenderer != null) trailRenderer.enabled = false;
                
                // Disable collision on the original card
                Collider2D col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
                
                // Disable damage source on original card
                DamageSource ds = GetComponent<DamageSource>();
                if (ds != null) ds.enabled = false;
                
                // Flag as split and destroy original
                hasSplit = true;
                Destroy(gameObject, 0.1f);
            }
        }
        
        // Debug movement
        Debug.DrawRay(transform.position, transform.up * 0.5f, Color.red);
    }
    
    // Create actual Card instances with full functionality
    private void CreateRealSplitCards()
    {
        Debug.Log($"Creating functional split cards for {cardType}");
        
        // Calculate 3 directions
        Vector2 forwardDir = direction.normalized;
        Vector2 leftDir = RotateVector(forwardDir, splitAngle);
        Vector2 rightDir = RotateVector(forwardDir, -splitAngle);
        
        // Add debug lines to see the directions
        Debug.DrawRay(transform.position, (Vector3)forwardDir * 3f, Color.yellow, 2f);
        Debug.DrawRay(transform.position, (Vector3)leftDir * 3f, Color.yellow, 2f);
        Debug.DrawRay(transform.position, (Vector3)rightDir * 3f, Color.yellow, 2f);
        
        // Find CardThrower to get the card prefab
        CardThrower cardThrower = FindCardThrower();
        if (cardThrower == null)
        {
            Debug.LogError("Could not find CardThrower for split cards!");
            return;
        }
        
        GameObject cardPrefab = cardThrower.GetCardPrefab();
        if (cardPrefab == null)
        {
            Debug.LogError("Could not get card prefab from CardThrower!");
            return;
        }
        
        // Create a visual flash when splitting
        GameObject flash = new GameObject("SplitFlash");
        flash.transform.position = transform.position;
        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
        sr.sprite = spriteRenderer.sprite;
        sr.color = Color.white;
        flash.transform.localScale = transform.localScale * 1.5f;
        Destroy(flash, 0.2f);
        
        // Create three actual Card instances
        CreateFunctionalSplitCard(cardPrefab, forwardDir);
        CreateFunctionalSplitCard(cardPrefab, leftDir);
        CreateFunctionalSplitCard(cardPrefab, rightDir);
    }
    
    // Create a split card that's fully functional with collision and damage
    private void CreateFunctionalSplitCard(GameObject cardPrefab, Vector2 direction)
    {
        // Calculate a small offset to prevent immediate collisions
        Vector3 spawnOffset = new Vector3(
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f),
            0
        );
        
        // Instantiate from the actual card prefab with offset position
        GameObject splitCard = Instantiate(cardPrefab, transform.position + spawnOffset, Quaternion.identity);
        
        // Make it smaller
        splitCard.transform.localScale = transform.localScale * splitSizeMultiplier;
        
        // Get the Card component
        Card cardComponent = splitCard.GetComponent<Card>();
        if (cardComponent == null)
        {
            Debug.LogError("Split card missing Card component!");
            Destroy(splitCard);
            return;
        }
        
        // Create a deep copy of stats to avoid null reference
        CardStats statsCopy = null;
        if (stats != null)
        {
            statsCopy = new CardStats();
            
            // Copy stats without using constructor that doesn't exist
            // Instead, create the CardStat and set baseValue directly
            if (stats.speed != null)
            {
                statsCopy.speed = new CardStat();
                statsCopy.speed.baseValue = stats.speed.baseValue;
            }
            
            if (stats.damage != null)
            {
                statsCopy.damage = new CardStat();
                statsCopy.damage.baseValue = stats.damage.baseValue;
            }
            
            if (stats.explosionRadius != null)
            {
                statsCopy.explosionRadius = new CardStat();
                statsCopy.explosionRadius.baseValue = stats.explosionRadius.baseValue;
            }
            
            if (stats.criticalChance != null)
            {
                statsCopy.criticalChance = new CardStat();
                statsCopy.criticalChance.baseValue = stats.criticalChance.baseValue;
            }
            
            if (stats.homingStrength != null)
            {
                statsCopy.homingStrength = new CardStat();
                statsCopy.homingStrength.baseValue = stats.homingStrength.baseValue;
            }
        }
        else
        {
            Debug.LogError("Original card stats are null when trying to create split card!");
            statsCopy = new CardStats(); // Create a default stats object to avoid null reference
        }
        
        // Initialize with copied stats and no further splitting
        cardComponent.Initialize(
            cardType,
            statsCopy, // Using the copy
            effectPrefab,
            cardColor,
            collisionSounds,
            damageModifier * splitDamageMultiplier,
            false,  // No further splitting
            splitAngle,
            splitDamageMultiplier,
            hasVampire, // Pass vampire flag to split cards
            vampireHealPercent, // Pass vampire heal percent to split cards
            hasHoming,
            homingStrength,
            homingRange
        );
        
        // Launch in the direction
        cardComponent.Launch(direction);
        
        // Ensure damage source is properly set up
        DamageSource damageSource = splitCard.GetComponent<DamageSource>();
        if (damageSource != null)
        {
            // Make sure damage source is enabled
            damageSource.enabled = true;
            
            // Don't try to access protected damageAmount field directly
            // The damage will be calculated through the Card's GetDamage method instead
            // when something collides with this card
            Debug.Log($"Split card damage source enabled, damage will be {Mathf.RoundToInt(GetDamage() * splitDamageMultiplier)}");
        }
        
        // Ensure the card has an active collider set as trigger
        Collider2D collider = splitCard.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            collider.enabled = true;
        }
        
        // Make sure useCollision is enabled
        cardComponent.SetUseCollision(true);
        
        // Start ignoring collisions with other cards
        StartCoroutine(IgnoreCardCollisions(splitCard));
        
        Debug.Log($"Created fully functional split card ({cardType}) with direction {direction}");
    }
    
    // Add coroutine to find and ignore collisions with other cards
    private IEnumerator IgnoreCardCollisions(GameObject cardObject)
    {
        // Wait for next frame to ensure other cards are fully initialized
        yield return null;
        
        // Find all cards in the scene
        Card[] allCards = GameObject.FindObjectsOfType<Card>();
        Collider2D cardCollider = cardObject.GetComponent<Collider2D>();
        
        // Ignore collisions between this card and all other cards
        if (cardCollider != null)
        {
            foreach (Card otherCard in allCards)
            {
                if (otherCard.gameObject != cardObject) // Don't ignore self
                {
                    Collider2D otherCollider = otherCard.GetComponent<Collider2D>();
                    if (otherCollider != null)
                    {
                        Physics2D.IgnoreCollision(cardCollider, otherCollider, true);
                    }
                }
            }
        }
    }
    
    // Reset collision handling to a simpler approach that's guaranteed to work
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Skip collisions with other cards, player, or parent
        if (other.GetComponent<Card>() != null || 
            other.gameObject.GetComponent<PlayerController>() != null || 
            other.transform == transform.parent)
        {
            return;
        }
        
        // Skip collisions with enemy projectiles
        Projectile enemyProjectile = other.GetComponent<Projectile>();
        if (enemyProjectile != null && enemyProjectile.GetIsEnemyProjectile())
        {
            return;
        }
        
        // Handle collision with everything else
        Debug.Log($"Card hit: {other.gameObject.name} (layer: {other.gameObject.layer})");
        OnHit(other.gameObject);
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
        
        // Apply card special effect based on type
        ApplySpecialEffect(target);
        
        // Check if we have vampire effect
        if (hasVampire)
        {
            // Get damage value
            int damageValue = GetDamage();
            
            // Apply vampire healing
            ApplyVampire(transform.position, damageValue);
        }
        
        // Destroy the card
        Destroy(gameObject);
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
        
        // Log hit details
        Debug.Log($"Card {cardType} applying special effect to {target.name} with damage: {damage}, radius: {radius}, hasChainLightning: {hasChainLightning}");
        
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
        
        // Apply chain lightning if upgrade is active
        if (hasChainLightning)
        {
            Debug.Log($"Applying chain lightning from Red card to {target.name}");
            ApplyChainLightning(transform.position, target, damage);
        }
    }
    
    protected virtual void ApplyGreenCardEffect(GameObject target, float radius, int damage)
    {
        // Green card - larger area damage (radius already affected by upgrade in CardThrower)
        ApplyDamageInRadius(transform.position, radius, damage);
        
        // Apply chain lightning if upgrade is active
        if (hasChainLightning)
        {
            Debug.Log($"Applying chain lightning from Green card to {target.name}");
            ApplyChainLightning(transform.position, target, damage);
        }
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
            
            // Prevent knockback for stunned enemies
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (Collider2D hitCollider in hitColliders)
            {
                EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    // Temporarily set knockback thrust to 0 for stunned enemies
                    enemyHealth.SetKnockbackThrust(0f);
                    StartCoroutine(ResetKnockbackAfterStun(enemyHealth));
                }
            }
        }
        
        // Apply chain lightning if upgrade is active
        if (hasChainLightning)
        {
            Debug.Log($"Applying chain lightning from Blue card to {target.name}");
            ApplyChainLightning(transform.position, target, damage);
        }
    }
    
    private IEnumerator ResetKnockbackAfterStun(EnemyHealth enemyHealth)
    {
        // Wait for the stun duration
        CardThrower cardThrower = FindCardThrower();
        if (cardThrower != null)
        {
            yield return new WaitForSeconds(cardThrower.GetBlueStunDuration());
            // Reset knockback thrust to default value
            enemyHealth.ResetKnockbackThrust();
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
        if (currentTarget == null)
        {
            // Try to find a new target
            FindNearestTarget();
            return;
        }
        
        // Check if target is still in range
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > homingRange)
        {
            currentTarget = null;
            return;
        }
        
        // Calculate direction to target
        Vector2 targetDirection = (currentTarget.position - transform.position).normalized;
        
        // Calculate angle between current direction and target
        float angle = Vector2.SignedAngle(transform.up, targetDirection);
        
        // Only apply homing if the angle is small (card is already roughly aimed at target)
        if (Mathf.Abs(angle) < 30f)
        {
            // Apply very subtle rotation based on homing strength
            float rotation = angle * homingStrength;
            
            // Apply rotation
            transform.Rotate(Vector3.forward, rotation);
            
            // Update movement direction to match new rotation
            direction = transform.up;
        }
        
        // Debug visualization
        Debug.DrawRay(transform.position, targetDirection * 2f, Color.red);
        Debug.DrawRay(transform.position, transform.up * 2f, Color.green);
    }
    
    protected virtual void FindNearestTarget()
    {
        // Find all enemies in range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, homingRange);
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;
        
        foreach (Collider2D collider in colliders)
        {
            // Check if it's an enemy by looking for EnemyHealth component
            EnemyHealth enemyHealth = collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = collider.transform;
                }
            }
        }
        
        currentTarget = closestTarget;
    }
    
    public virtual int GetDamage()
    {
        // Check for critical hit
        isCriticalHit = Random.value < stats.criticalChance.GetValue();
        
        // Start with base damage from stats
        float baseDamage = stats.damage.GetValue();
        
        // Apply damage modifier (used by fan shot)
        float modifiedDamage = baseDamage * damageModifier;
        
        // Round to nearest integer
        int damage = Mathf.RoundToInt(modifiedDamage);
        
        // Apply critical hit multiplier if applicable
        if (isCriticalHit)
        {
            damage *= 2;
        }
        
        // Ensure minimum damage of 1
        damage = Mathf.Max(1, damage);
        
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
        int totalDamageDealt = 0;
        foreach (Collider2D col in colliders)
        {
            // Try to apply damage to anything with health
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                hitCount++;
                totalDamageDealt += damage;
                Debug.Log($"Applied {damage} splash damage to {col.gameObject.name}");
            }
        }
        
        // Apply vampire if we dealt damage and have the upgrade
        if (hasVampire && totalDamageDealt > 0)
        {
            ApplyVampire(position, totalDamageDealt);
        }
        
        if (hitCount == 0)
        {
            Debug.LogWarning("No enemies found in splash radius. Ensure enemies have EnemyHealth component and colliders.");
        }
    }
    
    // New method to apply vampire effect
    protected virtual void ApplyVampire(Vector2 position, int damageDealt)
    {
        // Calculate healing amount (20% of damage dealt)
        int healAmount = Mathf.RoundToInt(damageDealt * vampireHealPercent);
        if (healAmount <= 0) healAmount = 1; // Minimum 1 healing
        
        Debug.Log($"<color=green>VAMPIRE EFFECT TRIGGERED:</color> Attempting to heal for {healAmount} from {damageDealt} damage");
        
        // Try to heal the player
        PlayerHealth playerHealth = FindPlayerHealth();
        if (playerHealth != null)
        {
            // DIRECT APPROACH for the actual game's PlayerHealth
            // Call HealPlayer() repeatedly based on heal amount
            // This matches how the game's PlayerHealth.HealPlayer() works (always heals by 1)
            for (int i = 0; i < healAmount; i++)
            {
                playerHealth.HealPlayer();
            }
            Debug.Log($"<color=green>VAMPIRE HEAL SUCCESS:</color> Called HealPlayer() {healAmount} times");
            
            // Show visual effect
            CreateVampireEffect(position, healAmount);
        }
        else
        {
            Debug.LogWarning("Cannot apply vampire: PlayerHealth component not found!");
            
            // Try all other approaches as fallback
            bool healed = TryAllHealingMethods(healAmount, position);
            
            if (!healed)
            {
                Debug.LogError($"<color=red>VAMPIRE HEAL FAILED:</color> Couldn't heal player by any method!");
            }
        }
    }
    
    // Fallback method that tries all healing approaches
    private bool TryAllHealingMethods(int healAmount, Vector2 position)
    {
        bool healed = false;
        
        // Try SendMessage approach first
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.gameObject.SendMessage("HealPlayer", SendMessageOptions.DontRequireReceiver);
            PlayerController.Instance.gameObject.SendMessage("Heal", healAmount, SendMessageOptions.DontRequireReceiver);
            PlayerController.Instance.gameObject.SendMessage("AddHealth", healAmount, SendMessageOptions.DontRequireReceiver);
            
            // We don't know if the SendMessage succeeded, so we'll try the other methods too
        }
        
        // Try reflection approaches
        healed = TryReflectionHealing(healAmount);
        
        // Show effect even if healing failed
        CreateVampireEffect(position, healAmount);
        
        return healed;
    }
    
    // Try healing using reflection (accessing methods and fields)
    private bool TryReflectionHealing(int healAmount)
    {
        bool healed = false;
        
        // Get PlayerHealth component if we can find it
        PlayerHealth playerHealth = FindPlayerHealth();
        if (playerHealth != null)
        {
            // Try several common method names
            string[] healMethodNames = new string[] { "Heal", "HealPlayer", "AddHealth", "RestoreHealth", "GainHealth" };
            foreach (string methodName in healMethodNames)
            {
                try
                {
                    System.Reflection.MethodInfo method = playerHealth.GetType().GetMethod(methodName);
                    if (method != null)
                    {
                        // Check parameter count
                        var parameters = method.GetParameters();
                        if (parameters.Length == 0)
                        {
                            // Call without parameters multiple times
                            for (int i = 0; i < healAmount; i++)
                            {
                                method.Invoke(playerHealth, null);
                            }
                        }
                        else if (parameters.Length == 1)
                        {
                            // Call with healAmount parameter
                            method.Invoke(playerHealth, new object[] { healAmount });
                        }
                        
                        healed = true;
                        Debug.Log($"<color=green>VAMPIRE HEAL:</color> Healed player using method {methodName}");
                        break;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Error trying to call {methodName}: {e.Message}");
                }
            }
            
            // If no method worked, try to access health field directly
            if (!healed)
            {
                healed = TryModifyHealthField(playerHealth, healAmount);
            }
        }
        
        return healed;
    }
    
    // Try modifying health fields directly
    private bool TryModifyHealthField(PlayerHealth playerHealth, int healAmount)
    {
        bool healed = false;
        
        try
        {
            // Try to find health field (common names)
            string[] healthFieldNames = new string[] { "currentHealth", "health", "playerHealth", "healthPoints", "hp" };
            foreach (string fieldName in healthFieldNames)
            {
                System.Reflection.FieldInfo field = playerHealth.GetType().GetField(fieldName, 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    // Get current value
                    int currentHealth = (int)field.GetValue(playerHealth);
                    // Set new value
                    field.SetValue(playerHealth, currentHealth + healAmount);
                    healed = true;
                    Debug.Log($"<color=green>VAMPIRE HEAL:</color> Healed player for {healAmount} by directly modifying {fieldName}");
                    break;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error trying to modify health field directly: {e.Message}");
        }
        
        return healed;
    }
    
    // Helper method to find player health
    protected virtual PlayerHealth FindPlayerHealth()
    {
        // Try to find through PlayerController first if it exists
        if (PlayerController.Instance != null)
        {
            PlayerHealth health = PlayerController.Instance.GetComponent<PlayerHealth>();
            if (health != null)
            {
                return health;
            }
        }
        
        // Fallback: look for any PlayerHealth in the scene
        return Object.FindObjectOfType<PlayerHealth>();
    }
    
    // Create visual effect for vampire
    protected virtual void CreateVampireEffect(Vector2 position, int healAmount)
    {
        // If no vampire effect prefab assigned, create a sprite-based effect
        if (vampireEffectPrefab == null)
        {
            // Create main effect object
            GameObject effect = new GameObject("VampireEffect");
            effect.transform.position = position;
            
            // Create pixelated blood droplets
            for (int i = 0; i < 5; i++) // Reduced number of droplets
            {
                GameObject droplet = new GameObject("BloodDroplet");
                droplet.transform.SetParent(effect.transform);
                droplet.transform.position = position;
                
                // Add sprite renderer
                SpriteRenderer dropletRenderer = droplet.AddComponent<SpriteRenderer>();
                
                // Create a pixelated blood splatter - use a much smaller texture for pixelation
                Texture2D dropletTexture = new Texture2D(8, 8);
                
                // Force the texture to be point-filtered (no interpolation) for pixelated look
                dropletTexture.filterMode = FilterMode.Point;
                
                // Create various pixel art blood drop shapes
                int dropShape = Random.Range(0, 3); // Three different drop shapes
                Color bloodRed = new Color(0.7f, 0f, 0f, 1f); // Darker red
                Color darkRed = new Color(0.5f, 0f, 0f, 1f); // Even darker red for shading
                
                // Clear the texture first (transparent)
                for (int y = 0; y < dropletTexture.height; y++)
                {
                    for (int x = 0; x < dropletTexture.width; x++)
                    {
                        dropletTexture.SetPixel(x, y, new Color(0, 0, 0, 0));
                    }
                }
                
                // Create pixel art blood shapes
                switch (dropShape)
                {
                    case 0: // Small rounded drop
                        dropletTexture.SetPixel(3, 5, bloodRed);
                        dropletTexture.SetPixel(4, 5, bloodRed);
                        dropletTexture.SetPixel(2, 4, bloodRed);
                        dropletTexture.SetPixel(3, 4, bloodRed);
                        dropletTexture.SetPixel(4, 4, bloodRed);
                        dropletTexture.SetPixel(5, 4, bloodRed);
                        dropletTexture.SetPixel(3, 3, bloodRed);
                        dropletTexture.SetPixel(4, 3, bloodRed);
                        // Add shading pixels
                        dropletTexture.SetPixel(3, 2, darkRed);
                        break;
                        
                    case 1: // Splatter shape
                        dropletTexture.SetPixel(4, 6, bloodRed);
                        dropletTexture.SetPixel(1, 5, bloodRed);
                        dropletTexture.SetPixel(3, 5, bloodRed);
                        dropletTexture.SetPixel(4, 5, bloodRed);
                        dropletTexture.SetPixel(2, 4, bloodRed);
                        dropletTexture.SetPixel(3, 4, bloodRed);
                        dropletTexture.SetPixel(4, 4, bloodRed);
                        dropletTexture.SetPixel(5, 4, bloodRed);
                        dropletTexture.SetPixel(6, 4, bloodRed);
                        dropletTexture.SetPixel(3, 3, bloodRed);
                        dropletTexture.SetPixel(4, 3, bloodRed);
                        dropletTexture.SetPixel(2, 2, bloodRed);
                        dropletTexture.SetPixel(5, 2, darkRed);
                        break;
                        
                    case 2: // Classic droplet shape
                        dropletTexture.SetPixel(3, 6, bloodRed);
                        dropletTexture.SetPixel(4, 6, bloodRed);
                        dropletTexture.SetPixel(3, 5, bloodRed);
                        dropletTexture.SetPixel(4, 5, bloodRed);
                        dropletTexture.SetPixel(2, 4, bloodRed);
                        dropletTexture.SetPixel(3, 4, bloodRed);
                        dropletTexture.SetPixel(4, 4, bloodRed);
                        dropletTexture.SetPixel(5, 4, bloodRed);
                        dropletTexture.SetPixel(2, 3, bloodRed);
                        dropletTexture.SetPixel(3, 3, bloodRed);
                        dropletTexture.SetPixel(4, 3, bloodRed);
                        dropletTexture.SetPixel(5, 3, bloodRed);
                        dropletTexture.SetPixel(3, 2, bloodRed);
                        dropletTexture.SetPixel(4, 2, bloodRed);
                        dropletTexture.SetPixel(3, 1, darkRed);
                        dropletTexture.SetPixel(4, 1, darkRed);
                        break;
                }
                
                dropletTexture.Apply();
                
                // Create sprite from texture with point filtering for crisp pixel look
                Sprite dropletSprite = Sprite.Create(
                    dropletTexture, 
                    new Rect(0, 0, dropletTexture.width, dropletTexture.height), 
                    new Vector2(0.5f, 0.5f),
                    8f // Pixelated sprites need higher pixels per unit
                );
                
                dropletRenderer.sprite = dropletSprite;
                // Dark red blood
                dropletRenderer.color = new Color(0.7f, 0.0f, 0.0f, 1.0f);
                dropletRenderer.sortingOrder = 10; // Make sure it's visible on top
                
                // Make droplets much smaller - about 1/3 the size
                float randomScale = Random.Range(0.7f, 0.9f);
                droplet.transform.localScale = new Vector3(randomScale, randomScale, 1f);
                
                // Add a pixelated trail effect
                TrailRenderer trail = droplet.AddComponent<TrailRenderer>();
                trail.startWidth = 0.2f; // Thinner trail
                trail.endWidth = 0.0f;
                trail.time = 0.15f; // Shorter trail
                trail.material = new Material(Shader.Find("Sprites/Default"));
                trail.material.mainTexture = dropletTexture;
                
                // Set trail color
                Gradient trailGradient = new Gradient();
                trailGradient.SetKeys(
                    new GradientColorKey[] { 
                        new GradientColorKey(new Color(0.7f, 0.0f, 0.0f), 0.0f),
                        new GradientColorKey(new Color(0.5f, 0.0f, 0.0f), 1.0f) 
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0.8f, 0.0f),
                        new GradientAlphaKey(0.0f, 1.0f)
                    }
                );
                trail.colorGradient = trailGradient;
                trail.sortingOrder = 9; // Behind the droplet
                
                // Random direction for initial burst
                float angle = Random.Range(0, 360);
                float speed = Random.Range(1.5f, 3.0f); // Slower for smoother movement
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                
                // Store direction in a component to use in animation
                BloodDroplet dropletComponent = droplet.AddComponent<BloodDroplet>();
                dropletComponent.Initialize(direction, speed, Random.Range(0.8f, 1.0f));
            }
            
            // Create vampire rune (central effect) - pixelated as well
            GameObject runeEffect = new GameObject("VampireRune");
            runeEffect.transform.SetParent(effect.transform);
            runeEffect.transform.localPosition = Vector3.zero;
            
            // Add sprite renderer for the rune
            SpriteRenderer sr = runeEffect.AddComponent<SpriteRenderer>();
            
            // Create a pixelated pentagram/rune (16x16 for more detail but still pixelated)
            Texture2D runeTexture = new Texture2D(16, 16);
            runeTexture.filterMode = FilterMode.Point; // Pixelated look
            
            // Clear the texture first
            for (int y = 0; y < runeTexture.height; y++)
            {
                for (int x = 0; x < runeTexture.width; x++)
                {
                    runeTexture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
            
            // Draw a pixelated pentagram/rune shape (dark red with bright red outline)
            Color brightRed = new Color(0.8f, 0.0f, 0.0f, 0.9f);
            Color innerRed = new Color(0.6f, 0.0f, 0.0f, 0.7f);
            
            // Outer circle
            runeTexture.SetPixel(7, 15, brightRed);
            runeTexture.SetPixel(8, 15, brightRed);
            runeTexture.SetPixel(5, 14, brightRed);
            runeTexture.SetPixel(6, 14, brightRed);
            runeTexture.SetPixel(9, 14, brightRed);
            runeTexture.SetPixel(10, 14, brightRed);
            runeTexture.SetPixel(4, 13, brightRed);
            runeTexture.SetPixel(11, 13, brightRed);
            runeTexture.SetPixel(3, 12, brightRed);
            runeTexture.SetPixel(12, 12, brightRed);
            runeTexture.SetPixel(2, 11, brightRed);
            runeTexture.SetPixel(13, 11, brightRed);
            runeTexture.SetPixel(2, 10, brightRed);
            runeTexture.SetPixel(13, 10, brightRed);
            runeTexture.SetPixel(1, 9, brightRed);
            runeTexture.SetPixel(14, 9, brightRed);
            runeTexture.SetPixel(1, 8, brightRed);
            runeTexture.SetPixel(14, 8, brightRed);
            runeTexture.SetPixel(1, 7, brightRed);
            runeTexture.SetPixel(14, 7, brightRed);
            runeTexture.SetPixel(1, 6, brightRed);
            runeTexture.SetPixel(14, 6, brightRed);
            runeTexture.SetPixel(2, 5, brightRed);
            runeTexture.SetPixel(13, 5, brightRed);
            runeTexture.SetPixel(2, 4, brightRed);
            runeTexture.SetPixel(13, 4, brightRed);
            runeTexture.SetPixel(3, 3, brightRed);
            runeTexture.SetPixel(12, 3, brightRed);
            runeTexture.SetPixel(4, 2, brightRed);
            runeTexture.SetPixel(11, 2, brightRed);
            runeTexture.SetPixel(5, 1, brightRed);
            runeTexture.SetPixel(6, 1, brightRed);
            runeTexture.SetPixel(9, 1, brightRed);
            runeTexture.SetPixel(10, 1, brightRed);
            runeTexture.SetPixel(7, 0, brightRed);
            runeTexture.SetPixel(8, 0, brightRed);
            
            // Inner star/pentagram shape
            runeTexture.SetPixel(7, 12, innerRed);
            runeTexture.SetPixel(8, 12, innerRed);
            runeTexture.SetPixel(6, 11, innerRed);
            runeTexture.SetPixel(9, 11, innerRed);
            runeTexture.SetPixel(5, 10, innerRed);
            runeTexture.SetPixel(10, 10, innerRed);
            runeTexture.SetPixel(4, 9, innerRed);
            runeTexture.SetPixel(11, 9, innerRed);
            runeTexture.SetPixel(6, 9, innerRed);
            runeTexture.SetPixel(9, 9, innerRed);
            runeTexture.SetPixel(7, 8, innerRed);
            runeTexture.SetPixel(8, 8, innerRed);
            runeTexture.SetPixel(6, 7, innerRed);
            runeTexture.SetPixel(9, 7, innerRed);
            runeTexture.SetPixel(4, 7, innerRed);
            runeTexture.SetPixel(11, 7, innerRed);
            runeTexture.SetPixel(5, 6, innerRed);
            runeTexture.SetPixel(10, 6, innerRed);
            runeTexture.SetPixel(6, 5, innerRed);
            runeTexture.SetPixel(9, 5, innerRed);
            runeTexture.SetPixel(7, 4, innerRed);
            runeTexture.SetPixel(8, 4, innerRed);
            
            runeTexture.Apply();
            
            // Create sprite from texture
            Sprite runeSprite = Sprite.Create(
                runeTexture, 
                new Rect(0, 0, runeTexture.width, runeTexture.height), 
                new Vector2(0.5f, 0.5f),
                16f // Higher pixels per unit for pixel art
            );
            
            sr.sprite = runeSprite;
            // Darker red
            sr.color = new Color(0.7f, 0.0f, 0.0f, 0.9f);
            sr.sortingOrder = 11; // Above the droplets
            
            // Make rune smaller but keep pixelated look - about 1/3 the size
            runeEffect.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            
            // Start the animation and flight to player
            StartCoroutine(AnimateVampireEffect(effect, runeEffect));
        }
        else
        {
            // Use the assigned prefab
            GameObject effect = Instantiate(vampireEffectPrefab, position, Quaternion.identity);
            
            // Auto-destroy after 2 seconds if it doesn't destroy itself
            Destroy(effect, 2f);
        }
    }
    
    // Helper class for blood droplet animation
    private class BloodDroplet : MonoBehaviour
    {
        private Vector2 direction;
        private float speed;
        private float lifetime;
        private float elapsedTime = 0f;
        private Vector3 startPosition;
        
        public void Initialize(Vector2 direction, float speed, float lifetime)
        {
            this.direction = direction;
            this.speed = speed;
            this.lifetime = lifetime;
            this.startPosition = transform.position;
        }
        
        private void Update()
        {
            // Update time
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / lifetime;
            
            // Initial burst movement with smoother slowdown
            if (elapsedTime < 0.3f)
            {
                float deceleration = Mathf.SmoothStep(1f, 0.2f, elapsedTime / 0.3f);
                transform.position += (Vector3)(direction * speed * deceleration * Time.deltaTime);
            }
            // After initial burst, move toward player with smooth acceleration
            else if (PlayerController.Instance != null)
            {
                Vector3 playerPos = PlayerController.Instance.transform.position;
                float moveProgress = Mathf.SmoothStep(0f, 1f, (normalizedTime - 0.3f) / 0.7f);
                transform.position = Vector3.Lerp(
                    transform.position,
                    playerPos,
                    moveProgress * 0.15f // Smoother movement to player
                );
            }
            
            // Smoother fade out
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color color = sr.color;
                color.a = Mathf.SmoothStep(1f, 0f, normalizedTime);
                sr.color = color;
            }
            
            // Destroy when lifetime is up
            if (elapsedTime >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
    
    // Animate the vampire effect
    protected virtual IEnumerator AnimateVampireEffect(GameObject effect, GameObject runeEffect = null)
    {
        float duration = 1.2f; // Slightly shorter duration
        float startTime = Time.time;
        Vector3 startPos = effect.transform.position;
        
        // Find the player position to move toward
        Vector3 playerPos = Vector3.zero;
        if (PlayerController.Instance != null)
        {
            playerPos = PlayerController.Instance.transform.position;
        }
        
        while (Time.time < startTime + duration)
        {
            float elapsed = Time.time - startTime;
            float normalizedTime = elapsed / duration;
            
            // Stay in place for first 0.2 seconds, then move toward player
            if (normalizedTime > 0.2f && playerPos != Vector3.zero)
            {
                float moveTime = (normalizedTime - 0.2f) / 0.8f; // 0 to 1 during remaining time
                // Use SmoothStep for smoother acceleration
                moveTime = Mathf.SmoothStep(0f, 1f, moveTime);
                effect.transform.position = Vector3.Lerp(
                    startPos,
                    playerPos,
                    moveTime
                );
            }
            
            // Rune effect animation
            if (runeEffect != null)
            {
                // Rotate the rune - smoother rotation
                runeEffect.transform.Rotate(0, 0, 180f * Time.deltaTime);
                
                // Smooth pulsing
                float pulseFactor = Mathf.Sin(normalizedTime * Mathf.PI * 4);
                float scale = 0.6f + pulseFactor * pulseFactor * 0.1f; // Smaller pulse
                runeEffect.transform.localScale = new Vector3(scale, scale, 1f);
                
                // Fade in/out
                SpriteRenderer sr = runeEffect.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color color = sr.color;
                    
                    // Smoother fade in/out
                    if (normalizedTime < 0.2f)
                    {
                        color.a = Mathf.SmoothStep(0f, 0.9f, normalizedTime / 0.2f);
                    }
                    else
                    {
                        color.a = Mathf.SmoothStep(0.9f, 0f, (normalizedTime - 0.2f) / 0.8f);
                    }
                    
                    sr.color = color;
                }
            }
            
            yield return null;
        }
        
        // Destroy the effect at the end
        Destroy(effect);
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
    
    // Helper function to rotate a vector
    protected Vector2 RotateVector(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        
        float tx = v.x;
        float ty = v.y;
        
        return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
    }
    
    // Add back the public method to set the useCollision flag
    public void SetUseCollision(bool useCollision)
    {
        this.useCollision = useCollision;
    }
    
    // Chain Lightning implementation
    protected virtual void ApplyChainLightning(Vector2 position, GameObject primaryTarget, int baseDamage)
    {
        // Skip if no primary target or no chain lightning
        if (primaryTarget == null || !hasChainLightning)
        {
            Debug.LogWarning("Cannot apply chain lightning: Primary target is null or chain lightning is disabled");
            return;
        }
        
        // Ensure primary target is an enemy
        EnemyHealth primaryEnemyHealth = primaryTarget.GetComponent<EnemyHealth>();
        if (primaryEnemyHealth == null)
        {
            Debug.LogWarning($"Chain lightning primary target {primaryTarget.name} is not an enemy (no EnemyHealth component)");
            return;
        }
        
        // The collision position (where the card hits) should be the origin of lightning
        Vector2 collisionPoint = position;
        
        Debug.Log($"Applying chain lightning from collision point {collisionPoint} with 10m range and damage multiplier {chainLightningDamageMultiplier}");
        
        // Calculate chain lightning damage (60% of original by default)
        int chainDamage = Mathf.RoundToInt(baseDamage * chainLightningDamageMultiplier);
        
        // DEBUG: Visual to show chain lightning range (10 meters)
        float visualRange = 10f;
        Debug.DrawRay(collisionPoint, Vector3.up * visualRange, Color.blue, 2.0f);
        Debug.DrawRay(collisionPoint, Vector3.right * visualRange, Color.blue, 2.0f);
        Debug.DrawRay(collisionPoint, Vector3.down * visualRange, Color.blue, 2.0f);
        Debug.DrawRay(collisionPoint, Vector3.left * visualRange, Color.blue, 2.0f);
        
        // Find all enemies in range except the primary target
        int hitCount = 0;
        int totalChainDamage = 0;
        
        // Get all enemies in the scene and filter by range
        EnemyHealth[] allEnemies = GameObject.FindObjectsOfType<EnemyHealth>();
        Debug.Log($"Found {allEnemies.Length} enemies in the scene for chain lightning");
        
        // Create source glow effect at the collision point
        CreateSourceGlowEffect(collisionPoint);
        
        // Create a smaller lightning explosion at the collision point
        CreateMassiveLightningExplosion(collisionPoint, 0.8f);
        
        foreach (EnemyHealth enemyHealth in allEnemies)
        {
            // Skip the primary target
            if (enemyHealth.gameObject == primaryTarget)
            {
                continue;
            }
            
            // Check if enemy is within range
            float distance = Vector2.Distance(collisionPoint, enemyHealth.transform.position);
            if (distance > visualRange)
            {
                continue;
            }
            
            // Apply damage directly to enemies in range
            Debug.Log($"Applying chain lightning to secondary target: {enemyHealth.gameObject.name}");
            
            // Apply the chain damage directly
            enemyHealth.TakeDamage(chainDamage);
            hitCount++;
            totalChainDamage += chainDamage;
            
            // Create lightning visual effect between collision point and secondary targets
            CreateLightningEffect(collisionPoint, enemyHealth.transform.position);
        }
        
        if (hitCount > 0)
        {
            Debug.Log($"Chain lightning hit {hitCount} enemies for {totalChainDamage} total damage");
            
            // Play chain lightning sound if available
            PlayChainLightningSound();
            
            // Apply vampire if we have the upgrade and dealt damage
            if (hasVampire && totalChainDamage > 0)
            {
                ApplyVampire(position, totalChainDamage);
            }
        }
        else
        {
            Debug.Log("No secondary targets found for chain lightning");
        }
    }
    
    // Create a visual lightning effect between two points
    protected virtual void CreateLightningEffect(Vector2 startPoint, Vector2 endPoint)
    {
        // Create lightning parent object
        GameObject lightningEffect = new GameObject("LightningEffect");
        lightningEffect.transform.position = startPoint;
        
        // Create a line renderer for the lightning
        LineRenderer lightning = lightningEffect.AddComponent<LineRenderer>();
        if (lightning == null)
        {
            Debug.LogError("Failed to add LineRenderer component");
            Destroy(lightningEffect);
            return;
        }
        
        lightning.positionCount = 6; // Reduced segments for less jagged effect
        
        // Calculate the distance and direction
        float distance = Vector2.Distance(startPoint, endPoint);
        Vector2 direction = (endPoint - startPoint).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        
        // Set width based on distance - thinner for less intense look
        float width = Mathf.Clamp(distance * 0.08f, 0.05f, 0.15f);
        lightning.startWidth = width;
        lightning.endWidth = width * 0.5f;
        
        // Set material and color based on card type
        lightning.material = new Material(Shader.Find("Sprites/Default"));
        
        // Use a softer blue color
        Color lightningColor = new Color(0.3f, 0.5f, 0.8f, 0.8f);
        
        // Add white core to the lightning
        lightning.startColor = new Color(1f, 1f, 1f, 0.6f);
        lightning.endColor = lightningColor;
        
        // Set positions to create a less jagged lightning bolt effect
        lightning.SetPosition(0, startPoint);
        lightning.SetPosition(lightning.positionCount - 1, endPoint);
        
        // Create intermediate points with less jitter
        for (int i = 1; i < lightning.positionCount - 1; i++)
        {
            float t = (float)i / (lightning.positionCount - 1);
            // Calculate base point along the line
            Vector2 basePoint = Vector2.Lerp(startPoint, endPoint, t);
            
            // Add smaller random displacement perpendicular to the line
            float jitter = Mathf.PerlinNoise(t * 5, Time.time) * distance * 0.08f - (distance * 0.04f);
            Vector2 offset = perpendicular * jitter;
            
            // Set the position
            lightning.SetPosition(i, basePoint + offset);
        }
        
        // Set sorting layer to be above the game objects
        lightning.sortingOrder = 10;
        
        // Add a subtle glow effect
        try
        {
            LineRenderer glow = CreateGlowEffect(lightningEffect, lightning);
            
            // Add a fade-out script to the lightning effect
            LightningFadeOut fadeOut = lightningEffect.AddComponent<LightningFadeOut>();
            if (fadeOut != null)
            {
                fadeOut.Initialize(0.3f, lightning, glow); // Shorter duration for less visual clutter
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating glow effect: {e.Message}");
            // Continue even if glow creation fails
        }
        
        // Add subtle particles for less visual impact
        try
        {
            AddLightningParticles(lightningEffect, startPoint, endPoint);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error adding particles: {e.Message}");
            // Continue even if particle creation fails
        }
    }
    
    private LineRenderer CreateGlowEffect(GameObject parent, LineRenderer sourceLightning)
    {
        if (parent == null || sourceLightning == null)
        {
            Debug.LogError("Cannot create glow effect: parent or sourceLightning is null");
            return null;
        }

        // Create a new GameObject for the glow
        GameObject glowObject = new GameObject("LightningGlow");
        glowObject.transform.SetParent(parent.transform);
        glowObject.transform.localPosition = Vector3.zero;

        // Add LineRenderer component
        LineRenderer glow = null;
        try
        {
            glow = glowObject.AddComponent<LineRenderer>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to add LineRenderer component: {e.Message}");
            Destroy(glowObject);
            return null;
        }

        if (glow == null)
        {
            Debug.LogError("LineRenderer component is null after creation");
            Destroy(glowObject);
            return null;
        }

        // Copy basic properties from source lightning
        try
        {
            glow.positionCount = sourceLightning.positionCount;
            glow.startWidth = sourceLightning.startWidth * 1.5f;
            glow.endWidth = sourceLightning.endWidth * 1.5f;
            glow.sortingOrder = sourceLightning.sortingOrder - 1;

            // Set material and color
            Material glowMaterial = new Material(Shader.Find("Sprites/Default"));
            if (glowMaterial == null)
            {
                Debug.LogError("Failed to create glow material");
                Destroy(glowObject);
                return null;
            }

            glow.material = glowMaterial;
            Color glowColor = new Color(0.4f, 0.6f, 0.9f, 0.2f); // More subtle glow
            glow.startColor = glowColor;
            glow.endColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0.05f);

            // Copy positions from the source lightning
            Vector3[] positions = new Vector3[sourceLightning.positionCount];
            sourceLightning.GetPositions(positions);
            glow.SetPositions(positions);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting up glow properties: {e.Message}");
            Destroy(glowObject);
            return null;
        }

        return glow;
    }
    
    private void AddLightningParticles(GameObject parent, Vector2 startPoint, Vector2 endPoint)
    {
        // Create a particle system for additional flair
        GameObject particleObj = new GameObject("LightningParticles");
        particleObj.transform.SetParent(parent.transform);
        particleObj.transform.position = Vector2.Lerp(startPoint, endPoint, 0.5f);
        
        ParticleSystem particleSystem = particleObj.AddComponent<ParticleSystem>();
        
        // Configure particle system
        var main = particleSystem.main;
        main.startLifetime = 0.2f;
        main.startSpeed = 0.5f;
        main.startSize = 0.1f;
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Bright blue/white particles
        main.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(0.5f, 0.8f, 1f));
        
        // Emission settings
        var emission = particleSystem.emission;
        emission.rateOverTime = 30;
        emission.SetBursts(new ParticleSystem.Burst[] { 
            new ParticleSystem.Burst(0f, 20) 
        });
        
        // Shape settings - use a box shape instead of Line (which doesn't exist in this Unity version)
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        
        // Calculate direction and distance
        Vector2 direction = endPoint - startPoint;
        float distance = direction.magnitude;
        
        // Make a thin, long box aligned with the direction
        shape.scale = new Vector3(distance, 0.1f, 0.1f);
        
        // Rotate to align with the direction from start to end point
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        shape.rotation = new Vector3(0, 0, angle);
        
        particleSystem.Play();
    }
    
    // Create a glow effect at the source point
    private void CreateSourceGlowEffect(Vector2 position)
    {
        // Create a sprite for the glow
        GameObject glowObj = new GameObject("ChainLightningSourceGlow");
        glowObj.transform.position = position;
        
        SpriteRenderer sprite = glowObj.AddComponent<SpriteRenderer>();
        sprite.sprite = Resources.Load<Sprite>("Circle"); // Use a circle sprite or create one if needed
        
        if (sprite.sprite == null)
        {
            Debug.LogWarning("Circle sprite not found, creating a particle effect instead");
            
            // If no sprite is available, use particles instead
            ParticleSystem particles = glowObj.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 0.4f;
            main.startSpeed = 0.1f;
            main.startSize = 0.5f;
            main.startColor = new Color(0.5f, 0.8f, 1f, 0.7f);
            
            var emission = particles.emission;
            emission.rateOverTime = 20;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;
            
            particles.Play();
            
            // Auto-destroy the particle system
            Destroy(glowObj, 0.4f);
        }
        else
        {
            // Configure the sprite
            sprite.color = new Color(0.5f, 0.8f, 1f, 0.7f); // Light blue with transparency
            sprite.sortingOrder = 5;
            glowObj.transform.localScale = Vector3.one * 0.7f;
            
            // Add a fade out script
            StartCoroutine(FadeOutGlow(glowObj, 0.4f));
        }
    }
    
    private IEnumerator FadeOutGlow(GameObject glowObj, float duration)
    {
        SpriteRenderer sprite = glowObj.GetComponent<SpriteRenderer>();
        float elapsed = 0;
        Color startColor = sprite.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            sprite.color = new Color(startColor.r, startColor.g, startColor.b, 
                Mathf.Lerp(startColor.a, 0, elapsed / duration));
            
            // Pulse the size
            float scale = 0.7f + (0.3f * Mathf.Sin(elapsed * 15f));
            glowObj.transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        Destroy(glowObj);
    }
    
    // Play chain lightning sound effect
    protected virtual void PlayChainLightningSound()
    {
        // Use existing collision sounds for now
        if (collisionSounds != null && collisionSounds.Length > 0)
        {
            AudioClip soundToPlay = null;
            foreach (AudioClip clip in collisionSounds)
            {
                if (clip != null)
                {
                    soundToPlay = clip;
                    break;
                }
            }
            
            if (soundToPlay != null)
            {
                // Create temporary audio source for the sound
                GameObject tempAudio = new GameObject("ChainLightningSoundSource");
                tempAudio.transform.position = transform.position;
                AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
                tempSource.clip = soundToPlay;
                tempSource.pitch = 1.5f; // Higher pitch for lightning sound
                tempSource.spatialBlend = 1.0f;
                tempSource.volume = 3.0f;
                tempSource.PlayOneShot(soundToPlay, 3.0f);
                
                // Destroy after playing
                Destroy(tempAudio, soundToPlay.length + 0.1f);
            }
        }
    }
    
    // Implementation for CreateMassiveLightningExplosion (replacing any duplicates)
    protected virtual void CreateMassiveLightningExplosion(Vector2 position, float radius)
    {
        // Create a particle system for a massive lightning explosion
        GameObject explosionObj = new GameObject("MassiveLightningExplosion");
        explosionObj.transform.position = position;
        
        // Add particle system
        ParticleSystem particles = explosionObj.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.startSize = 0.2f;
        main.maxParticles = 100;
        
        // Set bright blue color
        main.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(0.5f, 0.8f, 1f));
        
        // Emission bursts
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { 
            new ParticleSystem.Burst(0f, 50) 
        });
        
        // Shape - sphere
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radius * 0.5f;
        
        // Add light
        Light light = explosionObj.AddComponent<Light>();
        light.color = new Color(0.5f, 0.8f, 1f);
        light.intensity = 3f;
        light.range = radius * 2f;
        
        // Add fading script for the light
        StartCoroutine(FadeExplosionLight(light, 0.5f));
        
        // Play and auto-destroy
        particles.Play();
        Destroy(explosionObj, 1.0f);
    }
    
    // Implementation for CreatePersistentLightningConnection (replacing any duplicates)
    protected virtual void CreatePersistentLightningConnection(Vector2 startPoint, Vector2 endPoint, float duration)
    {
        GameObject connectionObj = new GameObject("PersistentLightningConnection");
        connectionObj.transform.position = startPoint;
        
        // Create line renderer
        LineRenderer line = connectionObj.AddComponent<LineRenderer>();
        line.positionCount = 8;
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        
        // Set material and color
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.white;
        line.endColor = new Color(0.5f, 0.8f, 1f);
        
        // Set positions with some jitter for lightning effect
        Vector2 direction = (endPoint - startPoint).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        float distance = Vector2.Distance(startPoint, endPoint);
        
        line.SetPosition(0, startPoint);
        line.SetPosition(line.positionCount - 1, endPoint);
        
        for (int i = 1; i < line.positionCount - 1; i++)
        {
            float t = (float)i / (line.positionCount - 1);
            Vector2 basePoint = Vector2.Lerp(startPoint, endPoint, t);
            float jitter = Mathf.PerlinNoise(t * 10, Time.time) * distance * 0.1f;
            Vector2 offset = perpendicular * jitter;
            line.SetPosition(i, basePoint + offset);
        }
        
        // Add jitter animation
        StartCoroutine(AnimateLightningConnection(line, startPoint, endPoint, duration));
        
        // Auto-destroy
        Destroy(connectionObj, duration + 0.1f);
    }
    
    // Coroutine for lightning connection animation
    private IEnumerator AnimateLightningConnection(LineRenderer line, Vector2 startPoint, Vector2 endPoint, float duration)
    {
        float elapsed = 0f;
        Vector2 direction = (endPoint - startPoint).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        float distance = Vector2.Distance(startPoint, endPoint);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            // Update all middle points with new jitter
            for (int i = 1; i < line.positionCount - 1; i++)
            {
                float t = (float)i / (line.positionCount - 1);
                Vector2 basePoint = Vector2.Lerp(startPoint, endPoint, t);
                float jitter = Mathf.PerlinNoise(t * 10 + elapsed * 3, Time.time) * distance * 0.1f;
                Vector2 offset = perpendicular * jitter;
                line.SetPosition(i, basePoint + offset);
            }
            
            yield return null;
        }
    }
    
    // Implementation for ContinuousSparkEffect (replacing any duplicates)
    private IEnumerator ContinuousSparkEffect(Vector3 position, float duration)
    {
        GameObject sparkObj = new GameObject("ContinuousSpark");
        sparkObj.transform.position = position;
        
        // Add particle system
        ParticleSystem particles = sparkObj.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 0.2f;
        main.startSpeed = 1f;
        main.startSize = 0.1f;
        main.maxParticles = 30;
        
        // Bright blue/white sparks
        main.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(0.5f, 0.8f, 1f));
        
        // Emission
        var emission = particles.emission;
        emission.rateOverTime = 20;
        
        // Shape
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        
        // Start system
        particles.Play();
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Stop emission and wait for particles to die
        var emissionModule = particles.emission;
        emissionModule.enabled = false;
        
        yield return new WaitForSeconds(main.startLifetime.constant);
        
        // Destroy
        Destroy(sparkObj);
    }
    
    // Helper for fading explosion light
    private IEnumerator FadeExplosionLight(Light light, float duration)
    {
        float initialIntensity = light.intensity;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            light.intensity = Mathf.Lerp(initialIntensity, 0f, elapsed / duration);
            yield return null;
        }
        
        light.intensity = 0f;
    }
}

// Helper class for lightning fade out
public class LightningFadeOut : MonoBehaviour
{
    private float duration;
    private float elapsed = 0;
    private LineRenderer lightning;
    private LineRenderer glow;
    
    public void Initialize(float duration, LineRenderer lightning, LineRenderer glow = null)
    {
        this.duration = duration;
        this.lightning = lightning;
        this.glow = glow;
    }
    
    private void Update()
    {
        elapsed += Time.deltaTime;
        
        if (elapsed >= duration)
        {
            Destroy(gameObject);
            return;
        }
        
        // Fade out the lightning
        if (lightning != null)
        {
            // Reduce alpha over time
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            lightning.startColor = new Color(lightning.startColor.r, lightning.startColor.g, lightning.startColor.b, alpha);
            lightning.endColor = new Color(lightning.endColor.r, lightning.endColor.g, lightning.endColor.b, alpha);
            
            // Make the lightning jagged and dynamic by shifting points
            if (lightning.positionCount > 2)
            {
                Vector3 start = lightning.GetPosition(0);
                Vector3 end = lightning.GetPosition(lightning.positionCount - 1);
                Vector3 dir = (end - start).normalized;
                Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0).normalized;
                
                for (int i = 1; i < lightning.positionCount - 1; i++)
                {
                    float t = (float)i / (lightning.positionCount - 1);
                    Vector3 basePoint = Vector3.Lerp(start, end, t);
                    
                    // Create dynamic jittering effect
                    float jitter = Mathf.Sin((elapsed * 20f) + (i * 1.5f)) * 0.1f;
                    jitter += Mathf.PerlinNoise(t * 10 + elapsed * 5, Time.time) * 0.15f - 0.075f;
                    
                    lightning.SetPosition(i, basePoint + perpendicular * jitter * Vector3.Distance(start, end));
                    
                    // Update glow positions to match
                    if (glow != null && i < glow.positionCount)
                    {
                        glow.SetPosition(i, basePoint + perpendicular * jitter * Vector3.Distance(start, end));
                    }
                }
            }
        }
        
        // Fade out the glow
        if (glow != null)
        {
            float glowAlpha = Mathf.Lerp(0.3f, 0f, elapsed / duration);
            glow.startColor = new Color(glow.startColor.r, glow.startColor.g, glow.startColor.b, glowAlpha);
            glow.endColor = new Color(glow.endColor.r, glow.endColor.g, glow.endColor.b, glowAlpha * 0.3f);
        }
    }
}
