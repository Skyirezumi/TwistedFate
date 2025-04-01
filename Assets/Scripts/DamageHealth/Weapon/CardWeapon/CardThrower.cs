using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardThrower : MonoBehaviour
{
    [Header("Card Prefabs")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform throwPoint;
    
    [Header("Card Appearances")]
    [SerializeField] private Sprite redCardSprite;
    [SerializeField] private Sprite greenCardSprite;
    [SerializeField] private Sprite blueCardSprite;
    
    [Header("Card Effect Prefabs")]
    [SerializeField] private GameObject redEffectPrefab;
    [SerializeField] private GameObject greenEffectPrefab;
    [SerializeField] private GameObject blueEffectPrefab;
    
    [Header("Card Stats")]
    [SerializeField] private CardStats redCardStats = new CardStats();
    [SerializeField] private CardStats greenCardStats = new CardStats();
    [SerializeField] private CardStats blueCardStats = new CardStats();
    
    [Header("Throwing Settings")]
    [SerializeField] private float throwCooldown = 0.5f;
    [SerializeField] private float cardSpeed = 10f;
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private float cardSwitchCooldown = 1.0f;
    
    [Header("UI")]
    [SerializeField] private AudioClip cooldownEndSound;
    [SerializeField] private Image cooldownImage; // UI Image for cooldown indicator
    
    private bool canThrow = true;
    private bool canSwitchCardType = true;
    private Camera mainCamera;
    private float currentCooldown;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        // Initialize cooldown UI if assigned
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0;
        }
    }
    
    // This is only used for cooldown tracking
    private void Update()
    {
        // Cooldown timer update for UI
        if (!canThrow)
        {
            UpdateCooldownUI();
        }
    }
    
    // This method is called by PlayerController using the new Input System
    public void TriggerThrowCard()
    {
        if (canThrow)
        {
            ThrowCard();
        }
    }
    
    private void ThrowCard()
    {
        // Start cooldown
        StartCoroutine(ThrowCooldown());
        
        // Get mouse position in screen space
        Vector3 mousePosition = Input.mousePosition;
        // Convert to world coordinates
        mousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
        // Set Z to match throwPoint so we have a proper 2D comparison
        mousePosition.z = throwPoint.position.z;
        
        // Calculate direction from throwPoint to mouse position
        Vector2 direction = mousePosition - throwPoint.position;
        direction.Normalize();
        
        Debug.Log($"Mouse position: {mousePosition}, Direction: {direction}");
        
        if (direction.magnitude < 0.1f)
        {
            Debug.LogWarning("Direction magnitude too small, defaulting to right");
            direction = Vector2.right;
        }
        
        // Randomly select which card to throw
        int randomCard = Random.Range(0, 3);
        Card.CardType cardType = Card.CardType.Red;
        Sprite cardSprite = redCardSprite;
        GameObject effectPrefab = redEffectPrefab;
        CardStats stats = redCardStats;
        Color cardColor = Color.red;
        
        switch (randomCard)
        {
            case 0: // Red card
                cardType = Card.CardType.Red;
                cardSprite = redCardSprite;
                effectPrefab = redEffectPrefab;
                stats = redCardStats;
                cardColor = Color.red;
                Debug.Log("Throwing RED card");
                break;
            case 1: // Green card
                cardType = Card.CardType.Green;
                cardSprite = greenCardSprite;
                effectPrefab = greenEffectPrefab;
                stats = greenCardStats;
                cardColor = Color.green;
                Debug.Log("Throwing GREEN card");
                break;
            case 2: // Blue card
                cardType = Card.CardType.Blue;
                cardSprite = blueCardSprite;
                effectPrefab = blueEffectPrefab;
                stats = blueCardStats;
                cardColor = Color.blue;
                Debug.Log("Throwing BLUE card");
                break;
        }
        
        // Play throw sound if assigned
        if (throwSound != null)
        {
            AudioSource.PlayClipAtPoint(throwSound, throwPoint.position, 0.6f);
        }
        
        // Calculate exact angle for card orientation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Instantiate card with correct rotation
        GameObject cardObject = Instantiate(cardPrefab, throwPoint.position, Quaternion.identity);
        
        // Get the Card component and set it up
        Card card = cardObject.GetComponent<Card>();
        if (card != null)
        {
            // First set sprite
            SpriteRenderer spriteRenderer = cardObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && cardSprite != null)
            {
                spriteRenderer.sprite = cardSprite;
                spriteRenderer.color = cardColor; // Set color directly too
                Debug.Log($"Set sprite and color for {cardType} card");
            }
            else
            {
                Debug.LogError("Card sprite or renderer missing!");
            }
            
            // Then initialize with correct stats
            card.Initialize(cardType, stats, effectPrefab, cardColor);
            
            // Launch the card AFTER initialization
            card.Launch(direction);
            
            // Verify card is set up correctly
            Debug.Log($"Card launched with direction {direction}");
        }
        else
        {
            Debug.LogError("Card component missing from prefab!");
        }
    }
    
    private IEnumerator ThrowCooldown()
    {
        canThrow = false;
        currentCooldown = throwCooldown;
        
        // Update UI immediately to show full cooldown
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 1;
        }
        
        // Wait for cooldown
        yield return new WaitForSeconds(throwCooldown);
        
        // Cooldown finished
        canThrow = true;
        
        // Play cooldown end sound if assigned
        if (cooldownEndSound != null)
        {
            AudioSource.PlayClipAtPoint(cooldownEndSound, transform.position, 0.6f);
        }
        
        // Reset cooldown UI
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0;
        }
    }
    
    private void UpdateCooldownUI()
    {
        if (cooldownImage != null)
        {
            // Decrease cooldown timer
            currentCooldown -= Time.deltaTime;
            
            // Update fill amount (1 = full, 0 = empty)
            cooldownImage.fillAmount = currentCooldown / throwCooldown;
        }
    }
    
    private IEnumerator SwitchCooldownRoutine()
    {
        yield return new WaitForSeconds(cardSwitchCooldown);
        canSwitchCardType = true;
    }
    
    // Method for increasing card damage (called by SlotMachine)
    public void IncreaseDamage(float amount)
    {
        // Increase damage for all card types
        redCardStats.damage.baseValue += amount;
        greenCardStats.damage.baseValue += amount;
        blueCardStats.damage.baseValue += amount;
        
        Debug.Log($"Card damage increased! Red: {redCardStats.damage.baseValue}, Green: {greenCardStats.damage.baseValue}, Blue: {blueCardStats.damage.baseValue}");
    }
} 