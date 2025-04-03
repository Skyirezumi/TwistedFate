using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeckoNPC : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    
    [Header("Sound Settings")]
    [SerializeField] private AudioClip[] talkingSounds;
    [SerializeField] private float soundFrequency = 0.2f; // How often to play sounds during dialogue
    [SerializeField] private float minVolume = 0.2f;
    [SerializeField] private float maxVolume = 1.0f;
    [SerializeField] private float maxHearingDistance = 10f;
    [SerializeField] private float minHearingDistance = 2f;
    
    private AudioSource audioSource;
    private Coroutine talkingCoroutine;
    private bool isTalking = false;
    private Transform playerTransform;
    
    private void Awake()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.volume = maxVolume;
        }
        
        // Find player
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
    }
    
    // Start talking - called when dialogue begins
    public void StartTalking()
    {
        if (isTalking) return;
        
        isTalking = true;
        if (talkingCoroutine != null)
        {
            StopCoroutine(talkingCoroutine);
        }
        talkingCoroutine = StartCoroutine(PlayTalkingSounds());
    }
    
    // Stop talking - called when dialogue ends
    public void StopTalking()
    {
        isTalking = false;
        if (talkingCoroutine != null)
        {
            StopCoroutine(talkingCoroutine);
            talkingCoroutine = null;
        }
    }
    
    // This coroutine plays talking sounds at random intervals while the NPC is talking
    private IEnumerator PlayTalkingSounds()
    {
        if (talkingSounds == null || talkingSounds.Length == 0)
        {
            Debug.LogWarning("GeckoNPC: No talking sounds assigned!");
            yield break;
        }
        
        while (isTalking)
        {
            // Choose a random talking sound
            AudioClip randomSound = talkingSounds[Random.Range(0, talkingSounds.Length)];
            
            if (randomSound != null)
            {
                // Calculate volume based on distance to player
                float volume = CalculateVolumeBasedOnDistance();
                
                // Randomize pitch slightly for variety
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                
                // Play the sound
                audioSource.PlayOneShot(randomSound, volume);
                
                // Debug
                Debug.Log($"GeckoNPC: Playing talking sound at volume {volume}");
            }
            
            // Wait a random amount of time before the next sound
            float waitTime = soundFrequency * Random.Range(0.8f, 1.2f);
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    // Update is called each frame to adjust the volume based on player distance
    private void Update()
    {
        if (isTalking && playerTransform != null)
        {
            // Update volume based on player distance
            audioSource.volume = CalculateVolumeBasedOnDistance();
        }
    }
    
    // Calculate volume based on distance to player
    private float CalculateVolumeBasedOnDistance()
    {
        if (playerTransform == null)
        {
            // Default to min volume if player not found
            return minVolume;
        }
        
        // Calculate distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        // Clamp distance between min and max hearing distances
        float clampedDistance = Mathf.Clamp(distance, minHearingDistance, maxHearingDistance);
        
        // Calculate normalized distance (0 = at min distance, 1 = at max distance)
        float normalizedDistance = (clampedDistance - minHearingDistance) / (maxHearingDistance - minHearingDistance);
        
        // Inverse lerp: closer = louder
        float volume = Mathf.Lerp(maxVolume, minVolume, normalizedDistance);
        
        return volume;
    }
    
    // Public method to test talking
    public void Test_StartTalking()
    {
        StartTalking();
        
        // Automatically stop after 5 seconds for testing
        StartCoroutine(StopAfterDelay(5f));
    }
    
    private IEnumerator StopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopTalking();
    }
} 