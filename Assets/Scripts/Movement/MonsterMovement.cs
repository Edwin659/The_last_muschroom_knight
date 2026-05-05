using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float moveSpeed = 2f;
    private int patrolDestination;
    public Transform playerTransform;
    public float chaseDistance = 5f;
    public float patrolPointReachDistance = 0.2f;
    public bool invertFacing;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isChasing;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (playerTransform == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }
    }

    void Update()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            if (animator != null)
            {
                animator.SetBool("isRunning", false);
            }
            return;
        }

        if (isChasing)
        {
            HandleChase();
        }
        else
        {
            if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) < chaseDistance)
            {
                isChasing = true;
            }
            else
            {
                HandlePatrol();
            }
        }
    }

    void HandlePatrol()
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", true);
        }

        Transform targetPoint = patrolPoints[patrolDestination];
        FaceTowardsX(targetPoint.position.x - transform.position.x);
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint.position) <= patrolPointReachDistance)
        {
            patrolDestination = (patrolDestination + 1) % patrolPoints.Length;
            Transform nextTargetPoint = patrolPoints[patrolDestination];
            FaceTowardsX(nextTargetPoint.position.x - transform.position.x);
        }
    }

    void HandleChase()
    {
        if (playerTransform == null)
        {
            isChasing = false;
            return;
        }

        if (animator != null)
        {
            animator.SetBool("isRunning", true);
        }

        if (transform.position.x > playerTransform.position.x)
        {
            FaceTowardsX(-1f);
            transform.position += Vector3.left * (moveSpeed * Time.deltaTime);
        }
        else if (transform.position.x < playerTransform.position.x)
        {
            FaceTowardsX(1f);
            transform.position += Vector3.right * (moveSpeed * Time.deltaTime);
        }

        if (Vector2.Distance(transform.position, playerTransform.position) > chaseDistance * 1.5f)
        {
            isChasing = false;
        }
    }

    void FaceTowardsX(float directionX)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (directionX > 0.01f)
        {
            spriteRenderer.flipX = invertFacing;
        }
        else if (directionX < -0.01f)
        {
            spriteRenderer.flipX = !invertFacing;
        }
    }
}
