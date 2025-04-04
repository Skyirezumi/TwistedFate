using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;
    
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private float defaultDisplayTime = 3f;
    
    private Coroutine autoCloseCoroutine;
    
    private void Awake()
    {
        Instance = this;
        
        // Ensure dialog panel is hidden at start
        HideDialogueComponents();
    }
    
    private void Start()
    {
        // Double-check that everything is hidden at start
        HideDialogueComponents();
    }
    
    public void ShowDialog(string message)
    {
        Debug.Log("DialogManager: Showing dialog: " + message);
        
        // Stop any existing auto-close coroutine
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        
        // Show the panel and set text
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);
            
            if (dialogText != null)
            {
                // Make sure text component itself is enabled
                dialogText.gameObject.SetActive(true);
                dialogText.text = message;
            }
            else
            {
                Debug.LogError("DialogManager: Dialog text component not assigned!");
            }
        }
        else
        {
            Debug.LogError("DialogManager: Dialog panel not assigned!");
        }
        
        // Start auto-close coroutine if using default behavior
        // (NPCs will manually close dialog when needed)
        autoCloseCoroutine = StartCoroutine(AutoCloseDialog());
    }
    
    private System.Collections.IEnumerator AutoCloseDialog()
    {
        yield return new WaitForSeconds(defaultDisplayTime);
        CloseDialog();
        autoCloseCoroutine = null;
    }
    
    public void CloseDialog()
    {
        Debug.Log("DialogManager: Closing dialog");
        
        HideDialogueComponents();
        
        // Stop auto-close coroutine if it's running
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }
    
    // Helper method to ensure everything is hidden
    private void HideDialogueComponents()
    {
        // Hide panel
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        
        // Also explicitly hide the text component
        if (dialogText != null)
        {
            dialogText.gameObject.SetActive(false);
        }
    }
} 