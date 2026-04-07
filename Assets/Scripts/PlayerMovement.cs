using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D playerRb;
    public float speed;
    public float input;
    public SpriteRenderer spriteRenderer;
    public float jumpForce;

    public LayerMask groundLayer;
    public bool isGrounded;
    public Transform feetPosition;
    public float groundCheckCircle;

    public float jumpTime = 0.35f;
    public float jumpTimeCounter;
    private bool isJumping;

    public Animator playerAnim;

    public float KBForce;
    public float KBCounter;
    public float KBTotalTIme;
    public bool KnockFromRight;

    void FixedUpdate()
    {
        if (KBCounter <= 0)
        {
            playerRb.linearVelocity = new Vector2(input * speed, playerRb.linearVelocity.y);
        }

        else
        {
            if (KnockFromRight == true)
            {
                playerRb.linearVelocity = new Vector2(-KBForce, KBForce);
            }

            if (KnockFromRight == false)
            {
                playerRb.linearVelocity = new Vector2(KBForce, KBForce);
            }
             KBCounter -= Time.deltaTime;
        }
    
        if (input < 0)
        {
            spriteRenderer.flipX = true;
            playerAnim.SetBool("isWalking", true);
        }
        else if (input > 0)
        {
            spriteRenderer.flipX = false;
            playerAnim.SetBool("isWalking", true);
        }
        else if (input ==0)
        {
            playerAnim.SetBool("isWalking", false);
        }

        
    }
}
