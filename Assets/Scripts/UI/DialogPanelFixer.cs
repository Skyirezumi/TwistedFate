using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Add this to your scene temporarily to fix dialogue panel issues
public class DialogPanelFixer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI dialogText;
    
    [Header("Options")]
    [SerializeField] private bool fixHierarchy = true;
    [SerializeField] private bool fixVisibility = true;
    
    void Start()
    {
        if (dialogPanel == null)
        {
            // Try to find it in the scene
            dialogPanel = GameObject.Find("DialoguePanel");
            if (dialogPanel == null)
            {
                Debug.LogError("DialogPanelFixer: Could not find DialoguePanel in scene!");
                return;
            }
        }
        
        // Is the text a child of the panel? If not, fix the hierarchy
        if (fixHierarchy && dialogText != null && dialogText.transform.parent != dialogPanel.transform)
        {
            Debug.Log("DialogPanelFixer: Fixing text hierarchy - moving text to be a child of panel");
            dialogText.transform.SetParent(dialogPanel.transform, false);
        }
        
        // If text was not assigned, try to find it
        if (dialogText == null)
        {
            // Try to find in children first
            dialogText = dialogPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            
            // If not found in children, look in the scene
            if (dialogText == null)
            {
                Debug.LogWarning("DialogPanelFixer: Could not find TextMeshProUGUI in panel children! Looking in scene...");
                dialogText = FindObjectOfType<TextMeshProUGUI>();
                
                if (dialogText != null && fixHierarchy)
                {
                    // Move it under the panel
                    dialogText.transform.SetParent(dialogPanel.transform, false);
                    Debug.Log("DialogPanelFixer: Found text in scene and moved it under panel");
                }
            }
        }
        
        // Fix visibility - set both to inactive at start
        if (fixVisibility)
        {
            Debug.Log("DialogPanelFixer: Setting panel and text to inactive");
            dialogPanel.SetActive(false);
            
            if (dialogText != null)
            {
                dialogText.gameObject.SetActive(false);
            }
        }
        
        // Find DialogManager and set the references
        DialogManager dialogManager = FindObjectOfType<DialogManager>();
        if (dialogManager != null)
        {
            // Set fields via reflection
            var panelField = dialogManager.GetType().GetField("dialogPanel", 
                             System.Reflection.BindingFlags.Instance | 
                             System.Reflection.BindingFlags.NonPublic);
                             
            var textField = dialogManager.GetType().GetField("dialogText",
                            System.Reflection.BindingFlags.Instance | 
                            System.Reflection.BindingFlags.NonPublic);
                            
            if (panelField != null && textField != null)
            {
                Debug.Log("DialogPanelFixer: Setting DialogManager references");
                panelField.SetValue(dialogManager, dialogPanel);
                textField.SetValue(dialogManager, dialogText);
            }
        }
        
        // Self-destruct after fixing everything
        Debug.Log("DialogPanelFixer: Job complete!");
        Destroy(this);
    }
} 