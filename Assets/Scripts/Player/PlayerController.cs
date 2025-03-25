using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public bool FacingLeft { get; set; }

    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float dashSpeed = 4f;
    [SerializeField] private float dashDuration = 0.2f;

    [SerializeField] private float dashCooldown = 1f;
    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;

    private SpriteRenderer spriteRenderer;

    [SerializeField] private TrailRenderer trailRenderer;
    
    [Header("Card Throwing")]
    [SerializeField] private CardThrower cardThrower;

    private bool isDashing = false;

    private Animator animator;
    // Start is called before the first frame update

    private void Awake()
    {
        Instance = this;
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    void Start()
    {
        playerControls.Combat.Dash.performed += _ => Dash();
        playerControls.Combat.ThrowCard.performed += _ => ThrowCard();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * movementSpeed * Time.fixedDeltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerInput();
        AdjustPlayerFacing();
    }

    private void PlayerInput()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();
        animator.SetFloat("MoveX", movement.x);
        animator.SetFloat("MoveY", movement.y);
    }

    private void AdjustPlayerFacing()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);

        if (mousePosition.x < playerScreenPoint.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    private void Dash()
    {
        if (isDashing) return;
        isDashing = true;
        movementSpeed *= dashSpeed;
        trailRenderer.emitting = true;
        StartCoroutine(EndDashRoutine());
    }

    private IEnumerator EndDashRoutine()
    {
        yield return new WaitForSeconds(dashDuration);
        trailRenderer.emitting = false;
        movementSpeed /= dashSpeed;
        yield return new WaitForSeconds(dashCooldown);
        isDashing = false;
    }
    
    private void ThrowCard()
    {
        if (cardThrower != null)
        {
            cardThrower.TriggerThrowCard();
        }
        else
        {
            Debug.LogWarning("CardThrower component not assigned to PlayerController!");
        }
    }
}
