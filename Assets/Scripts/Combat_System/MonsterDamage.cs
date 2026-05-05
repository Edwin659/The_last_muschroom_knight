using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDamage : MonoBehaviour
{
    public int damage;
    public PlayerHealth playerHealth;
    public PlayerMovement playerMovement;
    [SerializeField] private float damageCooldown = 0.2f;
    private float nextDamageTime;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerHit(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerHit(other);
    }

    private void HandlePlayerHit(Collider2D other)
    {
        if (Time.time < nextDamageTime || other == null)
        {
            return;
        }

        Rigidbody2D hitRigidbody = other.attachedRigidbody;
        Transform hitTransform = hitRigidbody != null ? hitRigidbody.transform : other.transform;

        PlayerHealth hitHealth = hitTransform.GetComponentInParent<PlayerHealth>();
        PlayerMovement hitMovement = hitTransform.GetComponentInParent<PlayerMovement>();
        if (hitHealth == null || hitMovement == null)
        {
            return;
        }

        playerHealth = hitHealth;
        playerMovement = hitMovement;

        bool knockFromRight = hitTransform.position.x <= transform.position.x;
        playerMovement.ApplyKnockback(knockFromRight, 0.45f, 0.55f);

        playerHealth.TakeDamage(damage);
        nextDamageTime = Time.time + damageCooldown;
    }
}
