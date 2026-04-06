using UnityEngine;

public class FlyingEnemyScript : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float flyingSpeed = 2f;
    [SerializeField] private float patrolRange = 5f;

    [Header("Vertical Movement")]
    [SerializeField] private float verticalChangeInterval = 2f;
    [SerializeField] private float maxVerticalSpeed = 1.5f;
    [SerializeField] private float maxHeight = 5f;

    [Header("Wall Detection")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.7f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform rotatePoint;
    [SerializeField] private float shootCooldown = 2f;
    [SerializeField] private float shootRange = 8f;

    [Header("Player")]
    [SerializeField] private GameObject playerObject;



    private Rigidbody2D rb;
    private Vector3 spawnPoint;

    private int direction = 1;

    private float currentVerticalSpeed;
    private float verticalTimer;

    private float shootTimer;

    private float flipCooldown = 0.5f;
    private float lastFlipTime;

    float targetVerticalSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnPoint = transform.position;

        PickNewVerticalMovement();
    }

    void Update()
    {
        RotateTowardsPlayer();
        CheckWall();
        CheckGround(); 
        CheckBounds();
        HandleVerticalMovement();
        HandleShooting();
    }

    void FixedUpdate()
    {
        Move();
    }

    // ---------------- MOVEMENT ----------------

    void Move()
    {
        // Smooth vertical movement
        currentVerticalSpeed = Mathf.Lerp(currentVerticalSpeed, targetVerticalSpeed, Time.deltaTime * 2f);

        float clampedY = Mathf.Clamp(transform.position.y, -Mathf.Infinity, maxHeight);

        transform.position = new Vector3(
            transform.position.x,
            clampedY,
            transform.position.z
        );

        rb.linearVelocity = new Vector2(
            direction * flyingSpeed,
            currentVerticalSpeed
        );
    }

    void HandleVerticalMovement()
    {
        verticalTimer += Time.deltaTime;

        if (verticalTimer >= verticalChangeInterval)
        {
            PickNewVerticalMovement();
        }
    }

    void PickNewVerticalMovement()
    {
        targetVerticalSpeed = Random.Range(-maxVerticalSpeed, maxVerticalSpeed);
        verticalTimer = 0f;
    }

    // ---------------- WALL / BOUNDS ----------------

    void CheckWall()
    {
        if (Time.time < lastFlipTime + flipCooldown) return;

        Vector2 dir = new Vector2(direction, 0);

        bool wallAhead = Physics2D.Raycast(
            wallCheck.position,
            dir,
            wallCheckDistance,
            wallLayer
        );

        Debug.DrawRay(wallCheck.position, dir * wallCheckDistance, Color.red);

        if (wallAhead)
        {
            Flip();
        }
    }

    void CheckBounds()
    {
        float distance = transform.position.x - spawnPoint.x;

        if (Mathf.Abs(distance) >= patrolRange)
        {
            direction = distance > 0 ? -1 : 1;
            Flip();
        }
    }

    void Flip()
    {
        direction *= -1;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        lastFlipTime = Time.time;
    }

    // ---------------- GROUND ----------------


    void CheckGround()
    {
        if (Time.time < lastFlipTime + flipCooldown) return;

        Vector2 dir = Vector2.down;

        bool groundBelow = Physics2D.Raycast(
            groundCheck.position,
            dir,
            groundCheckDistance,
            groundLayer
        );

        Debug.DrawRay(groundCheck.position, dir * groundCheckDistance, Color.blue);

        if (!groundBelow)
        {
            Flip();
        }
    }



    // ---------------- SHOOTING ----------------

    void RotateTowardsPlayer()
    {
        if (playerObject == null || rotatePoint == null) return;

        Vector3 dir = playerObject.transform.position - rotatePoint.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        rotatePoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void HandleShooting()
    {
        if (playerObject == null) return;

        //Distance check to only shoot when player is within range
        Vector3 target = playerObject.transform.position + Vector3.up * 0.5f;
        float distance = Vector2.Distance(rotatePoint.position, target);
        if (distance > shootRange) return;

        shootTimer += Time.deltaTime;

        if (shootTimer >= shootCooldown)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    void Shoot()
    {
        if (fireballPrefab == null || rotatePoint == null) return;

        GameObject bullet = Instantiate(fireballPrefab, rotatePoint.position, rotatePoint.rotation);
        soundManager.Instance.PlaySound("fireballSwoosh");

        FireballScript fb = bullet.GetComponent<FireballScript>();
        fb.SetOwner(gameObject);
    }

    // ---------------- DAMAGE ----------------

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HealthHandler health = other.GetComponent<HealthHandler>();

            if (health != null)
            {
                health.ChangeHealth(-20f);
            }

            Debug.Log("Player hit by flying enemy!");
        }
    }
}