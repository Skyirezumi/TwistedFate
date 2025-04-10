using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class VictoryScreen : MonoBehaviour
{
    [SerializeField] Button mainMenuButton;
    [SerializeField] Button playAgainButton;
    [SerializeField] TextMeshProUGUI victoryText;
    [SerializeField] TextMeshProUGUI timeText;
    
    private void Start()
    {
        // Hide the panel at start, GameManager will show it
        gameObject.SetActive(false);
        
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
    
    public void SetCompletionTime(float timeInSeconds)
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            timeText.text = string.Format("Completion Time: {0:00}:{1:00}", minutes, seconds);
        }
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