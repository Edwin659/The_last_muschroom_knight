using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    public Animator playerAnim;
    public float stompBounce = 5f;

    private Collider2D lastEnemyHit;
    private Collider2D playerCollider;
    private bool canAttack = true;
    private bool hasJumpAttacked = false;

    void Awake()
    {
        playerCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryStomp(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (hasJumpAttacked)
            return;

        BossHealth bossHealth = other.GetComponentInParent<BossHealth>();

        if (bossHealth != null)
        {
            if (!other.CompareTag("Weak Point"))
                return;
            TryStomp(other);
            return;
        }

        if (other.CompareTag("Bunny") || other.CompareTag("Pidgeon"))
        {
            lastEnemyHit = other;
        }
    }


    void TryStomp(Collider2D other)
    {
        if (hasJumpAttacked)
            return;

        BossHealth bossHealth = other.GetComponentInParent<BossHealth>();
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

        if (bossHealth == null && enemy == null)
            return;

        if (!IsStompHit(other, bossHealth))
            return;

        lastEnemyHit = other;

        if (bossHealth != null)
        {
            if (bossHealth.isInvincible)
                return;
            bossHealth.TakeHit(1);
        }
        else
        {
            enemy.TakeDamage(10);
        }

        hasJumpAttacked = true;
        canAttack = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (bossHealth != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stompBounce);
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.linearVelocity += new Vector2(0f, 2f);
        }

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other, true);

        Invoke(nameof(EnableCollision), 0.5f);
        Invoke(nameof(ResetAttack), 0.5f);
    }

    bool IsStompHit(Collider2D other, BossHealth bossHealth)
    {
        if (bossHealth == null)
            return transform.position.y > other.transform.position.y + 0.1f;

        if (!other.CompareTag("Weak Point"))
            return false;

        float playerBottom = playerCollider != null
            ? playerCollider.bounds.min.y
            : transform.position.y;

        return playerBottom >= other.bounds.min.y - 0.12f;
    }

    public void DealDamage()
    {
        if (!canAttack || lastEnemyHit == null)
            return;

        BossHealth bossHealth = lastEnemyHit.GetComponentInParent<BossHealth>();
        EnemyHealth enemy = lastEnemyHit.GetComponentInParent<EnemyHealth>();

        if (bossHealth != null)
        {
            if (bossHealth.isInvincible)
                return;
            bossHealth.TakeHit(1);
            Debug.Log("Boss touché !");
        }
        else if (enemy != null)
        {
            enemy.TakeDamage(20);
            Debug.Log("Ennemi touché !");
        }

        canAttack = false;
        Invoke(nameof(ResetAttack), 0.5f);
    }



    private void ResetAttack()
    {
        canAttack = true;
        hasJumpAttacked = false;
    }

    private void EnableCollision()
    {
        if (lastEnemyHit != null)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), lastEnemyHit, false);
            lastEnemyHit = null;
        }
    }
}