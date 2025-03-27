using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioManagerSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Game/Setup/Create Audio Manager")]
    public static void CreateAudioManager()
    {
        // Check if AudioManager already exists
        if (AudioManager.Instance != null)
        {
            Debug.Log("AudioManager already exists in the scene!");
            Selection.activeGameObject = AudioManager.Instance.gameObject;
            return;
        }
        
        // Create new GameObject
        GameObject audioManagerObj = new GameObject("AudioManager");
        
        // Add AudioManager component
        AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();
        
        // Add Audio Source
        AudioSource audioSource = audioManagerObj.AddComponent<AudioSource>();
        
        // Set some basic properties
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 1f;
        
        // Select the new object
        Selection.activeGameObject = audioManagerObj;
        
        Debug.Log("AudioManager created! Now add music clips to the 'Normal Playlist'");
    }
#endif
} 