// ------------------------- Camera + Player Controller Method -------------------------

using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug

[RequireComponent(typeof(CharacterController))] // Adds CharacterController component to the GameObject if it doesn't already have one

public class CharacterController3D : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("COMPONENTS")]
    CharacterController characterController; // Placeholder for the CharacterController component
    public Animator animator; // Placholder for the Character's Animations

    [Header("ATTRIBUTES")]
    public float walkingSpeed = 3.5f; // Speed at which the character walks
    public float runningSpeed = 8f; // Speed at which the character runs
    public float jumpPower = 8.0f; // Power of the character's jump

    [Header("STAMINA SETTINGS")]
    public float stamina = 100f; // Current/Reserved stamina (initialized to 100)
    public float maxStamina = 100f; // Maximum stamina (set to 100)
    public float staminaDrainRate = 15f; // How fast stamina drains per second while running
    public float staminaRecoveryRate = 5f; // How fast stamina recovers per second when not running

    [Header("ENVIRONMENT INTERACTIONS")]
    public float gravityWeight = 20.0f; // Gravity applied to the character
    public float interactableRayDistance =5f; // How far the raycast will travel

    [Header("CAMERA NECK MOVEMENT")]
    public GameObject playerCamera; // Placeholder for the camera GameObject
    Vector3 moveDirection = Vector3.zero; // Placeholder for the character's movement direction

    public float lookSpeed = 2.0f; // Mouse look sensitivity
    public float lookXLimit = 45.0f; // Maximum angle the camera can look up or down
    float rotationX = 0; // Placeholder for the camera's X rotation

    [Header("BOOLEANS")]
    [HideInInspector] // Hides the variable from the Inspector but still allows it to be access on other scripts
    public bool canMove = true; // Determines if the character can move
    public bool isExhausted = false; // Determines if the character is exhausted (stamina is 0 or less)
    int runCooldown = 0; // Placeholder for the cooldown variable

    public bool isIdle; // Determines if the character is idle
    public bool isWalking; // Determines if the character is running
    public bool isRunning; // Determines if the character is running
    public bool isJumping; // Determines if the character is jumping

    public bool isPunching;
    public bool isBlocking; 

    // ------------------------- METHODS -------------------------
    void Start() // Start is called before the first frame update
    {
        characterController = GetComponent<CharacterController>(); // Automatically references the CharacterController component

        Cursor.lockState = CursorLockMode.Locked; // Locks the cursor
        Cursor.visible = false; // Hides the cursor 
    }

    void Update() // Update is called once per frame
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Accepts "Esc" key input from the keyboard
        {
            Cursor.lockState = CursorLockMode.None; // Unlocks the cursor
            Cursor.visible = true; // Shows the cursor
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // Locks the cursor
            Cursor.visible = false; // Hides the cursor
        }

        // Calculates move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward); // This checks forrward and backward movement
        Vector3 right = transform.TransformDirection(Vector3.right); // This checks forward and backward movement
        
        // Condition for movement
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;  // Moves the character forward and backward
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0; // Moves the character left and right
        float movementDirectionY = moveDirection.y; // Placeholder for the character's y-axis movement
        moveDirection = (forward * curSpeedX) + (right * curSpeedY); // Identifies what direction the character is moving

        Debug.Log($"Move Direction: {moveDirection}"); // Logs the character's movement direction

        if (moveDirection.magnitude > 0.01f)
        {
            // For updating the animator
            if (Input.GetKey(KeyCode.LeftShift) && stamina > 0 && !isExhausted) // Accepts "Left Shift" key input from the keyboard
            {
                isIdle = false; // Returns false if the character is not idle
                isRunning = true; // Returns true if the Left Shift key is pressed
                isWalking = false; // Returns false if the Left Shift key is not pressed
                HandleAnimation(); // Calls the HandleAnimation function

                stamina -= staminaDrainRate * Time.deltaTime; // Decreases the stamina over time while running
            }
            else
            {
                isIdle = false; // Returns false if the character is not idle
                isRunning = false; // Returns false if the Left Shift key is not pressed
                isWalking = true; // Returns true if the Left Shift key is pressed
                HandleAnimation(); // Calls the HandleAnimation function

                if (stamina < maxStamina)
                {
                    if (runCooldown == 0 && stamina <= 1)
                    {
                        isExhausted = true; // Returns false if the character is not exhausted
                        runCooldown = 1; // Increments the cooldown variable
                    }
                    stamina += staminaRecoveryRate * Time.deltaTime; // Increases the stamina over time while not running
                }

                if (Input.GetKeyUp(KeyCode.LeftShift))
                {
                    isExhausted = false; // Returns false after releasing from run
                    runCooldown = 0; // Resets the cooldown variable    
                }
            }

            // Ensure stamina stays between 0 and maxStamina
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        }
        else
        {
            isIdle = true; // Returns true if the character is idle
            isWalking = false; // Returns false if the character is not walking
            isRunning = false; // Returns false if the character is not running
            HandleAnimation();

            if (stamina < maxStamina)
            {
                stamina += staminaRecoveryRate * Time.deltaTime; // Increases the stamina over time while not running
            }
        }

        if (canMove && characterController.isGrounded) 
        {
            // Condition for jumping
            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpPower; // Makes the character jump
                isJumping = true; // Returns true if the character is jumping
                HandleAnimation(); // Calls the HandleAnimation function
            }
            else
            {
                moveDirection.y = movementDirectionY; // Keeps the character grounded
                isJumping = false; // Returns false if the character is jumping
                HandleAnimation(); // Calls the HandleAnimation function
            }
        }

        if (!characterController.isGrounded) 
        {
            // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
            // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
            // as an acceleration (ms^-2)
            moveDirection.y -= gravityWeight * Time.deltaTime; // Applies gravity to the character
        }

        if (Input.GetMouseButtonDown(0))
        {
            isPunching = true;
            isBlocking = false;
            HandleAnimation(); // Calls the HandleAnimation function
        }
        else if (Input.GetMouseButton(1))
        {
            isBlocking = true;
            isPunching = false;
            HandleAnimation(); // Calls the HandleAnimation function
        }
        else 
        {
            isBlocking = false;
            isPunching = false;
            HandleAnimation(); // Calls the HandleAnimation function
        }

            // Controller Movement
            characterController.Move(moveDirection * Time.deltaTime); // Moves the character based on the moveDirection

        // Camera Movement
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed; // Mouse y-axis movement sensitivity
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit); // Sets the maximum angle the camera can look up or down
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0); // Rotates the camera based on the mouse's y-axis movement

            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0); // Rotates the character based on the mouse's x-axis movement
        }

        PerformRaycast(); // Calls the PerformRaycast function
    }

    void HandleAnimation() // Handles the character's animations
    {
        animator.SetBool("isIdle", isIdle); // Sets the character's idle animation
        animator.SetBool("isWalking", isWalking); // Sets the character's walking animation
        animator.SetBool("isRunning", isRunning); // Sets the character's running animation

        animator.SetBool("isPunching", isPunching);
        animator.SetBool("isBlocking", isBlocking);
        //animator.SetBool("isJumping", isJumping); // Sets the character's jumping animation
    }

    void PerformRaycast() // Performs a raycast from the camera's center
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // Creates a ray from the camera's center

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
        Debug.DrawRay(ray.origin, ray.direction * interactableRayDistance, Color.red); // Draws the raycast in the Scene view
    }
}
