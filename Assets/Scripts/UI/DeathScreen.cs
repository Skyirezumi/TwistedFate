using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    public static DeathScreen Instance { get; private set; }

    [SerializeField] private GameObject deathScreenPanel;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button mainMenuButton;
    
    private void Awake()
    {
        // Set up singleton pattern
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
        Debug.Log("DeathScreen - Start method called");
        
        // Try to find the panel if it's not assigned
        if (deathScreenPanel == null)
        {
            // Look for a child panel
            deathScreenPanel = transform.GetChild(0).gameObject;
            
            if (deathScreenPanel == null)
            {
                // Try to find it by tag
                deathScreenPanel = GameObject.FindWithTag("DeathScreenPanel");
                
                if (deathScreenPanel == null)
                {
                    // Last resort - use this gameObject itself
                    deathScreenPanel = gameObject;
                    Debug.LogWarning("Used DeathScreen gameObject itself as the panel");
                }
            }
        }
        
        // Initially hide the death screen
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
            Debug.Log("Death screen panel hidden");
        }
        else
        {
            Debug.LogError("Death screen panel reference is missing!");
        }
        
        // Look for buttons if they're not assigned
        if (tryAgainButton == null || mainMenuButton == null)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            if (buttons.Length >= 2)
            {
                if (tryAgainButton == null) tryAgainButton = buttons[0];
                if (mainMenuButton == null) mainMenuButton = buttons[1];
                Debug.Log("Found buttons automatically: " + buttons.Length);
            }
        }
        
        // Add button listeners
        if (tryAgainButton != null && mainMenuButton != null)
        {
            tryAgainButton.onClick.AddListener(RestartLevel);
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            Debug.Log("Button listeners added");
        }
        else
        {
            Debug.LogError("Button references are missing! Try Again: " + (tryAgainButton != null) + ", Main Menu: " + (mainMenuButton != null));
        }
    }
    
    public void ShowDeathScreen()
    {
        Debug.Log("Death Screen - ShowDeathScreen method called!");
        if (deathScreenPanel == null)
        {
            Debug.LogError("Death screen panel is null when trying to show it!");
            return;
        }
        
        deathScreenPanel.SetActive(true);
        
        // We don't need to disable the player controller here - the PlayerHealth script does this
        // This avoids trying to access a potentially destroyed PlayerController
    }
    
    private void RestartLevel()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void ReturnToMainMenu()
    {
        // Load the main menu scene by name instead of index
        SceneManager.LoadScene("StartMenu");
    }
} 