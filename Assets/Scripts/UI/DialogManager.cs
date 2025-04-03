using System.Collections;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float fadeSpeed = 1.0f;
    [SerializeField] private float displayDuration = 3.0f;
    
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI dialogText;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        dialogText = GetComponentInChildren<TextMeshProUGUI>();
        if (dialogText == null)
        {
            Debug.LogError("No TextMeshProUGUI component found in children of DialogManager!");
        }
        
        // Ensure dialog is hidden at start
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }
    
    public void ShowDialog(string message)
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimateDialog(message));
    }
    
    private IEnumerator AnimateDialog(string message)
    {
        // Fade in
        canvasGroup.alpha = 0;
        dialogText.text = "";
        
        yield return StartCoroutine(FadeIn());
        
        // Typewriter effect
        yield return StartCoroutine(TypewriterEffect(message));
        
        // Wait for display duration
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        gameObject.SetActive(false);
    }
    
    private IEnumerator TypewriterEffect(string message)
    {
        dialogText.text = "";
        
        foreach (char c in message.ToCharArray())
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
    
    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0;
        
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        canvasGroup.alpha = 1;
    }
    
    private IEnumerator FadeOut()
    {
        canvasGroup.alpha = 1;
        
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        canvasGroup.alpha = 0;
    }

    // Add a public getter for the display duration
    public float GetDisplayDuration()
    {
        // Return the total estimated time for dialog display
        // This includes fade in, typewriter time for a typical message, display duration, and fade out
        float typicalCharCount = 100; // Assume average dialog length
        float totalEstimatedTime = (1/fadeSpeed) + (typewriterSpeed * typicalCharCount) + displayDuration + (1/fadeSpeed);
        
        return totalEstimatedTime;
    }
} 