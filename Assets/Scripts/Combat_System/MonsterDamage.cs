using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDamage : MonoBehaviour
{
    public int damage = 1;
    [SerializeField] private float damageCooldown = 1.0f;
    private float nextDamageTime;

    private void OnTriggerStay2D(Collider2D other)
    {
        HandlePlayerHit(other);
    }

    private void HandlePlayerHit(Collider2D other)
    {
        if (Time.time < nextDamageTime || other == null) return;

        PlayerHealth hitHealth = other.GetComponentInParent<PlayerHealth>();
        PlayerMovement hitMovement = other.GetComponentInParent<PlayerMovement>();


        if (hitHealth == null || hitMovement == null) return;
        if (hitHealth.isDead) return;

        Vector2 offset = other.transform.position - transform.position;

        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            bool knockFromRight = other.transform.position.x <= transform.position.x;
            hitMovement.ApplyKnockback(knockFromRight, 0.45f, 0.55f);

            hitHealth.TakeDamage(damage,transform);
            nextDamageTime = Time.time + damageCooldown;
        }
        else
        {
            //jumps to see later
        }
    }
}
