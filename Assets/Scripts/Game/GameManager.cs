using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Victory Settings")]
    [SerializeField] private int bossesToWin = 4;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private TMP_Text killCounterText;
    
    private int bossesKilled = 0;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Find win screen if not assigned
        if (winScreen == null)
        {
            winScreen = GameObject.Find("WinScreen");
            if (winScreen == null)
            {
                Debug.LogWarning("Win screen not found! Create a GameObject named 'WinScreen' in your scene.");
            }
        }
        
        // Hide win screen at start
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }
        
        // Initialize kill counter text
        UpdateKillCounterText();
    }
    
    public void BossKilled()
    {
        bossesKilled++;
        Debug.Log($"Boss killed! Total: {bossesKilled}/{bossesToWin}");
        
        // Update UI
        UpdateKillCounterText();
        
        // Check if we've won
        if (bossesKilled >= bossesToWin)
        {
            ShowWinScreen();
        }
    }
    
    private void UpdateKillCounterText()
    {
        if (killCounterText != null)
        {
            killCounterText.text = $"Bosses Defeated: {bossesKilled}/{bossesToWin}";
        }
    }
    
    private void ShowWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
            // Pause the game
            Time.timeScale = 0f;
        }
    }
} 