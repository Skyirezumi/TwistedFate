using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenCard : Card
{
    [Header("Green Card Settings")]
    [SerializeField] private float poisonDuration = 3f;
    [SerializeField] private GameObject poisonEffectPrefab;
    
    protected override void Start()
    {
        base.Start();
        // Set green trail color
        trailColor = Color.green;
        SetupTrail();
    }
    
    public override void ApplySpecialEffect(GameObject target)
    {
        if (ShouldBackfire())
        {
            // Future implementation: Backfire effect
            // e.g. Poison affects player instead of enemies
            Debug.Log("Green card backfired! (Effect not implemented yet)");
            return;
        }
        
        // Currently just does normal damage through DamageSource
        // Future: Will apply poison effect that does damage over time
        
        // Visual effect for hit
        if (poisonEffectPrefab != null)
        {
            Instantiate(poisonEffectPrefab, transform.position, Quaternion.identity);
        }
        
        Debug.Log("Green card hit: Future poison effect will be implemented here");
    }
} 