// ------------------------- Camera + Player Controller Method -------------------------

using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug

[RequireComponent(typeof(CharacterController))] // Adds CharacterController component to the GameObject if it doesn't already have one

public class CharacterController3D : MovementManager
{
    // ------------------------- VARIABLES -------------------------

    [Header("COMPONENTS")]
    public CharacterController characterController; // Placeholder for the CharacterController component

    [Header("CAMERA NECK MOVEMENT")]
    public GameObject playerCamera; // Placeholder for the camera GameObject
    public float lookSpeed = 2.0f; // Mouse look sensitivity
    public float lookXLimit = 45.0f; // Maximum angle the camera can look up or down
    public float rotationX = 0; // Placeholder for the camera's X rotation

    public Vector2 inputDirection; // Placeholder for the character's input direction
    public bool wantsToJump; // Determines if the character wants to jump

    // ------------------------- METHODS -------------------------

    // Start is called before the first frame update
    void Start() 
    {
        characterController = GetComponent<CharacterController>(); // Automatically references the CharacterController component
    }

    // Handles player input for movements
    public override void HandleInput()
    {
        // Calls from this class (CharacterController3D)
        MoveControl();
        RunControl();
        JumpControl();

        // Calls from the parent class (MovementManager)
        CalculateMovement(inputDirection, isRunning);
        CalculateJump(wantsToJump);
    }

    // Gets player input for movement direction
    public virtual void MoveControl() 
    {
        inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    // Gets player input for running
    public virtual void RunControl()
    {
        isRunning = Input.GetKey(KeyCode.LeftShift);
    }

    // Gets player input for jumping
    public virtual void JumpControl()
    {
        wantsToJump = Input.GetKeyDown(KeyCode.Space);
    }
}
