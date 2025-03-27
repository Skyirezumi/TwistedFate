using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    [Tooltip("Arrow sprite prefab that will point to enemies")]
    [SerializeField] private GameObject arrowPrefab;
    
    [Tooltip("Distance from the pivot where the arrow will appear")]
    [SerializeField] private float indicatorRadius = 1f;
    
    [Tooltip("Offset the indicator vertically from pivot's center")]
    [SerializeField] private float verticalOffset = 0.5f;
    
    [Tooltip("Transform that arrows will rotate around. If null, uses player")]
    [SerializeField] private Transform pivotTransform;
    
    [Tooltip("The angle offset to apply to the arrow (if arrow points up, use -90)")]
    [SerializeField] private float arrowAngleOffset = -90f;
    
    // Dictionary to track which enemies have arrows assigned
    private Dictionary<GameObject, GameObject> enemyArrows = new Dictionary<GameObject, GameObject>();
    
    // Reference to player transform for positioning
    private Transform playerTransform;
    
    // Singleton pattern for easy access
    public static DirectionalIndicator Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Find player reference on start
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
            
            // If no pivot is specified, use the player
            if (pivotTransform == null)
            {
                pivotTransform = playerTransform;
            }
            
            // Position this object at the pivot
            transform.position = pivotTransform.position;
            
            // Make this follow the pivot if it's the player
            if (pivotTransform == playerTransform)
            {
                transform.SetParent(playerTransform);
                transform.localPosition = Vector3.zero;
            }
        }
    }
    
    private void Update()
    {
        // Update player reference if needed
        if (playerTransform == null && PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
            if (pivotTransform == null)
            {
                pivotTransform = playerTransform;
            }
        }
        
        UpdateArrows();
        CleanupDestroyedEnemies();
    }
    
    /// <summary>
    /// Add an enemy to be tracked with a directional arrow
    /// </summary>
    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null || enemyArrows.ContainsKey(enemy))
            return;
            
        // Create a new arrow for this enemy
        GameObject arrow = Instantiate(arrowPrefab, transform);
        enemyArrows.Add(enemy, arrow);
    }
    
    /// <summary>
    /// Remove an enemy from being tracked
    /// </summary>
    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemy == null || !enemyArrows.ContainsKey(enemy))
            return;
            
        // Destroy the arrow associated with this enemy
        if (enemyArrows[enemy] != null)
        {
            Destroy(enemyArrows[enemy]);
        }
        
        enemyArrows.Remove(enemy);
    }
    
    /// <summary>
    /// Update position and rotation of all arrows to point to their targets
    /// </summary>
    private void UpdateArrows()
    {
        if (pivotTransform == null)
            return;
            
        Vector3 pivotPosition = pivotTransform.position;
        
        foreach (var pair in enemyArrows)
        {
            GameObject enemy = pair.Key;
            GameObject arrow = pair.Value;
            
            if (enemy == null || arrow == null)
                continue;
                
            // Get direction from pivot to enemy
            Vector3 direction = enemy.transform.position - pivotPosition;
            direction.z = 0;
            
            // Position the arrow at the radius distance from pivot in the direction of the enemy
            Vector3 arrowPosition = pivotPosition + direction.normalized * indicatorRadius;
            arrowPosition.y += verticalOffset;
            
            // If pivot is player, use local position
            if (pivotTransform == playerTransform && arrow.transform.parent == transform)
            {
                arrow.transform.localPosition = transform.InverseTransformPoint(arrowPosition);
            }
            else
            {
                arrow.transform.position = arrowPosition;
            }
            
            // Calculate the angle to rotate the arrow
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle + arrowAngleOffset);
        }
    }
    
    /// <summary>
    /// Remove any destroyed enemies from our tracking
    /// </summary>
    private void CleanupDestroyedEnemies()
    {
        List<GameObject> enemiesToRemove = new List<GameObject>();
        
        foreach (var enemy in enemyArrows.Keys)
        {
            if (enemy == null)
            {
                enemiesToRemove.Add(enemy);
            }
        }
        
        foreach (var enemy in enemiesToRemove)
        {
            if (enemyArrows.TryGetValue(enemy, out GameObject arrow) && arrow != null)
            {
                Destroy(arrow);
            }
            enemyArrows.Remove(enemy);
        }
    }
} 