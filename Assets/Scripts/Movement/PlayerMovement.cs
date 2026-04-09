using System;
using System.Collections;
using System.Collections.Generic;
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

    //Animation
    private Animator playerAnim;

    //For knok back
    public float KBForce;
    public float KBCounter;
    public float KBTotalTIme;
    public bool KnockFromRight;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>(); // sur le parent
        playerAnim = GetComponentInChildren<Animator>(); // sur l’enfant
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        input = Input.GetAxisRaw("Horizontal");

        if (Input.GetKey(KeyCode.LeftShift) && input != 0)//If running Shift pressed
        {
            currentspeed = runningSpeed;
            playerAnim.SetBool("IsRunning", true);
        }
        else
        {
            playerAnim.SetBool("IsRunning", false);
            currentspeed = speed;
        }

        //Walk
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
        playerAnim.SetBool("IsWalking", input != 0);

        //jump
        if (isGrounded && playerRb.linearVelocity.y<=0.1f)
        {
            jumpsleft = maxJumps;
        }
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            OnJump();
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(feetPosition.position, groundCheckCircle, groundLayer);

        if (KBCounter <= 0)//if nothing just move
        {
            playerRb.linearVelocity = new Vector2(input * currentspeed, playerRb.linearVelocity.y);

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
    }
}

