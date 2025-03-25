using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card : MonoBehaviour
{
    [Header("Basic Card Settings")]
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected float lifetime = 5f;
    [SerializeField] protected int damageAmount = 1;
    [SerializeField] protected TrailRenderer trailRenderer;
    [SerializeField] protected Color trailColor = Color.white;
    
    protected SpriteRenderer spriteRenderer;
    protected DamageSource damageSource;
    protected Vector2 direction;
    protected Rigidbody2D rb;
    
    // Chance for effect to backfire (to be implemented later)
    [Range(0f, 1f)]
    [SerializeField] protected float backfireChance = 0.1f;
    
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageSource = GetComponent<DamageSource>();
        rb = GetComponent<Rigidbody2D>();
        
        if (!trailRenderer)
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }
    }
    
    protected virtual void Start()
    {
        Destroy(gameObject, lifetime);
        SetupTrail();
    }
    
    protected virtual void Update()
    {
        // Move in the set direction at constant speed
        transform.Translate(direction * speed * Time.deltaTime);
    }
    
    protected virtual void SetupTrail()
    {
        if (trailRenderer)
        {
            // Set the trail color
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(trailColor, 0.0f), new GradientColorKey(trailColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trailRenderer.colorGradient = gradient;
        }
    }
    
    public virtual void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public virtual float GetSpeed()
    {
        return speed;
    }
    
    public virtual void Launch(Vector2 dir)
    {
        direction = dir.normalized;
        
        // Ensure card is correctly oriented in the direction of travel
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    // This will be overridden by each card type to implement special effects
    public abstract void ApplySpecialEffect(GameObject target);
    
    // Called to determine if the effect backfires
    protected virtual bool ShouldBackfire()
    {
        return Random.value < backfireChance;
    }
    
    // What happens when the card hits something
    protected virtual void OnHit(GameObject target)
    {
        ApplySpecialEffect(target);
        Destroy(gameObject);
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore collisions with objects on the same layer and with the player/throwpoint
        if (other.gameObject.layer != gameObject.layer && 
            other.gameObject.GetComponent<PlayerController>() == null &&
            other.gameObject.transform != transform.parent)
        {
            OnHit(other.gameObject);
        }
    }
}
