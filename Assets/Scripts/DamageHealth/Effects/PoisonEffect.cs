using System.Collections;
using UnityEngine;

public class PoisonEffect : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 2f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private Color poisonColor = new Color(0.4f, 1f, 0.4f, 0.7f);
    
    private EnemyHealth enemyHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isPoisoned = false;
    private Coroutine poisonCoroutine;
    
    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    public void ApplyPoison(float dps, float poisonDuration)
    {
        // Set values
        damagePerSecond = dps;
        duration = poisonDuration;
        
        // Stop existing poison and start new one
        if (poisonCoroutine != null)
        {
            StopCoroutine(poisonCoroutine);
        }
        
        poisonCoroutine = StartCoroutine(PoisonRoutine());
    }
    
    private IEnumerator PoisonRoutine()
    {
        isPoisoned = true;
        float elapsedTime = 0f;
        float damageInterval = 0.5f; // Apply damage every half second
        float timeSinceLastDamage = 0f;
        
        // Visual effect - tint the sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.color = poisonColor;
        }
        
        // Create poison particles
        GameObject poisonParticles = CreatePoisonParticles();
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            timeSinceLastDamage += Time.deltaTime;
            
            // Apply damage at intervals
            if (timeSinceLastDamage >= damageInterval)
            {
                timeSinceLastDamage = 0f;
                ApplyPoisonDamage(damagePerSecond * damageInterval);
            }
            
            yield return null;
        }
        
        // Stop poison effect
        isPoisoned = false;
        
        // Restore original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // Clean up particles
        if (poisonParticles != null)
        {
            Destroy(poisonParticles);
        }
    }
    
    private void ApplyPoisonDamage(float damage)
    {
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(Mathf.RoundToInt(damage));
        }
    }
    
    private GameObject CreatePoisonParticles()
    {
        // Create a simple particle effect for poison
        GameObject particleObject = new GameObject("PoisonParticles");
        particleObject.transform.parent = transform;
        particleObject.transform.localPosition = Vector3.zero;
        
        ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
        
        // Configure basic particle system
        var main = particleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.2f, 0.8f, 0.2f, 0.5f), new Color(0.3f, 1f, 0.3f, 0.7f));
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Emission
        var emission = particleSystem.emission;
        emission.rateOverTime = 10;
        
        // Shape
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
        
        particleSystem.Play();
        
        return particleObject;
    }
    
    private void OnDestroy()
    {
        // Clean up any running coroutines when the object is destroyed
        if (poisonCoroutine != null)
        {
            StopCoroutine(poisonCoroutine);
        }
    }
} 