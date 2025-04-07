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
    [SerializeField] private float cardSwitchCooldown = 1.0f;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] cardShootSounds; // Array of 4 shooting sounds
    [SerializeField] private AudioClip[] cardCollisionSounds; // Array of 4 collision sounds
    [SerializeField] private AudioClip cooldownEndSound;
    [SerializeField] private AudioSource audioSource; // Reference to an AudioSource component
    
    [Header("UI")]
    [SerializeField] private Image cooldownImage; // UI Image for cooldown indicator
    
    [Header("Card Upgrades")]
    [SerializeField] private bool greenCardAreaUpgrade = false;
    [SerializeField] private bool blueCardStunUpgrade = false;
    [SerializeField] private bool redCardPoisonUpgrade = false;
    [SerializeField] private bool redFanShotUpgrade = false; // Red fan shot
    [SerializeField] private bool blueFanShotUpgrade = false; // Blue fan shot
    [SerializeField] private bool greenFanShotUpgrade = false; // Green fan shot
    [SerializeField] private bool redVampireUpgrade = false; // Red vampire upgrade
    [SerializeField] private bool blueVampireUpgrade = false; // Blue vampire upgrade
    [SerializeField] private bool greenVampireUpgrade = false; // Green vampire upgrade
    [SerializeField] private float greenAreaIncreaseAmount = 1.5f;
    [SerializeField] private float blueStunDuration = 1.0f;
    [SerializeField] private float redPoisonDuration = 3.0f;
    [SerializeField] private float redPoisonDamagePerSecond = 2.0f;
    [SerializeField] private float fanShotAngleSpread = 15f; // Angle between fan cards
    [SerializeField] private float fanShotDamageMultiplier = 0.7f; // Damage reduction for each fan card
    [SerializeField] private float vampireHealPercent = 0.2f; // Vampire heals 20% of damage
    
    private bool canThrow = true;
    private bool canSwitchCardType = true;
    private Camera mainCamera;
    private float currentCooldown;
    
    // Start method
    private void Start()
    {
        mainCamera = Camera.main;
        
        // Initialize cooldown UI if assigned
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0;
        }
        
        // Try to get an AudioSource if not explicitly assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && transform.parent != null)
            {
                audioSource = transform.parent.GetComponent<AudioSource>();
            }
            
            if (audioSource == null)
            {
                Debug.LogWarning("No AudioSource found for CardThrower - adding one");
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1.0f; // 3D sound
                audioSource.volume = 1.0f;
            }
        }
        
        // Comment out automatic enabling of all upgrades
        // Now only cards with specific upgrades will split
        // redFanShotUpgrade = true;
        // blueFanShotUpgrade = true;
        // greenFanShotUpgrade = true;
        // Debug.Log("<color=yellow>TESTING: All fan shot upgrades enabled for testing</color>");
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
        Vector2 baseDirection = mousePosition - throwPoint.position;
        baseDirection.Normalize();
        
        Debug.Log($"Base direction: {baseDirection}");
        
        if (baseDirection.magnitude < 0.1f)
        {
            Debug.LogWarning("Direction magnitude too small, defaulting to right");
            baseDirection = Vector2.right;
        }
        
        // Randomly select which card to throw (type remains consistent for fan shot)
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
                Debug.Log("Selected RED card type");
                break;
            case 1: // Green card
                cardType = Card.CardType.Green;
                cardSprite = greenCardSprite;
                effectPrefab = greenEffectPrefab;
                stats = greenCardStats;
                cardColor = Color.green;
                Debug.Log("Selected GREEN card type");
                break;
            case 2: // Blue card
                cardType = Card.CardType.Blue;
                cardSprite = blueCardSprite;
                effectPrefab = blueEffectPrefab;
                stats = blueCardStats;
                cardColor = Color.blue;
                Debug.Log("Selected BLUE card type");
                break;
        }
        
        // Play throw sound
        PlayThrowSound();
        
        // Check if this card type has its specific fan shot upgrade
        bool shouldSplit = false;
        bool hasVampire = false;
        
        // Check for fan shot upgrade
        if (cardType == Card.CardType.Red && redFanShotUpgrade)
        {
            shouldSplit = true;
            Debug.Log("<color=red>RED fan shot upgrade active!</color>");
        }
        else if (cardType == Card.CardType.Blue && blueFanShotUpgrade)
        {
            shouldSplit = true;
            Debug.Log("<color=blue>BLUE fan shot upgrade active!</color>");
        }
        else if (cardType == Card.CardType.Green && greenFanShotUpgrade)
        {
            shouldSplit = true;
            Debug.Log("<color=green>GREEN fan shot upgrade active!</color>");
        }
        
        // Check for vampire upgrade
        if (cardType == Card.CardType.Red && redVampireUpgrade)
        {
            hasVampire = true;
            Debug.Log("<color=red>RED vampire upgrade active!</color>");
        }
        else if (cardType == Card.CardType.Blue && blueVampireUpgrade)
        {
            hasVampire = true;
            Debug.Log("<color=blue>BLUE vampire upgrade active!</color>");
        }
        else if (cardType == Card.CardType.Green && greenVampireUpgrade)
        {
            hasVampire = true;
            Debug.Log("<color=green>GREEN vampire upgrade active!</color>");
        }
        
        // Spawn and launch card with appropriate settings
        SpawnAndLaunchCard(cardType, cardSprite, effectPrefab, stats, cardColor, baseDirection, 1.0f, shouldSplit, hasVampire);
    }
    
    // Helper method to spawn and launch a single card
    private void SpawnAndLaunchCard(Card.CardType type, Sprite sprite, GameObject effect, CardStats baseStats, Color color, Vector2 direction, float damageMultiplier, bool shouldSplit = false, bool hasVampire = false)
    {
        // Create new card at throw point
        GameObject cardObject = Instantiate(cardPrefab, throwPoint.position, Quaternion.identity);
        SpriteRenderer sr = cardObject.GetComponent<SpriteRenderer>();
        
        // Set card sprite
        if (sr != null)
        {
            sr.sprite = sprite;
        }
        
        // Get Card component and initialize it
        Card card = cardObject.GetComponent<Card>();
        if (card != null)
        {
            // Direction should be normalized to ensure consistent speed
            Vector2 normalizedDirection = direction.normalized;
            
            // Pass the effect prefab and color, and set the shouldSplit flag for fan shot
            // Also pass vampire upgrade status
            card.Initialize(
                type, 
                baseStats, 
                effect, 
                color, 
                cardCollisionSounds, 
                damageMultiplier, 
                shouldSplit, 
                fanShotAngleSpread, 
                fanShotDamageMultiplier,
                hasVampire,
                vampireHealPercent
            );
            
            // Launch in the calculated direction
            card.Launch(normalizedDirection);
            
            // Set damage amount (a bit redundant since Card.GetDamage() now uses stats.damage)
            DamageSource damageSource = cardObject.GetComponent<DamageSource>();
            if (damageSource != null)
            {
                int actualDamage;
                float damageModifier = 1.0f;
                
                // Apply damage multiplier if needed (for charge shots or fan shots)
                if (damageMultiplier != 1.0f)
                {
                    damageModifier = damageMultiplier;
                }
                
                // Use baseValue from stats and apply modifier
                actualDamage = Mathf.RoundToInt(baseStats.damage.baseValue * damageModifier);
                
                // Don't try to set damageAmount directly since it's protected
                // The Card component already handles damage calculation in GetDamage()
                
                Debug.Log($"Launched card ({type}) in direction {direction} with effective damage {actualDamage}, shouldSplit: {shouldSplit}, hasLifesteal: {vampireHealPercent}");
            }
            else
            {
                Debug.LogError("Card component missing DamageSource!");
            }
        }
        else
        {
            Debug.LogError("Card component missing from prefab!");
            Destroy(cardObject); // Clean up useless object
        }
    }
    
    // Helper method to play throw sound
    private void PlayThrowSound()
    {
        if (audioSource != null && cardShootSounds != null && cardShootSounds.Length > 0)
        {
            AudioClip soundToPlay = null;
            int attempts = 0;
            while (soundToPlay == null && attempts < cardShootSounds.Length * 2) // Prevent infinite loop
            {
                int randomIndex = Random.Range(0, cardShootSounds.Length);
                soundToPlay = cardShootSounds[randomIndex];
                attempts++;
            }

            if (soundToPlay != null)
            {
                audioSource.volume = 6.0f; // Keep the EXTREME volume
                audioSource.PlayOneShot(soundToPlay, 6.0f);
                Debug.Log($"Playing card throw sound: {soundToPlay.name} at EXTREME volume 6.0");
                StartCoroutine(ResetVolumeAfterSound());
            }
            else
            {
                Debug.LogWarning("Could not find a non-null card shoot sound to play!");
            }
        }
        else
        {
            Debug.LogWarning("AudioSource or card shoot sounds missing!");
        }
    }
    
    // Helper function to rotate a vector
    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        
        float tx = v.x;
        float ty = v.y;
        
        return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
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
            audioSource.PlayOneShot(cooldownEndSound, 0.6f);
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
        // The damage is now a CardStat object (not a simple float)
        // Access the baseValue of the CardStat properly
        if (redCardStats != null && redCardStats.damage != null)
        {
            redCardStats.damage.baseValue += amount;
        }
        
        if (greenCardStats != null && greenCardStats.damage != null)
        {
            greenCardStats.damage.baseValue += amount;
        }
        
        if (blueCardStats != null && blueCardStats.damage != null)
        {
            blueCardStats.damage.baseValue += amount;
        }
        
        Debug.Log($"CardThrower: Increased all card damage by {amount}. New damage values - " + 
                  $"Red: {redCardStats?.damage?.baseValue ?? 0}, " +
                  $"Green: {greenCardStats?.damage?.baseValue ?? 0}, " +
                  $"Blue: {blueCardStats?.damage?.baseValue ?? 0}");
    }
    
    // Add method to get the current damage
    public float GetCurrentDamage()
    {
        // Calculate average damage from all card types
        float totalDamage = 0;
        int cardTypeCount = 0;
        
        if (redCardStats != null && redCardStats.damage != null)
        {
            totalDamage += redCardStats.damage.baseValue;
            cardTypeCount++;
        }
        
        if (greenCardStats != null && greenCardStats.damage != null)
        {
            totalDamage += greenCardStats.damage.baseValue;
            cardTypeCount++;
        }
        
        if (blueCardStats != null && blueCardStats.damage != null)
        {
            totalDamage += blueCardStats.damage.baseValue;
            cardTypeCount++;
        }
        
        if (cardTypeCount > 0)
        {
            return totalDamage / cardTypeCount;
        }
        
        return 0;
    }
    
    // Helper method to reset volume after playing a loud sound
    private IEnumerator ResetVolumeAfterSound()
    {
        yield return null; // Wait one frame
        audioSource.volume = 1.0f; // Reset to default volume
    }
    
    // Apply upgrades
    public void ApplyGreenAreaUpgrade()
    {
        if (!greenCardAreaUpgrade)
        {
            greenCardAreaUpgrade = true;
            
            // Increase explosion radius for green cards
            if (greenCardStats != null)
            {
                float oldValue = greenCardStats.explosionRadius.GetValue();
                
                // Directly modify the base value instead of using AddModifier
                greenCardStats.explosionRadius.baseValue += greenAreaIncreaseAmount;
                
                float newValue = greenCardStats.explosionRadius.GetValue();
                
                Debug.Log($"Green card area upgraded: {oldValue} -> {newValue}");
            }
            else
            {
                Debug.LogError("Green card stats not found for area upgrade!");
            }
        }
        else
        {
            Debug.Log("Green card area upgrade already applied!");
        }
    }
    
    public void ApplyBlueStunUpgrade()
    {
        if (!blueCardStunUpgrade)
        {
            blueCardStunUpgrade = true;
            Debug.Log($"Blue card stun upgrade applied: {blueStunDuration} seconds");
        }
        else
        {
            Debug.Log("Blue card stun upgrade already applied!");
        }
    }
    
    public void ApplyRedPoisonUpgrade()
    {
        if (!redCardPoisonUpgrade)
        {
            redCardPoisonUpgrade = true;
            Debug.Log($"Red card poison upgrade applied: {redPoisonDamagePerSecond} damage per second for {redPoisonDuration} seconds");
        }
        else
        {
            Debug.Log("Red card poison upgrade already applied!");
        }
    }
    
    // Apply upgrade methods for fan shot for each card type
    public void ApplyRedFanShotUpgrade()
    {
        if (!redFanShotUpgrade)
        {
            redFanShotUpgrade = true;
            Debug.Log("Red Fan Shot upgrade applied!");
        }
        else
        {
            Debug.Log("Red Fan Shot upgrade already applied!");
        }
    }
    
    public void ApplyBlueFanShotUpgrade()
    {
        if (!blueFanShotUpgrade)
        {
            blueFanShotUpgrade = true;
            Debug.Log("Blue Fan Shot upgrade applied!");
        }
        else
        {
            Debug.Log("Blue Fan Shot upgrade already applied!");
        }
    }
    
    public void ApplyGreenFanShotUpgrade()
    {
        if (!greenFanShotUpgrade)
        {
            greenFanShotUpgrade = true;
            Debug.Log("Green Fan Shot upgrade applied!");
        }
        else
        {
            Debug.Log("Green Fan Shot upgrade already applied!");
        }
    }
    
    // Apply vampire upgrade
    public void ApplyRedVampireUpgrade()
    {
        if (!redVampireUpgrade)
        {
            redVampireUpgrade = true;
            Debug.Log("Red Vampire upgrade applied!");
        }
        else
        {
            Debug.Log("Red Vampire upgrade already applied!");
        }
    }
    
    public void ApplyBlueVampireUpgrade()
    {
        if (!blueVampireUpgrade)
        {
            blueVampireUpgrade = true;
            Debug.Log("Blue Vampire upgrade applied!");
        }
        else
        {
            Debug.Log("Blue Vampire upgrade already applied!");
        }
    }
    
    public void ApplyGreenVampireUpgrade()
    {
        if (!greenVampireUpgrade)
        {
            greenVampireUpgrade = true;
            Debug.Log("Green Vampire upgrade applied!");
        }
        else
        {
            Debug.Log("Green Vampire upgrade already applied!");
        }
    }
    
    // Getters for upgrade status including the new fan shot upgrades
    public bool HasGreenAreaUpgrade() { return greenCardAreaUpgrade; }
    public bool HasBlueStunUpgrade() { return blueCardStunUpgrade; }
    public bool HasRedPoisonUpgrade() { return redCardPoisonUpgrade; }
    public bool HasRedFanShotUpgrade() { return redFanShotUpgrade; }
    public bool HasBlueFanShotUpgrade() { return blueFanShotUpgrade; }
    public bool HasGreenFanShotUpgrade() { return greenFanShotUpgrade; }
    public bool HasRedVampireUpgrade() { return redVampireUpgrade; }
    public bool HasBlueVampireUpgrade() { return blueVampireUpgrade; }
    public bool HasGreenVampireUpgrade() { return greenVampireUpgrade; }
    
    // Getters for upgrade parameters
    public float GetGreenAreaIncreaseAmount() { return greenAreaIncreaseAmount; }
    public float GetBlueStunDuration() { return blueStunDuration; }
    public float GetRedPoisonDuration() { return redPoisonDuration; }
    public float GetRedPoisonDamagePerSecond() { return redPoisonDamagePerSecond; }
    public float GetFanShotAngleSpread() { return fanShotAngleSpread; }
    public float GetFanShotDamageMultiplier() { return fanShotDamageMultiplier; }
    public float GetVampireHealPercent() { return vampireHealPercent; }
    
    // Getter for card prefab (needed for splitting cards)
    public GameObject GetCardPrefab() { return cardPrefab; }
} 