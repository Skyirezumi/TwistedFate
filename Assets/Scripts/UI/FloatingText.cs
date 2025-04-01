using System.Collections;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 1.0f;
    [SerializeField] private float fadeSpeed = 0.5f;
    [SerializeField] private float scaleSpeed = 0.5f;
    [SerializeField] private float initialDelay = 0.2f;
    [SerializeField] private float lifetime = 2.0f;
    
    private TextMeshPro textMesh;
    private Vector3 initialScale;
    private Color initialColor;
    
    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            Debug.LogError("FloatingText script requires a TextMeshPro component!");
            Destroy(gameObject);
            return;
        }
        
        initialScale = transform.localScale;
        initialColor = textMesh.color;
        
        // Start with zero scale (for pop-in effect)
        transform.localScale = Vector3.zero;
    }
    
    private void Start()
    {
        StartCoroutine(AnimateText());
    }
    
    private IEnumerator AnimateText()
    {
        // Initial delay
        yield return new WaitForSeconds(initialDelay);
        
        // Pop-in effect
        float popDuration = 0.2f;
        float popTimer = 0f;
        
        while (popTimer < popDuration)
        {
            popTimer += Time.deltaTime;
            float progress = popTimer / popDuration;
            transform.localScale = initialScale * Mathf.Sin(progress * Mathf.PI * 0.5f);
            yield return null;
        }
        
        transform.localScale = initialScale;
        
        // Float up and fade out
        float timer = 0f;
        
        while (timer < lifetime)
        {
            timer += Time.deltaTime;
            float progress = timer / lifetime;
            
            // Move upward
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
            
            // Fade out after halfway point
            if (progress > 0.5f)
            {
                float fadeProgress = (progress - 0.5f) * 2f; // 0 to 1 during second half
                Color newColor = initialColor;
                newColor.a = Mathf.Lerp(initialColor.a, 0f, fadeProgress);
                textMesh.color = newColor;
                
                // Scale up slightly
                float scaleMultiplier = 1f + (fadeProgress * scaleSpeed);
                transform.localScale = initialScale * scaleMultiplier;
            }
            
            yield return null;
        }
        
        // Ensure we end fully transparent
        Color finalColor = initialColor;
        finalColor.a = 0f;
        textMesh.color = finalColor;
        
        // Destroy after animation
        Destroy(gameObject);
    }
} 