using UnityEngine;

public class BountyEnemy : MonoBehaviour
{
    [Header("Bounty Settings")]
    [SerializeField] private string bountyName = "Wanted Criminal";
    [SerializeField] private int goldReward = 50;
    
    [Header("Visual")]
    [SerializeField] private GameObject bountyIndicator;
    [SerializeField] private Color bountyColor = Color.red;
    
    private EnemyHealth enemyHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Add a death listener to the enemy health component
        if (enemyHealth == null)
        {
            Debug.LogError("BountyEnemy needs an EnemyHealth component!");
        }
    }
    
    private void Start()
    {
        // Register with GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterBountyEnemy(this);
        }
        
        // Apply visual effects to indicate this is a bounty enemy
        ApplyBountyVisuals();
        
        // Make sure this enemy is trackable
        EnsureTrackable();
    }
    
    private void OnEnable()
    {
        // Subscribe to enemy death
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += OnEnemyDeath;
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from enemy death
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= OnEnemyDeath;
        }
    }
    
    private void ApplyBountyVisuals()
    {
        // Change color to indicate this is a bounty
        if (spriteRenderer != null)
        {
            spriteRenderer.color = bountyColor;
        }
        
        // Show bounty indicator if provided
        if (bountyIndicator != null)
        {
            bountyIndicator.SetActive(true);
        }
        else
        {
            // Create a simple bounty indicator
            CreateDefaultBountyIndicator();
        }
    }
    
    private void CreateDefaultBountyIndicator()
    {
        // Create a simple crown or star above the enemy
        GameObject indicator = new GameObject("BountyIndicator");
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = new Vector3(0, 1.2f, 0); // Above the enemy
        
        // Add a sprite renderer
        SpriteRenderer indicatorSprite = indicator.AddComponent<SpriteRenderer>();
        
        // Try to find a star or crown sprite
        indicatorSprite.sprite = Resources.Load<Sprite>("Star");
        if (indicatorSprite.sprite == null)
        {
            // Use a simple circular sprite if no star is found
            indicatorSprite.sprite = Resources.Load<Sprite>("Circle");
        }
        
        // Set color to gold
        indicatorSprite.color = new Color(1f, 0.8f, 0, 1); // Gold color
        
        // Set size
        indicator.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // Set sorting order to be above the enemy
        indicatorSprite.sortingOrder = spriteRenderer != null ? 
            spriteRenderer.sortingOrder + 1 : 5;
        
        // Store reference
        bountyIndicator = indicator;
        
        // Add a simple rotation animation
        indicator.AddComponent<RotateObject>();
    }
    
    private void EnsureTrackable()
    {
        // Make sure this enemy has a TrackableEnemy component
        if (GetComponent<TrackableEnemy>() == null)
        {
            gameObject.AddComponent<TrackableEnemy>();
        }
    }
    
    private void OnEnemyDeath()
    {
        // Report the bounty kill to the GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncrementBountyKill();
            
            // Drop extra gold
            DropExtraGold();
        }
    }
    
    private void DropExtraGold()
    {
        // Use the existing PickUpSpawner to drop extra gold if possible
        PickUpSpawner pickUpSpawner = GetComponent<PickUpSpawner>();
        if (pickUpSpawner != null)
        {
            // We could call DropItems multiple times or modify it to drop more gold,
            // but this is the simplest approach without modifying existing classes
            for (int i = 0; i < goldReward / 10; i++) // Assume each gold pickup is worth 10
            {
                pickUpSpawner.DropItems();
            }
        }
    }
}

// Simple component to rotate an object
public class RotateObject : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;
    
    private void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
} 