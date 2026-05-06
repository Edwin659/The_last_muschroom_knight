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
    private float chaseCheckTimer = 0f;
    public float chaseCheckInterval = 0.5f;
    private bool canAttack = true;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();


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

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Case 1 : attack
        if (distanceToPlayer <= attackDistance)
        {
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
            if (animator.GetBool("isHurt"))
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
    void MoveAlongGround(float directionX)
    {

        Vector2 currentVelocity = rb.linearVelocity;
        currentVelocity.x = directionX * moveSpeed;
        rb.linearVelocity = currentVelocity;
        FaceTowardsX(directionX);
    }

}
