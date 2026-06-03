using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TriangleSpikeKill : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        TryKillPlayer(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryKillPlayer(collision.collider);
    }

    static void TryKillPlayer(Collider2D other)
    {
        if (other == null) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null)
            health = other.GetComponentInParent<PlayerHealth>();

        if (health != null && !health.isDead)
            health.KillInstant();
    }
}
