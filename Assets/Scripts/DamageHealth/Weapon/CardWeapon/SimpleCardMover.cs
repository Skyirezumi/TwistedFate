using UnityEngine;

// Simple movement component for split cards
public class SimpleCardMover : MonoBehaviour
{
    public Vector2 direction = Vector2.up;
    public float speed = 10f;
    
    void Update()
    {
        // Very basic movement implementation
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }
} 