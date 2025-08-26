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

    public Vector2 inputDirection;
    public bool wantsToJump;

    // ------------------------- METHODS -------------------------

    void Start() // Start is called before the first frame update
    {
        characterController = GetComponent<CharacterController>(); // Automatically references the CharacterController component
    }

    public override void HandleInput()
    {
        MoveControl();
        RunControl();
        JumpControl();

        CalculateMovement(inputDirection, isRunning);
        CalculateJump(wantsToJump);
    }

    public virtual void MoveControl() 
    {
        inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    public virtual void RunControl()
    {
        isRunning = Input.GetKey(KeyCode.LeftShift);
    }

    public virtual void JumpControl()
    {
        wantsToJump = Input.GetKeyDown(KeyCode.Space);
    }
}
