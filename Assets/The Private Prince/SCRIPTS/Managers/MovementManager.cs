using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovementManager : MonoBehaviour, IMoveable
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    public Animator animator;

    [Header("MOVEMENT COMPASS")]
    protected Vector3 moveDirection = Vector3.zero; // Placeholder for the character's movement direction
    protected float curSpeedX;
    protected float curSpeedY;
    protected float verticalVelocity;

    [Header("ATTRIBUTES")]
    public float walkingSpeed = 3.5f; // Speed at which the character walks
    public float runningSpeed = 8f; // Speed at which the character runs
    public float jumpPower = 8.0f; // Power of the character's jump
    public float gravityWeight = 20.0f;

    [Header("STATES")]
    public bool isRunning = false; // Determines if the character is running
    public bool canMove = true; // Determines if the character can move
    //public bool wantsToRun = false; // Determines if the character can run
    public bool isGrounded;

    // ------------------------- METHODS -------------------------

    public enum CharacterState { isIdle, isWalking, isRunning, isJumping }
    public CharacterState currentCharacState;

    public virtual void HandleInput() 
    {
        // Freezes the Characters
        if (!canMove)
        {
            moveDirection = Vector3.zero; // Instantly stop movement
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

    public virtual void CalculateMovement(Vector2 inputDirection, bool wantsToRun) 
    {
        // Calculates move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward); // This checks forrward and backward movement
        Vector3 right = transform.TransformDirection(Vector3.right); // This checks forward and backward movement

        // Condition for movement
        float targetSpeed = wantsToRun ? runningSpeed : walkingSpeed;
        curSpeedX = canMove ? targetSpeed * inputDirection.y : 0;  // Moves the character forward and backward
        curSpeedY = canMove ? targetSpeed * inputDirection.x : 0; // Moves the character left and right

        moveDirection = (forward * curSpeedX) + (right * curSpeedY); // Identifies what direction the character is moving
        isRunning = isRunning && inputDirection.magnitude > 0.1f;

        Debug.Log($"Move Direction: {moveDirection}"); // Logs the character's movement direction
    }

    public virtual void CalculateJump(bool wantsToJump)
    {
        if (wantsToJump && isGrounded)
        {
            verticalVelocity = jumpPower;
        }
        
        // Apply gravity
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

    public virtual void UpdateAnimations()
    {
        if (!isGrounded)
        {
            SetAnimationState(CharacterState.isJumping);
        }
        else
        {
            // Calculate horizontal movement magnitude (ignore Y axis for movement detection)
            Vector3 horizontalMove = new Vector3(moveDirection.x, 0, moveDirection.z);

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

    public virtual void SetAnimationState(CharacterState newState)
    {
        if (currentCharacState == newState) return;

        animator.SetBool(currentCharacState.ToString(), false);
        animator.SetBool(newState.ToString(), true);
        currentCharacState = newState;
    }

    public virtual void ApplyMovement(CharacterController controller)
    {
        if (controller != null)
        {
            controller.Move(moveDirection * Time.deltaTime);
            isGrounded = controller.isGrounded;
        }
    }

    public virtual void MoveTowards(Vector3 targetPosition, float speed, bool run = false)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        CalculateMovement(new Vector2(direction.x, direction.z), run);
    }
}
