using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
