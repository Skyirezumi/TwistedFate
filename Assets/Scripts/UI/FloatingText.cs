using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 1.0f;
    [SerializeField] private float duration = 5f;
    
    private TextMeshPro textComponent;
    private float startTime;
    private Color originalColor;
    private Transform target; // Reference to the character to follow
    private Vector3 offset; // Offset from the target
    
    public static FloatingText Create(GameObject prefab, Vector3 position, string text, Color color)
    {
        if (prefab == null)
        {
            Debug.LogError("FloatingText: Prefab is null!");
            return null;
        }
        
        // Create the text object closer to the character
        Vector3 offset = Vector3.up * 0.2f; // Significantly lower height offset (from 0.8f to 0.2f)
        GameObject instance = Instantiate(prefab, position + offset, Quaternion.identity);
        
        // Get components
        FloatingText floater = instance.GetComponent<FloatingText>();
        TextMeshPro tmp = instance.GetComponent<TextMeshPro>();
        
        if (floater == null || tmp == null)
        {
            Debug.LogError("FloatingText: Missing required components on prefab!");
            Destroy(instance);
            return null;
        }
        
        // Store target to follow (usually the player)
        if (PlayerController.Instance != null)
        {
            floater.target = PlayerController.Instance.transform;
            floater.offset = offset; // Store initial offset
        }
        
        // Set text and color
        tmp.text = text;
        tmp.color = color;
        
        // Set smaller font size
        tmp.fontSize = 2.5f;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 1.5f;
        tmp.fontSizeMax = 4f;
        
        // Reasonable outline
        tmp.outlineWidth = 0.15f;
        tmp.outlineColor = Color.black;
        
        // Normal scale
        instance.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        
        // Make sure it faces the camera correctly (fix mirroring)
        if (Camera.main != null)
        {
            // Look at camera instead of away from it
            instance.transform.forward = Camera.main.transform.forward;
        }
        
        // Destroy after duration
        Destroy(instance, floater.duration);
        
        return floater;
    }
    
    private void Awake()
    {
        textComponent = GetComponent<TextMeshPro>();
        if (textComponent == null)
        {
            Debug.LogError("FloatingText: No TextMeshPro component found!");
            enabled = false;
        }
    }
    
    private void Start()
    {
        startTime = Time.time;
        if (textComponent != null)
        {
            originalColor = textComponent.color;
        }
    }
    
    private void Update()
    {
        // Follow the target if available
        if (target != null)
        {
            // Follow target position but maintain the offset which gradually rises
            offset += Vector3.up * floatSpeed * Time.deltaTime;
            transform.position = target.position + offset;
        }
        else
        {
            // Otherwise just float upward
            transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
        }
        
        // Make text face camera correctly (fix mirroring)
        if (Camera.main != null)
        {
            // Look at camera position instead of away from it
            transform.forward = Camera.main.transform.forward;
        }
        
        // Fade out in second half of duration
        float t = (Time.time - startTime) / duration;
        if (t > 0.5f && textComponent != null)
        {
            Color newColor = originalColor;
            newColor.a = 1f - ((t - 0.5f) * 2f);
            textComponent.color = newColor;
        }
    }
} 