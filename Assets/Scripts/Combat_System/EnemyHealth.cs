using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 30;
    private int currentHealth;
    private Animator enemyAnim;
    public Slider healthSlider;
    public bool isDead = false;
    public bool isHurt = false;

    void Start()
    {
        currentHealth = maxHealth;
        enemyAnim = GetComponent<Animator>();
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (healthSlider != null)
            healthSlider.value = currentHealth;
        if (GetComponent<EnemySoundController>() != null)
            GetComponent<EnemySoundController>().PlayHurt();


        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            if (enemyAnim != null)
                enemyAnim.SetTrigger("isHit");
            isHurt = true;
            StartCoroutine(HurtRoutine());
        }
    }
    private IEnumerator HurtRoutine()
    {
        yield return new WaitForSeconds(2f);
        isHurt = false;
    }

    private void Die()
    {
        isDead = true;
        if (enemyAnim != null)
            enemyAnim.SetTrigger("isDead");
        if (GetComponent<EnemySoundController>() != null)
            GetComponent<EnemySoundController>().PlayDeath();
        GetComponent<Collider2D>().enabled = false; //stop collider
        GetComponent<Rigidbody2D>().simulated = false;
        StartCoroutine(DieRoutine());
    }
    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
