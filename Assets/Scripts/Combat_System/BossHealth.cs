using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boss-only health: 5 HP, 1 damage per hit, invincibility frames, hurt/death anims
/// </summary>
public class BossHealth : MonoBehaviour
{
    // Animator hashes
    private static readonly int IsRunningHash = Animator.StringToHash("isRunning");
    private static readonly int IsThrowingHash = Animator.StringToHash("isThrowing");
    private static readonly int IsHurtHash = Animator.StringToHash("isHurt");

    [Header("Boss Settings")]
    public int maxHealth = 5;
    public float invincibilityDuration = 1.5f;
    public float deathAnimDuration = 1.2f;

    [Header("UI")]
    public Slider healthSlider;

    [Header("References")]
    public DoorController doorController;

    // State flags
    public bool isDead { get; private set; }
    public bool isHurt { get; private set; }
    public bool isInvincible { get; private set; }

    // Private fields
    private int currentHealth;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private SquirrelBossAI bossAI;
    private MonsterDamage contactDamage;
    private Rigidbody2D rb;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossAI = GetComponent<SquirrelBossAI>();
        contactDamage = GetComponent<MonsterDamage>();
        rb = GetComponent<Rigidbody2D>();

        originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeHit(int amount = 1)
    {
        if (isDead || isInvincible) return;

        currentHealth -= amount;
        if (healthSlider != null) healthSlider.value = currentHealth;

        var sound = GetComponent<BossSoundController>();
        if (sound != null) sound.PlayHurt();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        if (bossAI != null) bossAI.OnDamaged();

        PlayHurtAnimation();

        isHurt = true;
        StartCoroutine(HurtRoutine());
        StartCoroutine(InvincibilityRoutine());
        StartCoroutine(FlashRoutine());
    }

    void PlayHurtAnimation()
    {
        if (animator == null) return;

        animator.SetBool(IsRunningHash, false);
        animator.ResetTrigger(IsThrowingHash);
        animator.ResetTrigger(IsHurtHash);
        animator.SetTrigger(IsHurtHash);
    }

    private IEnumerator HurtRoutine()
    {
        yield return new WaitForSeconds(invincibilityDuration);
        isHurt = false;
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    private IEnumerator FlashRoutine()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < 4; i++)
        {
            spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 1f);
            yield return new WaitForSeconds(0.08f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void Die()
    {
        isDead = true;

        if (animator != null) animator.SetTrigger("isDead");
        if (bossAI != null) bossAI.enabled = false;

        var sound = GetComponent<BossSoundController>();
        if (sound != null) sound.PlayDeath();

        if (contactDamage != null) contactDamage.enabled = false;
        if (doorController != null) doorController.SetBossDead();

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        if (rb != null)
        {
            rb.simulated = false;
        }

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathAnimDuration);
        Destroy(gameObject);
    }
}
