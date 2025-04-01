using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotMachineAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform slotMachinePanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Image[] reelImages;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float textScaleDuration = 0.5f;
    [SerializeField] private float reelPulseDuration = 0.3f;
    [SerializeField] private float initialScale = 0.5f;
    [SerializeField] private float resultTextBounceScale = 1.2f;
    
    private Coroutine currentAnimation;
    
    private void Awake()
    {
        // Get components if not assigned
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            
        if (slotMachinePanel == null)
            slotMachinePanel = GetComponent<RectTransform>();
            
        // Set initial values
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
    
    public void PlayShowAnimation()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
            
        currentAnimation = StartCoroutine(ShowAnimation());
    }
    
    public void PlayHideAnimation()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
            
        currentAnimation = StartCoroutine(HideAnimation());
    }
    
    public void PlayResultAnimation()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
            
        currentAnimation = StartCoroutine(ResultAnimation());
    }
    
    private IEnumerator ShowAnimation()
    {
        // Make sure panel is visible
        gameObject.SetActive(true);
        
        // Start from small scale
        if (slotMachinePanel != null)
            slotMachinePanel.localScale = new Vector3(initialScale, initialScale, initialScale);
        
        // Fade in 
        float startTime = Time.time;
        float progress = 0f;
        
        while (progress < 1f)
        {
            progress = (Time.time - startTime) / fadeInDuration;
            
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
                
            if (slotMachinePanel != null)
            {
                float currentScale = Mathf.Lerp(initialScale, 1f, progress);
                slotMachinePanel.localScale = new Vector3(currentScale, currentScale, currentScale);
            }
            
            yield return null;
        }
        
        // Ensure we finish at exactly the target values
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
            
        if (slotMachinePanel != null)
            slotMachinePanel.localScale = Vector3.one;
    }
    
    private IEnumerator HideAnimation()
    {
        // Start from full scale and opacity
        float startTime = Time.time;
        float progress = 0f;
        
        while (progress < 1f)
        {
            progress = (Time.time - startTime) / fadeOutDuration;
            
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
                
            if (slotMachinePanel != null)
            {
                float currentScale = Mathf.Lerp(1f, initialScale, progress);
                slotMachinePanel.localScale = new Vector3(currentScale, currentScale, currentScale);
            }
            
            yield return null;
        }
        
        // Ensure we finish at exactly the target values
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
            
        if (slotMachinePanel != null)
            slotMachinePanel.localScale = new Vector3(initialScale, initialScale, initialScale);
            
        // Hide the panel
        gameObject.SetActive(false);
    }
    
    private IEnumerator ResultAnimation()
    {
        // Bounce the result text
        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.transform.localScale = Vector3.zero;
            
            float startTime = Time.time;
            float progress = 0f;
            
            while (progress < 1f)
            {
                progress = (Time.time - startTime) / textScaleDuration;
                
                // Use a bounce effect
                float scale = Mathf.Sin(progress * Mathf.PI) * resultTextBounceScale;
                resultText.transform.localScale = new Vector3(scale, scale, scale);
                
                yield return null;
            }
            
            resultText.transform.localScale = Vector3.one;
        }
        
        // Make reels pulse
        if (reelImages != null && reelImages.Length > 0)
        {
            Vector3[] originalScales = new Vector3[reelImages.Length];
            
            // Store original scales
            for (int i = 0; i < reelImages.Length; i++)
            {
                if (reelImages[i] != null)
                    originalScales[i] = reelImages[i].transform.localScale;
            }
            
            // Pulse animation
            float startTime = Time.time;
            float progress = 0f;
            
            while (progress < 1f)
            {
                progress = (Time.time - startTime) / reelPulseDuration;
                
                // Pulse scale using sine wave
                float pulse = 1f + 0.2f * Mathf.Sin(progress * Mathf.PI * 4);
                
                for (int i = 0; i < reelImages.Length; i++)
                {
                    if (reelImages[i] != null)
                    {
                        reelImages[i].transform.localScale = originalScales[i] * pulse;
                    }
                }
                
                yield return null;
            }
            
            // Restore original scales
            for (int i = 0; i < reelImages.Length; i++)
            {
                if (reelImages[i] != null)
                    reelImages[i].transform.localScale = originalScales[i];
            }
        }
    }
} 