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
    [SerializeField] private bool useColoredSquaresInsteadOfSprites = false; // Option to use colored squares
    
    // Define upgrade types based on sprite names
    [Header("Icon Type Mappings")]
    [SerializeField] private string damageIconKeyword = "damage";
    [SerializeField] private string healthIconKeyword = "health";
    [SerializeField] private string speedIconKeyword = "speed";
    [SerializeField] private string dashIconKeyword = "dash";
    
    [Header("Animation Settings")]
    [SerializeField] private float spinDuration = 0.5f;
    [SerializeField] private int spinSpeed = 20; // Frames per second for spinning
    [SerializeField] private float reelStopDelay = 0.5f; // Delay between each reel stopping
    
    [Header("UI Positioning")]
    [SerializeField] private bool positionAtBottomLeft = true;
    [SerializeField] private float bottomLeftPadding = 20f;
    [SerializeField] private Vector2 customSize = new Vector2(300f, 200f);
    
    // Force icons to specific types based on their array position
    [Header("Fixed Icon Types")]
    [Tooltip("When enabled, icon types are based solely on array position, not sprite names")]
    [SerializeField] private bool useFixedIconOrder = true;
    
    // Will be referenced by SlotMachine script
    public float SpinDuration 
    { 
        get 
        {
            // Total duration includes base spin time plus delays for each reel after the first
            return spinDuration + (reelStopDelay * (reelImages != null ? reelImages.Length - 1 : 0));
        } 
    }
    
    private int[] reelResults = new int[3]; // Stores the final result for each reel
    private Coroutine spinCoroutine;
    private List<Coroutine> activeCoroutines = new List<Coroutine>();
    private bool isSpinning = false;
    
    private void Awake()
    {
        Debug.Log("SlotMachineUI: Awake called");
        
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
        else
        {
            Debug.LogError("SlotMachineUI: ResultText is null! UI will not show results.");
        }
        
        // Make sure the reel images have their Image Type set properly and are visible
        if (reelImages != null && reelImages.Length > 0)
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
            Debug.LogError("SlotMachineUI: No reel images assigned or the array is empty!");
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
                }
                else
                {
                    Debug.LogWarning($"SlotMachineUI: Symbol sprite at index {i} is null!");
                }
            }
            
            useColoredSquaresInsteadOfSprites = false;
        }

        // Ensure the UI is visible
        EnsureCanvasVisibility();
        
        // Log UI state
        Debug.Log($"SlotMachineUI: UI GameObject initialized");
    }
    
    private void Start()
    {
        // Double check all parent canvases are enabled
        EnsureParentCanvasEnabled();
    }
    
    private void OnEnable()
    {
        Debug.Log("SlotMachineUI: OnEnable called - ensuring UI visibility");
        
        // Check all images are enabled and visible
        EnsureReelImagesVisible();
        
        // Make sure all parent canvases are enabled
        EnsureParentCanvasEnabled();
    }
    
    private void OnDisable()
    {
        Debug.Log("SlotMachineUI: OnDisable called - cleaning up coroutines");
        
        // Stop all active coroutines
        StopAllCoroutines();
        activeCoroutines.Clear();
        isSpinning = false;
    }
    
    private void EnsureReelImagesVisible()
    {
        if (reelImages == null || reelImages.Length == 0) return;
        
        foreach (Image reel in reelImages)
        {
            if (reel != null)
            {
                if (!reel.enabled)
                {
                    Debug.LogWarning($"SlotMachineUI: Reel image was disabled, enabling it: {reel.name}");
                    reel.enabled = true;
                }
                
                // Make sure alpha is 1
                Color c = reel.color;
                if (c.a < 1f)
                {
                    Debug.LogWarning($"SlotMachineUI: Reel image alpha was {c.a}, fixing to 1.0");
                    c.a = 1f;
                    reel.color = c;
                }
                
                // Make sure its parent is active
                if (reel.transform.parent != null && !reel.transform.parent.gameObject.activeSelf)
                {
                    Debug.LogWarning($"SlotMachineUI: Reel parent was inactive, activating it: {reel.transform.parent.name}");
                    reel.transform.parent.gameObject.SetActive(true);
                }
            }
        }
    }
    
    private void EnsureParentCanvasEnabled()
    {
        Transform parent = transform.parent;
        while (parent != null)
        {
            Canvas parentCanvas = parent.GetComponent<Canvas>();
            if (parentCanvas != null)
            {
                if (!parentCanvas.enabled)
                {
                    Debug.LogWarning($"SlotMachineUI: Parent canvas was disabled, enabling it: {parent.name}");
                    parentCanvas.enabled = true;
                }
                
                CanvasGroup canvasGroup = parent.GetComponent<CanvasGroup>();
                if (canvasGroup != null && canvasGroup.alpha < 1f)
                {
                    Debug.LogWarning($"SlotMachineUI: Parent canvas group alpha was {canvasGroup.alpha}, fixing to 1.0");
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
                
                if (!parent.gameObject.activeSelf)
                {
                    Debug.LogWarning($"SlotMachineUI: Parent canvas GameObject was inactive, activating it: {parent.name}");
                    parent.gameObject.SetActive(true);
                }
            }
            parent = parent.parent;
        }
    }
    
    private void EnsureCanvasVisibility()
    {
        if (gameObject != null)
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                if (canvasGroup.alpha < 1f)
                {
                    Debug.LogWarning($"SlotMachineUI: Canvas group alpha was {canvasGroup.alpha}, fixing to 1.0");
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
            }
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
        
        // If already spinning, ignore this request
        if (isSpinning)
        {
            Debug.LogWarning("SlotMachineUI: Ignoring StartSpin request because the slot machine is already spinning");
            return;
        }
        
        // Stop any existing coroutines
        StopAllActiveCoroutines();
        
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
        isSpinning = true;
        spinCoroutine = StartCoroutine(SpinReels());
        activeCoroutines.Add(spinCoroutine);
    }
    
    private void StopAllActiveCoroutines()
    {
        // Stop all active coroutines
        foreach (Coroutine coroutine in activeCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeCoroutines.Clear();
    }
    
    private IEnumerator SpinReels()
    {
        Debug.Log("SlotMachineUI: SpinReels started");
        
        // Safety check
        if (reelImages == null || reelImages.Length == 0)
        {
            Debug.LogError("SlotMachineUI: No reel images assigned! Cannot spin reels.");
            isSpinning = false;
            yield break;
        }
        
        // Ensure UI visibility before spinning
        EnsureReelImagesVisible();
        EnsureParentCanvasEnabled();
        EnsureCanvasVisibility();
        
        // Generate all results first
        for (int i = 0; i < reelImages.Length; i++)
        {
            reelResults[i] = Random.Range(0, 4);
            Debug.Log($"SlotMachineUI: Generated result for reel {i}: {reelResults[i]} ({GetUpgradeTypeName(reelResults[i])})");
        }
        
        // Log the reels in a clearer format
        Debug.Log($"SlotMachineUI: FULL REEL RESULTS - " +
                  $"[{reelResults[0]} ({GetUpgradeTypeName(reelResults[0])}), " +
                  $"{reelResults[1]} ({GetUpgradeTypeName(reelResults[1])}), " +
                  $"{reelResults[2]} ({GetUpgradeTypeName(reelResults[2])})]");
        
        // Start all reels spinning
        List<Coroutine> spinCoroutines = new List<Coroutine>();
        for (int i = 0; i < reelImages.Length; i++)
        {
            if (reelImages[i] != null)
            {
                // Create a longer spin duration for each successive reel
                float adjustedSpinDuration = spinDuration + (i * reelStopDelay);
                
                Coroutine reelSpin = StartCoroutine(SpinReel(reelImages[i], i, adjustedSpinDuration));
                spinCoroutines.Add(reelSpin);
                activeCoroutines.Add(reelSpin);
            }
        }
        
        // Wait for all reels to finish (base duration + delay for each additional reel)
        float totalDuration = spinDuration + (reelStopDelay * (reelImages.Length - 1));
        Debug.Log($"SlotMachineUI: Total spin sequence will take {totalDuration} seconds");
        
        yield return new WaitForSeconds(totalDuration);
        
        // Calculate results for debugging
        int numMatches = CountMatches();
        int firstIconType = GetFirstIconType();
        int firstIconCount = CountIconType(firstIconType);
        
        // Log the final results with detailed analysis
        Debug.Log($"SlotMachineUI: Spin complete - Final Results: [{reelResults[0]} ({GetUpgradeTypeName(reelResults[0])}), " +
                 $"{reelResults[1]} ({GetUpgradeTypeName(reelResults[1])}), " +
                 $"{reelResults[2]} ({GetUpgradeTypeName(reelResults[2])})]");
        
        Debug.Log($"SlotMachineUI: ANALYSIS - " +
                 $"First Icon: {firstIconType} ({GetUpgradeTypeName(firstIconType)}), " +
                 $"First Icon Count: {firstIconCount}, " +
                 $"Total Match Pairs: {numMatches}");
        
        // Spin is now complete
        isSpinning = false;
    }
    
    private IEnumerator SpinReel(Image reelImage, int reelIndex, float duration)
    {
        if (reelImage == null)
        {
            Debug.LogError($"SlotMachineUI: Reel image is null for reel {reelIndex}!");
            yield break;
        }
        
        Debug.Log($"SlotMachineUI: Spinning reel {reelIndex} for {duration} seconds");
        
        // Define colors for each upgrade type
        Color[] upgradeColors = { Color.red, Color.green, Color.cyan, Color.yellow };
        
        // Calculate total frames for the spin
        int totalFrames = Mathf.FloorToInt(duration * spinSpeed);
        float frameDelay = 1f / spinSpeed;
        
        // Force image to be enabled and visible
        reelImage.enabled = true;
        
        // Spin animation
        for (int i = 0; i < totalFrames; i++)
        {
            // Show random symbol during spin
            int randomIndex = Random.Range(0, 4);
            UpdateReelImage(reelImage, randomIndex, upgradeColors);
            
            yield return new WaitForSeconds(frameDelay);
        }
        
        // Show final result at the end of this reel's spin
        ShowFinalResult(reelImage, reelResults[reelIndex]);
        
        // Play a sound effect when this reel stops (if available)
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip, 0.5f);
        }
    }
    
    private void ShowFinalResult(Image reelImage, int resultIndex)
    {
        if (reelImage == null)
        {
            Debug.LogError("SlotMachineUI: Trying to show final result with null reel image!");
            return;
        }
        
        Color[] upgradeColors = { Color.red, Color.green, Color.cyan, Color.yellow };
        UpdateReelImage(reelImage, resultIndex, upgradeColors);
        
        // Log visibility for debugging
        Debug.Log($"SlotMachineUI: Setting final result for reel - Image enabled: {reelImage.enabled}, " +
                 $"Type: {resultIndex} ({GetUpgradeTypeName(resultIndex)}), " +
                 $"Color: {reelImage.color}, Alpha: {reelImage.color.a}");
    }
    
    private void UpdateReelImage(Image reelImage, int index, Color[] colors)
    {
        if (reelImage == null)
        {
            Debug.LogError("SlotMachineUI: Trying to update null reel image!");
            return;
        }
        
        // Ensure the image is enabled
        reelImage.enabled = true;
        
        if (useColoredSquaresInsteadOfSprites)
        {
            // For colored squares, set the color based on the index
            Color color = colors[index];
            color.a = 1f; // Ensure full opacity
            
            reelImage.sprite = null; // Clear the sprite
            reelImage.color = color; // Set the color
            reelImage.type = Image.Type.Simple; // Simple filled image
        }
        else if (symbolSprites != null && symbolSprites.Length > index && symbolSprites[index] != null)
        {
            // Use the appropriate sprite with white color
            reelImage.sprite = symbolSprites[index];
            reelImage.color = Color.white;
            reelImage.type = Image.Type.Simple; // Use Simple to show the full sprite
        }
        else
        {
            // Fallback to colored squares if sprites are missing
            Color color = colors[index];
            color.a = 1f; // Ensure full opacity
            
            reelImage.sprite = null;
            reelImage.color = color;
            reelImage.type = Image.Type.Simple;
            
            Debug.LogWarning($"SlotMachineUI: Missing sprite for index {index}, using colored square instead");
        }
        
        // Ensure the image is refreshed
        reelImage.SetMaterialDirty();
    }
    
    public void ShowResult(string message, int upgradeType)
    {
        if (resultText != null)
        {
            resultText.text = message;
            
            // Ensure result text is active and visible
            if (!resultText.gameObject.activeSelf)
            {
                resultText.gameObject.SetActive(true);
            }
            
            // Set the text color based on the upgrade type
            Color textColor = Color.white;
            switch (upgradeType)
            {
                case 0: textColor = Color.red; break;    // Damage
                case 1: textColor = Color.green; break;  // Health
                case 2: textColor = Color.cyan; break;   // Speed
                case 3: textColor = Color.yellow; break; // Dash
            }
            resultText.color = textColor;
            
            Debug.Log($"SlotMachineUI: Showing result message: '{message}' with color {textColor}");
            
            // Double check parent is active
            if (!resultText.transform.parent.gameObject.activeSelf)
            {
                Debug.LogWarning("SlotMachineUI: Result text parent was inactive, activating it");
                resultText.transform.parent.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("SlotMachineUI: resultText is null! Cannot show result message.");
        }
    }
    
    // Count matches
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
        
        // No matches - return the first icon type
        Debug.Log($"No matching pairs found, returning first icon type: {reelResults[0]}");
        return reelResults[0];
    }
    
    // Get the first icon type
    public int GetFirstIconType()
    {
        // Return the first (leftmost) reel's result
        if (reelResults.Length > 0)
        {
            Debug.Log($"SlotMachineUI: First icon type is {reelResults[0]} ({GetUpgradeTypeName(reelResults[0])})");
            return reelResults[0];
        }
        
        Debug.LogError("SlotMachineUI: reelResults array is empty! Returning default value 0.");
        return 0; // Default to damage type
    }
    
    // Get the match count for the SlotMachine script
    public int GetMatchCount()
    {
        int matches = CountMatches();
        Debug.Log($"SlotMachineUI: Match count is {matches}");
        return matches;
    }
    
    // Helper method to get upgrade type name for debugging
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
    
    // Public method to check if the slot machine is currently spinning
    public bool IsSpinning()
    {
        return isSpinning;
    }
    
    // Modified CountIconType to add more debug info
    public int CountIconType(int iconType)
    {
        int count = 0;
        
        // Go through all reel results and count matches for the specific type
        for (int i = 0; i < reelResults.Length; i++)
        {
            if (reelResults[i] == iconType)
            {
                count++;
                Debug.Log($"SlotMachineUI: Found match for icon type {iconType} ({GetUpgradeTypeName(iconType)}) in reel {i}");
            }
        }
        
        Debug.Log($"SlotMachineUI: Counted {count} occurrences of icon type {iconType} ({GetUpgradeTypeName(iconType)})");
        return count;
    }
    
    // Add a method to get the reel results directly
    public int[] GetReelResults()
    {
        Debug.Log($"SlotMachineUI: GetReelResults called, returning: [{string.Join(", ", reelResults)}]");
        return reelResults;
    }
} 