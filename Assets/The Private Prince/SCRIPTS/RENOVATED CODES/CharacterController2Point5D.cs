using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using TMPro;
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject, Transform, Vector3, etc.

using UnityEngine.InputSystem; // Grants access to Unity's new Input System for handling player inputs

[RequireComponent(typeof(CharacterController))] // Requires this GameObject to have a CharacterController component in order to function properly
public class CharacterController2Point5D : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // Reference to the PlayerInput component for handling new input system actions and controls
    private PrivatePrinceControls ppControls;

    [Header("OBJECT REFERENCES")]
    [SerializeField] private CharacterController characController; // Reference to the CharacterController component for controlling character movement
    [SerializeField] private Animator animatorController; // Reference to the Animator component for controlling character animations
    [SerializeField] private GameObject spriteRoot; // Reference to the root GameObject that contains the characer sprites for flipping their facing direction
    //[SerializeField] private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component for handling sprite rendering and flipping

    [Header("CHARACTER ATTRIBUTES")]
    //[SerializeField] private float horizontal; // Placeholder for horizontal movement inputs
    //[SerializeField] private float vertical; // Placeholder for vertical movement inputs
    [SerializeField] private float movementSpeed = 6f; // Speed at which the character moves

    [Space]

    [SerializeField] private float gravity = -9.81f; // Gravity force applied to the character (Earth based gravity)
    [SerializeField] private float gravityMultiplier = 3f; // Multiplier to increase the effect of gravity for a more grounded feel
    [SerializeField] private float freefallVelocity = 0f; // Current vertical velocity of the character for applying gravity and simulating freefall

    [Header("COMBAT ATTRIBUTES")]
    [SerializeField] private int attackDamage = 1; // Amount of damage dealt per attack
    [SerializeField] protected float attackCooldown = 0.25f; // Amount of time between each attack
    [SerializeField] protected float blockCooldown = 0f; // Amount of recovery time after blocking an attack

    [Header("INTERACTIONS")]
    [SerializeField] private Vector3 attackBoxCastSize = new Vector3(1f, 1f, 1f); // Defines the size of the attack box cast
    [SerializeField] private Vector3 interactionBoxSize = new Vector3(0.1f, 1f, 0.5f); // -joseph

    [SerializeField] protected LayerMask obstacleLayer; // Layer for obstacles that can block the cast
    [SerializeField] protected LayerMask exludedLayerMask; // Layer mask to filter unwanted targets

    [Space(8f)] // Adds spacing in the Inspector

    [SerializeField] private Transform raycastEmitter; // Point from which the raycast will be emitted
    [SerializeField] private float raycastLength = 2f; // Defines how long the raycast would be

    [Space(8f)] // Adds spacing in the Inspector

    [SerializeField] private GameObject interactIcon; // Icon that will pop up when near interactable object

    [Header("BOOLEANS")]
    public bool onCyberScan = false; // ...
    public bool inDialogue = false; // Indicates if the player is currently in a dialogue
    public bool canAttack = true; // Indicates if the player can perform an attack
    public bool canMove = true; // Indicates if the player can move
    public bool hasHit = false; // Indicates if the player has hit something with its attack
    public bool isBlocking = false; //  Indicates if the player is currently blocking an attack with their shield
    private bool wasBlocking = false; // Tracks previous block state to prevent continuous reset

    [Header("DIALOGUE")]
    [SerializeField] private DialogueUI dialogueUI; // Reference to the DialogueUI component for handling dialogues

    // Coroutine references to prevent multiple coroutines
    private Coroutine attackCoroutine;
    private Coroutine blockCoroutine;

    // ------------------------- UNITY METHODS -------------------------
    #region UNITY LOGICS

    // Awake is called before all frame updates
    private void Awake()
    {
        // Get required components if not assigned
        if (characController == null)
            characController = this.GetComponent<CharacterController>();

        if (animatorController == null)
            animatorController = this.GetComponentInChildren<Animator>();

        if (spriteRoot == null)
            Debug.LogWarning("Sprite Root was not set. Please assign the root GameObject that contains the character sprites to flip them according to the input direction.");

        // Validate raycast emitter
        if (raycastEmitter == null)
            Debug.LogWarning("Raycast Emitter is not assigned. Please assign a Transform for attack raycasts.");

        // Assigns the gameObject's "Player Input" component for the new input system to this script
        if (ppControls == null && GameplayInputManager.Instance != null)
        {
            // Accesses the controls from the PlayerInputManager singleton instance
            ppControls = GameplayInputManager.Instance.Controls;

            Debug.Log($"New Input System was set: {ppControls}");
        }
        else if (GameplayInputManager.Instance == null)
        {
            Debug.LogError("PlayerInputManager singleton not found! Make sure it exists in the scene.");
        }
    }

    private void OnEnable()
    {
        // Ensure subscriptions are active when object is enabled
        if (ppControls != null)
        {
            // Subscribes to the performed events
            ppControls.Player.Attack.performed += NewAttack;
            ppControls.Player.Block.performed += NewBlock;
            ppControls.Player.Block.canceled += OnBlockReleased;
        }

        // NOTE: MIGHT BE JUST THE TEMPORARY PLACEMENT 
        // Calls the method that sets the player to the saved checkpoint spawn
        ApplySpawn();
    }

    private void OnDisable()
    {
        // Clean up subscriptions when object is disabled
        if (ppControls != null)
        {
            // Subscribes to the performed events
            ppControls.Player.Attack.performed -= NewAttack;
            ppControls.Player.Block.performed -= NewBlock;
            ppControls.Player.Block.canceled -= OnBlockReleased;
        }
    }

    // Start is called once the script is loaded 
    private void Start()
    {
        //interactIcon.SetActive(false);

        // Initialize wasBlocking
        wasBlocking = false;
    }

    // Update is called once per frame
    private void Update()
    {
        // Early return if critical components are missing
        if (characController == null || animatorController == null) return;

        ApplyGravity();

        if (InDialogue())
            return;

        // Check block state only when it changes
        float currentBlockValue = ppControls?.Player.Block.ReadValue<float>() ?? 0f;
        if (currentBlockValue <= 0f && wasBlocking)
        {
            ResetCharacMood("Blocking");
            wasBlocking = false;
        }
        else if (currentBlockValue > 0f)
        {
            wasBlocking = true;
        }

        Move();
    }

    #endregion

    // ------------------------- INTERACTIONS -------------------------
    #region INTERACTION LOGICS

    // Method to check if the player is currently in dialogue
    public bool InDialogue()
    {
        if (inDialogue)
        {
            Debug.Log("Player in Dialogue!");

            // Stops the movement animation when in dialogue
            if (animatorController != null)
                animatorController.SetBool("isMoving", false);

            return true;
        }

        return false;
    }

    // ...
    public void InCyberScan()
    {
        // ...
        onCyberScan = !onCyberScan;

        // ...
        if (onCyberScan == true) 
        {
            // Disables Movements
            canAttack = false;
            isBlocking = false;

            // Slows down time
            Time.timeScale = 0.5f;

            return;
        }

        // Disables Movements
        canAttack = true;
        isBlocking = true;

        // Turn back normal time
        Time.timeScale = 1f;

        return;
    }

    public void OpenInteractableIcon() // - joseph
    {
        if (interactIcon != null)
            interactIcon.SetActive(true);
    }

    public void CloseInteractableIcon() // - joseph
    {
        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    private void CheckInteraction() // - joseph
    {
        Collider[] hits = Physics.OverlapBox(transform.position, interactionBoxSize / 2f, transform.rotation);

        if (hits.Length > 0)
        {
            foreach (Collider c in hits)
            {
                IObject interactable = c.transform.GetComponent<IObject>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }

    #endregion

    // ---------------------------- STATES -------------------------

    public void SetCharacMood(string mood)
    {
        switch (mood)
        {
            case "Blocking":
                isBlocking = true;

                // Safe check for IDamageable
                IDamageable thisDamageable = this.GetComponent<IDamageable>();
                if (thisDamageable != null)
                    thisDamageable.iVulnerable = false;

                canMove = false;
                canAttack = false;
                break;

            case "Attacking":
                canAttack = false;
                canMove = false;
                isBlocking = false;
                break;

            default:
                Debug.LogWarning($"Unknown mood set: {mood}");
                break;
        }
    }

    public void ResetCharacMood(string mood)
    {
        switch (mood)
        {
            case "Blocking":
                Debug.Log("Player block reset!");

                isBlocking = false;

                IDamageable thisDamageable = this.GetComponent<IDamageable>();
                if (thisDamageable != null)
                    thisDamageable.iVulnerable = true;

                canMove = true;
                canAttack = true;
                break;

            case "Attacking":
                Debug.Log("Player attack reset!");

                canAttack = true;
                canMove = true;
                isBlocking = false;
                break;

            case "Moving":
                Debug.Log("Player movement reset!");

                canMove = true;
                canAttack = true;
                isBlocking = false;
                break;

            default:
                Debug.LogWarning($"Unknown mood reset: {mood}");
                break;
        }
    }

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
        if (!canMove || ppControls == null) return;

        Debug.Log("Player is Moving!");

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
                        -Mathf.Abs(currentScale.x),
                        currentScale.y,
                        currentScale.z
                    );
                }
                else if (inputVector.x > 0f)
                {
                    // Flips the sprite root to face right
                    spriteRoot.transform.localScale = new Vector3(
                        Mathf.Abs(currentScale.x),
                        currentScale.y,
                        currentScale.z
                    );
                }
            }

            // Animates the character when moving
            AnimationSetbool("isMoving", true);
        }
        else
        {
            // Animates the character when NOT moving
            AnimationSetbool("isMoving", false);
        }

        // Applies movement to the character
        characController.Move(movement * movementSpeed * Time.deltaTime);
    }

    #endregion

    // ---------------------------- COMBATS ---------------------------
    #region COMBAT LOGICS

    public virtual void Attack()
    {
        if (!canAttack || inDialogue) return;

        Debug.Log("Player performed attack");

        SetCharacMood("Attacking");

        // Stop any existing attack coroutine
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);

        attackCoroutine = StartCoroutine(AttackSequence(attackCooldown));
    }

    public virtual void NewAttack(InputAction.CallbackContext context)
    {
        if (!canAttack || inDialogue || context.performed == false) return;

        // Calls the Animation that fires the attack animation
        AnimationSetbool("isAttacking", true);

        // Call Attack directly since we're using coroutine management there
        Attack();
    }

    // Coroutine for handling the attack sequence with delay and cooldown
    protected IEnumerator AttackSequence(float cooldown)
    {
        #region BOXCAST Detection Logic

        // Validate required components
        if (raycastEmitter == null || spriteRoot == null)
        {
            Debug.LogError("Raycast Emitter or Sprite Root is missing! Cannot perform attack.");
            yield break;
        }

        // Gets the half dimension of the full attack box size
        Vector3 halfExtents = attackBoxCastSize / 2f;

        // Sets the direction the character is facing
        bool isFacingLeft = spriteRoot.transform.localScale.x < 0f;
        Vector3 attackDirection = isFacingLeft ? Vector3.left : Vector3.right;
        Quaternion boxRotation = this.transform.rotation;

        // Variable to store information about what the BoxCast has hit
        RaycastHit hitInfo;

        // Perform the box cast
        if (Physics.BoxCast(
            raycastEmitter.transform.position,
            halfExtents,
            attackDirection,
            out hitInfo,
            boxRotation,
            raycastLength,
            ~exludedLayerMask
        ))
        {
            // Get components from hit object
            IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();
            IKnockable knockable = hitInfo.collider.GetComponent<IKnockable>();

            // Apply damage if possible
            if (damageable != null)
            {
                Debug.Log($"Enemy: {hitInfo.transform.name} HAS BEEN DAMAGED!");
                damageable.TakeDamage(attackDamage, false, this.transform);

                // Apply knockback if possible
                if (knockable != null)
                {
                    knockable.KnockBack(this.transform, hitInfo.transform);
                }
            }
        }

        // Visualizes the BoxCast in the Scene View for debugging
        DebugBoxCast.SimpleDrawBoxCast(raycastEmitter.transform.position, halfExtents, boxRotation, attackDirection, raycastLength, Color.red);

        #endregion

        // Cooldown duration before the player can attack again
        yield return new WaitForSeconds(cooldown);

        // Resets the attack animation state
        AnimationSetbool("isAttacking", false);

        // Resets the character's mood and states after the attack sequence is complete
        ResetCharacMood("Attacking");

        attackCoroutine = null;
    }

    // Method for Blocking Attacks Logic
    public virtual void Block()
    {
        Debug.Log("Player is Blocking!");

        if (isBlocking || inDialogue) return;

        SetCharacMood("Blocking");

        // Start block cooldown if needed
        if (blockCooldown > 0f)
        {
            if (blockCoroutine != null)
                StopCoroutine(blockCoroutine);
            blockCoroutine = StartCoroutine(BlockCooldownSequence(blockCooldown));
        }
    }

    public virtual void NewBlock(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Block();
    }

    private void OnBlockReleased(InputAction.CallbackContext context)
    {
        // This will be called when the block button is released
        ResetCharacMood("Blocking");
    }

    // Coroutine for handling the blocking sequence with delay and cooldown
    protected IEnumerator BlockCooldownSequence(float cooldown)
    {
        // Shielding Cooldown duration after blocking an attack
        yield return new WaitForSeconds(cooldown);

        // Reset block state
        IDamageable thisDamageable = this.GetComponent<IDamageable>();
        if (thisDamageable != null)
            thisDamageable.iVulnerable = true;

        isBlocking = false;
        canAttack = true;
        canMove = true;

        blockCoroutine = null;
    }

    #endregion

    // -------------------------- ANIMATIONS ---------------------------
    #region ANIMATION LOGICS

    // Method for Character Animation
    public void Animate(string animParamater, float inputValue, float transitionSmooth, float transitionCounter)
    {
        if (animatorController == null) return;
        animatorController.SetFloat(animParamater, inputValue, transitionSmooth, transitionCounter);
    }

    // Method for Character Animation with bool parameters
    public void AnimationSetbool(string paramaterName, bool boolState)
    {
        if (animatorController == null) return;
        animatorController.SetBool(paramaterName, boolState);
    }

    #endregion

    // ------------------------- MEMORY CLEANERS -------------------------

    private void OnDestroy()
    {
        if (ppControls != null)
        {
            // Unsubscribe from all events
            ppControls.Player.Attack.performed -= NewAttack;
            ppControls.Player.Block.performed -= NewBlock;
            ppControls.Player.Block.canceled -= OnBlockReleased;
        }

        // Stop all coroutines
        StopAllCoroutines();
    }
}