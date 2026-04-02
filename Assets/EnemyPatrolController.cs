using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public float speed = 2f;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    public Transform groundCheck;
    public Transform wallCheck;

    public float groundCheckDistance = 0.2f;
    public float wallCheckDistance = 0.2f;

    private int direction = -1;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

    }

    void Update()
    {
        // Move
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        // Check wall ahead
        bool wallAhead = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * direction,
            wallCheckDistance,
            wallLayer
        );

        // Flip logic
        if (wallAhead)
        {
            Flip();
        }
    }

    void Flip()
    {
        direction *= -1;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}