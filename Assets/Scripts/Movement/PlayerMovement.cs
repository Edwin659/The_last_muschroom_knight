using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Player
    private Rigidbody2D playerRb;
    public float speed;
    public float runningSpeed;
    float currentspeed;
    float input;
    private SpriteRenderer spriteRenderer;


    //Ground
    public LayerMask groundLayer;
    public bool isGrounded;
    public Transform feetPosition;
    public float groundCheckCircle;

    //jumping
    public float jumpTime = 0.35f;
    public float jumpTimeCounter;
    public float jumpForce;
    public int maxJumps = 2;
    private int jumpsleft;
    private bool wasGrounded = true;

    //Attack
    private bool isAttacking;


    //Animation
    private Animator playerAnim;
    // private float lastSlopeAngle = 0f;
    private Vector2 previousNormal = Vector2.up;


    //For knok back
    public float KBForce;
    public float KBCounter;
    public float KBTotalTIme;
    public bool KnockFromRight;

    // Audio
    public AudioSource audioSource;
    public AudioClip WalkSound;
    public AudioClip RunSound;
    public AudioClip JumpSound;
    public AudioClip LandingSound;
    public AudioClip AttackSound;


    //Friction
    public PhysicsMaterial2D noFriction;
    public PhysicsMaterial2D highFriction;

    //Temporisation
    private float groundAlignDelay = 0.1f;
    private float groundAlignTimer = 0f;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>(); // Parent
        playerAnim = GetComponentInChildren<Animator>(); // Children
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (isAttacking)
        {
            input = 0;
        }
        else
        {
            input = Input.GetAxisRaw("Horizontal");
        }

        //Run
        if (Input.GetKey(KeyCode.LeftShift) && input != 0 && !isAttacking)
        {
            currentspeed = runningSpeed;
            playerAnim.SetBool("IsRunning", true);
            if (isGrounded)
            {
                audioSource.pitch = 2.0f;
                if (!audioSource.isPlaying)
                    audioSource.PlayOneShot(RunSound);
            }
        }
        else
        {
            playerAnim.SetBool("IsRunning", false);
            currentspeed = speed;
            audioSource.pitch = 1.5f;
        }

        //Walk
        if (!isAttacking)
        {
            if (input != 0)
            {
                playerAnim.SetBool("IsWalking", true);

                if (isGrounded && !playerAnim.GetBool("IsRunning"))
                {
                    if (!audioSource.isPlaying)
                        audioSource.PlayOneShot(WalkSound);
                }
            }
            else
            {
                playerAnim.SetBool("IsWalking", false);
            }
        }

        if (input < 0)//left
        {
            spriteRenderer.flipX = false;
        }
        else if (input > 0)//right
        {
            //turn right
            spriteRenderer.flipX = true;
        }

            //jump
        if (isGrounded && playerRb.linearVelocity.y<=0.1f)
        {
            jumpsleft = maxJumps;
        }
        bool jumpPressed = Keyboard.current != null &&
            (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame);
        if (jumpPressed && !isAttacking)
        {
            Debug.Log("Jump pressed!");
            OnJump();
        }
        //Landing
        if (!wasGrounded && isGrounded && playerRb.linearVelocity.y <= 0.1f)
        {
            audioSource.PlayOneShot(LandingSound);
        }

        wasGrounded = isGrounded;

        //attack
        if (Input.GetKey(KeyCode.E))
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        bool center = Physics2D.OverlapCircle(feetPosition.position, groundCheckCircle, groundLayer);
        bool left = Physics2D.OverlapCircle(feetPosition.position + Vector3.left * 0.2f, groundCheckCircle, groundLayer);
        bool right = Physics2D.OverlapCircle(feetPosition.position + Vector3.right * 0.2f, groundCheckCircle, groundLayer);

        isGrounded = center || left || right;
        if (isGrounded)
        {
            if (groundAlignTimer > 0f) {
                groundAlignTimer -= Time.fixedDeltaTime;
            }
            else { 
                RaycastHit2D centerHit = Physics2D.Raycast(feetPosition.position, Vector2.down, 1f, groundLayer);
                RaycastHit2D leftHit = Physics2D.Raycast(feetPosition.position + Vector3.left * 0.2f, Vector2.down, 1f, groundLayer);
                RaycastHit2D rightHit = Physics2D.Raycast(feetPosition.position + Vector3.right * 0.2f, Vector2.down, 1f, groundLayer);

                RaycastHit2D hit = default;
                if (centerHit.collider != null) hit = centerHit;
                else if (leftHit.collider != null) hit = leftHit;
                else if (rightHit.collider != null) hit = rightHit; 

                if (hit.collider != null)
                {
                    Vector2 smoothedNormal = Vector2.Lerp(previousNormal, hit.normal, 0.3f);
                    previousNormal = smoothedNormal;

                    float angle = Mathf.Atan2(smoothedNormal.y, smoothedNormal.x) * Mathf.Rad2Deg;
                    //Debug.Log(angle);
                    float targetAngle = angle - 90;

                    //if 60f>angle>6f
                    if (Mathf.Abs(targetAngle) > 6f && Mathf.Abs(targetAngle) < 60f)
                    {
                        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
                        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
                    }
                    else
                    {
                        transform.rotation = Quaternion.identity;
                    }
                }
            }
        }
        else
        {
            //if no angle
            transform.rotation = Quaternion.identity;
            groundAlignTimer = groundAlignDelay;
        }

        //KnockBack
        if (KBCounter <= 0)//if nothing just move
        {
            if (!isAttacking) // prevent movement while attacking
            {
                playerRb.linearVelocity = new Vector2(input * currentspeed, playerRb.linearVelocity.y);
            }
        }
        else
        {
            if (KnockFromRight == true)//if the player is knock on the right, he moves back on left
            {
                playerRb.linearVelocity = new Vector2(-KBForce, KBForce);
            }

            if (KnockFromRight == false)//if the player is knock on the left, he moves back on right
            {
                playerRb.linearVelocity = new Vector2(KBForce, KBForce);
            }
            KBCounter -= Time.deltaTime;//Times's counter before the player regains power 
        }
        if (isGrounded && input == 0)
        {
            playerRb.sharedMaterial = highFriction;
        }
        else
        {
            playerRb.sharedMaterial = noFriction;
        }
    }
    public void OnJump()
    {
        if (jumpsleft == maxJumps) { audioSource.PlayOneShot(JumpSound); }
        if (jumpsleft > 0)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0);
            playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpsleft--;
            transform.rotation = Quaternion.identity;
        }
        else
        {
            Debug.Log("No jumps left " + jumpsleft);
        }
    }
    public void Attack()
    {
        isAttacking = true;
        playerAnim.SetTrigger("IsAttacking");
        playerRb.linearVelocity = Vector2.zero;
        Invoke("PlayAttackSound", 0.7f);
        
    }
    void PlayAttackSound()
    {
        audioSource.PlayOneShot(AttackSound);
    }
    public void EndAttack()
    {
        isAttacking = false;
        Debug.Log("Attack end");
    }
}

