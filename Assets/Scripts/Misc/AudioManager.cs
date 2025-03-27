using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Settings")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private List<AudioClip> normalPlaylist = new List<AudioClip>();
    [SerializeField] private float crossFadeDuration = 1.5f;
    [SerializeField] private float minTimeBetweenSongs = 0.5f;

    [Header("Boss Music")]
    [SerializeField] private bool debugMode = false;
    
    private List<int> shuffledPlaylistIndices = new List<int>();
    private int currentPlaylistIndex = 0;
    private bool isSwitchingTracks = false;
    private AudioClip currentBossTheme = null;
    private AudioClip previousNormalTrack = null;
    private bool wasPlayingBossTheme = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize audio source if not set
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = false;
            musicSource.playOnAwake = false;
        }
        
        // Initialize the shuffled playlist
        ShufflePlaylist();
    }
    
    private void Start()
    {
        // Start playing the first song
        if (normalPlaylist.Count > 0 && !isSwitchingTracks)
        {
            PlayNextSong();
        }
    }
    
    private void Update()
    {
        // Check if we need to play the next song in the normal playlist
        if (!isSwitchingTracks && currentBossTheme == null && !musicSource.isPlaying && normalPlaylist.Count > 0)
        {
            if (debugMode) Debug.Log("Music ended, playing next track");
            PlayNextSong();
        }
    }
    
    private void ShufflePlaylist()
    {
        // Create an ordered list of indices
        shuffledPlaylistIndices = new List<int>();
        for (int i = 0; i < normalPlaylist.Count; i++)
        {
            shuffledPlaylistIndices.Add(i);
        }
        
        // Shuffle the indices
        for (int i = 0; i < shuffledPlaylistIndices.Count; i++)
        {
            int temp = shuffledPlaylistIndices[i];
            int randomIndex = Random.Range(i, shuffledPlaylistIndices.Count);
            shuffledPlaylistIndices[i] = shuffledPlaylistIndices[randomIndex];
            shuffledPlaylistIndices[randomIndex] = temp;
        }
        
        // Reset the playlist index
        currentPlaylistIndex = 0;
    }
    
    private void PlayNextSong()
    {
        if (normalPlaylist.Count == 0) return;
        
        // If we've reached the end of the playlist, shuffle again
        if (currentPlaylistIndex >= shuffledPlaylistIndices.Count)
        {
            ShufflePlaylist();
        }
        
        // Get the next song
        int nextIndex = shuffledPlaylistIndices[currentPlaylistIndex];
        currentPlaylistIndex++;
        
        // Play the song
        AudioClip nextSong = normalPlaylist[nextIndex];
        StartCoroutine(FadeAndPlayTrack(nextSong));
    }
    
    private IEnumerator FadeAndPlayTrack(AudioClip newTrack, bool isBossTrack = false)
    {
        if (isSwitchingTracks) yield break;
        isSwitchingTracks = true;
        
        // Store current track info (if it's a normal track and we're switching to boss)
        if (!wasPlayingBossTheme && isBossTrack)
        {
            previousNormalTrack = musicSource.clip;
        }
        
        // If we have a track playing, fade it out
        float currentVolume = musicSource.volume;
        if (musicSource.isPlaying)
        {
            // Fade out current track
            float timer = 0;
            while (timer < crossFadeDuration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(currentVolume, 0, timer / crossFadeDuration);
                yield return null;
            }
        }
        
        // Switch tracks
        musicSource.Stop();
        musicSource.clip = newTrack;
        musicSource.volume = 0;
        musicSource.Play();
        
        // Update boss theme tracking
        wasPlayingBossTheme = isBossTrack;
        if (isBossTrack)
        {
            currentBossTheme = newTrack;
        }
        else
        {
            currentBossTheme = null;
        }
        
        // Fade in new track
        float fadeInTimer = 0;
        while (fadeInTimer < crossFadeDuration)
        {
            fadeInTimer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, 1f, fadeInTimer / crossFadeDuration);
            yield return null;
        }
        
        musicSource.volume = 1f;
        isSwitchingTracks = false;
    }
    
    // Called when player enters a boss's attack range
    public void PlayBossTheme(AudioClip bossTheme)
    {
        if (bossTheme == currentBossTheme) return;
        if (debugMode) Debug.Log("Playing boss theme: " + bossTheme.name);
        
        StartCoroutine(FadeAndPlayTrack(bossTheme, true));
    }
    
    // Called when player exits all boss attack ranges
    public void ReturnToNormalMusic()
    {
        if (currentBossTheme == null) return;
        if (debugMode) Debug.Log("Returning to normal music");
        
        // If we stored a previous track, return to it
        if (previousNormalTrack != null)
        {
            StartCoroutine(FadeAndPlayTrack(previousNormalTrack));
        }
        else
        {
            PlayNextSong();
        }
    }
} 