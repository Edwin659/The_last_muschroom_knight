using UnityEngine;

public class BossSoundController : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Clips")]
    public AudioClip presenceClip;
    public AudioClip hurtClip;
    public AudioClip deathClip;
    public AudioClip attackClip;
    public AudioClip throwClip;



    [Header("Presence Settings")]
    public Transform player;
    public float presenceDistance = 5f;
    public float presenceCooldown = 3f;

    private float lastPresenceTime;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        HandlePresenceSound();
    }

    void HandlePresenceSound()
    {
        if (presenceClip == null || player == null || audioSource == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < presenceDistance && Time.time > lastPresenceTime + presenceCooldown)
        {
            audioSource.PlayOneShot(presenceClip);
            lastPresenceTime = Time.time;
        }
    }

    public void PlayHurt()
    {
        if (hurtClip != null && audioSource != null)
            audioSource.PlayOneShot(hurtClip);
    }

    public void PlayAttack()
    {
        if (attackClip != null && audioSource != null)
            audioSource.PlayOneShot(attackClip);
    }

    public void PlayThrow()
    {
        if (attackClip != null && audioSource != null)
            audioSource.PlayOneShot(throwClip);
    }

    public void PlayDeath()
    {
        if (deathClip != null && audioSource != null)
            audioSource.PlayOneShot(deathClip);
    }
}
