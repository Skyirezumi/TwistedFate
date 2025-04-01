using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDebugger : MonoBehaviour
{
    [Header("UI Element to Debug")]
    [SerializeField] private GameObject targetUI;
    
    [Header("Debug Options")]
    [SerializeField] private bool logHierarchy = true;
    [SerializeField] private bool checkImageVisibility = true;
    [SerializeField] private bool checkTextVisibility = true;
    [SerializeField] private bool checkCanvasSettings = true;
    
    [Header("Auto Fix")]
    [SerializeField] private bool fixVisibilityIssues = true;
    
    public void DebugUI()
    {
        if (targetUI == null)
        {
            Debug.LogError("UIDebugger: No target UI assigned!");
            return;
        }
        
        Debug.Log($"========= DEBUGGING UI: {targetUI.name} =========");
        
        // Check canvas settings if this is a canvas
        Canvas canvas = targetUI.GetComponent<Canvas>();
        if (canvas != null && checkCanvasSettings)
        {
            Debug.Log($"Canvas Info: Render Mode: {canvas.renderMode}, Sort Order: {canvas.sortingOrder}, " +
                      $"Pixel Perfect: {canvas.pixelPerfect}, Enabled: {canvas.enabled}");
        }
        
        // Check parent hierarchy
        if (logHierarchy)
        {
            Transform current = targetUI.transform;
            string hierarchy = current.name;
            
            while (current.parent != null)
            {
                current = current.parent;
                hierarchy = current.name + " > " + hierarchy;
                
                // Check if any parent is disabled
                if (!current.gameObject.activeSelf)
                {
                    Debug.LogWarning($"UIDebugger: Parent '{current.name}' is inactive! This will make the target invisible.");
                }
            }
            
            Debug.Log($"Hierarchy: {hierarchy}");
        }
        
        // Check for RectTransform issues
        RectTransform rt = targetUI.GetComponent<RectTransform>();
        if (rt != null)
        {
            Debug.Log($"RectTransform Info: Size: {rt.sizeDelta}, Position: {rt.anchoredPosition}, " +
                      $"Anchors: ({rt.anchorMin}, {rt.anchorMax}), Pivot: {rt.pivot}");
            
            // Check if size is zero
            if (rt.sizeDelta.x <= 0 || rt.sizeDelta.y <= 0)
            {
                Debug.LogWarning("UIDebugger: RectTransform has zero or negative size!");
                
                if (fixVisibilityIssues)
                {
                    rt.sizeDelta = new Vector2(Mathf.Max(rt.sizeDelta.x, 10), Mathf.Max(rt.sizeDelta.y, 10));
                    Debug.Log("UIDebugger: Fixed - Set minimum size to (10,10)");
                }
            }
        }
        
        // Check Image components
        if (checkImageVisibility)
        {
            Image[] images = targetUI.GetComponentsInChildren<Image>(true);
            Debug.Log($"Found {images.Length} Image components in children");
            
            foreach (Image img in images)
            {
                bool isVisible = IsImageVisible(img);
                Debug.Log($"Image '{img.name}': Enabled: {img.enabled}, Color: {img.color}, " +
                          $"Sprite: {(img.sprite ? img.sprite.name : "None")}, Is Visible: {isVisible}");
                
                if (!isVisible && fixVisibilityIssues)
                {
                    FixImageVisibility(img);
                }
            }
        }
        
        // Check Text components
        if (checkTextVisibility)
        {
            TextMeshProUGUI[] texts = targetUI.GetComponentsInChildren<TextMeshProUGUI>(true);
            Debug.Log($"Found {texts.Length} TextMeshProUGUI components in children");
            
            foreach (TextMeshProUGUI text in texts)
            {
                bool isVisible = text.enabled && text.color.a > 0;
                Debug.Log($"Text '{text.name}': Enabled: {text.enabled}, Color: {text.color}, " +
                          $"Text: '{text.text}', Font Size: {text.fontSize}, Is Visible: {isVisible}");
                
                if (!isVisible && fixVisibilityIssues)
                {
                    FixTextVisibility(text);
                }
            }
        }
        
        Debug.Log("============ DEBUG COMPLETE ============");
    }
    
    private bool IsImageVisible(Image img)
    {
        return img.enabled && img.color.a > 0 && img.gameObject.activeSelf;
    }
    
    private void FixImageVisibility(Image img)
    {
        if (!img.enabled)
        {
            img.enabled = true;
            Debug.Log($"UIDebugger: Fixed - Enabled Image component on '{img.name}'");
        }
        
        if (img.color.a <= 0)
        {
            Color color = img.color;
            color.a = 1f;
            img.color = color;
            Debug.Log($"UIDebugger: Fixed - Set alpha to 1 on Image '{img.name}'");
        }
        
        if (img.sprite == null)
        {
            try
            {
                img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                Debug.Log($"UIDebugger: Fixed - Assigned default sprite to Image '{img.name}'");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"UIDebugger: Could not assign default sprite: {e.Message}");
            }
        }
    }
    
    private void FixTextVisibility(TextMeshProUGUI text)
    {
        if (!text.enabled)
        {
            text.enabled = true;
            Debug.Log($"UIDebugger: Fixed - Enabled TextMeshProUGUI component on '{text.name}'");
        }
        
        if (text.color.a <= 0)
        {
            Color color = text.color;
            color.a = 1f;
            text.color = color;
            Debug.Log($"UIDebugger: Fixed - Set alpha to 1 on TextMeshProUGUI '{text.name}'");
        }
        
        if (string.IsNullOrEmpty(text.text))
        {
            text.text = "[Text]";
            Debug.Log($"UIDebugger: Fixed - Added default text to TextMeshProUGUI '{text.name}'");
        }
    }
    
    [ContextMenu("Debug UI Now")]
    public void DebugUIContextMenu()
    {
        DebugUI();
    }
} 