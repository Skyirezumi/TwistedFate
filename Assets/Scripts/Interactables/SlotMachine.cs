using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlotMachine : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private float cooldownTime = 0f; // Set to 0 to remove cooldown
    
    [Header("Slot Machine Settings")]
    [SerializeField] private GameObject slotMachineUI;
    [SerializeField] private AudioClip spinSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private ParticleSystem winParticles;
    
    [Header("Stat Upgrade Settings")]
    [SerializeField] private float damageUpgradeAmount = 1f;
    [SerializeField] private float healthUpgradeAmount = 10f;
    [SerializeField] private float speedUpgradeAmount = 0.5f;
    [SerializeField] private float dashUpgradeAmount = 0.2f;
    
    [Header("Economy Settings")]
    [SerializeField] private int spinCost = 1;
    [SerializeField] private string notEnoughMoneyMessage = "Need more gold! (1 gold per spin)";
    [SerializeField] private string noLuckMessage = "No luck today!";
    private bool showingMoneyMessage = false;
    private float moneyMessageTimer = 0f;
    private const float moneyMessageDuration = 3f;
    
    private bool isInRange = false;
    private bool canInteract = true;
    private float cooldownTimer = 0f;
    private AudioSource audioSource;
    
    // References to UI elements
    private SlotMachineUI slotUI;
    private SlotMachineAnimator slotAnimator;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (slotMachineUI != null)
        {
            slotUI = slotMachineUI.GetComponent<SlotMachineUI>();
            slotAnimator = slotMachineUI.GetComponent<SlotMachineAnimator>();
            slotMachineUI.SetActive(false);
        }
    }
    
    private void Start()
    {
        // Hide the interaction prompt at start
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    private void Update()
    {
        CheckPlayerDistance();
        HandleInteraction();
        HandleCooldown();
        HandleMoneyMessage();
    }
    
    private void CheckPlayerDistance()
    {
        if (PlayerController.Instance == null) return;
        
        float distance = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        isInRange = distance <= interactionRange;
        
        // Show/hide interaction prompt based on distance
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(isInRange && canInteract);
        }
    }
    
    private void HandleInteraction()
    {
        if (isInRange && canInteract && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Check if the player has enough gold
            if (EconomyManager.Instance != null && EconomyManager.Instance.HasEnoughGold(spinCost))
            {
                StartCoroutine(PlaySlotMachine());
            }
            else
            {
                ShowNotEnoughMoneyMessage();
            }
        }
    }
    
    private void HandleCooldown()
    {
        if (!canInteract)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                canInteract = true;
                if (isInRange && interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }
            }
        }
    }
    
    private void HandleMoneyMessage()
    {
        if (showingMoneyMessage)
        {
            moneyMessageTimer -= Time.deltaTime;
            if (moneyMessageTimer <= 0f)
            {
                showingMoneyMessage = false;
                
                // Hide the message in the UI
                if (slotUI != null && slotMachineUI != null && slotMachineUI.activeSelf)
                {
                    slotUI.ShowResult("", 0);
                    slotMachineUI.SetActive(false);
                }
            }
        }
    }
    
    private void ShowNotEnoughMoneyMessage()
    {
        Debug.Log($"SlotMachine: Not enough money - need {spinCost} gold");
        
        showingMoneyMessage = true;
        moneyMessageTimer = moneyMessageDuration;
        
        // Show message using the slot machine UI
        if (slotMachineUI != null)
        {
            slotMachineUI.SetActive(true);
            
            if (slotUI != null)
            {
                slotUI.ShowResult(notEnoughMoneyMessage, 0); // Use damage color (red) for message
            }
        }
    }
    
    private IEnumerator PlaySlotMachine()
    {
        Debug.Log("SlotMachine: Starting PlaySlotMachine sequence");
        
        // Spend the gold first
        if (EconomyManager.Instance != null)
        {
            if (!EconomyManager.Instance.SpendGold(spinCost))
            {
                // Double-check that the player still has enough gold
                ShowNotEnoughMoneyMessage();
                yield break;
            }
        }
        
        // Start cooldown (only used to prevent interaction while animation is playing)
        canInteract = false;
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Show the slot machine UI with animation if possible
        if (slotMachineUI != null)
        {
            Debug.Log("SlotMachine: Activating UI panel");
            slotMachineUI.SetActive(true);
            
            // Check if we have the SlotMachineAnimator component
            if (slotAnimator != null)
            {
                Debug.Log("SlotMachine: Using SlotMachineAnimator for animations");
                slotAnimator.PlayShowAnimation();
            }
            else
            {
                Debug.Log("SlotMachine: No SlotMachineAnimator found - this is normal if using Unity Animator");
            }
        }
        else
        {
            Debug.LogError("SlotMachine: No UI panel assigned!");
            yield break; // Can't continue without UI
        }
        
        // Play spin sound
        if (spinSound != null && audioSource != null)
        {
            audioSource.clip = spinSound;
            audioSource.Play();
        }
        
        // Wait for animation duration (if using UI)
        if (slotUI != null)
        {
            Debug.Log("SlotMachine: Starting spin via SlotMachineUI");
            slotUI.StartSpin();
            yield return new WaitForSeconds(slotUI.SpinDuration);
        }
        else
        {
            Debug.LogWarning("SlotMachine: No SlotMachineUI component found on UI panel!");
            // If no UI script, wait a short time
            yield return new WaitForSeconds(2f);
        }
        
        // Get match count and apply upgrade based on it
        int matchCount = 1; // Default to 1 (no matches)
        int matchedIconType = 0; // Default to damage type
        
        if (slotUI != null)
        {
            matchCount = slotUI.GetMatchCount();
            matchedIconType = slotUI.GetMatchingIconType();
        }
        
        if (matchCount == 1)
        {
            // No matches - no upgrade
            Debug.Log("SlotMachine: All different icons - No upgrade");
            
            // Show a "no luck" message
            if (slotUI != null)
            {
                slotUI.ShowResult(noLuckMessage, Random.Range(0, 4));
            }
        }
        else
        {
            // Apply upgrade based on the matched icon type with strength based on match count
            ApplyUpgrade(matchedIconType, matchCount);
            
            // Play win sound
            if (winSound != null && audioSource != null)
            {
                audioSource.clip = winSound;
                audioSource.Play();
            }
            
            // Show win particles
            if (winParticles != null)
            {
                // Scale particles based on match count
                var main = winParticles.main;
                main.startSize = main.startSize.constant * (matchCount == 3 ? 1.5f : 1f);
                main.startSpeed = main.startSpeed.constant * (matchCount == 3 ? 1.5f : 1f);
                
                winParticles.Play();
            }
        }
        
        // Play result animation if possible
        if (slotAnimator != null)
        {
            slotAnimator.PlayResultAnimation();
        }
        
        // Keep UI visible for a moment
        yield return new WaitForSeconds(2f);
        
        // Hide the slot machine UI with animation if possible
        if (slotMachineUI != null)
        {
            if (slotAnimator != null)
            {
                slotAnimator.PlayHideAnimation();
                // The animator will deactivate the UI when done
                yield return new WaitForSeconds(0.5f); // Give the animation time to start
            }
            else
            {
                // If no animator, just hide it directly
                Debug.Log("SlotMachine: Hiding UI panel directly");
                slotMachineUI.SetActive(false);
            }
        }
        
        Debug.Log("SlotMachine: PlaySlotMachine sequence completed");
        
        // Re-enable interaction immediately
        canInteract = true;
        if (isInRange && interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
    }
    
    // New method that applies a specific upgrade based on matched icon type
    private void ApplyUpgrade(int upgradeType, int matchCount)
    {
        if (PlayerController.Instance == null) return;
        
        // If all icons are different (matchCount == 1), don't apply upgrade
        if (matchCount == 1) return;
        
        string upgradeMessage = "";
        
        // Calculate multiplier - double strength for 3 matches
        float multiplier = (matchCount == 3) ? 2f : 1f;
        
        switch (upgradeType)
        {
            case 0: // Damage upgrade (Red)
                UpgradeDamage(damageUpgradeAmount * multiplier);
                upgradeMessage = matchCount == 3 ? "Damage GREATLY increased!" : "Damage increased!";
                break;
            case 1: // Health upgrade (Green)
                UpgradeHealth(healthUpgradeAmount * multiplier);
                upgradeMessage = matchCount == 3 ? "Health GREATLY increased!" : "Health increased!";
                break;
            case 2: // Speed upgrade (Blue)
                UpgradeSpeed(speedUpgradeAmount * multiplier);
                upgradeMessage = matchCount == 3 ? "Speed GREATLY increased!" : "Movement speed increased!";
                break;
            case 3: // Dash upgrade (Yellow)
                UpgradeDash(dashUpgradeAmount * multiplier);
                upgradeMessage = matchCount == 3 ? "Dash GREATLY improved!" : "Dash range increased!";
                break;
        }
        
        // Show upgrade message in UI if available
        if (slotUI != null)
        {
            slotUI.ShowResult(upgradeMessage, upgradeType);
        }
        
        // Create floating text above player to show upgrade
        CreateFloatingText(PlayerController.Instance.transform.position, upgradeMessage, GetUpgradeColor(upgradeType));
        
        // Log for debugging
        Debug.Log($"Player received upgrade: {upgradeMessage} (Type: {GetUpgradeTypeName(upgradeType)}, Strength: {multiplier}x)");
    }
    
    // Helper method to get the name of the upgrade type
    private string GetUpgradeTypeName(int upgradeType)
    {
        switch (upgradeType)
        {
            case 0: return "Damage";
            case 1: return "Health";
            case 2: return "Speed";
            case 3: return "Dash";
            default: return "Unknown";
        }
    }
    
    private void CreateFloatingText(Vector3 position, string text, Color color)
    {
        // Create a simple text popup if available
        // This is optional and requires a floating text system
        GameObject floatingTextPrefab = Resources.Load<GameObject>("Prefabs/UI/FloatingText");
        if (floatingTextPrefab != null)
        {
            GameObject floatingText = Instantiate(floatingTextPrefab, position + Vector3.up, Quaternion.identity);
            
            // Set text and color if possible
            TMPro.TextMeshPro textComponent = floatingText.GetComponent<TMPro.TextMeshPro>();
            if (textComponent != null)
            {
                textComponent.text = text;
                textComponent.color = color;
            }
            
            // Destroy it after a few seconds
            Destroy(floatingText, 2f);
        }
    }
    
    private Color GetUpgradeColor(int upgradeType)
    {
        switch (upgradeType)
        {
            case 0: return Color.red; // Damage
            case 1: return Color.green; // Health
            case 2: return Color.cyan; // Speed
            case 3: return Color.yellow; // Dash
            default: return Color.white;
        }
    }
    
    private void UpgradeDamage(float amount = -1f)
    {
        // Use the default amount if none specified
        if (amount < 0) amount = damageUpgradeAmount;
        
        // Get card thrower reference from player
        CardThrower cardThrower = PlayerController.Instance.GetComponent<CardThrower>();
        if (cardThrower != null)
        {
            // Increase damage of all card types
            cardThrower.IncreaseDamage(amount);
        }
    }
    
    private void UpgradeHealth(float amount = -1f)
    {
        // Use the default amount if none specified
        if (amount < 0) amount = healthUpgradeAmount;
        
        // Increase max health
        PlayerController player = PlayerController.Instance;
        player.IncreaseMaxHealth(amount);
    }
    
    private void UpgradeSpeed(float amount = -1f)
    {
        // Use the default amount if none specified
        if (amount < 0) amount = speedUpgradeAmount;
        
        // Increase movement speed
        PlayerController player = PlayerController.Instance;
        player.IncreaseMovementSpeed(amount);
    }
    
    private void UpgradeDash(float amount = -1f)
    {
        // Use the default amount if none specified
        if (amount < 0) amount = dashUpgradeAmount;
        
        // Increase dash range/speed
        PlayerController player = PlayerController.Instance;
        player.IncreaseDashPower(amount);
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualize interaction range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
} 