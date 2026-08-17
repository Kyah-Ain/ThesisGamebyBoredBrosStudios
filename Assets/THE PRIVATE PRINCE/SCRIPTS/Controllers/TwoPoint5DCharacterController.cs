using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
// Requires this GameObject to have a CharacterController component in order to function properly
[RequireComponent(typeof(CharacterController))] 
public class TwoPoint5DCharacterController : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // Reference to the PlayerInput component for handling new input system actions and controls
    private PrivatePrinceControls ppControls;
 
    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain
    [SerializeField] private CharacterController characController; // Reference to the CharacterController component for controlling character movement
    [SerializeField] private Animator animator; // Reference to the Animator component for controlling character animations
    [SerializeField] private GameObject spriteRoot; // Reference to the root GameObject that contains the characer sprites for flipping their facing direction
    // [SerializeField] private SpriteRenderer[] characSprites; // Reference to the SpriteRenderer component for handling sprite rendering and flipping

    [Header("CHARACTER ATTRIBUTES")]
    [SerializeField] private float gravity = -9.81f; // Gravity force applied to the character (Earth based gravity)
    [SerializeField] private float gravityMultiplier = 3f; // Multiplier to increase the effect of gravity for a more grounded feel
    [SerializeField] private float freefallVelocity = 0f; // Current vertical velocity of the character for applying gravity and simulating freefall
    [SerializeField] private float movementSpeed = 6f; // Speed at which the character moves
 
    // ------------------------- UNITY METHODS -------------------------
    #region UNITY LOGICS
 
    // ...
    private void Awake()
    {
        // Checks if our reference for the script was not set
        if(debuggerNiAin == null)
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();

        // Get required components if not assigned
        if (characController == null)
            characController = this.GetComponent<CharacterController>();

        if (animator == null)
            animator = this.GetComponentInChildren<Animator>();

        // Evaluates if there is controls initialized in the "GameplayInputManager"
        if (GameplayInputManager.Instance.Controls == null)
        {
            debuggerNiAin.Error("PlayerInputManager singleton not found! Make sure it exists in the scene.");
        }
        else 
        {
            // Accesses the controls from the PlayerInputManager singleton instance
            ppControls = GameplayInputManager.Instance.Controls;

            debuggerNiAin.Error($"New Input System was set: {ppControls}");
        }
    }
    
    // ...
    private void OnEnable()
    {
        // NOTE: MIGHT BE JUST THE TEMPORARY PLACEMENT
        // Calls the method that sets the player to the saved checkpoint spawn
        ApplySpawn();
    }
 
    // Update is called once per frame
    private void Update()
    {
        // Early return if critical components are missing
        if (characController == null || animator == null) return;

        Move();

        ApplyGravity();
    }
 
    #endregion
 
    // --------------------------- MOVEMENT ---------------------------
    #region MOVEMENT LOGICS
 
    // Method for spawning the player at the Spawn Point
    public virtual void ApplySpawn()
    {
        if (SaveManager.Instance != null)
        {
            characController.enabled = false;
 
            Transform destination = SaveManager.Instance.spawnPoint;
 
            // ...
            Vector3 faceDirection = destination.TransformDirection(Vector3.forward);
            this.transform.position = destination.position + faceDirection;
 
            // ...
            Physics.SyncTransforms();
 
            characController.enabled = true;
        }
    }

    // Method for applying gravity to the character to simulate freefall and grounded movement
    public virtual void ApplyGravity()
    {
        if (characController == null) return;

        if (!characController.isGrounded)
        {
            Debug.Log("Player is Falling!");

            freefallVelocity += gravity * gravityMultiplier * Time.deltaTime;
            characController.Move(new Vector3(0, freefallVelocity, 0) * Time.deltaTime);
        }
        else
        {
            freefallVelocity = -1.0f; // Resets vertical velocity when grounded
        }
    }
 
    // Method for Character Movement Logic
    public virtual void Move()
    {
        debuggerNiAin.Log("Player is Moving!");
 
        // Reads the movement input from the new input system
        Vector2 inputVector = ppControls.Player.Move.ReadValue<Vector2>();
 
        // Handle zero input case for normalized vector
        Vector3 movement;
        if (inputVector.magnitude > 0.1f)
        {
            movement = new Vector3(inputVector.x, freefallVelocity, inputVector.y).normalized;
        }
        else
        {
            movement = new Vector3(0, freefallVelocity, 0);
        }
 
        // Evaluates if there is a movement
        if (inputVector.x != 0 || inputVector.y != 0f)
        {
            // Flip sprite based on direction if spriteRoot exists
            if (spriteRoot != null)
            {
                // Gets a reference to the current scale of the sprite root
                Vector3 currentScale = spriteRoot.transform.localScale;
 
                // Determines the direction the character is facing
                if (inputVector.x < 0f)
                {
                    // Flips the sprite root to face left by negating the x scale
                    spriteRoot.transform.localScale = new Vector3(
                        Mathf.Abs(currentScale.x),
                        currentScale.y,
                        currentScale.z
                    );
                }
                else if (inputVector.x > 0f)
                {
                    // Flips the sprite root to face right
                    spriteRoot.transform.localScale = new Vector3(
                        -Mathf.Abs(currentScale.x),
                        currentScale.y,
                        currentScale.z
                    );
                }
            }

            // // Flip sprite based on direction if spriteRoot exists
            // if (characSprites != null)
            // {
            //     // Determines the direction the character is facing
            //     if (inputVector.x < 0f)
            //     {
            //         // Flips the sprite root to face left 
            //         foreach (SpriteRenderer sprite in characSprites)
            //         {
            //             sprite.flipX = true;
            //         }
            //     }
            //     else if (inputVector.x > 0f)
            //     {
            //         // Flips the sprite root to face right
            //         foreach (SpriteRenderer sprite in characSprites)
            //         {
            //             sprite.flipX = false;
            //         }
            //     }
            // }
 
            // Animates the character when moving
            animator.SetBool("isMoving", true);
        }
        else
        {
            // Animates the character when NOT moving
            animator.SetBool("isMoving", false);
        }
 
        // Applies movement to the character
        characController.Move(movement * movementSpeed * Time.deltaTime);
    }
 
    #endregion
}
