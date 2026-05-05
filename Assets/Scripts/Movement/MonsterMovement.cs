using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float moveSpeed;
    private int patrolDestination;
    public Transform playerTransform;
    public float chaseDistance;

    private Animator animator;
    private bool isChasing;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isChasing)
        {
            HandleChase();
        }
        else
        {
            if (Vector2.Distance(transform.position, playerTransform.position) < chaseDistance)
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
        animator.SetBool("isRunning", true);

        if (patrolDestination == 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, patrolPoints[0].position, moveSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, patrolPoints[0].position) < 0.2f)
            {
                transform.localScale = new Vector3(1, 1, 1);
                patrolDestination = 1;
            }
        }
        else if (patrolDestination == 1)
        {
            transform.position = Vector2.MoveTowards(transform.position, patrolPoints[1].position, moveSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, patrolPoints[1].position) < 0.2f)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                patrolDestination = 0;
            }
        }
    }

    void HandleChase()
    {
        animator.SetBool("isRunning", true);

        if (transform.position.x > playerTransform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
            transform.position += Vector3.left * (moveSpeed * Time.deltaTime);
        }
        else if (transform.position.x < playerTransform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            transform.position += Vector3.right * (moveSpeed * Time.deltaTime);
        }

        if (Vector2.Distance(transform.position, playerTransform.position) > chaseDistance * 1.5f)
        {
            isChasing = false;
        }
    }
}
