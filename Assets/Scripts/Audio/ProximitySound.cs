using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximitySound : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioClip[] ambientSounds; // Array of sounds to play randomly
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private float minVolume = 0.0f; // Changed from 0.1f to 0.0f for complete silence
    [SerializeField] private float minTimeBetweenSounds = 2.0f; // Minimum time between random sounds
    [SerializeField] private float maxTimeBetweenSounds = 6.0f; // Maximum time between random sounds
    
    [Header("Distance Settings")]
    [SerializeField] private float maxDistance = 10f; // Distance at which sound is completely silent
    [SerializeField] private float minDistance = 1f;  // Distance at which sound is at maximum volume
    [SerializeField] private AnimationCurve volumeFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private float fadeOutBuffer = 0.5f; // Extra buffer zone for complete silence
    
    [Header("Options")]
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool continuousAmbience = true; // If true, plays continuously; if false, plays random sounds with pauses
    [SerializeField] private Transform target; // Optional specific target
    
    private AudioSource audioSource;
    private Transform playerTransform;
    private float nextSoundTime;
    private Coroutine soundSequenceCoroutine;
    
    private void Start()
    {
        // Initialize audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure audio source
        audioSource.loop = continuousAmbience;
        audioSource.spatialBlend = 0f; // Use 2D sound since we're manually controlling volume
        audioSource.volume = minVolume;
        
        // Find the player if no specific target is set
        if (target == null && PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
        else
        {
            playerTransform = target;
        }
        
        // Start playing if set to play on awake
        if (playOnAwake && ambientSounds != null && ambientSounds.Length > 0)
        {
            if (continuousAmbience)
            {
                // Choose a random sound for continuous play
                audioSource.clip = ambientSounds[Random.Range(0, ambientSounds.Length)];
                audioSource.Play();
            }
            else
            {
                // Start the random sound sequence
                soundSequenceCoroutine = StartCoroutine(PlayRandomSounds());
            }
        }
    }
    
    private void Update()
    {
        UpdateVolume();
    }
    
    private void UpdateVolume()
    {
        if (playerTransform == null || audioSource == null) return;
        
        // Calculate distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        // If beyond max distance plus buffer, set volume to zero immediately
        if (distance > maxDistance + fadeOutBuffer)
        {
            audioSource.volume = 0f;
            return;
        }
        
        // Normalize distance between min and max distance
        float normalizedDistance = Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));
        
        // Use volume curve to calculate volume factor (1 at min distance, 0 at max distance)
        float volumeFactor = volumeFalloff.Evaluate(normalizedDistance);
        
        // Scale between min and max volume
        float targetVolume = Mathf.Lerp(maxVolume, minVolume, normalizedDistance);
        
        // Apply additional curve-based falloff for smoother transition
        targetVolume *= volumeFactor;
        
        // Ensure volume reaches exactly 0 at max distance
        if (distance >= maxDistance)
        {
            targetVolume = 0f;
        }
        
        // Apply volume with smooth transition
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * 3f); // Faster transition
    }
    
    // Coroutine to play random sounds with delays between them
    private IEnumerator PlayRandomSounds()
    {
        while (true)
        {
            // Check if we have sounds and if we're close enough to hear them
            if (ambientSounds != null && ambientSounds.Length > 0 && 
                playerTransform != null && 
                Vector3.Distance(transform.position, playerTransform.position) < maxDistance)
            {
                // Choose a random sound
                AudioClip randomSound = ambientSounds[Random.Range(0, ambientSounds.Length)];
                
                if (randomSound != null)
                {
                    // Play the sound
                    audioSource.clip = randomSound;
                    audioSource.Play();
                    
                    // Wait for it to finish
                    yield return new WaitForSeconds(randomSound.length);
                }
            }
            
            // Random delay between sounds
            float delay = Random.Range(minTimeBetweenSounds, maxTimeBetweenSounds);
            yield return new WaitForSeconds(delay);
        }
    }
    
    // Public method to start playing sounds
    public void StartSound()
    {
        if (audioSource != null && ambientSounds != null && ambientSounds.Length > 0)
        {
            if (continuousAmbience)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = ambientSounds[Random.Range(0, ambientSounds.Length)];
                    audioSource.Play();
                }
            }
            else
            {
                if (soundSequenceCoroutine == null)
                {
                    soundSequenceCoroutine = StartCoroutine(PlayRandomSounds());
                }
            }
        }
    }
    
    // Public method to stop the sound
    public void StopSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        if (soundSequenceCoroutine != null)
        {
            StopCoroutine(soundSequenceCoroutine);
            soundSequenceCoroutine = null;
        }
    }
    
    // Change to a new random sound (for continuous mode)
    public void ChangeSound()
    {
        if (continuousAmbience && audioSource != null && ambientSounds != null && ambientSounds.Length > 0)
        {
            // Get a new random sound that's different from the current one
            if (ambientSounds.Length > 1 && audioSource.clip != null)
            {
                AudioClip currentClip = audioSource.clip;
                AudioClip newClip = currentClip;
                
                // Try to get a different clip
                while (newClip == currentClip)
                {
                    newClip = ambientSounds[Random.Range(0, ambientSounds.Length)];
                }
                
                // Set and play the new clip
                audioSource.clip = newClip;
                audioSource.Play();
            }
            else
            {
                // Just play a random sound if we only have one or none
                audioSource.clip = ambientSounds[Random.Range(0, ambientSounds.Length)];
                audioSource.Play();
            }
        }
    }
    
    // For visual debugging of distance ranges in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
} 