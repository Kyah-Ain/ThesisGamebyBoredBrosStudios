// ------------------------- Camera + Player Controller Method -------------------------

using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug

[RequireComponent(typeof(CharacterController))] // Adds CharacterController component to the GameObject if it doesn't already have one

public class CharacterController3D : MovementManager
{
    // ------------------------- VARIABLES -------------------------
    [Header("COMPONENTS")]
    CharacterController characterController; // Placeholder for the CharacterController component

    [Header("STAMINA SETTINGS")]
    public float stamina = 100f; // Current/Reserved stamina (initialized to 100)
    public float maxStamina = 100f; // Maximum stamina (set to 100)
    public float staminaDrainRate = 15f; // How fast stamina drains per second while running
    public float staminaRecoveryRate = 5f; // How fast stamina recovers per second when not running
    
    [Header("CAMERA NECK MOVEMENT")]
    public GameObject playerCamera; // Placeholder for the camera GameObject
    public float lookSpeed = 2.0f; // Mouse look sensitivity
    public float lookXLimit = 45.0f; // Maximum angle the camera can look up or down
    float rotationX = 0; // Placeholder for the camera's X rotation

    [Header("BOOLEANS")]
    public bool isExhausted = false; // Determines if the character is exhausted (stamina is 0 or less)
    float exhaustionTimer = 0f;

    [Header("INTERACTION")]
    public float interactableRayDistance = 5f; // How far the raycast will travel

    // ------------------------- METHODS -------------------------
    void Start() // Start is called before the first frame update
    {
        characterController = GetComponent<CharacterController>(); // Automatically references the CharacterController component
    }
    
    void Update() // Update is called once per frame
    {
        HandleInput();
        NeckMovement();
        UpdateStamina();
        ApplyMovement(characterController);
        UpdateAnimations();
        PerformRaycast();
    }
    void HandleInput()
    {
        Vector2 inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && !isExhausted && stamina > 0;
        bool wantsToJump = Input.GetKeyDown(KeyCode.Space);

        CalculateMovement(inputDirection, wantsToRun);
        CalculateJump(wantsToJump);
    }

    public void NeckMovement() 
    {
        // Camera Movement
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed; // Mouse y-axis movement sensitivity
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit); // Sets the maximum angle the camera can look up or down
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0); // Rotates the camera based on the mouse's y-axis movement

            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0); // Rotates the character based on the mouse's x-axis movement
        }
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


    void PerformRaycast() // Performs a raycast from the camera's center
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // Creates a ray from the camera's center
        Debug.DrawRay(ray.origin, ray.direction * interactableRayDistance, Color.red); // Draws the raycast in the Scene view

        if (Input.GetMouseButtonDown(0)) // Accepts left mouse button input
        {
            if (Physics.Raycast(ray, out RaycastHit hit, interactableRayDistance)) // Checks if the raycast hits an object
            {
                if (hit.collider.CompareTag("NPC")) // Checks if the raycast hits an object with the "NPC" tag
                {
                    Debug.Log($"Hit {hit.collider.name} that has a collider TAG");
                }
                else // Checks if the raycast hits an object with a NPC script instead
                {
                    Debug.Log($"Hit {hit.collider.name} that has a collider SCRIPT");
                    //hit.collider.GetComponent<NPC>()?.Interact(); // Calls the Interact function from the NPC script if the raycast hits an NPC
                }
            }
        }
    }
}
