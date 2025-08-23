using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Rigidbody2D rb;
    public float speed; // For Movement
    public float jumpingPower; // For Jump
    public float horizontal;
    public float vertical;
    private bool isFacingRight = true; // For Flip

    private Animator animator;
    public bool isWalking;

    public Transform groundCheck; // For Jump
    public LayerMask groundLayer; // For Jump

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();


    }

    void Update()
    {
        PlayerMovement();
        HandleAnimation();
        Flip();

    }

    void PlayerMovement()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        Debug.Log("Horizontal Value: " + horizontal);
        if (Input.GetButtonDown("Jump") && IsGrounded()) // If Button Hit and is player grounded
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower); // JUMP
        }

        isWalking = horizontal != 0 ? true : false;

    }

    void HandleAnimation()
    {
        animator.SetBool("isWalking", isWalking);
    }

    void Flip() 
    {           //Flip to Left                      //Flip to Right
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector2 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }


    void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y); // Movement Left and Right
    }


    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }



}
