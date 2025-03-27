using UnityEngine;

/// <summary>
/// Attach this script to any enemy that should have a directional indicator arrow pointing to it
/// </summary>
public class TrackableEnemy : MonoBehaviour
{
    // Reference to the DirectionalIndicator singleton
    private object directionalIndicator;
    
    private void Start()
    {
        // Find the DirectionalIndicator by type
        directionalIndicator = FindObjectOfType(typeof(MonoBehaviour).Assembly.GetType("DirectionalIndicator"));
        
        // Register with the directional indicator system if found
        if (directionalIndicator != null)
        {
            object instance = directionalIndicator.GetType().GetProperty("Instance").GetValue(null);
            if (instance != null)
            {
                directionalIndicator.GetType().GetMethod("RegisterEnemy").Invoke(instance, new object[] { gameObject });
            }
        }
    }
    
    private void OnDestroy()
    {
        // Unregister when destroyed if directional indicator exists
        if (directionalIndicator != null)
        {
            object instance = directionalIndicator.GetType().GetProperty("Instance").GetValue(null);
            if (instance != null)
            {
                directionalIndicator.GetType().GetMethod("UnregisterEnemy").Invoke(instance, new object[] { gameObject });
            }
        }
    }
} 