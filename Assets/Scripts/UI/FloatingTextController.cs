using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Simple controller for floating text that moves upward and fades out
public class FloatingTextController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float floatSpeed = 1.0f; // Units per second
    [SerializeField] private float fadeDuration = 1.5f; // Time to fade out in seconds
    [SerializeField] private float lifetime = 2.0f; // Total lifetime in seconds
    
    private TextMeshPro textComponent;
    private float startTime;
    private Color originalColor;
    
    private void Awake()
    {
        textComponent = GetComponent<TextMeshPro>();
        if (textComponent == null)
        {
            // Fallback to TextMesh if TextMeshPro isn't available
            TextMesh textMesh = GetComponent<TextMesh>();
            if (textMesh != null)
            {
                originalColor = textMesh.color;
            }
            else
            {
                Debug.LogError("FloatingTextController: No text component found!");
            }
        }
        else
        {
            originalColor = textComponent.color;
        }
        
        startTime = Time.time;
    }
    
    private void Update()
    {
        // Move upward
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
        
        // Calculate fade based on lifetime
        float elapsedTime = Time.time - startTime;
        float normalizedTime = Mathf.Clamp01(elapsedTime / lifetime);
        
        // Start fading after some delay
        float fadeStartTime = lifetime - fadeDuration;
        float fadeAlpha = 1.0f;
        
        if (elapsedTime > fadeStartTime)
        {
            float fadeElapsed = elapsedTime - fadeStartTime;
            fadeAlpha = 1.0f - (fadeElapsed / fadeDuration);
        }
        
        // Apply fade
        if (textComponent != null)
        {
            Color newColor = originalColor;
            newColor.a = fadeAlpha;
            textComponent.color = newColor;
        }
        else
        {
            // Fallback to TextMesh if TextMeshPro isn't available
            TextMesh textMesh = GetComponent<TextMesh>();
            if (textMesh != null)
            {
                Color newColor = originalColor;
                newColor.a = fadeAlpha;
                textMesh.color = newColor;
            }
        }
        
        // Destroy when lifetime is over
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    // Helper method to set the text and color
    public void Setup(string text, Color color)
    {
        if (textComponent != null)
        {
            textComponent.text = text;
            textComponent.color = color;
            originalColor = color;
        }
        else
        {
            // Fallback to TextMesh if TextMeshPro isn't available
            TextMesh textMesh = GetComponent<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = text;
                textMesh.color = color;
                originalColor = color;
            }
        }
    }
} 