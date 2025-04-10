using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class BossDialogPair
{
    public GameObject bossPrefab;
    public string dialogText;
}

public class GeccoInteraction : MonoBehaviour
{
    [Tooltip("Interaction prompt GameObject")]
    [SerializeField] private GameObject interactionPrompt;
    
    [Tooltip("Distance at which player can interact")]
    [SerializeField] private float interactionRange = 3f;
    
    [Tooltip("Cooldown between spawns in seconds")]
    [SerializeField] private float spawnCooldown = 10f;
    
    [Header("Boss Settings")]
    [Tooltip("Boss prefabs with their dialog text")]
    [SerializeField] private List<BossDialogPair> bossDialogPairs = new List<BossDialogPair>();
    
    [Tooltip("Spawn offset from the player")]
    [SerializeField] private float spawnDistanceFromPlayer = 5f;
    
    [Header("Dialog Settings")]
    [Tooltip("Dialog Container GameObject")]
    [SerializeField] private GameObject dialogContainer;
    
    private bool canInteract = true;
    private bool isInRange = false;
    private float cooldownTimer = 0f;
    private DialogManager dialogManager;
    private GeckoTalkSound talkSound;
    
    private void Start()
    {
        // Ensure interaction prompt is hidden at start
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Get dialog manager
        if (dialogContainer != null)
        {
            dialogManager = dialogContainer.GetComponent<DialogManager>();
            if (dialogManager == null)
            {
                dialogManager = dialogContainer.AddComponent<DialogManager>();
            }
        }
        
        // Get sound component
        talkSound = GetComponent<GeckoTalkSound>();
        if (talkSound == null)
        {
            // Add it if missing
            talkSound = gameObject.AddComponent<GeckoTalkSound>();
            Debug.Log("GeccoInteraction: Added GeckoTalkSound component");
        }
    }
    
    private void Update()
    {
        CheckPlayerDistance();
        HandleInteraction();
        HandleCooldown();
    }
    
    private void CheckPlayerDistance()
    {
        if (PlayerController.Instance == null) return;
        
        float distance = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
        isInRange = distance <= interactionRange;
        
        // Show/hide interaction prompt based on distance
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(isInRange && canInteract);
        }
    }
    
    private void HandleInteraction()
    {
        if (isInRange && canInteract && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Play sound directly on E key press
            if (talkSound != null)
            {
                talkSound.PlayTalkSound();
                Debug.Log("GeccoInteraction: Called PlayTalkSound from HandleInteraction");
            }
            
            SpawnRandomBoss();
            StartCooldown();
        }
    }
    
    private void HandleCooldown()
    {
        if (!canInteract)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                canInteract = true;
                if (isInRange && interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }
            }
        }
    }
    
    private void SpawnRandomBoss()
    {
        if (bossDialogPairs == null || bossDialogPairs.Count == 0)
        {
            Debug.LogWarning("No boss prefabs assigned to Gecco!");
            return;
        }
        
        // Pick random boss
        int randomIndex = Random.Range(0, bossDialogPairs.Count);
        BossDialogPair selectedBossPair = bossDialogPairs[randomIndex];
        
        if (selectedBossPair.bossPrefab == null)
        {
            Debug.LogWarning("Boss prefab is null!");
            return;
        }
        
        // Calculate spawn position (away from player)
        Vector2 playerPos = PlayerController.Instance.transform.position;
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPos = playerPos + (randomDirection * spawnDistanceFromPlayer);
        
        // Spawn the boss
        GameObject spawnedBoss = Instantiate(selectedBossPair.bossPrefab, spawnPos, Quaternion.identity);
        
        // Add BountyEnemy component to make it count for victory condition
        if (!spawnedBoss.GetComponent<BountyEnemy>())
        {
            BountyEnemy bountyEnemy = spawnedBoss.AddComponent<BountyEnemy>();
            SpriteRenderer spriteRenderer = spawnedBoss.GetComponent<SpriteRenderer>();
            
            // Set custom properties for each boss type
            string bossName = spawnedBoss.name.Replace("(Clone)", "").Trim();
            switch (bossName)
            {
                case "Mouse of Death":
                    if (spriteRenderer != null) 
                        spriteRenderer.color = new Color(1f, 0.5f, 0.5f); // Light red
                    break;
                case "Scorpion of Death":
                    if (spriteRenderer != null) 
                        spriteRenderer.color = new Color(1f, 0.3f, 0.3f); // Stronger red
                    break; 
                case "Goldfish of Death":
                    if (spriteRenderer != null) 
                        spriteRenderer.color = new Color(1f, 0.4f, 0.4f); // Medium red
                    break;
                case "Hedgehog of Death":
                    if (spriteRenderer != null) 
                        spriteRenderer.color = new Color(1f, 0.2f, 0.2f); // Very red
                    break;
            }
            
            Debug.Log($"Added BountyEnemy component to {bossName} - kill 4 to win!");
        }
        
        // Register the boss with the DirectionalIndicator if it exists
        if (DirectionalIndicator.Instance != null)
        {
            DirectionalIndicator.Instance.RegisterEnemy(spawnedBoss);
        }
        
        // Show dialog for this boss
        ShowDialog(selectedBossPair.dialogText);
    }
    
    private void ShowDialog(string dialogText)
    {
        if (dialogManager == null || string.IsNullOrEmpty(dialogText)) return;
        
        // Show dialog with typewriter effect
        dialogManager.ShowDialog(dialogText);
        
        // Register for dialog close notification
        StartCoroutine(WaitForDialogToClose());
    }
    
    // Wait for dialog to close and stop sounds
    private IEnumerator WaitForDialogToClose()
    {
        // Wait for dialog container to become inactive
        if (dialogContainer != null)
        {
            // Wait until it's no longer active
            while (dialogContainer.activeSelf)
            {
                yield return null;
            }
            
            // Dialog is now closed, stop sound
            if (talkSound != null)
            {
                talkSound.StopTalkSound();
            }
        }
    }
    
    private void StartCooldown()
    {
        canInteract = false;
        cooldownTimer = spawnCooldown;
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    // Optional: Visualize the interaction range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
} 