using UnityEngine;

public class PigeonEnemyMovement : MonoBehaviour
{
    public Transform playerTransform;
    [Tooltip("How fast the pigeon flies while chasing.")]
    public float flySpeed = 0.55f;
    [Tooltip("Brief speed boost when diving in to attack.")]
    public float attackFlySpeed = 0.85f;
    [Tooltip("Player must be within this distance to start or keep chasing.")]
    public float aggroRange = 1f;
    [Tooltip("Stops chasing if the player is farther than this from the pigeon's spawn point.")]
    public float maxLeashFromHome = 2f;
    public float attackDistance = 0.6f;
    public float hoverAmplitude = 0.15f;
    public float hoverSpeed = 2f;
    public float attackCooldown = 1f;

    private Animator animator;
    private EnemyHealth health;
    private Vector3 homePosition;
    private bool isChasing;
    private float nextAttackTime;
    private float hoverPhase;

    void Start()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        homePosition = transform.position;
    
        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                playerTransform = playerObject.transform;
        }
    }

    void Update()
    {
        if (health != null && health.isDead)
            return;

        if (playerTransform == null)
        {
            HoverAtHome();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        float playerDistanceFromHome = Vector2.Distance(playerTransform.position, homePosition);
        bool playerCloseEnough = distanceToPlayer <= aggroRange;
        bool playerNearNest = playerDistanceFromHome <= maxLeashFromHome;

        if (isChasing)
        {
            if (!playerCloseEnough || !playerNearNest)
            {
                isChasing = false;
                ReturnHome();
                return;
            }

            HandleChase(distanceToPlayer);
        }
        else if (playerCloseEnough && playerNearNest)
        {
            isChasing = true;
            HandleChase(distanceToPlayer);
        }
        else
        {
            ReturnHome();
        }
    }

    void HandleChase(float distanceToPlayer)
    {
        if (distanceToPlayer <= attackDistance)
        {
            TryAttack();
            MoveTowardsPlayer(attackFlySpeed);
        }
        else
        {
            MoveTowardsPlayer(flySpeed);
        }
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.isHurt)
            return;

        nextAttackTime = Time.time + attackCooldown;
    }

    void MoveTowardsPlayer(float speed)
    {
        Vector3 target = playerTransform.position;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        FaceTowards(target.x);
    }

    void ReturnHome()
    {
        hoverPhase += Time.deltaTime * hoverSpeed;
        Vector3 hoverOffset = new Vector3(0f, Mathf.Sin(hoverPhase) * hoverAmplitude, 0f);
        Vector3 target = homePosition + hoverOffset;
        transform.position = Vector3.MoveTowards(transform.position, target, flySpeed * 0.4f * Time.deltaTime);
        FaceTowards(target.x);
    }

    void HoverAtHome()
    {
        hoverPhase += Time.deltaTime * hoverSpeed;
        Vector3 hoverOffset = new Vector3(0f, Mathf.Sin(hoverPhase) * hoverAmplitude, 0f);
        transform.position = homePosition + hoverOffset;
    }

    void FaceTowards(float targetX)
    {
        Vector3 scale = transform.localScale;
        float directionX = Mathf.Sign(targetX - transform.position.x);
        if (directionX == 0)
            return;
        scale.x = Mathf.Abs(scale.x) * (directionX > 0 ? 1f : -1f);
        transform.localScale = scale;
    }
}
