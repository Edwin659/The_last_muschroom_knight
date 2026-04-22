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
                audioSource.pitch = 2.0f; // accÃ©lÃ¨re le son
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
            //transform.localScale = new Vector3(1, 1, 1);//turn left
            spriteRenderer.flipX = false;
        }
        else if (input > 0)//right
        {
            //turn right
            //transform.localScale = new Vector3(-1,1,1);
            spriteRenderer.flipX = true;
        }
        

        //jump
        if (isGrounded && playerRb.linearVelocity.y<=0.1f)
        {
            jumpsleft = maxJumps;
        }
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isAttacking == false)
        {
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
        isGrounded = Physics2D.OverlapCircle(feetPosition.position, groundCheckCircle, groundLayer);

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
    }
    public void OnJump()
    {

        if (jumpsleft > 0)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0);
            playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpsleft--;
        }
        else
        {
            Debug.Log("No jumps left " + jumpsleft);
        }
        if(jumpsleft == maxJumps) { audioSource.PlayOneShot(JumpSound); }
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

