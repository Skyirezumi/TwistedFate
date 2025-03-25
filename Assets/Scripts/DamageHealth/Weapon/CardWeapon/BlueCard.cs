using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueCard : Card
{
    [Header("Blue Card Settings")]
    [SerializeField] private int splitCardCount = 3;
    [SerializeField] private float splitAngle = 30f;
    [SerializeField] private GameObject splitEffectPrefab;
    
    protected override void Start()
    {
        base.Start();
        // Set blue trail color
        trailColor = Color.blue;
        SetupTrail();
    }
    
    public override void ApplySpecialEffect(GameObject target)
    {
        if (ShouldBackfire())
        {
            // Future implementation: Backfire effect
            // e.g. Split cards target player instead of continuing forward
            Debug.Log("Blue card backfired! (Effect not implemented yet)");
            return;
        }
        
        // Currently just does normal damage through DamageSource
        // Future: Will split into multiple cards on impact
        
        // Visual effect for hit
        if (splitEffectPrefab != null)
        {
            Instantiate(splitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        Debug.Log("Blue card hit: Future split effect will be implemented here");
    }
} 