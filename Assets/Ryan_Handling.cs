using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Jump Settings")]
    public float coyoteTime = 0.15f;      // Time allowed after leaving ground
    public int maxJumps = 4;              // 1 = normal jump, 2 = double jump

    [Header("Ground Check")]
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private BoxCollider2D boxCollider;

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
        boxCollider = GetComponent<BoxCollider2D>();
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
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
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

            jumpCount++;
            coyoteTimer = 0; // Prevent infinite coyote jumps
        }
    }

    private bool IsGrounded()
    {
        float extraHeight = 0.1f;

        RaycastHit2D hit = Physics2D.Raycast(
            boxCollider.bounds.center,
            Vector2.down,
            boxCollider.bounds.extents.y + extraHeight,
            groundLayer
        );

        Debug.DrawRay(
            boxCollider.bounds.center,
            Vector2.down * (boxCollider.bounds.extents.y + extraHeight),
            hit.collider != null ? Color.green : Color.red
        );

        return hit.collider != null;
    }
}
