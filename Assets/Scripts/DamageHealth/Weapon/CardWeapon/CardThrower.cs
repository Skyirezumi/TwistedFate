using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardThrower : MonoBehaviour
{
    [Header("Card Prefabs")]
    [SerializeField] private GameObject redCardPrefab;
    [SerializeField] private GameObject greenCardPrefab;
    [SerializeField] private GameObject blueCardPrefab;
    [SerializeField] private Transform throwPoint;
    
    [Header("Throwing Settings")]
    [SerializeField] private float throwCooldown = 0.5f;
    [SerializeField] private float cardSpeed = 10f;
    [SerializeField] private AudioClip throwSound;
    
    [Header("UI")]
    [SerializeField] private AudioClip cooldownEndSound;
    [SerializeField] private Image cooldownImage; // UI Image for cooldown indicator
    
    private bool canThrow = true;
    private Camera mainCamera;
    private float currentCooldown;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        // Initialize cooldown UI if assigned
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0;
        }
    }
    
    // This is now only used for cooldown tracking
    private void Update()
    {
        // Cooldown timer update for UI
        if (!canThrow)
        {
            UpdateCooldownUI();
        }
    }
    
    // This method is called by PlayerController using the new Input System
    public void TriggerThrowCard()
    {
        if (canThrow)
        {
            ThrowCard();
        }
    }
    
    private void ThrowCard()
    {
        // Start cooldown
        StartCoroutine(ThrowCooldown());
        
        // Get mouse position in world space
        Vector3 mousePosition = GetMouseWorldPosition();
        
        // Calculate direction precisely - direction from throw point to mouse
        Vector2 direction = (mousePosition - throwPoint.position).normalized;
        
        // Randomly select which card to throw
        int randomCard = Random.Range(0, 3);
        GameObject cardPrefab = null;
        
        switch (randomCard)
        {
            case 0:
                cardPrefab = redCardPrefab;
                break;
            case 1:
                cardPrefab = greenCardPrefab;
                break;
            case 2:
                cardPrefab = blueCardPrefab;
                break;
        }
        
        if (cardPrefab == null)
        {
            Debug.LogWarning("Missing card prefab!");
            return;
        }
        
        // Play throw sound if assigned
        if (throwSound != null)
        {
            AudioSource.PlayClipAtPoint(throwSound, throwPoint.position, 0.6f);
        }
        
        // Instantiate selected card
        GameObject cardObject = Instantiate(cardPrefab, throwPoint.position, Quaternion.identity);
        Card card = cardObject.GetComponent<Card>();
        
        if (card != null)
        {
            // Calculate angle and set rotation - more precise calculation
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            cardObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            // Set card speed from this component
            card.SetSpeed(cardSpeed);
            
            // Launch the card
            card.Launch(direction);
        }
    }
    
    // Gets precise mouse world position for accurate aiming
    private Vector3 GetMouseWorldPosition()
    {
        // Get the mouse position in screen space
        Vector3 screenPosition = Input.mousePosition;
        
        // Set the z position based on distance from camera to throwPoint
        screenPosition.z = mainCamera.WorldToScreenPoint(throwPoint.position).z;
        
        // Convert to world space with the correct z depth
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        
        return worldPosition;
    }
    
    private IEnumerator ThrowCooldown()
    {
        canThrow = false;
        currentCooldown = throwCooldown;
        
        // Update UI immediately to show full cooldown
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 1;
        }
        
        // Wait for cooldown
        yield return new WaitForSeconds(throwCooldown);
        
        // Cooldown finished
        canThrow = true;
        
        // Play cooldown end sound if assigned
        if (cooldownEndSound != null)
        {
            AudioSource.PlayClipAtPoint(cooldownEndSound, transform.position, 0.6f);
        }
        
        // Reset cooldown UI
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0;
        }
    }
    
    private void UpdateCooldownUI()
    {
        if (cooldownImage != null)
        {
            // Decrease cooldown timer
            currentCooldown -= Time.deltaTime;
            
            // Update fill amount (1 = full, 0 = empty)
            cooldownImage.fillAmount = currentCooldown / throwCooldown;
        }
    }
} 