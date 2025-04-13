using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // Add System namespace for Action

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 3;
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private float knockBackThrust = 15f;
    [SerializeField] private bool isBountyEnemy = false;

    private int currentHealth;
    private Knockback knockback;
    private Flash flash;
    private float originalKnockbackThrust; // Store original knockback value
    
    // Add event for health changes
    public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)

    private void Awake() {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
        originalKnockbackThrust = knockBackThrust; // Store original value
        
        // Automatically flag bosses with specific naming convention as bounty enemies
        if (gameObject.name.Contains("Death") || gameObject.name.Contains("Mouse of") || 
            gameObject.name.Contains("Goldfish of") || gameObject.name.Contains("Hedgehog of") || 
            gameObject.name.Contains("Scorpion of")) {
            isBountyEnemy = true;
        }
    }

    private void Start() {
        currentHealth = startingHealth;
    }

    public void TakeDamage(int damage) {
        currentHealth -= damage;
        knockback.GetKnockedBack(PlayerController.Instance.transform, knockBackThrust);
        StartCoroutine(flash.FlashRoutine());
        StartCoroutine(CheckDetectDeathRoutine());
        
        // Trigger health changed event
        OnHealthChanged?.Invoke(currentHealth, startingHealth);
    }
    
    // Add method to gain health
    public void GainHealth(int amount) {
        // Add health but don't exceed starting health
        currentHealth = Mathf.Min(currentHealth + amount, startingHealth);
        
        // Play heal effect
        if (flash != null) {
            // Flash green to indicate healing
            flash.FlashColor(Color.green, 0.2f);
        }
        
        // Trigger health changed event
        OnHealthChanged?.Invoke(currentHealth, startingHealth);
        
        // Log healing
        Debug.Log(gameObject.name + " gained " + amount + " health. Current health: " + currentHealth);
    }

    private IEnumerator CheckDetectDeathRoutine() {
        yield return new WaitForSeconds(flash.GetRestoreMatTime());
        DetectDeath();
    }

    public void DetectDeath() {
        if (currentHealth <= 0) {
            // Create death effect
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            
            // Drop items
            GetComponent<PickUpSpawner>()?.DropItems();
            
            // Check if this is a bounty enemy
            if (isBountyEnemy && GameManager.Instance != null) {
                // Notify the game manager
                GameManager.Instance.BountyEnemyKilled();
            }
            
            // Destroy the enemy
            Destroy(gameObject);
        }
    }
    
    // Add method to temporarily set knockback thrust
    public void SetKnockbackThrust(float newThrust)
    {
        knockBackThrust = newThrust;
    }
    
    // Add method to reset knockback thrust to original value
    public void ResetKnockbackThrust()
    {
        knockBackThrust = originalKnockbackThrust;
    }
    
    // Add getter for current health percentage
    public float GetHealthPercentage()
    {
        return (float)currentHealth / startingHealth;
    }
}
