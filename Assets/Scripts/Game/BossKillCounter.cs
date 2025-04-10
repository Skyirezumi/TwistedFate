using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossKillCounter : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI killCounterText;
    [SerializeField] private GameObject winScreen;
    
    [Header("Win Condition")]
    [SerializeField] private int requiredKillsToWin = 4;
    
    private int currentKillCount = 0;
    
    private void Start()
    {
        // Ensure win screen is hidden at start
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }
        
        // Update the counter display
        UpdateKillCounter();
    }
    
    public void IncrementKillCount()
    {
        currentKillCount++;
        UpdateKillCounter();
        
        // Check for win condition
        if (currentKillCount >= requiredKillsToWin)
        {
            ShowWinScreen();
        }
    }
    
    private void UpdateKillCounter()
    {
        if (killCounterText != null)
        {
            killCounterText.text = $"Bosses Defeated: {currentKillCount}/{requiredKillsToWin}";
        }
    }
    
    private void ShowWinScreen()
    {
        if (winScreen != null)
        {
            winScreen.SetActive(true);
            // You might want to pause the game here
            Time.timeScale = 0f;
        }
    }
} 