using System;
using System.Collections;
using UnityEngine;

public class CardShuffler : MonoBehaviour
{
    [SerializeField] private SpriteRenderer redCard;
    [SerializeField] private SpriteRenderer blueCard;
    [SerializeField] private SpriteRenderer yellowCard;
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private AudioClip shuffleSound;

    private AudioSource audioSource;
    private Vector3 originalPosition;
    private bool isAnimating = false;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        originalPosition = transform.position;
        
        // Hide cards initially
        if (redCard) redCard.enabled = false;
        if (blueCard) blueCard.enabled = false;
        if (yellowCard) yellowCard.enabled = false;
    }

    public void PlayShuffleAnimation(Action onComplete = null)
    {
        if (isAnimating) return;
        
        isAnimating = true;
        StartCoroutine(ShuffleRoutine(onComplete));
    }

    private IEnumerator ShuffleRoutine(Action onComplete)
    {
        // Reset positions and make cards visible
        if (redCard)
        {
            redCard.transform.localPosition = Vector3.zero;
            redCard.enabled = true;
        }
        
        if (blueCard)
        {
            blueCard.transform.localPosition = Vector3.zero;
            blueCard.enabled = true;
        }
        
        if (yellowCard)
        {
            yellowCard.transform.localPosition = Vector3.zero;
            yellowCard.enabled = true;
        }

        // Play sound
        if (shuffleSound != null && audioSource != null)
            audioSource.PlayOneShot(shuffleSound);

        // Simple shuffle animation
        float t = 0;
        while (t < animationDuration)
        {
            t += Time.deltaTime;
            float progress = t / animationDuration;
            
            // Move cards in a circular motion with offset
            float angle = progress * 360f;
            float radius = 2f * Mathf.Sin(progress * Mathf.PI);
            
            if (redCard)
            {
                redCard.transform.localPosition = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius, 
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                    0
                );
                
                // Add some rotation for more dynamic effect
                redCard.transform.localRotation = Quaternion.Euler(0, 0, angle * 2);
            }
            
            if (blueCard)
            {
                blueCard.transform.localPosition = new Vector3(
                    Mathf.Cos((angle + 120f) * Mathf.Deg2Rad) * radius, 
                    Mathf.Sin((angle + 120f) * Mathf.Deg2Rad) * radius,
                    0
                );
                
                blueCard.transform.localRotation = Quaternion.Euler(0, 0, angle * 2 + 120f);
            }
            
            if (yellowCard)
            {
                yellowCard.transform.localPosition = new Vector3(
                    Mathf.Cos((angle + 240f) * Mathf.Deg2Rad) * radius, 
                    Mathf.Sin((angle + 240f) * Mathf.Deg2Rad) * radius,
                    0
                );
                
                yellowCard.transform.localRotation = Quaternion.Euler(0, 0, angle * 2 + 240f);
            }
            
            yield return null;
        }

        // Return to center
        t = 0;
        Vector3 redPos = redCard ? redCard.transform.localPosition : Vector3.zero;
        Vector3 bluePos = blueCard ? blueCard.transform.localPosition : Vector3.zero;
        Vector3 yellowPos = yellowCard ? yellowCard.transform.localPosition : Vector3.zero;
        Quaternion redRot = redCard ? redCard.transform.localRotation : Quaternion.identity;
        Quaternion blueRot = blueCard ? blueCard.transform.localRotation : Quaternion.identity;
        Quaternion yellowRot = yellowCard ? yellowCard.transform.localRotation : Quaternion.identity;
        
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float progress = t / 0.3f;
            
            if (redCard)
            {
                redCard.transform.localPosition = Vector3.Lerp(redPos, Vector3.zero, progress);
                redCard.transform.localRotation = Quaternion.Lerp(redRot, Quaternion.identity, progress);
            }
            
            if (blueCard)
            {
                blueCard.transform.localPosition = Vector3.Lerp(bluePos, Vector3.zero, progress);
                blueCard.transform.localRotation = Quaternion.Lerp(blueRot, Quaternion.identity, progress);
            }
            
            if (yellowCard)
            {
                yellowCard.transform.localPosition = Vector3.Lerp(yellowPos, Vector3.zero, progress);
                yellowCard.transform.localRotation = Quaternion.Lerp(yellowRot, Quaternion.identity, progress);
            }
            
            yield return null;
        }

        // Fade out by scaling down
        t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float progress = t / 0.3f;
            float scale = 1 - progress;
            
            if (redCard) redCard.transform.localScale = new Vector3(scale, scale, scale);
            if (blueCard) blueCard.transform.localScale = new Vector3(scale, scale, scale);
            if (yellowCard) yellowCard.transform.localScale = new Vector3(scale, scale, scale);
            
            yield return null;
        }

        // Hide cards and reset scale
        if (redCard)
        {
            redCard.enabled = false;
            redCard.transform.localScale = Vector3.one;
        }
        
        if (blueCard)
        {
            blueCard.enabled = false;
            blueCard.transform.localScale = Vector3.one;
        }
        
        if (yellowCard)
        {
            yellowCard.enabled = false;
            yellowCard.transform.localScale = Vector3.one;
        }

        isAnimating = false;
        onComplete?.Invoke();
    }
} 