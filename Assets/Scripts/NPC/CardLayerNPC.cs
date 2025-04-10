using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CardLayerNPC : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private float interactionCooldown = 1f;
    
    [Header("Upgrade Settings")]
    [SerializeField] private int numUpgradesToShow = 3;
    [SerializeField] private int upgradeCost = 50;  // Cost in gold
    
    [Header("Audio")]
    [SerializeField] private AudioClip interactSound;
    [SerializeField] private float soundVolume = 1f;
    [SerializeField] private AudioClip[] talkingSounds;
    [SerializeField] private float talkingVolume = 0.7f;
    [SerializeField] private float talkingSoundFrequency = 0.2f;
    
    [Header("Dialogue Settings")]
    [SerializeField] private GameObject dialogContainer; // Add this reference for the dialogue UI
    [SerializeField] private string[] dialogLines = new string[] 
    {
        "Hey there, looking to upgrade your cards?",
        "I can enhance your cards... for a price.",
        "My upgrades will give your cards special powers."
    };
    [SerializeField] private float dialogueDisplayTime = 3f;
    
    private bool canInteract = true;
    private bool isInRange = false;
    private float cooldownTimer = 0f;
    private AudioSource interactAudioSource;
    private AudioSource talkingAudioSource; // Dedicated source for talking sounds
    private DialogManager dialogManager; // Direct reference to DialogManager
    
    private int currentDialogIndex = 0;
    private Coroutine talkingCoroutine;
    private bool isTalking = false;
    
    private Coroutine dialogueCoroutine;
    
    private bool isDialogueActive = false;
    
    private void Awake()
    {
        // Set up interaction audio source
        interactAudioSource = gameObject.AddComponent<AudioSource>();
        interactAudioSource.playOnAwake = false;
        
        // Set up dedicated talking audio source
        talkingAudioSource = gameObject.AddComponent<AudioSource>();
        talkingAudioSource.playOnAwake = false;
        talkingAudioSource.loop = false;
    }
    
    private void Start()
    {
        // Ensure interaction prompt is hidden at start
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Get dialog manager reference (like GeccoInteraction does)
        if (dialogContainer != null)
        {
            dialogManager = dialogContainer.GetComponent<DialogManager>();
            if (dialogManager == null)
            {
                dialogManager = dialogContainer.AddComponent<DialogManager>();
                Debug.Log("[CardLayerNPC] Added DialogManager component to dialogContainer");
            }
            
            // Make sure any existing dialogue is closed at start
            if (dialogManager != null)
            {
                dialogManager.CloseDialog();
            }
        }
        else
        {
            Debug.LogWarning("[CardLayerNPC] No dialogContainer assigned! Dialogue won't work.");
        }
    }
    
    private void Update()
    {
        CheckPlayerDistance();
        HandleInteraction();
        HandleCooldown();
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
        // Only allow interaction if not already in dialogue
        if (isInRange && canInteract && !isDialogueActive && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // First, make sure any existing dialogue is closed
            if (dialogManager != null)
            {
                dialogManager.CloseDialog();
            }
            
            // Play interaction sound
            PlayInteractionSound();
            
            Debug.Log("[CardLayerNPC] Starting interaction with NPC");
            
            // Play dialogue first
            ShowDialogue();
            
            // Start cooldown
            StartCooldown();
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
    
    private void OpenCardUpgradeUI()
    {
        // Check if player has enough gold
        if (EconomyManager.Instance != null && EconomyManager.Instance.GetCurrentGold() >= upgradeCost)
        {
            // Generate random upgrades
            CardUpgrade[] availableUpgrades = GenerateRandomUpgrades();
            
            // Check for shuffle effect
            SimpleCardShuffleEffect shuffleEffect = GetComponentInChildren<SimpleCardShuffleEffect>();
            if (shuffleEffect != null)
            {
                // Play the shuffle animation, then show upgrades when it completes
                shuffleEffect.PlayShuffleAnimation(() => {
                    // Open the UI to display upgrades after animation completes
                    CardUpgradeUI.Instance.ShowUpgrades(availableUpgrades, upgradeCost);
                });
            }
            else
            {
                // No shuffle effect found, show upgrades immediately
                CardUpgradeUI.Instance.ShowUpgrades(availableUpgrades, upgradeCost);
            }
        }
        else
        {
            // Show "not enough gold" message
            ShowMessage("You don't have enough gold for an upgrade. Come back when you have " + upgradeCost + " gold.");
        }
    }
    
    private CardUpgrade[] GenerateRandomUpgrades()
    {
        // Create pool of all possible upgrades
        List<CardUpgrade> allUpgrades = new List<CardUpgrade>()
        {
            new CardUpgrade(CardUpgradeType.GreenAreaOfEffect, "Larger Green AOE", "Increases the area of effect for green cards"),
            new CardUpgrade(CardUpgradeType.BlueStun, "Blue Stun", "Blue cards now stun enemies for 1 second"),
            new CardUpgrade(CardUpgradeType.RedPoison, "Red Poison", "Red cards apply poison damage over time"),
            new CardUpgrade(CardUpgradeType.RedFanShot, "Red Fan Shot", "Red cards split into 3 projectiles at half distance"),
            new CardUpgrade(CardUpgradeType.BlueFanShot, "Blue Fan Shot", "Blue cards split into 3 projectiles at half distance"),
            new CardUpgrade(CardUpgradeType.GreenFanShot, "Green Fan Shot", "Green cards split into 3 projectiles at half distance"),
            new CardUpgrade(CardUpgradeType.RedVampire, "Red Vampire", "Red cards heal you for 20% of damage dealt"),
            new CardUpgrade(CardUpgradeType.BlueVampire, "Blue Vampire", "Blue cards heal you for 20% of damage dealt"),
            new CardUpgrade(CardUpgradeType.GreenVampire, "Green Vampire", "Green cards heal you for 20% of damage dealt"),
            new CardUpgrade(CardUpgradeType.RedHomingPrecision, "Red Homing Precision", "Red cards slightly track moving targets"),
            new CardUpgrade(CardUpgradeType.BlueHomingPrecision, "Blue Homing Precision", "Blue cards slightly track moving targets"),
            new CardUpgrade(CardUpgradeType.GreenHomingPrecision, "Green Homing Precision", "Green cards slightly track moving targets"),
            new CardUpgrade(CardUpgradeType.RedChainLightning, "Red Chain Lightning", "Red cards damage nearby enemies for 60% of the original damage"),
            new CardUpgrade(CardUpgradeType.BlueChainLightning, "Blue Chain Lightning", "Blue cards damage nearby enemies for 60% of the original damage"),
            new CardUpgrade(CardUpgradeType.GreenChainLightning, "Green Chain Lightning", "Green cards damage nearby enemies for 60% of the original damage")
        };
        
        // Shuffle the list
        for (int i = 0; i < allUpgrades.Count; i++)
        {
            CardUpgrade temp = allUpgrades[i];
            int randomIndex = Random.Range(i, allUpgrades.Count);
            allUpgrades[i] = allUpgrades[randomIndex];
            allUpgrades[randomIndex] = temp;
        }
        
        // Take the first 'numUpgradesToShow' upgrades or all if there are fewer
        int count = Mathf.Min(numUpgradesToShow, allUpgrades.Count);
        CardUpgrade[] selectedUpgrades = new CardUpgrade[count];
        
        for (int i = 0; i < count; i++)
        {
            selectedUpgrades[i] = allUpgrades[i];
        }
        
        return selectedUpgrades;
    }
    
    private void PlayInteractionSound()
    {
        if (interactSound != null)
        {
            interactAudioSource.clip = interactSound;
            interactAudioSource.volume = soundVolume;
            interactAudioSource.Play();
        }
    }
    
    private void StartCooldown()
    {
        canInteract = false;
        cooldownTimer = interactionCooldown;
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    private void ShowMessage(string message)
    {
        if (dialogManager != null)
        {
            dialogManager.ShowDialog(message);
        }
        else
        {
            // Fallback: Show in Debug.Log
            Debug.Log("[Card Layer NPC]: " + message);
        }
    }
    
    private void ShowDialogue()
    {
        isDialogueActive = true;
        Debug.Log("[CardLayerNPC] Opening dialogue");
        
        // First stop any existing dialogue routines
        if (dialogueCoroutine != null)
        {
            Debug.Log("[CardLayerNPC] Stopping existing dialogue coroutine");
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }
        
        // Stop talking from previous dialogue if still active
        StopTalking();
        
        // Get a random dialogue line or cycle through them
        string dialogLine = dialogLines[currentDialogIndex];
        currentDialogIndex = (currentDialogIndex + 1) % dialogLines.Length;
        
        // Start talking sounds
        StartTalking();
        
        // Show the dialogue using direct reference
        if (dialogManager != null)
        {
            Debug.Log("[CardLayerNPC] Showing dialogue: " + dialogLine);
            dialogManager.ShowDialog(dialogLine);
        }
        else
        {
            Debug.LogWarning("[CardLayerNPC] DialogManager reference is null!");
        }
        
        // Open the card upgrade UI after a short delay
        dialogueCoroutine = StartCoroutine(OpenUpgradeUIAfterDelay());
    }
    
    private IEnumerator OpenUpgradeUIAfterDelay()
    {
        Debug.Log("[CardLayerNPC] Waiting " + dialogueDisplayTime + " seconds for dialogue display");
        // Wait for dialogue display time
        yield return new WaitForSeconds(dialogueDisplayTime);
        
        Debug.Log("[CardLayerNPC] Dialogue display time completed, stopping talking");
        // Stop talking when dialogue ends
        StopTalking();
        
        // Close dialogue using direct reference
        if (dialogManager != null)
        {
            Debug.Log("[CardLayerNPC] Explicitly closing dialogue");
            dialogManager.CloseDialog();
        }
        
        Debug.Log("[CardLayerNPC] Opening upgrade UI");
        // Open the upgrade UI
        OpenCardUpgradeUI();
        
        isDialogueActive = false;
        dialogueCoroutine = null;
    }
    
    private void StartTalking()
    {
        // Don't do anything if no sounds are available
        if (talkingSounds == null || talkingSounds.Length == 0) return;
        
        // If already in middle of a talking coroutine, don't start again
        if (talkingCoroutine != null) return;
        
        // Start talking coroutine
        isTalking = true;
        talkingCoroutine = StartCoroutine(PlayTalkingSounds());
        Debug.Log("[CardLayerNPC] Started talking sounds");
    }
    
    private void StopTalking()
    {
        // If not talking, nothing to stop
        if (!isTalking) return;
        
        // Set flag to stop coroutine loop
        isTalking = false;
        
        // Stop coroutine if it's running
        if (talkingCoroutine != null)
        {
            StopCoroutine(talkingCoroutine);
            talkingCoroutine = null;
            Debug.Log("[CardLayerNPC] Stopped talking coroutine");
        }
        
        // Stop any currently playing talking sound
        if (talkingAudioSource.isPlaying)
        {
            talkingAudioSource.Stop();
            Debug.Log("[CardLayerNPC] Stopped talking audio");
        }
    }
    
    private IEnumerator PlayTalkingSounds()
    {
        Debug.Log("[CardLayerNPC] Starting talking sound coroutine");
        
        // Initial delay before first sound
        yield return new WaitForSeconds(0.2f);
        
        while (isTalking)
        {
            // Only play sound if not already playing one
            if (!talkingAudioSource.isPlaying && talkingSounds.Length > 0)
            {
                // Get random clip
                AudioClip clip = talkingSounds[Random.Range(0, talkingSounds.Length)];
                if (clip != null)
                {
                    // Play sound with talking audio source
                    talkingAudioSource.clip = clip;
                    talkingAudioSource.volume = talkingVolume;
                    talkingAudioSource.Play();
                    
                    Debug.Log("[CardLayerNPC] Playing talking sound: " + clip.name);
                    
                    // Wait until sound finishes playing plus minimum delay
                    float waitTime = clip.length + talkingSoundFrequency;
                    yield return new WaitForSeconds(waitTime);
                }
                else
                {
                    // If clip is null, just wait a bit
                    yield return new WaitForSeconds(talkingSoundFrequency);
                }
            }
            else
            {
                // If already playing, wait a bit
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        Debug.Log("[CardLayerNPC] Talking sound coroutine ended");
        talkingCoroutine = null;
    }
    
    // Optional: Visualize the interaction range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
} 