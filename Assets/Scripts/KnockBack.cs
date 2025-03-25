using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockBack : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private float knockBackTime = 0.2f;

    public bool gettingKnockedBack {get; private set;}

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void GetKnockedBack(Transform damageSource, float knockBackThrust)
    {
        gettingKnockedBack = true;
        Vector2 knockBackDirection = (rb.position - (Vector2)damageSource.position).normalized * knockBackThrust * rb.mass;
        rb.AddForce(knockBackDirection, ForceMode2D.Impulse);
        StartCoroutine(StopKnockBack());
    }

    private IEnumerator StopKnockBack()
    {
        yield return new WaitForSeconds(knockBackTime);
        rb.velocity = Vector2.zero;
        gettingKnockedBack = false;
    }
    
}
