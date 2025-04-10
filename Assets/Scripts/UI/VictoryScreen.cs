using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class VictoryScreen : MonoBehaviour
{
    [SerializeField] private Button returnToMenuButton;
    [SerializeField] private Button continuePlayingButton;
    [SerializeField] private TMP_Text victoryText;
    
    private void Start()
    {
        // Initially hide the panel
        gameObject.SetActive(false);
        
        // Set up button listeners
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(ReturnToMenu);
        }
        
        if (continuePlayingButton != null)
        {
            continuePlayingButton.onClick.AddListener(ContinuePlaying);
        }
    }
    
    public void ShowVictoryScreen()
    {
        // Activate the panel
        gameObject.SetActive(true);
        
        // Set victory text
        if (victoryText != null)
        {
            victoryText.text = "Victory!\nAll bounty enemies have been defeated!";
        }
    }
    
    private void ReturnToMenu()
    {
        // Reset timescale in case it was changed
        Time.timeScale = 1f;
        
        // Load the start menu scene
        SceneManager.LoadScene("StartMenu");
    }
    
    private void ContinuePlaying()
    {
        // Resume gameplay
        Time.timeScale = 1f;
        
        // Hide the victory screen
        gameObject.SetActive(false);
    }
} 