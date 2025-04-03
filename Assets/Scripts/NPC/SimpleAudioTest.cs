using UnityEngine;

public class SimpleAudioTest : MonoBehaviour
{
    [SerializeField] private AudioClip testSound;
    [SerializeField] private float volume = 50f; // EXTREMELY LOUD
    
    // Start is called before the first frame update
    void Start()
    {
        // Play sound on start - basic test
        PlayTestSound();
    }
    
    // Update is called once per frame
    void Update()
    {
        // Also play on spacebar press
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayTestSound();
        }
    }
    
    public void PlayTestSound()
    {
        if (testSound == null)
        {
            Debug.LogError("SimpleAudioTest: No test sound assigned!");
            return;
        }
        
        // Create a temporary AudioSource
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = transform.position;
        
        // Add an AudioSource and configure it
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        tempSource.clip = testSound;
        tempSource.volume = volume;
        tempSource.spatialBlend = 0f; // 2D sound for testing
        tempSource.outputAudioMixerGroup = null; // Use default mixer
        tempSource.Play();
        
        // Log that we're playing the sound
        Debug.Log($"SimpleAudioTest: Playing sound '{testSound.name}' at EXTREME volume {volume}!");
        
        // Destroy the temporary GameObject after the clip finishes
        Destroy(tempGO, testSound.length + 0.1f);
    }
} 