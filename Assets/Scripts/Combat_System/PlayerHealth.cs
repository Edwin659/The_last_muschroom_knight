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
    public LifeBarController lifeBarController;
    public Transform deathZone;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private Collider2D ignoredSourceCollider;
    private Coroutine invincibilityRoutine;

    public AudioSource audioSource;
    public AudioClip hurtClip;
    public AudioClip deathClip;
    public AudioClip healClip;


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
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("KillZone"))
        {
            currentHealth = 0;
            Die(true); // withouts animation
        }
    }

    public void TakeDamage(int damage, Transform source, float knockbackForce = 12f)
    {
        if (!canBeHit || isDead)
            return;

        canBeHit = false;
        isHurt = true;
        currentHealth -= damage;

        if (hurtClip != null && audioSource != null)
            audioSource.PlayOneShot(hurtClip);

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

    //1.5s invincibility + sprite flash after any hit
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

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
            healthSlider.value = currentHealth;
        if (LifeBarController.instance != null)
            LifeBarController.instance.currentLives = 3;
        if (healClip != null && audioSource != null)
            audioSource.PlayOneShot(healClip);

        isHurt = false;
        canBeHit = true;
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

    public void Die(bool skipAnimation = false)
    {

        if (isDead)
            return;

        if (invincibilityRoutine != null)
            StopCoroutine(invincibilityRoutine);

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        if (!skipAnimation && playerAnim != null)
            playerAnim.SetTrigger("IsDead");

        isDead = true;

        if (deathClip != null && audioSource != null)
            audioSource.PlayOneShot(deathClip);



        //Stop movement
        playerAnim.SetBool("IsWalking", false);
        playerAnim.SetBool("IsRunning", false);
        GetComponent<PlayerMovement>().enabled = false; // stop movement

        if (playerCollider != null)
            playerCollider.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        //LifeBar
        if (LifeBarController.instance != null)
        {
            LifeBarController.instance.currentLives--;

            if (LifeBarController.instance.currentLives <= 0)
            {
                NoLife(); // Game Over
            }
            else
            {
                DieEnd(skipAnimation); // reload scene
            }
        }
    }
    public void DieEnd(bool skipAnimation)
    {
        if (skipAnimation)
        {
            // Reload immediat
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
        else
        {
            // Reload with delay
            StartCoroutine(ReloadSceneAfterDelay(1f));
        }
    }

    IEnumerator ReloadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        //Reload scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void NoLife()
    {
        // Save current scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastScene", currentSceneName);

        //No black heart
        if (LifeBarController.instance != null)
        {
            Destroy(LifeBarController.instance.gameObject);
            LifeBarController.instance = null;
        }
        //GameOver
        SceneManager.LoadScene("GameOverMenu");
    }
}
