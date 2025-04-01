using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAreaManager : MonoBehaviour
{
    public static PlayAreaManager Instance { get; private set; }
    
    // Play Area Boundaries - non-serialized constants
    private const float radius = 65.0f; // 1.3x larger (130-unit diameter)
    private Vector2 center = Vector2.zero;
    
    // Public properties to access boundaries
    public float Radius => radius;
    public Vector2 Center => center;
    
    [Header("Wall Settings")]
    [SerializeField] private Color wallColor = Color.red;
    [SerializeField] private float wallThickness = 0.5f;
    [SerializeField] private int segments = 32; // Number of segments for circular boundary
    
    // Wall visibility
    [SerializeField] private bool addSpriteRenderers = false; // Disable sprite renderers by default
    [SerializeField] private bool use3DRenderers = false; // Disable 3D renderers
    [SerializeField] private bool useLineRenderer = true; // Use line renderer for cleaner visualization
    
    // Wall containers
    private List<GameObject> wallSegments = new List<GameObject>();
    private LineRenderer circleLineRenderer;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            
            // Create walls immediately in Awake
            CreateCircularBoundary();
            
            // Add line renderer visualization if needed
            if (useLineRenderer)
            {
                CreateCircleLineRenderer();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void CreateCircularBoundary()
    {
        // Create a parent object for the walls
        GameObject wallContainer = new GameObject("CircularBoundary");
        wallContainer.transform.parent = transform;
        
        // Clear any existing walls
        wallSegments.Clear();
        
        // Create segments around the circle
        for (int i = 0; i < segments; i++)
        {
            float angle = i * (360f / segments);
            float nextAngle = (i + 1) * (360f / segments);
            
            // Create a wall segment
            GameObject segment = CreateWallSegment($"Segment_{i}", wallContainer.transform, angle, nextAngle);
            wallSegments.Add(segment);
        }
    }
    
    private GameObject CreateWallSegment(string name, Transform parent, float startAngle, float endAngle)
    {
        GameObject wall = new GameObject(name);
        wall.transform.parent = parent;
        
        // Calculate positions
        float startRad = startAngle * Mathf.Deg2Rad;
        float endRad = endAngle * Mathf.Deg2Rad;
        
        Vector2 startPos = new Vector2(Mathf.Cos(startRad), Mathf.Sin(startRad)) * radius;
        Vector2 endPos = new Vector2(Mathf.Cos(endRad), Mathf.Sin(endRad)) * radius;
        
        // Calculate center point of this segment
        Vector2 midPoint = (startPos + endPos) / 2;
        wall.transform.position = new Vector3(midPoint.x, midPoint.y, 0);
        
        // Calculate direction from center to midpoint for proper orientation
        Vector2 dirToCenterNormalized = midPoint.normalized;
        
        // Get the angle from the vector
        float radiansToDegrees = 180f / Mathf.PI;
        float segmentAngle = Mathf.Atan2(dirToCenterNormalized.y, dirToCenterNormalized.x) * radiansToDegrees;
        
        // Rotate 90 degrees to make the wall perpendicular to the radius
        segmentAngle += 90f;
        wall.transform.rotation = Quaternion.Euler(0, 0, segmentAngle);
        
        // Add collider for physics interactions
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        
        // Calculate segment length using the chord length formula
        float segmentLength = Vector2.Distance(startPos, endPos);
        collider.size = new Vector2(segmentLength, wallThickness);
        
        // Add a Sprite Renderer for 2D visibility only if needed
        if (addSpriteRenderers)
        {
            SpriteRenderer spriteRenderer = wall.AddComponent<SpriteRenderer>();
            spriteRenderer.color = wallColor;
            spriteRenderer.sortingOrder = 1000; // Ensure it's on top
            
            // Create a texture for the wall
            Texture2D tex = new Texture2D(4, 4);
            Color whiteColor = Color.white;
            
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    tex.SetPixel(x, y, whiteColor);
                }
            }
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            
            // Create sprite
            spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
            
            // Scale the sprite to match the collider
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = new Vector2(segmentLength, wallThickness);
        }
        
        // Add a Rigidbody2D for physics
        Rigidbody2D rb = wall.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        
        return wall;
    }
    
    private void CreateCircleLineRenderer()
    {
        // Create a child game object for the line renderer
        GameObject lineObj = new GameObject("BoundaryLine");
        lineObj.transform.parent = transform;
        
        // Add and configure line renderer
        circleLineRenderer = lineObj.AddComponent<LineRenderer>();
        circleLineRenderer.startWidth = wallThickness * 1.5f; // Slightly thicker line
        circleLineRenderer.endWidth = wallThickness * 1.5f;
        
        // Create a material for the line renderer
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = wallColor;
        circleLineRenderer.material = lineMaterial;
        
        // Bright red with slight transparency
        Color brightRed = new Color(1f, 0.2f, 0.2f, 0.9f);
        circleLineRenderer.startColor = brightRed;
        circleLineRenderer.endColor = brightRed;
        
        circleLineRenderer.sortingOrder = 2000; // Make sure it's visible
        circleLineRenderer.useWorldSpace = true;
        circleLineRenderer.loop = true; // Connect end to beginning
        circleLineRenderer.alignment = LineAlignment.View; // Always face camera
        
        // Calculate positions around the circle
        int pointCount = 120; // More points for smoother circle
        circleLineRenderer.positionCount = pointCount;
        
        for (int i = 0; i < pointCount; i++)
        {
            float angle = i * (360f / pointCount) * Mathf.Deg2Rad;
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            float z = -0.1f; // Slightly in front
            circleLineRenderer.SetPosition(i, new Vector3(x, y, z));
        }
    }
    
    // Clamp a position within the circular boundary
    public Vector3 ClampToPlayArea(Vector3 position)
    {
        Vector2 pos2D = new Vector2(position.x, position.y);
        Vector2 direction = pos2D - center;
        float distance = direction.magnitude;
        
        if (distance > radius)
        {
            // Clamp to radius
            direction = direction.normalized * radius;
            return new Vector3(center.x + direction.x, center.y + direction.y, position.z);
        }
        
        return position;
    }
    
    // Check if a position is within the play area
    public bool IsInPlayArea(Vector3 position)
    {
        Vector2 pos2D = new Vector2(position.x, position.y);
        return Vector2.Distance(pos2D, center) <= radius;
    }
    
    // Get a random position within the play area
    public Vector3 GetRandomPositionInPlayArea()
    {
        // Random angle and distance within the circle
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(0f, radius);
        
        float x = center.x + Mathf.Cos(angle) * distance;
        float y = center.y + Mathf.Sin(angle) * distance;
        
        return new Vector3(x, y, 0);
    }
    
    // Visualize the circle in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, radius);
    }
} 