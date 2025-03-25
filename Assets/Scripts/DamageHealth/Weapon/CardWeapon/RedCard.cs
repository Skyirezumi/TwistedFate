using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedCard : Card
{
    [Header("Red Card Settings")]
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private GameObject explosionEffectPrefab;
    
    protected override void Start()
    {
        base.Start();
        // Set red trail color
        trailColor = Color.red;
        SetupTrail();
    }
    
    public override void ApplySpecialEffect(GameObject target)
    {
        if (ShouldBackfire())
        {
            // Future implementation: Backfire effect
            // e.g. Explosion hurts player instead of enemies
            Debug.Log("Red card backfired! (Effect not implemented yet)");
            return;
        }
        
        // Currently just does normal damage through DamageSource
        // Future: Will create an explosion that damages all enemies in radius
        
        // Visual effect for hit
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        
        Debug.Log("Red card hit: Future explosion effect will be implemented here");
    }
} 