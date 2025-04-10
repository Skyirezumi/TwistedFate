using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flash : MonoBehaviour
{
    [SerializeField] private Material whiteFlashMat;
    [SerializeField] private float restoreDefaultMatTime = .2f;

    private Material defaultMat;
    private SpriteRenderer spriteRenderer;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultMat = spriteRenderer.material;
    }

    public float GetRestoreMatTime() {
        return restoreDefaultMatTime;
    }

    public IEnumerator FlashRoutine() {
        spriteRenderer.material = whiteFlashMat;
        yield return new WaitForSeconds(restoreDefaultMatTime);
        spriteRenderer.material = defaultMat;
    }
    
    // Flash with a custom color - useful for healing, buffs, etc.
    public void FlashColor(Color color, float duration) {
        StartCoroutine(FlashColorRoutine(color, duration));
    }
    
    private IEnumerator FlashColorRoutine(Color color, float duration) {
        // Store original color
        Color originalColor = spriteRenderer.color;
        
        // Set new color
        spriteRenderer.color = color;
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Restore original color
        spriteRenderer.color = originalColor;
    }
}
