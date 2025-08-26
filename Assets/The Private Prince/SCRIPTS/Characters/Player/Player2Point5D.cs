using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class Player2Point5D : CharacterController3D
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    public GameObject raycastEmitter;
    private SpriteRenderer spriteRenderer;

    [Header("STAMINA ENERGY")]
    public float stamina = 100f; // Current/Reserved stamina (initialized to 100)
    public float maxStamina = 100f; // Maximum stamina (set to 100)
    public float staminaDrainRate = 15f; // How fast stamina drains per second while running
    public float staminaRecoveryRate = 3f; // How fast stamina recovers per second when not running

    [Header("REGULATORS")]
    private bool isFacingRight = true; // For flipping the 2D Character (Left or Right)
    public bool isExhausted = false; // Determines if the character is exhausted (stamina is 0 or less)
    float exhaustionTimer = 0f;

    [Header("INTERACTABLES")]
    public float interactRaycast = 5f; // Defines how long the raycast would be
    public LayerMask hitLayers; // Defines what only can be interacted with the raycast

    // ------------------------- METHODS -------------------------

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update() // Update is called once per frame
    {
        HandleInput();
        HandleFlip();
        HandleStamina();
        ApplyMovement(characterController);
        UpdateAnimations();
        HandleRaycast();

        //if (Input.GetButtonDown("Fire1"))
        //{
        //    HandleRaycast();
        //}
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

    void HandleStamina()
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
    
    void HandleRaycast()
    {
        Vector3 rayOrigin = raycastEmitter.transform.position;
        Vector3 rayDirection = isFacingRight ? Vector3.right : Vector3.left;

        Ray interactionRay = new Ray(rayOrigin, rayDirection);
        Debug.DrawRay(rayOrigin, rayDirection * interactRaycast, Color.blue); // Visualizes the laser in the Unity Scene 

        //Debug.Log("Raycast has been established");

        if (Physics.Raycast(interactionRay, out RaycastHit hitInfo, interactRaycast, hitLayers))
        {
            Debug.Log($"Trying to interact with: {hitInfo.collider.name}");
            // Try to get an interface or script from the hit object

            IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();

            if (damageable != null & Input.GetButtonDown("Fire1"))
            {
                Debug.Log("Player damaged the player");

                damageable.TakeDamage(10);
            }
        }
        else 
        {
            Debug.Log("Raycast didn't hit any damageable game objects");
        }
    } 
}
