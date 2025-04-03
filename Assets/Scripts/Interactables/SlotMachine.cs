using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SlotMachine : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private float cooldownTime = 60f;
    
    [Header("Slot Machine Settings")]
    [SerializeField] private GameObject slotMachineUI;
    [SerializeField] private AudioClip spinSound;
    [SerializeField] private AudioClip winSound;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject floatingTextPrefab; // Assign a TextMeshPro prefab
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
    
    [Header("Floating Text")]
    [SerializeField] private float textDuration = 4f;
    [SerializeField] private float floatSpeed = 0.5f;
    
    private bool isInRange = false;
    private bool canInteract = true;
    private float cooldownTimer = 0f;
    private AudioSource audioSource;
    
    // References to UI elements
    private SlotMachineUI slotUI;
    private SlotMachineAnimator slotAnimator;
    
    // Coroutine management
    private Coroutine currentSlotMachineCoroutine;
    private bool isPlayingSlotMachine = false;
    
    private void Awake()
    {
        Debug.Log("SlotMachine: Awake");
        
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("SlotMachine: Added AudioSource component");
        }
        
        // Set up UI references
        if (slotMachineUI != null)
        {
            slotUI = slotMachineUI.GetComponent<SlotMachineUI>();
            if (slotUI == null)
            {
                Debug.LogError("SlotMachine: SlotMachineUI component not found on UI GameObject");
            }
            
            slotAnimator = slotMachineUI.GetComponent<SlotMachineAnimator>();
            if (slotAnimator == null)
            {
                Debug.Log("SlotMachine: No SlotMachineAnimator component found. This is optional.");
            }
            
            // Hide UI initially
            slotMachineUI.SetActive(false);
            Debug.Log("SlotMachine: UI initialized and hidden");
        }
        else
        {
            Debug.LogError("SlotMachine: No slotMachineUI GameObject assigned in inspector");
        }
    }
    
    private void Start()
    {
        // Hide the interaction prompt at start
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
            Debug.Log("SlotMachine: Interaction prompt hidden at start");
        }
        else
        {
            Debug.LogWarning("SlotMachine: No interaction prompt assigned!");
        }
        
        // Verify the UI references are valid
        VerifyUIReferences();
    }
    
    private void VerifyUIReferences()
    {
        if (slotMachineUI != null)
        {
            // Check that the slot machine UI has a Canvas component or is under a Canvas
            bool foundCanvas = false;
            Transform current = slotMachineUI.transform;
            while (current != null)
            {
                if (current.GetComponent<Canvas>() != null)
                {
                    foundCanvas = true;
                    break;
                }
                current = current.parent;
            }
            
            if (!foundCanvas)
            {
                Debug.LogError("SlotMachine: slotMachineUI is not under a Canvas! UI will not display correctly.");
            }
            
            // Double check SlotMachineUI component
            if (slotUI == null)
            {
                slotUI = slotMachineUI.GetComponent<SlotMachineUI>();
                if (slotUI == null)
                {
                    Debug.LogError("SlotMachine: SlotMachineUI component still not found on UI GameObject during verification.");
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up any coroutines
        if (currentSlotMachineCoroutine != null)
        {
            StopCoroutine(currentSlotMachineCoroutine);
            currentSlotMachineCoroutine = null;
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
        if (PlayerController.Instance == null) 
        {
            if (isInRange)
            {
                isInRange = false;
                UpdateInteractionPrompt();
            }
            return;
        }
        
        float distance = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        bool wasInRange = isInRange;
        isInRange = distance <= interactionRange;
        
        // Only update the prompt if the range status changed
        if (wasInRange != isInRange)
        {
            UpdateInteractionPrompt();
        }
    }
    
    private void UpdateInteractionPrompt()
    {
        // Show/hide interaction prompt based on distance and interaction state
        if (interactionPrompt != null)
        {
            bool shouldShow = isInRange && canInteract;
            if (interactionPrompt.activeSelf != shouldShow)
            {
                interactionPrompt.SetActive(shouldShow);
                if (shouldShow)
                {
                    Debug.Log("SlotMachine: Showing interaction prompt - player in range");
                }
                else
                {
                    Debug.Log("SlotMachine: Hiding interaction prompt - player out of range or can't interact");
                }
            }
        }
    }
    
    private void HandleInteraction()
    {
        // Prevent interaction if already playing
        if (isPlayingSlotMachine) return;
        
        if (isInRange && canInteract && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("SlotMachine: Interaction key pressed");
            
            // Check if the player has enough gold
            if (EconomyManager.Instance != null && EconomyManager.Instance.HasEnoughGold(spinCost))
            {
                Debug.Log($"SlotMachine: Player has enough gold ({spinCost}). Starting slot machine.");
                
                // Start the slot machine and track the coroutine
                if (currentSlotMachineCoroutine != null)
                {
                    StopCoroutine(currentSlotMachineCoroutine);
                }
                
                currentSlotMachineCoroutine = StartCoroutine(PlaySlotMachine());
            }
            else
            {
                Debug.Log($"SlotMachine: Not enough gold. Required: {spinCost}");
                ShowNotEnoughMoneyMessage();
            }
        }
    }
    
    private void HandleCooldown()
    {
        if (!canInteract && !isPlayingSlotMachine)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                canInteract = true;
                UpdateInteractionPrompt();
                Debug.Log("SlotMachine: Cooldown finished, interaction enabled");
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
                    Debug.Log("SlotMachine: Money message timer expired, hiding UI");
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
            // Ensure UI is visible
            slotMachineUI.SetActive(true);
            
            // Set up Canvas Group if present
            CanvasGroup canvasGroup = slotMachineUI.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            // Make sure parent canvas is enabled
            EnsureParentCanvasEnabled(slotMachineUI.transform);
            
            // Show message text
            if (slotUI != null)
            {
                slotUI.ShowResult(notEnoughMoneyMessage, 0); // Use damage color (red) for message
                Debug.Log("SlotMachine: Showing 'not enough money' message");
            }
        }
    }
    
    private void EnsureParentCanvasEnabled(Transform objTransform)
    {
        Transform parent = objTransform.parent;
        while (parent != null)
        {
            Canvas parentCanvas = parent.GetComponent<Canvas>();
            if (parentCanvas != null)
            {
                if (!parentCanvas.enabled)
                {
                    Debug.LogWarning($"SlotMachine: Parent canvas was disabled, enabling it: {parent.name}");
                    parentCanvas.enabled = true;
                }
                
                CanvasGroup canvasGroup = parent.GetComponent<CanvasGroup>();
                if (canvasGroup != null && canvasGroup.alpha < 1f)
                {
                    Debug.LogWarning($"SlotMachine: Parent canvas group alpha was {canvasGroup.alpha}, fixing to 1.0");
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
                
                if (!parent.gameObject.activeSelf)
                {
                    Debug.LogWarning($"SlotMachine: Parent canvas GameObject was inactive, activating it: {parent.name}");
                    parent.gameObject.SetActive(true);
                }
            }
            parent = parent.parent;
        }
    }
    
    private IEnumerator PlaySlotMachine()
    {
        Debug.Log("SlotMachine: Starting PlaySlotMachine sequence");
        
        // Set flag to prevent multiple plays
        isPlayingSlotMachine = true;
        
        // Spend the gold first - outside of try/catch
        bool goldSpent = false;
        if (EconomyManager.Instance != null)
        {
            goldSpent = EconomyManager.Instance.SpendGold(spinCost);
            if (!goldSpent)
            {
                Debug.LogWarning($"SlotMachine: Failed to spend {spinCost} gold!");
                ShowNotEnoughMoneyMessage();
                isPlayingSlotMachine = false;
                yield break;
            }
            else
            {
                Debug.Log($"SlotMachine: Successfully spent {spinCost} gold");
            }
        }
        
        // Prevent interaction during spin
        canInteract = false;
        UpdateInteractionPrompt();
        
        // Show the slot machine UI - still outside try/catch
        if (slotMachineUI == null)
        {
            Debug.LogError("SlotMachine: No UI panel assigned!");
            isPlayingSlotMachine = false;
            yield break;
        }
        
        // Set UI active
        slotMachineUI.SetActive(true);
        
        // Ensure the UI is visible
        CanvasGroup canvasGroup = slotMachineUI.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Make sure parent canvas is enabled
        EnsureParentCanvasEnabled(slotMachineUI.transform);
        
        Debug.Log("SlotMachine: UI panel activated");
        
        // Check if it's still not active after activating
        if (!slotMachineUI.activeSelf)
        {
            Debug.LogError("SlotMachine: UI GameObject did not activate properly!");
            
            // Force the entire parent canvas to be active
            Transform parent = slotMachineUI.transform.parent;
            while (parent != null)
            {
                parent.gameObject.SetActive(true);
                Canvas parentCanvas = parent.GetComponent<Canvas>();
                if (parentCanvas != null)
                {
                    parentCanvas.enabled = true;
                    break;
                }
                parent = parent.parent;
            }
            
            // Try activating again
            slotMachineUI.SetActive(true);
            
            // Double check
            if (!slotMachineUI.activeSelf)
            {
                Debug.LogError("SlotMachine: UI still won't activate! This is a critical error.");
                isPlayingSlotMachine = false;
                yield break;
            }
        }
        
        // Play spin sound
        if (spinSound != null && audioSource != null)
        {
            audioSource.clip = spinSound;
            audioSource.Play();
            Debug.Log("SlotMachine: Playing spin sound");
        }
        
        // Check if UI component exists before trying to use it
        if (slotUI == null)
        {
            Debug.LogWarning("SlotMachine: No SlotMachineUI component found!");
            yield return new WaitForSeconds(2f);
            
            // Clean up and exit if no UI
            if (slotMachineUI != null)
            {
                slotMachineUI.SetActive(false);
            }
            
            isPlayingSlotMachine = false;
            canInteract = true;
            cooldownTimer = cooldownTime;
            UpdateInteractionPrompt();
            yield break;
        }
        
        // Start the spin animation (no try-catch to avoid yield in try issues)
        Debug.Log("SlotMachine: Calling StartSpin on SlotMachineUI");
        slotUI.StartSpin();
        
        // Wait for spin animation to complete - outside try/catch
        float spinDuration = slotUI.SpinDuration;
        Debug.Log($"SlotMachine: Waiting for spin duration: {spinDuration} seconds");
        yield return new WaitForSeconds(spinDuration);
        
        // Sometimes the UI can disappear during the spin (race condition)
        if (slotMachineUI != null && !slotMachineUI.activeSelf)
        {
            Debug.LogWarning("SlotMachine: UI disappeared during spin! Re-activating");
            slotMachineUI.SetActive(true);
            EnsureParentCanvasEnabled(slotMachineUI.transform);
        }
        
        // Get the first icon type and match count - outside try/catch
        int firstIconType = slotUI.GetFirstIconType();
        
        // NEW: Get the actual reelResults array to check if first two positions match
        int[] reelResults = slotUI.GetReelResults();
        
        // Check if first two icons match (not just any two)
        bool firstTwoMatch = reelResults.Length >= 2 && reelResults[0] == reelResults[1];
        
        // Count total occurrences of first icon type
        int firstIconCount = slotUI.CountIconType(firstIconType);
        
        // Determine the effective count for upgrade calculation:
        // - If 3 of the same icon: count as 3
        // - If first two match: count as 2
        // - Otherwise count as 1 (even if first and third match but second doesn't)
        int effectiveCount = (firstIconCount == 3) ? 3 : (firstTwoMatch ? 2 : 1);
        
        Debug.Log($"SlotMachine: Spin complete - First Icon: {GetUpgradeTypeName(firstIconType)}, " +
                 $"First Icon Count: {firstIconCount}, First Two Match: {firstTwoMatch}, " +
                 $"Effective Count: {effectiveCount}");
        
        // Wait a moment after the spin stops
        yield return new WaitForSeconds(0.5f);
        
        // Apply the upgrade based on the first icon - in a try block without yields
        try
        {
            // Use effectiveCount instead of firstIconCount for the multiplier
            ApplyUpgrade(firstIconType, effectiveCount);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SlotMachine: Exception during upgrade application: {e.Message}\n{e.StackTrace}");
        }
        
        // Play win effects if there are matches of the first icon - outside try/catch
        if (effectiveCount > 1)
        {
            Debug.Log($"SlotMachine: Playing win effects for effective count {effectiveCount} of {GetUpgradeTypeName(firstIconType)}");
            
            if (winSound != null && audioSource != null)
            {
                audioSource.clip = winSound;
                audioSource.Play();
            }
            
            if (winParticles != null)
            {
                var main = winParticles.main;
                main.startSize = main.startSize.constant * (effectiveCount == 3 ? 1.5f : 1f);
                main.startSpeed = main.startSpeed.constant * (effectiveCount == 3 ? 1.5f : 1f);
                winParticles.Play();
            }
        }
        
        // Show the result message
        string resultMessage = effectiveCount > 1 ? 
            $"Great! {effectiveCount}x {GetUpgradeTypeName(firstIconType)} boost!" : 
            $"Got {GetUpgradeTypeName(firstIconType)} boost!";
        
        Debug.Log($"SlotMachine: Showing result message: '{resultMessage}'");
        slotUI.ShowResult(resultMessage, firstIconType);
        
        // Keep the result visible for 3 seconds
        yield return new WaitForSeconds(3f);
        
        // Hide the UI
        if (slotMachineUI != null)
        {
            slotMachineUI.SetActive(false);
            Debug.Log("SlotMachine: UI panel deactivated");
        }
        
        // Clean up
        isPlayingSlotMachine = false;
        canInteract = true;
        cooldownTimer = cooldownTime;
        UpdateInteractionPrompt();
        Debug.Log("SlotMachine: Interaction re-enabled");
    }
    
    private void ApplyUpgrade(int upgradeType, int iconCount)
    {
        if (PlayerController.Instance == null)
        {
            Debug.LogWarning("SlotMachine: Can't apply upgrade - PlayerController.Instance is null!");
            return;
        }
        
        // Calculate multiplier based on count of the specific icon type (not any matches)
        // New multiplier values: 0.3x for one symbol, 1x for two symbols, 5x for three symbols
        float multiplier = iconCount == 3 ? 5f : (iconCount == 2 ? 1f : 0.3f);
        
        Debug.Log($"SlotMachine: UPGRADE CALCULATION - Type: {upgradeType} ({GetUpgradeTypeName(upgradeType)}), " +
                 $"Icon Count: {iconCount}, Raw Multiplier: {multiplier}");

        // Get the base upgrade values before applying multiplier
        float damageBase = damageUpgradeAmount;
        float healthBase = healthUpgradeAmount;
        float speedBase = speedUpgradeAmount;
        float dashBase = dashUpgradeAmount;
        
        // Calculate final upgrade amount with multiplier
        float finalUpgradeAmount = 0f;
        
        // Apply the upgrade based on the first icon type
        switch (upgradeType)
        {
            case 0: // Damage (Red)
                finalUpgradeAmount = damageBase * multiplier;
                Debug.Log($"SlotMachine: Applying Damage upgrade - Base: {damageBase}, Multiplier: {multiplier}, Final: {finalUpgradeAmount}");
                UpgradeDamage(finalUpgradeAmount);
                break;
            case 1: // Health (Green)
                finalUpgradeAmount = healthBase * multiplier;
                Debug.Log($"SlotMachine: Applying Health upgrade - Base: {healthBase}, Multiplier: {multiplier}, Final: {finalUpgradeAmount}");
                UpgradeHealth(finalUpgradeAmount);
                break;
            case 2: // Speed (Blue)
                finalUpgradeAmount = speedBase * multiplier;
                Debug.Log($"SlotMachine: Applying Speed upgrade - Base: {speedBase}, Multiplier: {multiplier}, Final: {finalUpgradeAmount}");
                UpgradeSpeed(finalUpgradeAmount);
                break;
            case 3: // Dash (Yellow)
                finalUpgradeAmount = dashBase * multiplier;
                Debug.Log($"SlotMachine: Applying Dash upgrade - Base: {dashBase}, Multiplier: {multiplier}, Final: {finalUpgradeAmount}");
                UpgradeDash(finalUpgradeAmount);
                break;
            default:
                Debug.LogWarning($"SlotMachine: Unknown upgrade type: {upgradeType}");
                break;
        }
        
        // Create floating text to show the upgrade
        string upgradeMessage = iconCount == 3 ? 
            $"{GetUpgradeTypeName(upgradeType)} MASSIVELY increased!" : 
            (iconCount == 2 ? $"{GetUpgradeTypeName(upgradeType)} increased!" : 
            $"{GetUpgradeTypeName(upgradeType)} slightly increased!");
        CreateFloatingText(PlayerController.Instance.transform.position, upgradeMessage, GetUpgradeColor(upgradeType));
        
        Debug.Log($"Applied {GetUpgradeTypeName(upgradeType)} upgrade with {multiplier}x multiplier");
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
        Debug.Log($"SlotMachine: Creating floating text: '{text}' at {position}");
        
        // Use the new FloatingText system if prefab is assigned
        if (floatingTextPrefab != null)
        {
            FloatingText.Create(floatingTextPrefab, position, text, color);
        }
        else
        {
            Debug.LogWarning("SlotMachine: No floatingTextPrefab assigned! Using fallback method.");
            
            // Fallback method using simple TextMesh
            GameObject simpleText = new GameObject("SimpleFloatingText");
            simpleText.transform.position = position + Vector3.up * 2.5f;
            
            // Add TextMesh component
            TextMesh textMesh = simpleText.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.color = color;
            textMesh.fontSize = 40;
            textMesh.characterSize = 0.07f;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.fontStyle = FontStyle.Normal;
            
            // Add renderer
            MeshRenderer meshRenderer = simpleText.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = new Material(Shader.Find("GUI/Text Shader"));
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
            }
            
            // Face the camera
            if (Camera.main != null)
            {
                simpleText.transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
            }
            
            // Simple anonymous movement method instead of SimpleTextMover
            StartCoroutine(AnimateFloatingText(simpleText, 3f));
            
            // Destroy after duration
            Destroy(simpleText, 3f);
        }
    }
    
    // Coroutine to animate text without needing a separate class
    private IEnumerator AnimateFloatingText(GameObject textObj, float duration)
    {
        float startTime = Time.time;
        TextMesh textMesh = textObj.GetComponent<TextMesh>();
        Color originalColor = textMesh.color;
        
        while (Time.time - startTime < duration)
        {
            // Move upward
            textObj.transform.Translate(Vector3.up * Time.deltaTime);
            
            // Face camera
            if (Camera.main != null)
            {
                textObj.transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
            }
            
            // Fade out in the second half
            float t = (Time.time - startTime) / duration;
            if (t > 0.5f && textMesh != null)
            {
                Color newColor = originalColor;
                newColor.a = 1f - ((t - 0.5f) * 2f);
                textMesh.color = newColor;
            }
            
            yield return null;
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
    
    private void UpgradeDamage(float amount)
    {
        // Get card thrower reference from player
        CardThrower cardThrower = PlayerController.Instance.GetComponent<CardThrower>();
        if (cardThrower != null)
        {
            // Increase damage of all card types
            float beforeDamage = cardThrower.GetCurrentDamage();
            cardThrower.IncreaseDamage(amount);
            float afterDamage = cardThrower.GetCurrentDamage();
            
            Debug.Log($"SlotMachine: Damage upgrade applied - Before: {beforeDamage}, Added: {amount}, After: {afterDamage}, " +
                     $"Actual Increase: {afterDamage - beforeDamage}");
        }
        else
        {
            Debug.LogWarning("SlotMachine: CardThrower component not found on player!");
        }
    }
    
    private void UpgradeHealth(float amount)
    {
        // Increase max health
        PlayerController player = PlayerController.Instance;
        if (player != null)
        {
            float beforeHealth = player.GetMaxHealth();
            player.IncreaseMaxHealth(amount);
            float afterHealth = player.GetMaxHealth();
            
            Debug.Log($"SlotMachine: Health upgrade applied - Before: {beforeHealth}, Added: {amount}, After: {afterHealth}, " +
                     $"Actual Increase: {afterHealth - beforeHealth}");
        }
    }
    
    private void UpgradeSpeed(float amount)
    {
        // Use the exact amount specified
        PlayerController player = PlayerController.Instance;
        if (player != null)
        {
            // Track before and after values for debugging
            float beforeSpeed = player.GetCurrentMovementSpeed();
            player.IncreaseMovementSpeed(amount);
            float afterSpeed = player.GetCurrentMovementSpeed();
            
            Debug.Log($"SlotMachine: Speed upgrade applied - Before: {beforeSpeed}, Added: {amount}, After: {afterSpeed}, " +
                     $"Actual Increase: {afterSpeed - beforeSpeed}");
        }
    }
    
    private void UpgradeDash(float amount)
    {
        // Increase dash range/speed
        PlayerController player = PlayerController.Instance;
        if (player != null)
        {
            float beforeDash = player.GetCurrentDashPower();
            player.IncreaseDashPower(amount);
            float afterDash = player.GetCurrentDashPower();
            
            Debug.Log($"SlotMachine: Dash upgrade applied - Before: {beforeDash}, Added: {amount}, After: {afterDash}, " +
                     $"Actual Increase: {afterDash - beforeDash}");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualize interaction range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}