// ------------------------- Camera + Player Controller Method -------------------------

using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using Unity.VisualScripting;
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug

[RequireComponent(typeof(CharacterController))] // Adds CharacterController component to the GameObject if it doesn't already have one

public class Advanced3DCharacterController : MonoBehaviour 
{
    // ------------------------- COMPONENTS -------------------------

    [Header("COMPONENTS")]
    public CharacterController characterController; // Placeholder for the CharacterController component
    public Animator animator; // Placholder for the Character's Animations
    public GameObject playerCamera; // Placeholder for the camera GameObject
    public float interactableRayDistance = 5f; // Distance for raycasting to interact with objects in the game world

    // ------------------------- MOVEMENT SETTINGS ------------------------

    [Header("CHARACTER MOVEMENT")]
    public float walkingSpeed = 3.5f; // Speed at which the character walks
    public float runningSpeed = 8f; // Speed at which the character runs
    public float jumpPower = 8.0f; // Power of the character's jump
    public float gravityWeight = 20.0f; // Gravity applied to the character
    public bool canMove = true; // Determines if the character can move or not (useful for disabling movement during certain actions)

    [Header("CAMERA NECK MOVEMENT")]
    [SerializeField] float lookSpeed = 2.0f; // Mouse look sensitivity
    [SerializeField] float lookXLimit = 45.0f; // Maximum angle the camera can look up or down

    [SerializeField] Vector3 moveDirection = Vector3.zero; // Placeholder for the character's movement direction
    [SerializeField] float rotationX = 0; // Placeholder for the camera's X rotation

    // -------------------------- STAMINA SYSTEM --------------------------

    [Header("STAMINA SETTINGS")]
    public float maxStamina = 100f; // Maximum stamina (set to 100)
    public float staminaDrainRate = 15f; // How fast stamina drains per second while running
    public float staminaRecoveryRate = 5f; // How fast stamina recovers per second when not running

    [SerializeField] float stamina = 100f; // Current/Reserved stamina (initialized to 100)
    [SerializeField] bool isExhausted = false; // Determines if the character is exhausted (stamina is 0 or less)
    //[SerializeField] int runCooldown = 0; // Placeholder for the cooldown variable
    
    // ---------------------------- ANIMATIONS -----------------------------

    private enum MovementState // Enum to define the character's movement animation states
    {
        Idle,
        Walking,
        Running,
        Jumping
    }

    // ------------------------- NETWORK VARIABLES -------------------------

    private Vector3 networkPosition; // Placeholder for the character's network position (if applicable)
    private Quaternion networkRotation; // Placeholder for the character's network rotation (if applicable)
    private float networkRotationX; // Placeholder for the character's network rotation on the X-axis (if applicable)
    private MovementState currentState = MovementState.Idle; // Placeholder for the character's current movement state (default is Idle)

    // ------------------------------ METHODS ---------------------------

    // Start is called before the first frame update
    void Start()
    {
        if (!characterController) characterController = GetComponent<CharacterController>(); // Automatically references the CharacterController component
        if (!animator) animator = GetComponent<Animator>(); // Automatically references the Animator component

        stamina = maxStamina; // Initialize default stamina to maximum value

        if (playerCamera) playerCamera.SetActive(true); // Activate the player camera if it exists
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump") && canMove && characterController.isGrounded)
        {
        }

        HandleCursorToggle();
        HandleMovement();
        HandleCamera();
        HandleRaycast();
    }

