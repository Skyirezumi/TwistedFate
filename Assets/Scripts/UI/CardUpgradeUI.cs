using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardUpgradeUI : MonoBehaviour
{
    public static CardUpgradeUI Instance;
    
    [Header("UI References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private TextMeshProUGUI[] upgradeTitles;
    [SerializeField] private TextMeshProUGUI[] upgradeDescriptions;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button closeButton;
    
    [Header("Animation")]
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float fadeOutTime = 0.2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private float soundVolume = 0.7f;
    
    private CardUpgrade[] currentUpgrades;
    private int currentCost;
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    
    private void Awake()
    {
        Instance = this;
        
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Get or add CanvasGroup for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Hide panel at start
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        
        // Set up close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
        
        // Set up upgrade selection buttons
        if (upgradeButtons != null)
        {
            for (int i = 0; i < upgradeButtons.Length; i++)
            {
                int index = i; // Local copy for closure
                if (upgradeButtons[i] != null)
                {
                    upgradeButtons[i].onClick.AddListener(() => OnUpgradeSelected(index));
                }
            }
        }
    }
    
    private void Start()
    {
        // Additional initialization if needed
        Debug.Log("CardUpgradeUI: Initialized");
    }
    
    public void ShowUpgrades(CardUpgrade[] upgrades, int cost)
    {
        Debug.Log("CardUpgradeUI: Showing upgrade options");
        
        if (upgradePanel == null)
        {
            Debug.LogError("CardUpgradeUI: Upgrade panel reference is missing!");
            return;
        }
        
        // Store current upgrades and cost
        currentUpgrades = upgrades;
        currentCost = cost;
        
        // Update cost text
        if (costText != null)
        {
            costText.text = "Cost: " + cost + " Gold";
        }
        
        // Display upgrades
        int optionsToShow = Mathf.Min(upgradeButtons.Length, upgrades.Length);
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < optionsToShow)
            {
                upgradeButtons[i].gameObject.SetActive(true);
                
                // Set title and description
                if (upgradeTitles != null && i < upgradeTitles.Length)
                {
                    upgradeTitles[i].text = upgrades[i].title;
                }
                
                if (upgradeDescriptions != null && i < upgradeDescriptions.Length)
                {
                    upgradeDescriptions[i].text = upgrades[i].description;
                }
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
        
        // Show the panel
        upgradePanel.SetActive(true);
        
        // Fade in animation
        StartCoroutine(FadeIn());
        
        // Pause the game (optional)
        Time.timeScale = 0f;
    }
    
    private void OnUpgradeSelected(int upgradeIndex)
    {
        if (upgradeIndex < 0 || upgradeIndex >= currentUpgrades.Length)
        {
            Debug.LogError("CardUpgradeUI: Invalid upgrade index: " + upgradeIndex);
            return;
        }
        
        Debug.Log("CardUpgradeUI: Selected upgrade: " + currentUpgrades[upgradeIndex].title);
        
        // Check if player has enough gold
        if (EconomyManager.Instance != null && EconomyManager.Instance.GetCurrentGold() >= currentCost)
        {
            // Try to deduct gold - check for SpendGold method first
            if (TrySpendGold(currentCost))
            {
                // Apply the upgrade
                ApplyUpgrade(currentUpgrades[upgradeIndex]);
                
                // Play upgrade sound
                PlaySound(upgradeSound);
                
                // Close panel
                ClosePanel();
            }
            else
            {
                Debug.LogWarning("CardUpgradeUI: Failed to spend gold. Missing SpendGold method.");
            }
        }
        else
        {
            Debug.Log("CardUpgradeUI: Not enough gold!");
            // Could show a message to the player here
        }
    }
    
    private bool TrySpendGold(int amount)
    {
        if (EconomyManager.Instance == null) return false;
        
        // Try to call SpendGold method using reflection
        var economyManager = EconomyManager.Instance;
        var spendMethod = economyManager.GetType().GetMethod("SpendGold");
        
        if (spendMethod != null)
        {
            spendMethod.Invoke(economyManager, new object[] { amount });
            return true;
        }
        
        return false;
    }
    
    private void ApplyUpgrade(CardUpgrade upgrade)
    {
        // Find the card thrower component
        CardThrower cardThrower = FindCardThrower();
        if (cardThrower == null)
        {
            Debug.LogError("CardUpgradeUI: CardThrower component not found!");
            return;
        }
        
        // Apply the appropriate upgrade based on type
        switch (upgrade.type)
        {
            case CardUpgradeType.GreenAreaOfEffect:
                cardThrower.ApplyGreenAreaUpgrade();
                break;
            case CardUpgradeType.BlueStun:
                cardThrower.ApplyBlueStunUpgrade();
                break;
            case CardUpgradeType.RedPoison:
                cardThrower.ApplyRedPoisonUpgrade();
                break;
            case CardUpgradeType.RedFanShot:
                cardThrower.ApplyRedFanShotUpgrade();
                break;
            case CardUpgradeType.BlueFanShot:
                cardThrower.ApplyBlueFanShotUpgrade();
                break;
            case CardUpgradeType.GreenFanShot:
                cardThrower.ApplyGreenFanShotUpgrade();
                break;
            case CardUpgradeType.RedVampire:
                cardThrower.ApplyRedVampireUpgrade();
                break;
            case CardUpgradeType.BlueVampire:
                cardThrower.ApplyBlueVampireUpgrade();
                break;
            case CardUpgradeType.GreenVampire:
                cardThrower.ApplyGreenVampireUpgrade();
                break;
            case CardUpgradeType.RedHomingPrecision:
                cardThrower.ApplyRedHomingPrecisionUpgrade();
                break;
            case CardUpgradeType.BlueHomingPrecision:
                cardThrower.ApplyBlueHomingPrecisionUpgrade();
                break;
            case CardUpgradeType.GreenHomingPrecision:
                cardThrower.ApplyGreenHomingPrecisionUpgrade();
                break;
            case CardUpgradeType.RedChainLightning:
                cardThrower.ApplyRedChainLightningUpgrade();
                break;
            case CardUpgradeType.BlueChainLightning:
                cardThrower.ApplyBlueChainLightningUpgrade();
                break;
            case CardUpgradeType.GreenChainLightning:
                cardThrower.ApplyGreenChainLightningUpgrade();
                break;
            default:
                Debug.LogWarning("CardUpgradeUI: Unknown upgrade type: " + upgrade.type);
                break;
        }
        
        // Show a confirmation message
        Debug.Log("CardUpgradeUI: Applied upgrade: " + upgrade.title);
    }
    
    public void ClosePanel()
    {
        if (upgradePanel != null)
        {
            Debug.Log("CardUpgradeUI: Closing panel");
            upgradePanel.SetActive(false);
            
            // Play close sound
            PlaySound(closeSound);
        }
        
        // Resume the game if it was paused
        Time.timeScale = 1f;
    }
    
    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        float time = 0f;
        
        while (time < fadeInTime)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeInTime);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOut()
    {
        canvasGroup.alpha = 1f;
        float time = 0f;
        
        while (time < fadeOutTime)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeOutTime);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        
        // Hide panel after fade out
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }
    
    // Helper method to find any CardThrower in the scene
    private CardThrower FindCardThrower()
    {
        return FindObjectOfType<CardThrower>();
    }
}

// Class representing a single upgrade option in the UI
[System.Serializable]
public class UpgradeOptionUI
{
    public GameObject gameObject;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage;
    public Button selectButton;
    
    private int upgradeIndex;
    private System.Action<int> onSelectCallback;
    
    public void Setup(CardUpgrade upgrade, int index, System.Action<int> callback)
    {
        upgradeIndex = index;
        onSelectCallback = callback;
        
        // Set text content
        if (titleText != null)
        {
            titleText.text = upgrade.title;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = upgrade.description;
        }
        
        // Clear previous listeners and add new one
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => OnButtonClick());
        }
    }
    
    private void OnButtonClick()
    {
        onSelectCallback?.Invoke(upgradeIndex);
    }
} 