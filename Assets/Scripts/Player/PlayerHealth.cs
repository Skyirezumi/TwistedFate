using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    public bool IsDead { get; private set; }

    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float knockBackThrustAmount = 10f;
    [SerializeField] private float damageRecoveryTime = 1f;
    [SerializeField] private DeathScreen deathScreen;

    private Slider healthSlider;
    private int currentHealth;
    private bool canTakeDamage = true;
    private Knockback knockback;
    private Flash flash;

    const string HEALTH_SLIDER_TEXT = "Health Slider";
    readonly int DEATH_HASH = Animator.StringToHash("Death");

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }

        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
    }

    private void Start() {
        IsDead = false;
        currentHealth = maxHealth;
        UpdateHealthSlider();
    }

    private void OnCollisionStay2D(Collision2D other) {
        EnemyAI enemy = other.gameObject.GetComponent<EnemyAI>();

        if (enemy) {
            TakeDamage(1, other.transform);
        }
    }

    public void HealPlayer() {
        if (currentHealth < maxHealth) {
            currentHealth += 1;
            UpdateHealthSlider();
        }
    }

    public void TakeDamage(int damageAmount, Transform hitTransform) {
        if (!canTakeDamage) { return; }

        //Screensha.Instance.ShakeScreen();
        knockback.GetKnockedBack(hitTransform, knockBackThrustAmount);
        StartCoroutine(flash.FlashRoutine());
        canTakeDamage = false;
        currentHealth -= damageAmount;
        StartCoroutine(DamageRecoveryRoutine());
        UpdateHealthSlider();
        CheckIfPlayerDeath();
    }

    private void CheckIfPlayerDeath() {
        if (currentHealth <= 0) {
            IsDead = true;
            
            // Try to access the DeathScreen singleton first
            if (DeathScreen.Instance != null)
            {
                Debug.Log("Using DeathScreen singleton to show death screen");
                DeathScreen.Instance.ShowDeathScreen();
            }
            // As fallback, try the serialized reference
            else if (deathScreen != null) {
                Debug.Log("Using serialized reference to show death screen");
                deathScreen.ShowDeathScreen();
            } 
            else {
                // Last resort: try FindObjectOfType
                Debug.Log("Both Instance and reference are null, trying FindObjectOfType");
                DeathScreen foundDeathScreen = FindObjectOfType<DeathScreen>();
                if (foundDeathScreen != null) {
                    Debug.Log("Found death screen via FindObjectOfType");
                    foundDeathScreen.ShowDeathScreen();
                } else {
                    Debug.LogError("DeathScreen not found in the scene!");
                }
            }
            
            // Disable player components instead of destroying
            DisablePlayerOnDeath();
            
            // Optional: Play death animation
            Animator animator = GetComponent<Animator>();
            if (animator != null) {
                animator.SetTrigger(DEATH_HASH);
            }
        }
    }

    // New method to properly disable the player without destroying
    private void DisablePlayerOnDeath()
    {
        // Disable player controller but keep the GameObject
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null) {
            // Make sure the PlayerController also knows it's dead
            playerController.SetDeadState(true);
            
            // Disable controller component
            playerController.enabled = false;
        }
        
        // Disable rigidbody to stop all movement
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        // Disable colliders to prevent further interactions
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders) {
            collider.enabled = false;
        }
        
        // Optionally make the player slightly transparent
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers) {
            Color color = renderer.color;
            color.a = 0.5f;
            renderer.color = color;
        }
    }

    private IEnumerator DamageRecoveryRoutine() {
        yield return new WaitForSeconds(damageRecoveryTime);
        canTakeDamage = true;
    }

    private void UpdateHealthSlider() {
        if (healthSlider == null) {
            healthSlider = GameObject.Find(HEALTH_SLIDER_TEXT).GetComponent<Slider>();
        }

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    // Method to test player death
    public void TestPlayerDeath()
    {
        TakeDamage(maxHealth, transform);
    }
}