    // Method to toggle cursor visibility and lock state
    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Method to handle character movement, jumping, and gravity
    private void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward); // Forward direction relative to the character's orientation
        Vector3 right = transform.TransformDirection(Vector3.right); // Right direction relative to the character's orientation

        float speed = GetCurrentSpeed(); // Get the current speed based on input and stamina
        float curSpeedX = speed * Input.GetAxis("Vertical"); // Calculate current speed in the X direction based on vertical input
        float curSpeedY = speed * Input.GetAxis("Horizontal"); // Calculate current speed in the Y direction based on horizontal input
        float movementDirectionY = moveDirection.y; // Store the current Y movement direction (for jumping and gravity)

        moveDirection = (forward * curSpeedX) + (right * curSpeedY); // Calculate the movement direction based on input and speed
        moveDirection.y = HandleJumping(movementDirectionY); // Handle jumping and update the Y movement direction

        ApplyGravity();
        characterController.Move(moveDirection * Time.deltaTime); // Move the character based on the calculated movement direction and delta time
        UpdateStamina();
        UpdateAnimationState();
    }

    // Method to get the current speed based on input and stamina
    private float GetCurrentSpeed()
    {
        if (!canMove) return 0;
        bool tryRunning = Input.GetKey(KeyCode.LeftShift) && !isExhausted && stamina > 0; // Check if the player is trying to run
        return tryRunning ? runningSpeed : walkingSpeed; // Return running speed if running, otherwise return walking speed
    }

    // Method to handle jumping logic
    private float HandleJumping(float currentY)
    {
        if (Input.GetButtonDown("Jump") && canMove && characterController.isGrounded) // Check if the jump button is pressed, movement is allowed, and the character is grounded
        {
            currentState = MovementState.Jumping; // Set the current state to jumping
            return jumpPower; // Return the jump power to apply upward force
        }
        return currentY; // Return the current Y movement direction if not jumping
    }

    // Method to apply gravity to the character
    private void ApplyGravity()
    {
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravityWeight * Time.deltaTime; // Apply gravity if the character is not grounded
        }
    }

    // Method to update stamina based on running and recovery logic
    private void UpdateStamina()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && moveDirection.magnitude > 0.01f; // Check if the player is sprinting (running with movement input)    

        if (isSprinting && !isExhausted) 
        {
            stamina -= staminaDrainRate * Time.deltaTime; // Drain stamina while sprinting
            if (stamina <= 0) isExhausted = true; // Set exhausted state if stamina is 0 or less
        }
        else if (stamina < maxStamina)
        {
            stamina += staminaRecoveryRate * Time.deltaTime; // Recover stamina when not sprinting
            if (stamina >= maxStamina * 0.2f) isExhausted = false; // Reset exhausted state if stamina is above 20% of max stamina
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina); // Clamp stamina to ensure it stays within the range of 0 to maxStamina
    }

    // Method to update the character's animation state based on movement and jumping
    private void UpdateAnimationState()
    {
        if (!characterController.isGrounded)
        {
            currentState = MovementState.Jumping; // Set the current state to jumping if the character is not grounded
        }
        else if (moveDirection.magnitude > 0.01f)
        {
            currentState = Input.GetKey(KeyCode.LeftShift) ? MovementState.Running : MovementState.Walking; // Set the current state to running if sprinting, otherwise set it to walking
        }
        else
        {
            currentState = MovementState.Idle; // Set the current state to idle if there is no movement input
        }

        UpdateAnimator(); // Update the animator based on the current movement state
    }

    // Method to update the animator based on the current movement state
    private void UpdateAnimator() 
    {
        animator.SetBool("isIdle", currentState == MovementState.Idle); // Set the idle animation state
        animator.SetBool("isWalking", currentState == MovementState.Walking); // Set the walking animation state
        animator.SetBool("isRunning", currentState == MovementState.Running); // Set the running animation state
        animator.SetBool("isJumping", currentState == MovementState.Jumping); // Set the jumping animation state
    }

    // Method to handle camera movement and rotation
    private void HandleCamera()
    {
        if (!canMove) return;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed; // Adjust the camera's X rotation based on mouse input
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit); // Clamp the X rotation to prevent excessive looking up or down
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0); // Apply the clamped X rotation to the camera
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0); // Apply the Y rotation based on mouse input to the character's transform
    }

    // Method to synchronize the character's position, rotation, and movement state over the network
    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting) // If this PhotonView is owned by the local player
    //    {
    //        // Send the character's position, rotation, camera rotation, and current movement state over the network server
    //        stream.SendNext(transform.position);
    //        stream.SendNext(transform.rotation);
    //        stream.SendNext(rotationX);
    //        stream.SendNext((int)currentState);
    //    }
    //    else
    //    {
    //        // Receive the character's position, rotation, camera rotation, and current movement state from the network server
    //        networkPosition = (Vector3)stream.ReceiveNext();
    //        networkRotation = (Quaternion)stream.ReceiveNext();
    //        networkRotationX = (float)stream.ReceiveNext();
    //        currentState = (MovementState)stream.ReceiveNext();

    //        UpdateAnimator(); // Sync animations for remote players
    //    }
    //}

    // Method to smoothly synchronize the remote player's position and rotation
    private void SmoothSyncRemotePlayer()
    {
        // Smoothly interpolate the remote player's position and rotation
        transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
        transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10);
        playerCamera.transform.localRotation = Quaternion.Euler(networkRotationX, 0, 0);
    }

    // Method to handle raycasting for interactions with objects in the game worlds
    private void HandleRaycast()
    {
        //if (!photonView.IsMine || !Input.GetMouseButtonDown(0)) return; // Only allow raycasting if this PhotonView is owned by the local player and the left mouse button is pressed

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Create a ray from the camera to the center of the screen
        if (Physics.Raycast(ray, out RaycastHit hit, interactableRayDistance)) // Perform a raycast to check for objects within the specified distance
        {
            Debug.Log($"Hit: {hit.collider.name}");
            // Add interaction logic here
        }
    }
}