using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    public Animator playerAnim;
    private Collider2D lastEnemyHit;
    private bool canAttack = true;
    private bool hasJumpAttacked = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasJumpAttacked) return;
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy == null) return;
        lastEnemyHit = other;
        Vector2 offset = other.transform.position - transform.position;
        // Case : jump on 
        if (transform.position.y > other.transform.position.y + 0.1f)
        {
            Debug.Log("Jump attack!");

            enemy.TakeDamage(10);
            hasJumpAttacked = true;
            canAttack = false;


            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // reset vertical
            rb.linearVelocity += new Vector2(0f, 2); // rebond vers le haut

            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other, true);
            lastEnemyHit = other;
            Invoke("EnableCollision", 0.5f); // un peu plus long
            Invoke("ResetAttack", 0.5f);
        }

    }
    public void DealDamage()
    {
        if (!canAttack) return;
        if (lastEnemyHit != null)
        {
            EnemyHealth enemy = lastEnemyHit.GetComponent<EnemyHealth>();
            if (enemy != null) enemy.TakeDamage(20);
        }
        canAttack = false;
        Invoke("ResetAttack", 0.5f);
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