using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Animator playerAnim;
    public Slider healthSlider;
    public float invincibilityDuration = 1.5f;
    public float invincibilityFlashInterval = 0.1f;

    public bool isDead = false;
    public bool isHurt;
    public bool CanBeDamaged => canBeHit && !isDead;

    private bool canBeHit = true;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private Collider2D ignoredSourceCollider;
    private Coroutine invincibilityRoutine;

    void Start()
    {
        isDead = false;
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int damage, Transform source, float knockbackForce = 12f)
    {
        if (!canBeHit || isDead)
            return;

        canBeHit = false;
        isHurt = true;
        currentHealth -= damage;

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        if (playerAnim != null)
            playerAnim.SetBool("IsHit", true);

        if (knockbackForce > 0f)
        {
            Vector2 direction = (transform.position - source.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * knockbackForce, 0f);
        }

        if (invincibilityRoutine != null)
            StopCoroutine(invincibilityRoutine);

        invincibilityRoutine = StartCoroutine(InvincibilityRoutine(source));
    }

    // --- Holy grail: 1.5s invincibility + sprite flash after any hit ---
    private IEnumerator InvincibilityRoutine(Transform source)
    {
        Collider2D sourceCollider = source != null ? source.GetComponent<Collider2D>() : null;
        if (playerCollider != null && sourceCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, sourceCollider, true);
            ignoredSourceCollider = sourceCollider;
        }

        float elapsed = 0f;
        bool visible = true;

        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;

            visible = !visible;
            yield return new WaitForSeconds(invincibilityFlashInterval);
            elapsed += invincibilityFlashInterval;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        if (playerCollider != null && ignoredSourceCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, ignoredSourceCollider, false);
            ignoredSourceCollider = null;
        }

        if (playerAnim != null)
            playerAnim.SetBool("IsHit", false);

        isHurt = false;
        canBeHit = true;
        invincibilityRoutine = null;
    }

    public void Heal(int healing)
    {
        currentHealth += healing;
        if (currentHealth >= maxHealth)
            currentHealth = maxHealth;

        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }

    public void KillInstant()
    {
        if (isDead)
            return;

        currentHealth = 0;
        if (healthSlider != null)
            healthSlider.value = 0;

        Die();
    }

    public void Die()
    {
        if (isDead)
            return;

        if (invincibilityRoutine != null)
            StopCoroutine(invincibilityRoutine);

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        if (playerAnim != null)
            playerAnim.SetTrigger("IsDead");

        isDead = true;

        if (playerAnim != null)
        {
            playerAnim.SetBool("IsWalking", false);
            playerAnim.SetBool("IsRunning", false);
            playerAnim.SetTrigger("IsDead");
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        if (playerCollider != null)
            playerCollider.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Debug.Log("Player is dead");
    }

    public void DieEnd()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void NoLife()
    {
    }
}
