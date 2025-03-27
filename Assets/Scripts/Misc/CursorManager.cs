using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private bool lockCursor = true;
    [SerializeField] private CursorLockMode cursorLockMode = CursorLockMode.Confined;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
            Debug.LogError("CursorManager requires a SpriteRenderer component!");
    }

    void Start()
    {
        // Hide the cursor immediately
        HideCursor();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Re-hide cursor when application regains focus
        if (hasFocus)
            HideCursor();
    }

    void Update()
    {
        // Keep cursor hidden every frame to prevent it from reappearing
        Cursor.visible = false;

        // Update custom cursor position
        Vector2 cursorPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(cursorPos.x, cursorPos.y, Camera.main.nearClipPlane));
        worldPos.z = 0; // Keep the cursor at z=0 so it's visible in 2D
        transform.position = worldPos;
        
        // Handle escape key to toggle cursor lock (for testing/debugging)
        if (Input.GetKeyDown(KeyCode.Escape) && lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetMouseButtonDown(0) && lockCursor)
        {
            HideCursor();
        }
    }
    
    private void HideCursor()
    {
        // Hide system cursor
        Cursor.visible = false;
        
        // Lock the cursor according to settings
        if (lockCursor)
            Cursor.lockState = cursorLockMode;
    }
    
    private void OnDisable()
    {
        // Make cursor visible again when component is disabled
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
