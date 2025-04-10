using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Bounty System")]
    [SerializeField] private int requiredBountyKills = 4;
    [SerializeField] private TextMeshProUGUI bountyCounterText;
    
    [Header("Victory Screen")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button playAgainButton;
    
    [Header("Game Timer")]
    [SerializeField] private TextMeshProUGUI timerText;
    
    private int currentBountyKills = 0;
    private float gameStartTime;
    private bool gameFinished = false;

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
        }
    }

    private void Start()
    {
        // Initialize the game
        currentBountyKills = 0;
        gameStartTime = Time.time;
        gameFinished = false;
        
        // Initialize UI
        UpdateBountyCounter();
        
        // Make sure victory panel is hidden
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
        
        // Set up button listeners
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
        
        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(RestartGame);
        }
    }

    private void Update()
    {
        // Update timer if game is running
        if (!gameFinished && timerText != null)
        {
            UpdateGameTimer();
        }
    }
    
    public void IncrementBountyKill()
    {
        if (gameFinished) return;
        
        currentBountyKills++;
        UpdateBountyCounter();
        
        // Check if all bounties are completed
        if (currentBountyKills >= requiredBountyKills)
        {
            ShowVictoryScreen();
        }
    }
    
    public void RegisterBountyEnemy(BountyEnemy bountyEnemy)
    {
        // This method will be called by BountyEnemy components when they're created
        // We could track them in a list if needed
        Debug.Log("Bounty enemy registered with GameManager");
    }
    
    private void UpdateBountyCounter()
    {
        if (bountyCounterText != null)
        {
            bountyCounterText.text = $"Bounties: {currentBountyKills}/{requiredBountyKills}";
        }
    }
    
    private void UpdateGameTimer()
    {
        float elapsedTime = Time.time - gameStartTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    private void ShowVictoryScreen()
    {
        gameFinished = true;
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            
            // Find victory text if it exists and set the time
            TextMeshProUGUI victoryText = victoryPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (victoryText != null)
            {
                float totalTime = Time.time - gameStartTime;
                int minutes = Mathf.FloorToInt(totalTime / 60);
                int seconds = Mathf.FloorToInt(totalTime % 60);
                victoryText.text = $"Victory!\nAll bounties eliminated!\nTime: {minutes:00}:{seconds:00}";
            }
        }
        else
        {
            Debug.LogError("Victory panel is not assigned!");
        }
        
        // Possibly freeze the game or slow down time
        Time.timeScale = 0.5f;
    }
    
    private void RestartGame()
    {
        // Reset time scale
        Time.timeScale = 1.0f;
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void ReturnToMainMenu()
    {
        // Reset time scale
        Time.timeScale = 1.0f;
        
        // Load the main menu
        SceneManager.LoadScene("StartMenu");
    }
} 