using UnityEngine;

public class GeckoTestAudio : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private AudioClip testSound;
    [SerializeField] private float volume = 20f; // EXTREMELY LOUD
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        Debug.Log("GeckoTestAudio: Initializing");
        
        // Create audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // Pure 2D sound
        audioSource.volume = volume;
        
        Debug.Log("GeckoTestAudio: Audio source created with volume " + volume);
    }
    
    private void Start()
    {
        // Play a sound on Start
        PlayTestSound();
    }
    
    private void Update()
    {
        // Also play sound when spacebar is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("GeckoTestAudio: Space key pressed");
            PlayTestSound();
        }
    }
    
    public void PlayTestSound()
    {
        if (testSound == null)
        {
            Debug.LogError("ERROR: Test sound is null! Please assign a sound in the Inspector.");
            return;
        }
        
        Debug.Log($"GeckoTestAudio: Playing test sound '{testSound.name}' at volume {volume}");
        
        // Play directly if possible
        audioSource.PlayOneShot(testSound, volume);
        
        // Also try alternative play method
        AudioSource.PlayClipAtPoint(testSound, Camera.main.transform.position, volume);
        
        // Try playing on main camera as fallback
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            AudioSource cameraAudio = mainCamera.GetComponent<AudioSource>();
            if (cameraAudio == null)
            {
                cameraAudio = mainCamera.gameObject.AddComponent<AudioSource>();
            }
            cameraAudio.PlayOneShot(testSound, volume);
            Debug.Log("GeckoTestAudio: Playing sound through main camera");
        }
        
        Debug.Log("GeckoTestAudio: Sound play methods called");
    }
} 