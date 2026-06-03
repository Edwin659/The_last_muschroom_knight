using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Animator playerAnim;
    public Slider healthSlider;
    public bool isDead = false;
    public bool isHurt;
    private bool canBeHit = true;
    private Rigidbody2D rb;
    public LifeBarController lifeBarController;
    public Transform deathZone;

    void Start()
    {
        isDead = false;
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponentInChildren<Animator>(); // Children
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
            Die(true); // withou animation
        }
    }


    public void TakeDamage(int damage, Transform source)
    {
        if (!canBeHit || isDead) return;
        canBeHit = false;
        isHurt = true;
        currentHealth -= damage;
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }
        else
        {
            playerAnim.SetBool("IsHit", true);
            StartCoroutine(HurtRoutine());
        }

        //litle knockback
        Vector2 direction = (transform.position - source.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * 12f, 0f);


        //stop collision 
        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D sourceCollider = source.GetComponent<Collider2D>();
        if (playerCollider != null && sourceCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, sourceCollider, true);
            Invoke("EnableCollision", 0.5f); //0.5s
        }
        StartCoroutine(HitCooldown());
    }
    private void EnableCollision()
    {
        Collider2D playerCollider = GetComponent<Collider2D>();
        GameObject bunny = GameObject.FindWithTag("Bunny");
        if (bunny != null)
        {
            Collider2D sourceCollider = bunny.GetComponent<Collider2D>();
            if (playerCollider != null && sourceCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, sourceCollider, false);
            }
        }
    }
    IEnumerator HitCooldown()
    {
        yield return new WaitForSeconds(1f);
        canBeHit = true;
    }
    IEnumerator HurtRoutine()
    {
        yield return new WaitForSeconds(1f); // hurt time
        playerAnim.SetBool("IsHit", false);
        isHurt = false;
        canBeHit = true;
    }
    public void Heal(int healing)
    {
        currentHealth += healing;
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
    public void KillInstant()//Spike no kill instant just loose pv
    {
        if (isDead) return;
        currentHealth = 0;
        if (healthSlider != null)
            healthSlider.value = 0;
        Die();
    }

    public void Die(bool skipAnimation = false)
    {
        if (isDead) return;
        isDead = true;

        if (!skipAnimation && playerAnim != null)
            playerAnim.SetTrigger("IsDead");

        //Stop movement
        playerAnim.SetBool("IsWalking", false);
        playerAnim.SetBool("IsRunning", false);
        GetComponent<PlayerMovement>().enabled = false; // stop movement

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        //LifeBar
        if (LifeBarController.instance != null)
        {
            LifeBarController.instance.currentLives--;
            Debug.Log(LifeBarController.instance.currentLives);

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
            // Reload imm�diat
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
        else
        {
            // Reload avec d�lai (mort normale avec animation)
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
    public void NoLife() //bonus
    {
        Debug.Log("GAME OVER");

        // Save curret scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastScene", currentSceneName);

        //No blak heart
        if (LifeBarController.instance != null)
        {
            Destroy(LifeBarController.instance.gameObject);
            LifeBarController.instance = null;
        }
        //GameOver
        SceneManager.LoadScene("GameOverScene");
    }
}
