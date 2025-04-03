using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance { get; private set; }
    
    [Header("Pickup Sounds")]
    [SerializeField] private AudioClip coinCollectSound;
    [SerializeField] private AudioClip heartCollectSound;
    
    [Header("Settings")]
    [SerializeField] private float defaultVolume = 1.0f;
    [SerializeField] private int maxSounds = 10; // Maximum number of simultaneous sounds
    
    // Pool of audio sources for playing multiple sounds
    private List<AudioSource> audioSources = new List<AudioSource>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create initial pool of audio sources
            for (int i = 0; i < maxSounds; i++)
            {
                CreateNewAudioSource();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Get an available audio source from the pool
    private AudioSource GetAvailableAudioSource()
    {
        // Try to find an audio source that's not playing
        foreach (AudioSource source in audioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        // If all are in use, create a new one (but don't exceed max)
        if (audioSources.Count < maxSounds)
        {
            return CreateNewAudioSource();
        }
        
        // If we've reached the maximum, reuse the oldest one
        AudioSource oldestSource = audioSources[0];
        audioSources.RemoveAt(0);
        audioSources.Add(oldestSource);
        oldestSource.Stop();
        return oldestSource;
    }
    
    // Create a new audio source on this GameObject
    private AudioSource CreateNewAudioSource()
    {
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.loop = false;
        audioSources.Add(newSource);
        return newSource;
    }
    
    // Play a sound effect
    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource();
        source.clip = clip;
        source.volume = volume * defaultVolume;
        source.Play();
    }
    
    // Play the coin collect sound
    public void PlayCoinCollectSound()
    {
        PlaySound(coinCollectSound, 1.0f);
        Debug.Log("Playing coin collect sound");
    }
    
    // Play the heart collect sound
    public void PlayHeartCollectSound()
    {
        PlaySound(heartCollectSound, 1.0f);
        Debug.Log("Playing heart collect sound");
    }
} 