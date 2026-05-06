using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    //public int maxLife = 3;
    //public int currentLife;
    public int currentHealth;
    public Animator playerAnim;
    public Slider healthSlider;
    public bool isDead = false;
    public bool isHurt;
    private bool canBeHit = true;
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isDead = false;
        currentHealth = maxHealth;
        //currentLife = maxLife;
        rb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponentInChildren<Animator>(); // Children
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
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
            //Bonus if time (health = checkpoints) (life= level)
            //currentLife--;
            //if (currentLife <= 0){
            //NoLife();
            //}
            //else
            //{
            //UI at the begining 
            //UnityEngine.SceneManagement.SceneManager.LoadScene("");
            //}
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
        yield return new WaitForSeconds(0.5f);
        canBeHit = true;
    }
    IEnumerator HurtRoutine()
    {
        yield return new WaitForSeconds(0.5f); // hurt time
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
    public void Die() {

        if (isDead) return;
        if (playerAnim != null)
            playerAnim.SetTrigger("isDead");
        isDead = true;

        playerAnim.SetBool("IsWalking", false);
        playerAnim.SetBool("IsRunning", false);
        GetComponent<PlayerMovement>().enabled = false; // stop movement
        //GetComponent<PlayerAttack>().enabled = false;

        GetComponent<Collider2D>().enabled = false; //stop collider
        playerAnim.SetTrigger("IsDead");
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
        Debug.Log("Player is dead");
    }
    public void DieEnd()
    {
        //Reload scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Tesr-Tiles");

        // Game Over Scene
        //UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
    }
    public void NoLife() //bonus
    {
        // Game Over Scene
        //UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
    }
}
