using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Victory Settings")]
    [SerializeField] private int bountyEnemiesToWin = 4;
    [SerializeField] private VictoryScreen victoryScreen;
    [SerializeField] private TMP_Text killCounterText;
    
    private int bountyEnemiesKilled = 0;
    
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
        // Find victory screen if not assigned
        if (victoryScreen == null)
        {
            victoryScreen = FindObjectOfType<VictoryScreen>();
            if (victoryScreen == null)
            {
                Debug.LogWarning("Victory screen not found! Create a VictoryScreen in your scene.");
            }
        }
        
        // Initialize kill counter text
        UpdateKillCounterText();
    }
    
    public void BountyEnemyKilled()
    {
        bountyEnemiesKilled++;
        Debug.Log($"Bounty enemy killed! Total: {bountyEnemiesKilled}/{bountyEnemiesToWin}");
        
        // Update UI
        UpdateKillCounterText();
        
        // Check if we've won
        if (bountyEnemiesKilled >= bountyEnemiesToWin)
        {
            ShowVictoryScreen();
        }
    }
    
    private void UpdateKillCounterText()
    {
        if (killCounterText != null)
        {
            killCounterText.text = $"Bounty Enemies: {bountyEnemiesKilled}/{bountyEnemiesToWin}";
        }
    }
    
    private void ShowVictoryScreen()
    {
        Debug.Log("Victory! All bounty enemies defeated!");
        
        if (victoryScreen != null)
        {
            victoryScreen.ShowVictoryScreen();
        }
        else
        {
            Debug.LogError("Victory screen is null - can't show victory!");
        }
    }
} 