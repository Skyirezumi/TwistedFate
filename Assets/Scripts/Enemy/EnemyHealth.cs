using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 100;
    private int currentHealth;
    private KnockBack knockBack;

    private DamageFlash damageFlash;

    [SerializeField] private GameObject deathVFXPrefab;

    private void Awake() {
        damageFlash = GetComponent<DamageFlash>();
        knockBack = GetComponent<KnockBack>();
    }

    private void Start()
    {
        currentHealth = startingHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;    
        knockBack.GetKnockedBack(PlayerController.Instance.transform, 15f);
        StartCoroutine(damageFlash.FlashRoutine());
        StartCoroutine(CheckDetectDeathRoutine());
    }

    private IEnumerator CheckDetectDeathRoutine() {
        yield return new WaitForSeconds(damageFlash.GetRestoreDefaultMaterialTime());
        DetectDeath();
    }

    private void DetectDeath()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //other stuff for juiciness
        Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}