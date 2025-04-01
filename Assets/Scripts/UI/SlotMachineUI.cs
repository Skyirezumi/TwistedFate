using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotMachineUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image[] reelImages; // 3 reels
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private ParticleSystem confettiEffect;
    [SerializeField] private Animator slotAnimator; // This expects Unity's Animator component
    [SerializeField] private RectTransform canvasRect;
    
    [Header("Icon References")]
    [SerializeField] private Sprite[] symbolSprites; // 0 = damage, 1 = health, 2 = speed, 3 = dash, etc.
    [SerializeField] private bool useColoredSquaresInsteadOfSprites = false; // NEW option to use colored squares
    
    // Define upgrade types based on sprite names
    [Header("Icon Type Mappings")]
    [SerializeField] private string damageIconKeyword = "damage";
    [SerializeField] private string healthIconKeyword = "health";
    [SerializeField] private string speedIconKeyword = "speed";
    [SerializeField] private string dashIconKeyword = "dash";
    
    [Header("Animation Settings")]
    [SerializeField] private float spinDuration = 1.5f;
    [SerializeField] private float reelStopDelay = 0.3f;
    [SerializeField] private int spinSpeed = 20; // Frames per second for spinning
    
    [Header("UI Positioning")]
    [SerializeField] private bool positionAtBottomLeft = true;
    [SerializeField] private float bottomLeftPadding = 20f;
    [SerializeField] private Vector2 customSize = new Vector2(300f, 200f);
    
    // Force icons to specific types based on their array position
    [Header("Fixed Icon Types")]
    [Tooltip("When enabled, icon types are based solely on array position, not sprite names")]
    [SerializeField] private bool useFixedIconOrder = true;
    
    // Will be referenced by SlotMachine script
    public float SpinDuration => spinDuration;
    
    private int[] reelResults = new int[3]; // Stores the final result for each reel
    private Coroutine spinCoroutine;
    
    private void Awake()
    {
        // Position the UI at bottom left if requested
        if (positionAtBottomLeft && canvasRect != null)
        {
            PositionAtBottomLeft();
        }
        
        // Make sure we have a reference to the animator
        if (slotAnimator == null)
        {
            slotAnimator = GetComponent<Animator>();
        }
        
        // Hide result text initially
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
        
        // Make sure the reel images have their Image Type set properly and are visible
        if (reelImages != null)
        {
            Debug.Log($"SlotMachineUI: Found {reelImages.Length} reel images to set up");
            for (int i = 0; i < reelImages.Length; i++)
            {
                Image reel = reelImages[i];
                if (reel != null)
                {
                    // Important: Make sure the image is fully opaque
                    Color color = Color.white;
                    color.a = 1f;
                    reel.color = color;
                    
                    // Make sure the image is enabled
                    reel.enabled = true;
                    
                    // Log for debugging
                    Debug.Log($"SlotMachineUI: Set up reel {i} - Image enabled: {reel.enabled}, Color: {reel.color}, Raycast Target: {reel.raycastTarget}");
                    
                    // Make sure it has a sprite or simple square background
                    if (reel.sprite == null && useColoredSquaresInsteadOfSprites)
                    {
                        // Ensure we're using a basic square sprite for the image
                        try 
                        {
                            reel.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                            if (reel.sprite == null)
                            {
                                Debug.LogWarning($"Reel {i} couldn't get default sprite - making a white filled image");
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"Error getting default sprite: {e.Message}");
                        }
                        
                        // Make sure it's visible as a simple color square
                        reel.type = Image.Type.Simple;
                        reel.color = new Color(1f, 1f, 1f, 1f); // Full white and opaque
                    }
                }
                else
                {
                    Debug.LogError($"SlotMachineUI: Reel image at index {i} is null!");
                }
            }
        }
        else
        {
            Debug.LogError("SlotMachineUI: No reel images assigned!");
        }
        
        // Check if we have sprites assigned
        if (symbolSprites == null || symbolSprites.Length < 4)
        {
            Debug.LogWarning("SlotMachineUI: Not enough symbol sprites assigned, using colored squares instead");
            useColoredSquaresInsteadOfSprites = true;
        }
        else
        {
            Debug.Log($"SlotMachineUI: Using {symbolSprites.Length} custom icon sprites");
            
            // Log sprite names for debugging
            for (int i = 0; i < symbolSprites.Length; i++)
            {
                if (symbolSprites[i] != null)
                {
                    Debug.Log($"Icon {i}: {symbolSprites[i].name}");
                    
                    // Automatically detect icon types based on name
                    if (symbolSprites[i].name.ToLower().Contains(damageIconKeyword))
                    {
                        Debug.Log($"Detected Damage icon: {symbolSprites[i].name}");
                    }
                    else if (symbolSprites[i].name.ToLower().Contains(healthIconKeyword))
                    {
                        Debug.Log($"Detected Health icon: {symbolSprites[i].name}");
                    }
                    else if (symbolSprites[i].name.ToLower().Contains(speedIconKeyword))
                    {
                        Debug.Log($"Detected Speed icon: {symbolSprites[i].name}");
                    }
                    else if (symbolSprites[i].name.ToLower().Contains(dashIconKeyword))
                    {
                        Debug.Log($"Detected Dash icon: {symbolSprites[i].name}");
                    }
                }
            }
            
            useColoredSquaresInsteadOfSprites = false;
        }
    }
    
    private void PositionAtBottomLeft()
    {
        // Get the rect transform
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Set to bottom left anchor
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0, 0);
            
            // Set size
            rectTransform.sizeDelta = customSize;
            
            // Set position with padding
            rectTransform.anchoredPosition = new Vector2(bottomLeftPadding, bottomLeftPadding);
        }
    }
    
    public void StartSpin()
    {
        Debug.Log("SlotMachineUI: StartSpin called");
        
        // Stop any existing spin
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
        }
        
        // Hide previous result text
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
        
        // Play animation if available
        if (slotAnimator != null)
        {
            slotAnimator.SetTrigger("Spin");
        }
        
        // Start the spinning coroutine
        spinCoroutine = StartCoroutine(SpinReels());
    }
    
    private IEnumerator SpinReels()
    {
        Debug.Log("SlotMachineUI: SpinReels started");
        
        // Safety check
        if (reelImages == null || reelImages.Length == 0)
        {
            Debug.LogWarning("No reel images assigned to SlotMachineUI");
            yield break;
        }
        
        // Start all reels spinning
        for (int i = 0; i < reelImages.Length; i++)
        {
            if (reelImages[i] != null)
            {
                StartCoroutine(SpinReel(reelImages[i], i));
                yield return new WaitForSeconds(reelStopDelay); // Stagger the spin start slightly
            }
            else
            {
                Debug.LogError($"SlotMachineUI: Reel image at index {i} is null!");
            }
        }
    }
    
    private IEnumerator SpinReel(Image reelImage, int reelIndex)
    {
        Debug.Log($"SlotMachineUI: Starting spin for reel {reelIndex}");
        
        // If we're using colored squares instead of sprites, we'll bypass the sprite check
        if (!useColoredSquaresInsteadOfSprites && (symbolSprites == null || symbolSprites.Length == 0))
        {
            Debug.LogWarning("No symbol sprites assigned to SlotMachineUI - using colored squares instead");
            // Force colored squares mode
            useColoredSquaresInsteadOfSprites = true;
        }
        
        float spinTime = 0f;
        float reelSpinDuration = spinDuration - (reelIndex * reelStopDelay); // Each reel stops in sequence
        
        // Calculate how many frames we'll show during the spin
        int frameCount = Mathf.FloorToInt(reelSpinDuration * spinSpeed);
        
        // Define colors for each upgrade type
        Color[] upgradeColors = { Color.red, Color.green, Color.cyan, Color.yellow };
        
        // Spin the reel
        for (int i = 0; i < frameCount; i++)
        {
            // Choose a random symbol or color
            int randomIndex = Random.Range(0, 4); // Always use 4 types (damage, health, speed, dash)
            
            if (useColoredSquaresInsteadOfSprites)
            {
                // Use colored squares
                reelImage.sprite = null;
                reelImage.color = upgradeColors[randomIndex];
            }
            else
            {
                // Use sprites if available
                if (symbolSprites != null && symbolSprites.Length > randomIndex)
                {
                    reelImage.sprite = symbolSprites[randomIndex];
                    reelImage.color = Color.white; // Reset color when using sprites
                }
                else
                {
                    // Fallback to colored square
                    reelImage.sprite = null;
                    reelImage.color = upgradeColors[randomIndex];
                }
            }
            
            // If we're near the end, slow down the spinning
            float progress = (float)i / frameCount;
            float delay = Mathf.Lerp(1f / spinSpeed, 0.1f, progress);
            
            yield return new WaitForSeconds(delay);
        }
        
        // Set the final result for this reel
        int resultIndex = Random.Range(0, 4); // Always use 4 upgrade types
        
        // Apply the final look based on the result
        if (useColoredSquaresInsteadOfSprites)
        {
            // Use colored squares
            reelImage.sprite = null;
            reelImage.color = upgradeColors[resultIndex];
            
            // Store the result by icon type (0-3)
            reelResults[reelIndex] = resultIndex;
        }
        else if (symbolSprites != null && symbolSprites.Length > resultIndex)
        {
            // Use sprites if available
            Sprite resultSprite = symbolSprites[resultIndex];
            reelImage.sprite = resultSprite;
            reelImage.color = Color.white;
            
            // Determine the upgrade type
            int iconType;
            
            if (useFixedIconOrder)
            {
                // In fixed order mode, the sprite index IS the upgrade type
                iconType = resultIndex;
                Debug.Log($"Reel {reelIndex} landed on sprite at index {resultIndex} = {GetUpgradeTypeName(iconType)}");
            }
            else
            {
                // Try to determine from sprite name
                iconType = GetIconTypeFromSprite(resultSprite);
                Debug.Log($"Reel {reelIndex} landed on {resultSprite.name} (Type: {iconType})");
            }
            
            // Store the result by icon type (0-3)
            reelResults[reelIndex] = iconType;
        }
        else
        {
            // Fallback to colored square
            reelImage.sprite = null;
            reelImage.color = upgradeColors[resultIndex];
            
            // Store the result by icon type (0-3)
            reelResults[reelIndex] = resultIndex;
        }
        
        // If this is the last reel, we've completed the spin
        if (reelIndex == reelImages.Length - 1)
        {
            // All reels have stopped, check the result
            // This is handled by the main SlotMachine script
            
            // Count how many matches we have
            int matchCount = CountMatches();
            string matchResult = matchCount == 1 ? "No matches" : 
                                 matchCount == 2 ? "Two matching symbols" : 
                                 "Three matching symbols";
            
            Debug.Log($"Spin result: {matchResult} - Reel values: [{reelResults[0]}, {reelResults[1]}, {reelResults[2]}]");
            
            // Play win animation only if we have at least 2 matching icons
            if (matchCount >= 2 && confettiEffect != null)
            {
                confettiEffect.Play();
                // Play bigger effect for 3 matches
                if (matchCount == 3)
                {
                    // Make the effect more intense for a perfect match
                    var main = confettiEffect.main;
                    main.startSize = main.startSize.constant * 1.5f;
                    main.startSpeed = main.startSpeed.constant * 1.5f;
                    main.maxParticles = main.maxParticles * 2;
                }
            }
        }
    }
    
    public void ShowResult(string message, int upgradeType)
    {
        Debug.Log($"SlotMachineUI: Showing result message: '{message}' with type {upgradeType}");
        
        // Get the color for this upgrade type
        Color upgradeColor = Color.white;
        switch (upgradeType)
        {
            case 0: // Damage upgrade
                upgradeColor = Color.red;
                break;
            case 1: // Health upgrade
                upgradeColor = Color.green;
                break;
            case 2: // Speed upgrade
                upgradeColor = Color.cyan;
                break;
            case 3: // Dash upgrade
                upgradeColor = Color.yellow;
                break;
        }
        
        // Make all reels show the same result
        if (reelImages != null)
        {
            foreach (Image reel in reelImages)
            {
                if (useColoredSquaresInsteadOfSprites || symbolSprites == null || symbolSprites.Length <= upgradeType)
                {
                    // Use colored squares
                    reel.sprite = null;
                    reel.color = upgradeColor;
                }
                else
                {
                    // Use sprites
                    // Find the appropriate sprite that matches the upgrade type
                    Sprite matchingSprite = null;
                    
                    // First try to find by icon name
                    foreach (Sprite sprite in symbolSprites)
                    {
                        if (sprite != null)
                        {
                            int iconType = GetIconTypeFromSprite(sprite);
                            if (iconType == upgradeType)
                            {
                                matchingSprite = sprite;
                                break;
                            }
                        }
                    }
                    
                    // If no matching sprite found, fall back to index
                    if (matchingSprite == null && symbolSprites.Length > upgradeType)
                    {
                        matchingSprite = symbolSprites[upgradeType];
                    }
                    
                    if (matchingSprite != null)
                    {
                        reel.sprite = matchingSprite;
                        reel.color = Color.white;
                    }
                    else
                    {
                        // Last resort fallback
                        reel.sprite = null;
                        reel.color = upgradeColor;
                    }
                }
            }
        }
        
        // Show the result text
        if (resultText != null)
        {
            resultText.text = message;
            resultText.gameObject.SetActive(true);
            
            // Animate the text if animator exists
            if (slotAnimator != null)
            {
                slotAnimator.SetTrigger("ShowResult");
            }
        }
        
        // Play confetti
        if (confettiEffect != null)
        {
            confettiEffect.Play();
        }
    }
    
    // New helper method to count matches
    private int CountMatches()
    {
        // If we have 3 reels
        if (reelResults.Length == 3)
        {
            // Check for three of a kind
            if (reelResults[0] == reelResults[1] && reelResults[1] == reelResults[2])
            {
                return 3; // All three match
            }
            
            // Check for pairs
            if (reelResults[0] == reelResults[1] || 
                reelResults[1] == reelResults[2] || 
                reelResults[0] == reelResults[2])
            {
                return 2; // Two match
            }
            
            return 1; // No matches (all different)
        }
        
        // Default fallback
        return 0;
    }
    
    // Get the matching icon type (0=damage, 1=health, 2=speed, 3=dash)
    public int GetMatchingIconType()
    {
        // If we have 3 reels
        if (reelResults.Length == 3)
        {
            Debug.Log($"Finding matching icon type from results: [{reelResults[0]}, {reelResults[1]}, {reelResults[2]}]");
            
            // Three of a kind
            if (reelResults[0] == reelResults[1] && reelResults[1] == reelResults[2])
            {
                Debug.Log($"3 matching icons of type {reelResults[0]} ({GetUpgradeTypeName(reelResults[0])})");
                return reelResults[0]; // Return the icon type that matched
            }
            
            // Pairs - find which icon appears twice
            if (reelResults[0] == reelResults[1])
            {
                Debug.Log($"Pair of matching icons in reels 0 and 1: type {reelResults[0]} ({GetUpgradeTypeName(reelResults[0])})");
                return reelResults[0];
            }
            if (reelResults[1] == reelResults[2])
            {
                Debug.Log($"Pair of matching icons in reels 1 and 2: type {reelResults[1]} ({GetUpgradeTypeName(reelResults[1])})");
                return reelResults[1];
            }
            if (reelResults[0] == reelResults[2])
            {
                Debug.Log($"Pair of matching icons in reels 0 and 2: type {reelResults[0]} ({GetUpgradeTypeName(reelResults[0])})");
                return reelResults[0];
            }
        }
        
        // No matches or error - just return a random icon type
        int randomType = Random.Range(0, 4);
        Debug.Log($"No matching pairs found, returning random type: {randomType}");
        return randomType;
    }
    
    // Get the name of the upgrade type for debugging
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
    
    // Add a method to get the match count for the SlotMachine script
    public int GetMatchCount()
    {
        return CountMatches();
    }
    
    // New helper method to determine icon type based on sprite name
    private int GetIconTypeFromSprite(Sprite sprite)
    {
        if (sprite == null) return 0; // Default to damage type
        
        string spriteName = sprite.name.ToLower();
        Debug.Log($"Checking sprite type for: {spriteName}");
        
        // Debug all icon keywords for matching
        Debug.Log($"Looking for keywords - Damage: '{damageIconKeyword}', Health: '{healthIconKeyword}', " +
                 $"Speed: '{speedIconKeyword}', Dash: '{dashIconKeyword}'");
        
        // Make string comparisons more flexible by trimming and using contains
        if (spriteName.Contains(damageIconKeyword.ToLower().Trim()))
        {
            Debug.Log($"Identified as DAMAGE icon: {spriteName}");
            return 0; // Damage
        }
        else if (spriteName.Contains(healthIconKeyword.ToLower().Trim()))
        {
            Debug.Log($"Identified as HEALTH icon: {spriteName}");
            return 1; // Health
        }
        else if (spriteName.Contains(speedIconKeyword.ToLower().Trim()))
        {
            Debug.Log($"Identified as SPEED icon: {spriteName}");
            return 2; // Speed
        }
        else if (spriteName.Contains(dashIconKeyword.ToLower().Trim()))
        {
            Debug.Log($"Identified as DASH icon: {spriteName}");
            return 3; // Dash
        }
        
        // If we're here, try a different approach - checking sprite position in array
        Debug.Log("Keyword matching failed, trying position-based identification");
        for (int i = 0; i < symbolSprites.Length; i++)
        {
            if (symbolSprites[i] == sprite)
            {
                Debug.Log($"Identified by position: Icon type {i} based on position in symbolSprites array");
                switch (i % 4) {
                    case 0: return 0; // Damage
                    case 1: return 1; // Health
                    case 2: return 2; // Speed
                    case 3: return 3; // Dash
                    default: return i % 4;
                }
            }
        }
        
        Debug.LogWarning($"Could not identify icon type for sprite: {spriteName}. Assuming DAMAGE type.");
        return 0; // Default to damage
    }
} 