using System.Collections;
using UnityEngine;

// Add this to any GameObject to make it play sounds when the player is nearby
public class GeckoSoundPlayer : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioClip[] geckoSounds;
    [SerializeField] private float minTimeBetweenSounds = 0.2f;
    [SerializeField] private float maxTimeBetweenSounds = 0.6f;
    [SerializeField] private float soundVolume = 10f; // SUPER LOUD

    [Header("Player Detection")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private bool alwaysPlay = false; // Set to true to play sounds regardless of player distance

    // Private variables
    private AudioSource audioSource;
    private bool isPlayingSounds = false;
    
    private void Awake()
    {
        // Create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure audio source
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // 3D sound 
        audioSource.volume = soundVolume;
        audioSource.outputAudioMixerGroup = null; // Use default mixer
    }
    
    private void Start()
    {
        // Start playing sounds
        StartCoroutine(PlaySoundLoop());
    }
    
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    
    private IEnumerator PlaySoundLoop()
    {
        // Simple loop that plays sounds continuously
        while (true)
        {
            // Check if player is in range or if we should always play
            if (alwaysPlay || IsPlayerInRange())
            {
                PlayRandomSound();
            }
            
            // Wait a random amount of time
            float waitTime = Random.Range(minTimeBetweenSounds, maxTimeBetweenSounds);
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    private bool IsPlayerInRange()
    {
        // Check if player is in range
        if (PlayerController.Instance == null) return false;
        
        float distance = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
        return distance <= detectionRadius;
    }
    
    private void PlayRandomSound()
    {
        if (geckoSounds == null || geckoSounds.Length == 0)
        {
            Debug.LogWarning("GeckoSoundPlayer: No sounds assigned!");
            return;
        }
        
        // Select a random sound
        int randomIndex = Random.Range(0, geckoSounds.Length);
        AudioClip soundToPlay = geckoSounds[randomIndex];
        
        if (soundToPlay != null)
        {
            // Randomize pitch slightly for variety
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            
            // Play the sound EXTREMELY LOUD at the specified volume
            audioSource.PlayOneShot(soundToPlay, soundVolume);
            
            Debug.Log($"GeckoSoundPlayer: Playing sound '{soundToPlay.name}' at volume {soundVolume}");
        }
    }
    
    // Draw gizmos to show detection radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    
    // Public method for testing
    public void TestPlaySound()
    {
        PlayRandomSound();
    }
} 