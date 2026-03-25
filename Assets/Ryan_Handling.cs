using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private BoxCollider2D boxCollider;

    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // 🔹 Movement input (kun venstre/højre)
        movement = new Vector2(
            (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
            0
        );

        // 🔹 Animation - running
        bool isRunning = movement.x != 0;
        animator.SetBool("isRunning", isRunning);

        // 🔹 Flip sprite
        if (movement.x > 0)
            sr.flipX = true;
        else if (movement.x < 0)
            sr.flipX = false;

        // 🔹 Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("jumpTrigger");
        }
    }

    void FixedUpdate()
    {
        // 🔹 Platformer movement
        rb.linearVelocity = new Vector2(movement.x * moveSpeed, rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        float extraHeight = 0.1f;

        RaycastHit2D raycastHit = Physics2D.Raycast(
            boxCollider.bounds.center,
            Vector2.down,
            boxCollider.bounds.extents.y + extraHeight,
            groundLayer
        );

        // Debug ray
        Debug.DrawRay(
            boxCollider.bounds.center,
            Vector2.down * (boxCollider.bounds.extents.y + extraHeight),
            raycastHit.collider != null ? Color.green : Color.red
        );

        return raycastHit.collider != null;
    }
}