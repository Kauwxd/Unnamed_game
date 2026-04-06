using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Jump Settings")]
    public float coyoteTime = 0.15f;      // Time allowed after leaving ground
    public int maxJumps = 2;              // 1 = normal jump, 2 = double jump
    // public int jumpCooldown = 1; skal implementeres

    [Header("Ground Check")]
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private CapsuleCollider2D CapsuleCollider;

    private PlayerControls controls;
    private float moveX;

    private float coyoteTimer;
    private int jumpCount;

    private void Awake()
    {
        controls = new PlayerControls();

        // Jump callback
        controls.Player.Jump.performed += ctx => TryJump();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        CapsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        // Read horizontal movement
        moveX = controls.Player.MoveHorizontally.ReadValue<float>();

        // Animation
        animator.SetBool("isRunning", moveX != 0);

        // Flip sprite
        Vector3 scale = transform.localScale;

        if (moveX < 0 && transform.eulerAngles.y != 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (moveX > 0 && transform.eulerAngles.y != 180f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        transform.localScale = scale;

        if (IsGrounded() && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
        }

        // Update coyote timer
        if (IsGrounded())
        {
            coyoteTimer = coyoteTime;
            jumpCount = 0; // Reset jumps when grounded
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        float targetSpeed = moveX * moveSpeed;

        if (IsGrounded() && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
        }

        // Smooth i stedet for instant (MEGET vigtigt)
        float speedDif = targetSpeed - rb.linearVelocity.x;
        float acceleration = IsGrounded() ? 50f : 20f;

        rb.AddForce(new Vector2(speedDif * acceleration, 0f));
    }

    private void TryJump()
    {
        // Can jump if:
        // - On ground (coyoteTimer > 0)
        // - OR still have jumps left (double jump)
        if (coyoteTimer > 0 || jumpCount < maxJumps - 1)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("jumpTrigger");
            soundManager.Instance.PlaySound("jump1");

            jumpCount++;
            coyoteTimer = 0; // Prevent infinite coyote jumps
        }
    }


    private bool IsGrounded()
    {
        float extraHeight = 0.05f;

        RaycastHit2D hit = Physics2D.BoxCast(
            CapsuleCollider.bounds.center,
            CapsuleCollider.bounds.size,
            0f,
            Vector2.down,
            extraHeight,
            groundLayer
        );

        if (hit.collider != null)
        {
            // Kun ground hvis overfladen peger opad
            return hit.normal.y > 0.5f;
        }

        return false;
    }


    // private bool IsGrounded()
    //  {
    //  float extraHeight = 0.1f;

    //   RaycastHit2D hit = Physics2D.Raycast(
    ////       Vector2.down,
    //         CapsuleCollider.bounds.extents.y + extraHeight,
    //       groundLayer
    //   );

    //     Debug.DrawRay(
    //      CapsuleCollider.bounds.center,
    //       Vector2.down * (CapsuleCollider.bounds.extents.y + extraHeight),
    //      hit.collider != null ? Color.green : Color.red
    //   );
    //
    //      return hit.collider != null;
    // }
}
