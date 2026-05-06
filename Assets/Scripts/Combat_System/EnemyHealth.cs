using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 20;
    private int currentHealth;
    private Animator enemyAnim;

    void Start()
    {
        currentHealth = maxHealth;
        enemyAnim = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        enemyAnim.SetTrigger("isHurt");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        enemyAnim.SetTrigger("isDead");
        Destroy(gameObject);
    }
}
