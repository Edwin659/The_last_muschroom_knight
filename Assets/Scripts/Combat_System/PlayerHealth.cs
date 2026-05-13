using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int maxLife = 3;
    public int currentLife;
    public int currentHealth;
    private Animator playerAnim;
    public Slider healthSlider;
    public Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        //currentLife = maxLife;
        Debug.Log(currentHealth);
        rb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponentInChildren<Animator>(); // Children
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            Debug.Log("Slider updated to " + healthSlider.value);
        }
        else
        {
            Debug.LogWarning("HealthSlider is not assigned!");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthSlider.value = currentHealth;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
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
    }
    public void Heal(int healing)
    {
        currentHealth += healing;
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        healthSlider.value = currentHealth;
    }
    public void Die() {
        Debug.Log("Player is dead");
        playerAnim.SetTrigger("IsDead");
        GetComponent<PlayerMovement>().enabled = false; // stop movement
    }
    public void DieEnd()
    {
        //Reload scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);


    // Game Over Scene
    //UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
}
    public void NoLife() //bonus
    {
        // Game Over Scene
        //UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
    }
}
