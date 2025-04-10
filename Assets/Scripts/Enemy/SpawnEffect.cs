using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEffect : MonoBehaviour
{
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private ParticleSystem spawnParticles;
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private float volume = 0.7f;
    
    private void Start()
    {
        // Start self-destruct timer
        Destroy(gameObject, lifetime);
        
        // Play particles if available
        if (spawnParticles != null)
        {
            spawnParticles.Play();
        }
        
        // Play sound if available
        if (spawnSound != null)
        {
            // Play using AudioSource.PlayClipAtPoint which doesn't require AudioManager
            AudioSource.PlayClipAtPoint(spawnSound, transform.position, volume);
        }
    }
} 