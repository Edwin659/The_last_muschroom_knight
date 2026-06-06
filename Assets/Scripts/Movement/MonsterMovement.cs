using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterMovement : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float moveSpeed = 2f;
    private int patrolDestination;
    public Transform playerTransform;
    public float patrolPointReachDistance = 0.2f;
    public bool invertFacing;
    public float chaseDistance = 1f;
    public float stopDistance = 1.5f;
    public float attackDistance = 1f;
    public LayerMask groundLayer;


    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isChasing;
    private float lastDistanceToPlayer;
    public float chaseCheckInterval = 0.5f;
    private bool canAttack = true;
    private Rigidbody2D rb;
    EnemyHealth health;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();

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
        if (!isChasing)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (playerTransform != null && distanceToPlayer <= stopDistance && distanceToPlayer > attackDistance)
            {
                isChasing = true;
                lastDistanceToPlayer = distanceToPlayer;
            }
            else
            {
                HandlePatrol();
            }
        }
        else
        {
            HandleChase();
        }
    }

    void HandlePatrol()
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", true);
        }
        if (health != null && health.isHurt)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        };

        Transform targetPoint = patrolPoints[patrolDestination];
        float directionX = Mathf.Sign(targetPoint.position.x - transform.position.x);
        MoveAlongGround(directionX);

        if (Vector2.Distance(transform.position, targetPoint.position) <= patrolPointReachDistance)
        {
            patrolDestination = (patrolDestination + 1) % patrolPoints.Length;
        }

    }

    void HandleChase()
    {
        if (playerTransform == null)
        {
            isChasing = false;
            return;
        }
        if (health != null && health.isHurt)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        };

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Case 1 : attack
        if (distanceToPlayer <= attackDistance)
        {
            if (!canAttack)
                return;

            //player hurt ?
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null && playerHealth.isHurt)
            {
                //yes
                animator.SetBool("isRunning", false);
                canAttack = false;
                Invoke("ResetAttack", 2f);
                return;
            }

            // bunny hurt
            if (animator.GetBool("isHit"))
            {
                // bunny hurt
                animator.SetBool("isRunning", false);
                return;
            }
            animator.SetBool("isRunning", false);
            Debug.Log("Lapin attaque !");
            animator.SetTrigger("isAttacking");
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // stop horizontal
        }
        // Case 2 : chase
        else if (distanceToPlayer <= stopDistance && distanceToPlayer > attackDistance)
        {
            animator.SetBool("isRunning", true);
            float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);
            MoveAlongGround(directionX);
        }

        // Case 3 : run
        else
        {
            animator.SetBool("isRunning", false);
            float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);
            MoveAlongGround(directionX);
            isChasing = false;
        }
    }
    void ResetAttack()
    {
        canAttack = true;
    }


    void FaceTowardsX(float directionX)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (directionX > 0 ? 1 : -1);
        transform.localScale = scale;
    }

    void MoveAlongGround(float directionX)
    {
        if (health != null && health.isHurt)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isRunning", false);
            return;
        };
        Vector2 currentVelocity = rb.linearVelocity;
        currentVelocity.x = directionX * moveSpeed;
        rb.linearVelocity = currentVelocity;
        FaceTowardsX(directionX);
    }

}
