using System.Collections;
using UnityEngine;

public class SimpleCardShuffleEffect : MonoBehaviour
{
    [Header("Card Sprites")]
    [SerializeField] private SpriteRenderer redCardRenderer;
    [SerializeField] private SpriteRenderer blueCardRenderer;
    [SerializeField] private SpriteRenderer yellowCardRenderer;
    
    [Header("Animation Settings")]
    [SerializeField] private float shuffleDuration = 1.0f;
    [SerializeField] private AudioClip shuffleSound;
    
    private AudioSource audioSource;
    private bool isAnimating = false;
    
    void Awake()
    {
        // Add audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        
        // Hide cards initially
        HideCards();
    }
    
    private void HideCards()
    {
        if (redCardRenderer) redCardRenderer.enabled = false;
        if (blueCardRenderer) blueCardRenderer.enabled = false;
        if (yellowCardRenderer) yellowCardRenderer.enabled = false;
    }
    
    // Call this method right before showing upgrade choices
    public void PlayShuffleAnimation(System.Action onComplete = null)
    {
        if (isAnimating) return;
        
        isAnimating = true;
        StartCoroutine(ShuffleRoutine(onComplete));
    }
    
    private IEnumerator ShuffleRoutine(System.Action onComplete)
    {
        // Show cards
        if (redCardRenderer) redCardRenderer.enabled = true;
        if (blueCardRenderer) blueCardRenderer.enabled = true;
        if (yellowCardRenderer) yellowCardRenderer.enabled = true;
        
        // Reset positions
        if (redCardRenderer) redCardRenderer.transform.localPosition = Vector3.zero;
        if (blueCardRenderer) blueCardRenderer.transform.localPosition = Vector3.zero;
        if (yellowCardRenderer) yellowCardRenderer.transform.localPosition = Vector3.zero;
        
        // Reset rotations
        if (redCardRenderer) redCardRenderer.transform.localRotation = Quaternion.identity;
        if (blueCardRenderer) blueCardRenderer.transform.localRotation = Quaternion.identity;
        if (yellowCardRenderer) yellowCardRenderer.transform.localRotation = Quaternion.identity;
        
        // Play sound
        if (shuffleSound && audioSource)
        {
            audioSource.PlayOneShot(shuffleSound);
        }
        
        // Shuffle animation
        float time = 0;
        while (time < shuffleDuration)
        {
            time += Time.deltaTime;
            float progress = time / shuffleDuration;
            
            // Circular motion with rotation
            float angle = progress * 360f;
            float radius = 2f * Mathf.Sin(progress * Mathf.PI);
            
            // Move and rotate red card
            if (redCardRenderer)
            {
                redCardRenderer.transform.localPosition = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                    0
                );
                redCardRenderer.transform.localRotation = Quaternion.Euler(0, 0, angle * 2);
            }
            
            // Move and rotate blue card
            if (blueCardRenderer)
            {
                blueCardRenderer.transform.localPosition = new Vector3(
                    Mathf.Cos((angle + 120) * Mathf.Deg2Rad) * radius,
                    Mathf.Sin((angle + 120) * Mathf.Deg2Rad) * radius,
                    0
                );
                blueCardRenderer.transform.localRotation = Quaternion.Euler(0, 0, angle * 2);
            }
            
            // Move and rotate yellow card
            if (yellowCardRenderer)
            {
                yellowCardRenderer.transform.localPosition = new Vector3(
                    Mathf.Cos((angle + 240) * Mathf.Deg2Rad) * radius,
                    Mathf.Sin((angle + 240) * Mathf.Deg2Rad) * radius,
                    0
                );
                yellowCardRenderer.transform.localRotation = Quaternion.Euler(0, 0, angle * 2);
            }
            
            yield return null;
        }
        
        // Hide cards
        HideCards();
        
        // Animation complete
        isAnimating = false;
        
        // Call completion callback
        if (onComplete != null)
        {
            onComplete();
        }
    }
} 