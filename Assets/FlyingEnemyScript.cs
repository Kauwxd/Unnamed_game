using UnityEngine;

public class FlyingEnemyScript : MonoBehaviour
{
    [SerializeField]
    private float PlayerCollisionDamage = 20f;
    [SerializeField]
    private float FlyingSpeed = 2f;
    [SerializeField]
    private float VerticalMotion = 0.1f;
    [SerializeField]
    private float PatrolRange = 5f;
    [SerializeField]
    private GameObject PlayerObject;

    private float DistanceFromPlayer;
    private Vector3 SpawnPoint;
    private int PatrolDirection = 1;

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
        SpawnPoint = transform.position;

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

    void Update()
    {
        Patrol();
        TrackPlayer();
    }
    void Flip()
    {
        direction *= -1;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void Patrol()
    {
        // Horizontal patrol movement
        transform.position += new Vector3(FlyingSpeed * PatrolDirection * Time.deltaTime, 0, 0);

        // Vertical bobbing using a sine wave
        float newY = SpawnPoint.y + Mathf.Sin(Time.time) * VerticalMotion;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Reverse direction at patrol range boundaries
        float distanceFromSpawn = transform.position.x - SpawnPoint.x;
        if (distanceFromSpawn >= PatrolRange || distanceFromSpawn <= -PatrolRange)
        {
            PatrolDirection *= -1;
        }
    }

    void TrackPlayer()
    {
        if (PlayerObject == null) return;

        DistanceFromPlayer = Vector3.Distance(transform.position, PlayerObject.transform.position);
        Debug.Log($"Distance from player: {DistanceFromPlayer}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HealthHandler health = other.GetComponent<HealthHandler>();

            if (health != null)
            {
                health.ChangeHealth(PlayerCollisionDamage); // giver damage
            }

            Debug.Log("Player hit by flying enemy!");
        }
    }
}