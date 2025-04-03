using System.Collections;
using UnityEngine;

public class GeckoTalkSound : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioClip talkSound;
    [SerializeField] private float maxVolume = 10f; // Very loud base volume
    [SerializeField] private float minVolume = 0f;  // Silent at max distance
    
    [Header("Distance Settings")]
    [SerializeField] private float maxDistance = 10f; // No sound beyond this distance
    [SerializeField] private float minDistance = 1f;  // Full volume within this distance
    [SerializeField] private AnimationCurve volumeFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private float fadeOutBuffer = 0.5f; // Extra buffer for complete silence
    
    private AudioSource audioSource;
    private Transform playerTransform;
    private bool isSoundPlaying = false;
    
    private void Awake()
    {
        // Create audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound with manual volume control like ProximitySound
        audioSource.volume = 0f; // Start silent
    }
    
    private void Start()
    {
        // Find player reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log("GeckoTalkSound: Found player with tag 'Player'");
        }
        else
        {
            Debug.LogWarning("GeckoTalkSound: Player not found with tag 'Player'");
            if (PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
                Debug.Log("GeckoTalkSound: Found player using PlayerController.Instance");
            }
        }
        
        // Check if sound is assigned
        if (talkSound == null)
        {
            Debug.LogError("CRITICAL ERROR: No sound assigned to GeckoTalkSound! Please assign a sound in the Inspector.");
        }
        else
        {
            Debug.Log($"GeckoTalkSound: Sound '{talkSound.name}' is properly assigned and ready");
        }
    }
    
    private void Update()
    {
        // Only update volume if sound is playing
        if (isSoundPlaying)
        {
            UpdateVolume();
        }
    }
    
    // Update volume based on distance to player - similar to ProximitySound approach
    private void UpdateVolume()
    {
        if (playerTransform == null || audioSource == null) return;
        
        // Calculate distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        // Debug line to visualize player detection in Scene view
        Debug.DrawLine(transform.position, playerTransform.position, Color.red, Time.deltaTime);
        
        // Beyond max distance plus buffer, set volume to zero immediately
        if (distance > maxDistance + fadeOutBuffer)
        {
            audioSource.volume = 0f;
            return;
        }
        
        // Normalize distance between min and max
        float normalizedDistance = Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));
        
        // Use volume curve for smoother falloff
        float volumeFactor = volumeFalloff.Evaluate(normalizedDistance);
        
        // Scale between min and max volume
        float targetVolume = Mathf.Lerp(maxVolume, minVolume, normalizedDistance);
        
        // Apply curve-based falloff
        targetVolume *= volumeFactor;
        
        // Ensure volume is exactly 0 at max distance
        if (distance >= maxDistance)
        {
            targetVolume = 0f;
        }
        
        // Smooth transition to target volume
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * 3f);
        
        // Debug volume level
        if (Time.frameCount % 30 == 0) // Log every 30 frames to avoid spamming
        {
            Debug.Log($"GeckoTalkSound: Distance to player: {distance:F1}m, Volume: {audioSource.volume:F2}");
        }
    }
    
    public void PlayTalkSound()
    {
        if (talkSound == null)
        {
            Debug.LogError("ERROR: No talk sound assigned to GeckoTalkSound!");
            return;
        }
        
        // Set the clip and play
        audioSource.clip = talkSound;
        audioSource.Play();
        isSoundPlaying = true;
        
        // Initial volume update
        UpdateVolume();
        
        Debug.Log($"GeckoTalkSound: Started playing sound {talkSound.name}");
    }
    
    public void StopTalkSound()
    {
        // Stop the sound
        audioSource.Stop();
        isSoundPlaying = false;
        
        Debug.Log("GeckoTalkSound: Stopped playing sound");
    }
    
    // Helper method to visualize the hearing range in editor
    private void OnDrawGizmosSelected()
    {
        // Min distance - full volume
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        
        // Max distance - no sound
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
        
        // Max distance + buffer
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange
        Gizmos.DrawWireSphere(transform.position, maxDistance + fadeOutBuffer);
    }
} 