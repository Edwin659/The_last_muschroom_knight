using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossAcornProjectile : MonoBehaviour
{
    public int damage = 15;
    public float lifetime = 4f;

    [Header("Homing")]
    [Tooltip("Seconds before the acorn starts steering toward the player.")]
    public float homingDelay = 0.34f;
    [Tooltip("Turn rate right after homing begins.")]
    public float minTurnDegrees = 35f;
    [Tooltip("Turn rate after homing has ramped up.")]
    public float maxTurnDegrees = 78f;
    [Tooltip("Seconds to reach max turn rate.")]
    public float homingRampTime = 1.1f;
    [Tooltip("How far ahead to lead a moving player (0 = no lead, 1 = full lead).")]
    [Range(0f, 1f)] public float predictionFactor = 0.32f;
    [Tooltip("Seconds of player movement used for the lead calculation.")]
    public float predictionTime = 0.45f;

    [Header("Close Range")]
    public float closeRangeDistance = 4f;
    public float closeRangeTurnMultiplier = 1.75f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Transform playerTarget;
    private Vector2 direction;
    private float speed;
    private float launchTime;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 initialVelocity, Sprite sprite, Transform player)
    {
        if (sprite != null && spriteRenderer != null)
            spriteRenderer.sprite = sprite;

        playerTarget = player;
        ResolvePlayerTarget();

        speed = Mathf.Max(initialVelocity.magnitude, 0.01f);

        if (initialVelocity.sqrMagnitude > 0.01f)
            direction = initialVelocity.normalized;
        else
            direction = GetDirectionToPlayer();

        launchTime = Time.time;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.WakeUp();
        rb.linearVelocity = direction * speed;
    }

    void FixedUpdate()
    {
        float timeSinceLaunch = Time.time - launchTime;
        if (timeSinceLaunch < homingDelay)
            return;

        if (playerTarget == null)
            ResolvePlayerTarget();

        if (playerTarget == null || speed < 0.01f)
            return;

        Vector2 aimPoint = GetPredictedAimPoint();
        Vector2 toPlayer = aimPoint - (Vector2)transform.position;
        if (toPlayer.sqrMagnitude < 0.01f)
            return;

        if (rb.linearVelocity.sqrMagnitude > 0.01f)
            direction = rb.linearVelocity.normalized;

        float distance = toPlayer.magnitude;
        Vector2 desired = toPlayer / distance;

        float homingTime = timeSinceLaunch - homingDelay;
        float ramp = homingRampTime > 0f ? Mathf.Clamp01(homingTime / homingRampTime) : 1f;
        float turnRate = Mathf.Lerp(minTurnDegrees, maxTurnDegrees, ramp);

        if (distance < closeRangeDistance)
        {
            float closeT = 1f - Mathf.Clamp01(distance / closeRangeDistance);
            turnRate *= Mathf.Lerp(1f, closeRangeTurnMultiplier, closeT);

            // Stay level when very close so shots track the body, not sail overhead.
            float verticalLimit = Mathf.Lerp(0.18f, 0.42f, distance / closeRangeDistance);
            Vector2 flattened = new Vector2(
                Mathf.Sign(toPlayer.x != 0f ? toPlayer.x : direction.x),
                Mathf.Clamp(desired.y, -verticalLimit, verticalLimit)
            );
            if (flattened.sqrMagnitude > 0.01f)
                flattened.Normalize();

            desired = Vector2.Lerp(desired, flattened, closeT * 0.55f).normalized;
        }

        float currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnRate * Time.fixedDeltaTime);

        direction = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));
        rb.linearVelocity = direction * speed;
    }

    void ResolvePlayerTarget()
    {
        if (playerTarget != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTarget = player.transform;
    }

    Vector2 GetPlayerAimPoint()
    {
        if (playerTarget == null)
            return Vector2.zero;

        Collider2D col = playerTarget.GetComponent<Collider2D>();
        if (col == null)
            col = playerTarget.GetComponentInChildren<Collider2D>();

        if (col != null)
            return col.bounds.center;

        return playerTarget.position;
    }

    Vector2 GetPredictedAimPoint()
    {
        Vector2 aimPoint = GetPlayerAimPoint();

        if (playerTarget == null || predictionFactor <= 0f)
            return aimPoint;

        Rigidbody2D playerRb = playerTarget.GetComponent<Rigidbody2D>();
        if (playerRb == null)
            return aimPoint;

        aimPoint += playerRb.linearVelocity * (predictionTime * predictionFactor);
        return aimPoint;
    }

    Vector2 GetDirectionToPlayer()
    {
        Vector2 toPlayer = GetPredictedAimPoint() - (Vector2)transform.position;
        return toPlayer.sqrMagnitude > 0.01f ? toPlayer.normalized : Vector2.right;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null || !playerHealth.CanBeDamaged)
            return;

        playerHealth.TakeDamage(damage, transform);
        Destroy(gameObject);
    }
}
