using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core features like Datatypes, DateTime, Math, and Debug

public class Player2Point5D : CharacterController3D
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    public GameObject raycastEmitter; // The game object that will emits the raycast
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component of the character for flipping the sprite

    [Header("STAMINA ENERGY")]
    public float stamina = 100f; // Current/Reserved stamina (initialized to 100)
    public float maxStamina = 100f; // Maximum stamina (set to 100)
    public float staminaDrainRate = 15f; // How fast stamina drains per second while running
    public float staminaRecoveryRate = 3f; // How fast stamina recovers per second when not running

    [Header("REGULATORS")]
    private bool isFacingRight = true; // For flipping the 2D Character (Left or Right)
    public bool isExhausted = false; // Determines if the character is exhausted (stamina is 0 or less)
    float exhaustionTimer = 0f; // Timer to manage exhaustion duration

    [Header("INTERACTABLES")]
    public float interactRaycast = 5f; // Defines how long the raycast would be
    public LayerMask hitLayers; // Defines what only can be interacted with the raycast

    [Header("DIALOGUE")]
    [SerializeField] private DialogueUI dialogueUI;

    // ------------------------- METHODS -------------------------

    // Getter for dialogue UI
    public DialogueUI DialogueUI => dialogueUI;
    public IInteractable Interactable { get; set; }

    // Start is called before the first frame update
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Automatically references the SpriteRenderer component
    }

    // Update is called once per frame
    private void Update() 
    {
        //stops player from moving when in Dialogue
        if(dialogueUI != null && dialogueUI.IsOpen) return;

        // Calls from the parent class (CharacterController3D)
        HandleInput();

        // Calls from the parent class (MovementManager)
        UpdateAnimations();
        ApplyMovement(characterController);

        // Calls from this class (Player2Point5D)
        HandleFlip();
        HandleStamina();
        HandleRaycast();

        // Button prompt for Dialogue Interaction
        if(Input.GetKeyDown(KeyCode.E))
        {
            Interactable?.Interact(this); // Used null propagation for less lines
        }
     
    }

    // Handles player input for movements
    public override void HandleInput()
    {
        // Only allow running if not exhausted and has stamina left
        if (!isExhausted && stamina > 0)
        {
            // Calls from the parent class (CharacterController3D)
            base.RunControl();
        }
        else 
        {
            isRunning = false;
        }

        // Calls from the parent class (CharacterController3D)
        MoveControl();
        JumpControl();

        CalculateMovement(inputDirection, isRunning);
        CalculateJump(wantsToJump);
    }

    // Updates animation's face direction based on Movement States
    void HandleFlip()
    {
        // Get horizontal input equavalent to left and right arrow keys or A and D keys
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

    // Manages stamina drain and recovery
    void HandleStamina()
    {
        // Determine if stamina should drain or recover
        if (isRunning)
        {
            stamina -= staminaDrainRate * Time.deltaTime;
        }
        else if (stamina < maxStamina)
        {
            stamina += staminaRecoveryRate * Time.deltaTime;
        }

        // Lock stamina value between 0 and maxStamina
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);

        // Handle exhaustion cooldown
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

    // Handles raycasting for Interaction and Combat
    void HandleRaycast()
    {
        // Establishes the raycast's origin and direction
        Vector3 rayOrigin = raycastEmitter.transform.position;
        Vector3 rayDirection = isFacingRight ? Vector3.right : Vector3.left;

        // Creates the ray and visualizes it in the Scene view
        Ray interactionRay = new Ray(rayOrigin, rayDirection);
        Debug.DrawRay(rayOrigin, rayDirection * interactRaycast, Color.blue); // Visualizes the laser in the Unity Scene 
        //Debug.Log("Raycast has been established");

        // Checks if the ray hits an object within the specified distance and layers
        if (Physics.Raycast(interactionRay, out RaycastHit hitInfo, interactRaycast, hitLayers))
        {
            Debug.Log($"Trying to interact with: {hitInfo.collider.name}");

            // Traverse the hit object to find an IDamageable component
            IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();

            // If an IDamageable component is found, apply damage when the Fire1 button is pressed
            if (damageable != null & Input.GetButtonDown("Fire1"))
            {
                Debug.Log("Player damaged the player");

                damageable.TakeDamage(10);
            }
        }
    } 
}