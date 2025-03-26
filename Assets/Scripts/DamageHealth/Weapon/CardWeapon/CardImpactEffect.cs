using UnityEngine;

public class CardImpactEffect : MonoBehaviour
{
    public void SetExplosionSize(float radius)
    {
        // Base scaling factor (1.0 = default size)
        float baseRadius = 1.0f;
        float scaleFactor = radius / baseRadius;
        
        // Scale the transform (affects overall size)
        transform.localScale = Vector3.one * scaleFactor;
        
        // Get all particle systems and adjust values
        ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in systems)
        {
            // Get particle system modules
            var main = ps.main;
            var emission = ps.emission;
            
            // Scale speed based on radius (particles travel further)
            main.startSpeedMultiplier = main.startSpeed.constant * scaleFactor;
            
            // Scale lifetime slightly (bigger explosions last longer)
            main.startLifetimeMultiplier *= Mathf.Sqrt(scaleFactor);
            
            // Increase particle count for larger explosions
            if (emission.burstCount > 0)
            {
                ParticleSystem.Burst burst = emission.GetBurst(0);
                burst.count = burst.count.constant * scaleFactor;
                emission.SetBurst(0, burst);
            }
            
            // Scale start size too (optional)
            main.startSizeMultiplier *= scaleFactor * 0.7f; // 0.7f prevents oversizing
        }
        
        // If you have a light component, scale its range too
        Light light = GetComponentInChildren<Light>();
        if (light)
        {
            light.range = light.range * scaleFactor;
            light.intensity = light.intensity * (scaleFactor * 0.8f); // Prevent over-brightening
        }
    }
    
    public void SetColor(Color color)
    {
        ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in systems)
        {
            var main = ps.main;
            main.startColor = color;
        }
    }
} 