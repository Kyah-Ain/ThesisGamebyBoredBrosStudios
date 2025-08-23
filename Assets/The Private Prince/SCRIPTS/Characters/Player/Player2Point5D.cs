using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class Player2Point5D : CharacterController3D
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    private SpriteRenderer spriteRenderer;

    [Header("STAMINA ENERGY")]
    public float stamina = 100f; // Current/Reserved stamina (initialized to 100)
    public float maxStamina = 100f; // Maximum stamina (set to 100)
    public float staminaDrainRate = 15f; // How fast stamina drains per second while running
    public float staminaRecoveryRate = 5f; // How fast stamina recovers per second when not running

    [Header("REGULATORS")]
    private bool isFacingRight = true; // For flipping the 2D Character (Left or Right)
    public bool isExhausted = false; // Determines if the character is exhausted (stamina is 0 or less)
    float exhaustionTimer = 0f;

    // ------------------------- METHODS -------------------------

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update() // Update is called once per frame
    {
        HandleInput();
        HandleFlip();
        UpdateStamina();
        ApplyMovement(characterController);
        UpdateAnimations();
    }

    public override void HandleInput()
    {
        MoveControl();

        if (!isExhausted && stamina > 0)
        {
            base.RunControl();
        }
        else 
        {
            isRunning = false;
        }

        JumpControl();

        CalculateMovement(inputDirection, isRunning);
        CalculateJump(wantsToJump);
    }

    void HandleFlip()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        // METHOD 1: FLIPS THE SCALE OF THE OBJECT WITH A CAMERA SHAKENESS DURING TRANSITION
        //Flip to Left                       //Flip to Right
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector2 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }

        //// METHOD 2: JUST ROTATE THE GAME OBJECT FOR SMOOTHER TRANSITION
        ////Flip to Left                       //Flip to Right
        //if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        //{
        //    isFacingRight = !isFacingRight;

        //    // Flip the sprite on the X-axis
        //    spriteRenderer.flipX = !isFacingRight;
        //}
    }

    void UpdateStamina()
    {
        if (isRunning)
        {
            stamina -= staminaDrainRate * Time.deltaTime;
        }
        else if (stamina < maxStamina)
        {
            stamina += staminaRecoveryRate * Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0f, maxStamina);

        // Handle exhaustion
        if (stamina <= 0)
        {
            isExhausted = true;
            exhaustionTimer = 2f;
        }

        if (isExhausted)
        {
            exhaustionTimer -= Time.deltaTime;
            if (exhaustionTimer <= 0)
            {
                isExhausted = false;
            }
        }
    }

}
