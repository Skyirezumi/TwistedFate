using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // Add for Action/event support

public class EnemyHealth : MonoBehaviour
{
    // Event that will be triggered when the enemy dies
    public event Action OnDeath;
    
    [SerializeField] private int startingHealth = 3;
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private float knockBackThrust = 15f;

    private int currentHealth;
    private Knockback knockback;
    private Flash flash;
    private float originalKnockbackThrust; // Store original knockback value

    private void Awake() {
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
        originalKnockbackThrust = knockBackThrust; // Store original value
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
            // Trigger the death event before destroying the object
            OnDeath?.Invoke();
            
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            GetComponent<PickUpSpawner>().DropItems();
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
