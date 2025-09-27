using System; // Grants access to base system functions and datatypes
using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using Unity.VisualScripting; // Grants access to Unity's Visual Scripting features
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug
using UnityEngine.EventSystems; // Grants access to Unity's Event System features

public class MovementManager : MonoBehaviour, IMoveable
{
    // ------------------------- VARIABLES -------------------------

    [Header("ANIMATION")]
    public Animator animator; // Animation Manager component reference

    [Header("MOVEMENT COMPASS")]
    protected Vector3 moveDirection = Vector3.zero; // Placeholder for the character's movement direction
    protected float curSpeedX; // Current speed in the X direction
    protected float curSpeedY; // Current speed in the Y direction
    protected float verticalVelocity; // Current vertical velocity (for jumping and falling)

    [Header("ATTRIBUTES")]
    public float gravityWeight = 20.0f; // Weight of gravity affecting the character speed and jump
    public float walkingSpeed = 3.5f; // Speed at which the character walks
    public float runningSpeed = 8f; // Speed at which the character runs
    public float jumpPower = 8.0f; // Power of the character's jump

    // Interface implementation for IMoveable
    public float iWalkingSpeed { get => walkingSpeed; set => gravityWeight = value; }
    public float iGravityWeight { get => gravityWeight; set => gravityWeight = value; }

    [Header("STATES")]
    public bool isRunning = false; // Determines if the character is running
    public bool canMove = true; // Determines if the character can move
    public bool isGrounded; // Determines if the character is grounded (on the ground)

    // ------------------------- METHODS -------------------------

    public enum CharacterState { isIdle, isWalking, isRunning, isJumping }
    public CharacterState currentCharacState;

    // Sets up default state for the character
    public virtual void HandleInput() 
    {
        // Freezes the Characters
        if (!canMove)
        {
            // Instantly stop character movements
            moveDirection = Vector3.zero; 
            curSpeedX = 0;
            curSpeedY = 0;
            verticalVelocity = 0;
            return;
        }

        //// Set everything to zero to make sure
        //Vector2 inputDirection = new Vector2(0, 0);
        //bool wantsToRun = false;
        //bool wantsToJump = false;

        //CalculateMovement(inputDirection, wantsToRun);
        //CalculateJump(wantsToJump);
    }

    // Calculates movement based on input direction and running state
    public virtual void CalculateMovement(Vector2 inputDirection, bool wantsToRun) 
    {
        // Calculates move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward); // This checks forrward and backward movement
        Vector3 right = transform.TransformDirection(Vector3.right); // This checks forward and backward movement

        // Condition for movement
        float targetSpeed = wantsToRun ? runningSpeed : walkingSpeed; // Sets target speed based on running or walking
        curSpeedX = canMove ? targetSpeed * inputDirection.y : 0;  // Moves the character forward and backward
        curSpeedY = canMove ? targetSpeed * inputDirection.x : 0; // Moves the character left and right

        moveDirection = (forward * curSpeedX) + (right * curSpeedY); // Identifies what direction the character is moving
        isRunning = isRunning && inputDirection.magnitude > 0.1f; // Only allow running if there is a movement input

        //Debug.Log($"Move Direction: {moveDirection}"); // Logs the character's movement direction
    }

    // Calculates jump based on jump input and grounded state
    public virtual void CalculateJump(bool wantsToJump)
    {
        // Handle jumping values
        if (wantsToJump && isGrounded)
        {
            verticalVelocity = jumpPower;
        }

        // Applying gravity if not grounded
        if (!isGrounded)
        {
            verticalVelocity -= gravityWeight * Time.deltaTime;
        }
        else if (verticalVelocity < 0)
        {
            verticalVelocity = -0.5f; // Small negative value to stay grounded
        }

        moveDirection.y = verticalVelocity;
    }

    // Updates animation states based on movement
    public virtual void UpdateAnimations()
    {
        // Handle animation states / transitions
        if (!isGrounded)
        {
            SetAnimationState(CharacterState.isJumping);
        }
        else
        {
            // Calculate horizontal movement magnitude (ignore Y axis for movement detection)
            Vector3 horizontalMove = new Vector3(moveDirection.x, 0, moveDirection.z);

            // Determine if the character is walking or running based on movement magnitude
            if (horizontalMove.magnitude > 0.1f)
            {
                SetAnimationState(isRunning ? CharacterState.isRunning : CharacterState.isWalking);
            }
            else
            {
                SetAnimationState(CharacterState.isIdle);
            }
        } 
    }

    // Sets the animation state and updates animator parameters
    public virtual void SetAnimationState(CharacterState newState)
    {
        // Avoid replaying the same animation state
        if (currentCharacState == newState) return;

        // Updates animation states
        animator.SetBool(currentCharacState.ToString(), false);
        animator.SetBool(newState.ToString(), true);
        currentCharacState = newState;
    }

    // Applies movement to the character controller
    public virtual void ApplyMovement(CharacterController controller)
    {
        // Applies movement to the character controller
        if (controller != null)
        {
            controller.Move(moveDirection * Time.deltaTime);
            isGrounded = controller.isGrounded;
        }
    }

    // Optional method to move an uncontrolled character towards a target position
    public virtual void MoveTowards(Vector3 targetPosition, float speed, bool run = false)
    {
        // Calculate direction towards the target position
        Vector3 direction = (targetPosition - transform.position).normalized;
        CalculateMovement(new Vector2(direction.x, direction.z), run);
    }
}
