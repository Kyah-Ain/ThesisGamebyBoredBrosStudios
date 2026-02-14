using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject, Transform, Vector3, etc.

[RequireComponent(typeof(CharacterController))] // Requires this GameObject to have a CharacterController component in order to function properly

public class CharacterController2Point5D : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    [SerializeField] private CharacterController characController; // Reference to the CharacterController component for controlling character movement
    [SerializeField] private Animator animatorController; // Reference to the Animator component for controlling character animations
    [SerializeField] private GameObject spriteRoot; // Reference to the root GameObject that contains the characer sprites for flipping their facing direction
    //[SerializeField] private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component for handling sprite rendering and flipping

    [Header("CHARACTER ATTRIBUTES")]
    [SerializeField] private float horizontal; // Placeholder for horizontal movement inputs
    [SerializeField] private float vertical; // Placeholder for vertical movement inputs
    [SerializeField] private float movementSpeed = 6f; // Speed at which the character moves

    [Space]

    [SerializeField] private float gravity = -9.81f; // Gravity force applied to the character (Earth based gravity)
    [SerializeField] private float gravityMultiplier = 3f; // Multiplier to increase the effect of gravity for a more grounded feel
    [SerializeField] private float freefallVelocity = 0f; // Current vertical velocity of the character for applying gravity and simulating freefall

    [Header("COMBAT ATTRIBUTES")]
    [SerializeField] private int attackDamage = 1; // Amount of damage dealt per attack
    [SerializeField] protected float attackCooldown = 0.25f; // Amount of time between each attack
    //[SerializeField] protected float blockCooldown = 10f; // Amount of recovery time after blocking an attack

    [Header("INTERACTIONS")]
    [SerializeField] private Vector3 attackBoxCastSize = new Vector3(0.5f, 0.5f, 0.5f); // Defines the size of the attack box cast
    [SerializeField] private Vector3 interactionBoxSize = new Vector3(0.1f, 1f, 0.5f); // -joseph

    [SerializeField] protected LayerMask obstacleLayer; // Layer for obstacles that can block the cast
    [SerializeField] protected LayerMask exludedLayerMask; // Layer mask to filter unwanted targets

    [Space(8f)] // Adds spacing in the Inspector

    [SerializeField] private Transform raycastEmitter; // Point from which the raycast will be emitted
    [SerializeField] private float raycastLength = 2f; // Defines how long the raycast would be

    [Space(8f)] // Adds spacing in the Inspector

    [SerializeField] private GameObject interactIcon; // Icon that will pop up when near interactable object

    [Header("BOOLEANS")]
    [SerializeField] protected bool canAttack = true; // Indicates if the player can perform an attack
    [SerializeField] protected bool canMove = true; // Indicates if the player can move
    [SerializeField] protected bool hasHit = false; // Indicates if the player has hit something with its attack
    [SerializeField] protected bool isBlocking = false; // ...

    [Header("DIALOGUE")]
    [SerializeField] private DialogueUI dialogueUI; // Reference to the DialogueUI component for handling dialogues

    // Getter for dialogue UI
    public DialogueUI DialogueUI => dialogueUI;
    public IInteractable Interactable { get; set; }

    // ------------------------- UNITY METHODS -------------------------
    #region UNITY LOGICS

    // Awake is called before all frame updates
    private void Awake()
    {
        // Evaluates if there's no existing "Character Controller" component on the object
        if (characController == null)
        {
            // Stops player from moving when in Dialogue
            if (dialogueUI != null && dialogueUI.IsOpen) return;

            Debug.Log($"Character Controller was set: {characController}");

            if (characController != null) return;

            // Assigns the gameObject's "Character Controller" autmatically to this script
            characController = GetComponent<CharacterController>();

            if (animatorController != null) return;

            // Assigns the gameObject's "Animator Controller" automatically to this script
            animatorController = GetComponent<Animator>();

            if (spriteRoot != null) return;
            Debug.LogWarning("Sprite Root was not set. Please assign the root GameObject that contains the character sprites to flip them according to the input direction.");

            //if (spriteRenderer != null) return;

            //// Assigns the gameObject's "Sprite Renderer" automatically to this script
            //spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        else
        {
            Debug.LogError("ASSIGN A CHARACTER CONTROLLER FIRST BEFORE USING THIS SCRIPT");
        }
    }

    // Start is called once the script is loaded 
    private void Start()
    {
        //interactIcon.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        //stops player from moving when in Dialogue
        if (dialogueUI != null && dialogueUI.IsOpen) return;

        // Calls the method that handles character movement
        Move();

        // Simple Statement for Attack Key
        if (Input.GetButtonDown("Fire1"))
            Attack();

        // Interact Key
        if (Input.GetKeyUp(KeyCode.E))
            CheckInteraction();

        // Simple Statement for Block Key
        if (Input.GetButton("Fire2")) 
        {
            Block();
        }
        else if (isBlocking)
        {
            // ...
            IDamageable thisDamageable = this.GetComponent<IDamageable>();

            // ...
            thisDamageable.iVulnerable = true;

            // ...
            isBlocking = false;

            // ...
            canAttack = true;
            canMove = true;
        }
    }

    #endregion

    // ------------------------- INTERACTIONS -------------------------
    #region INTERACTION LOGICS

    public void OpenInteractableIcon() // - joseph
    {
        interactIcon.SetActive(true);
    }

    public void CloseInteractableIcon() // - joseph
    {
        interactIcon.SetActive(false);
    }

    private void CheckInteraction() // - joseph
    {
        Collider[] hits = Physics.OverlapBox(transform.position, interactionBoxSize / 2f, transform.rotation);

        if (hits.Length > 0)
        {
            foreach (Collider c in hits)
            {
                if (c.transform.GetComponent<IObject>())
                {
                    c.transform.GetComponent<IObject>().Interact();
                }
            }
        }
    }

    #endregion

    // --------------------------- MOVEMENT ---------------------------
    #region MOVEMENT LOGICS

    // Method for Character Movement Logic
    public void Move()
    {
        if (characController.isGrounded)
        {
            freefallVelocity = -1.0f; // Resets vertical velocity when grounded
        }

        freefallVelocity += gravity * gravityMultiplier * Time.deltaTime;

        if (!canMove) return;

        // Get's the Horizontal & Vertical Value from Unity's Input System
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        // Computes for the direction by merging horizontal & vertical positions
        // - ".normalized" so that moving diagonally would make us not move faster
        Vector3 direction = new Vector3(horizontal, freefallVelocity, vertical).normalized;

        // Evaluates if there is a movement
        // ".magnitude" to compute for the distance 
        if (direction.magnitude >= 0.1f)
        {
            // Animates the character when moving
            //Animate("Input Magnitude", direction.magnitude, 0.05f, Time.deltaTime);

            // Controls the "Character Controller" of a Unity game object
            characController.Move(direction * movementSpeed * Time.deltaTime);

            // Gets a reference to the current scale of the sprite root
            Vector3 currentScale = spriteRoot.transform.localScale;

            // Determines the direction the character is facing
            if (horizontal < 0f)
            {
                // Flips the sprite root to face left by negating the x scale
                spriteRoot.transform.localScale = new Vector3(
                    -Mathf.Abs(currentScale.x), // Multiplies the absolute value of the current x scale by -1 to flip it
                    currentScale.y, // Keeps the current y scale unchanged
                    currentScale.z // Keeps the current z scale unchanged
                );

                // Flips the sprites to face left if the horizontal input is negative
                //spriteRenderer.flipX = true;
            }
            else if (horizontal > 0f)
            {
                // Flips the sprite root to face right by positivizing the x scale
                spriteRoot.transform.localScale = new Vector3(
                    Mathf.Abs(currentScale.x), // Multiplies the absolute value of the current x scale by 1 to re-flip it back to normal
                    currentScale.y,
                    currentScale.z
                );

                // Resets the sprite to face right if the horizontal input is positive 
                //spriteRenderer.flipX = false;
            }
        }
        else
        {
            // Animates the character when NOT moving
            //Animate("Input Magnitude", 0f, 0.05f, Time.deltaTime);
        }
    }

    #endregion

    // ---------------------------- COMBATS ---------------------------
    #region COMBAT LOGICS

    // Handles raycasting for Interaction and Combat
    public virtual void Attack()
    {
        if (!canAttack) return;

        Debug.Log("Player performed attack");

        // Prevents further attacks until cooldown is over
        canAttack = false;

        // Prevents movement during attack
        canMove = false;

        // Calls the coroutine that handles the attack sequence
        StartCoroutine(AttackSequence(attackCooldown));
    }

    // Coroutine for handling the attack sequence with delay and cooldown
    protected IEnumerator AttackSequence(float cooldown)
    {
        #region BOXCAST Detection Logic...
        // Gets the half dimension of the full attack box size
        Vector3 halfExtents = attackBoxCastSize / 2f;

        // Sets the direction the character is facing from the spriteRoots' flip logic
        bool isFacingLeft = spriteRoot.transform.localScale.x < 0f;

        // Sets the direction the character is facing from the Sprite Renderer's flip logic
        //bool isFacingLeft = spriteRenderer.flipX;

        // Gets the direction of which way the character should be attacking
        Vector3 attackDirection = isFacingLeft ? Vector3.left : Vector3.right;

        // Sets the current rotation of the box to follow the character's rotation
        Quaternion boxRotation = this.transform.rotation;

        // Variable to store information about what the BoxCast has hit
        RaycastHit hitInfo;

        if (Physics.BoxCast(
            raycastEmitter.transform.position, // Starting Point
            halfExtents, // HALF the box dimensions
            attackDirection, // Direction on where to cast the box
            out hitInfo, // Information about what was hit
            boxRotation, // Current rotation of the box
            raycastLength, // The max distance the boxCast could reach
            ~exludedLayerMask // Layer Mask to filter unwanted targets
        ))
        {
            // Transforms the hit object into a damageable object if it implements IDamageable
            IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();

            // Transforms the hit object into a knockable object if it implements IKnockable
            IKnockable knockable = hitInfo.collider.GetComponent<IKnockable>();

            // Checks if the hit object can take damage
            if (damageable != null)
            {
                Debug.Log($"Enemy: {hitInfo.transform.name} HAS BEEN DAMAGED!");

                // Apply attack damage
                damageable.TakeDamage(attackDamage, false, this.transform);

                // Applies knockback to the target if it implements IKnockable
                knockable.KnockBack(this.transform, hitInfo.transform);
            }
        }

        // Visualizes the BoxCast in the Scene View for debugging (uses the static class from DebugBoxCastbyArian.cs)
        DebugBoxCast.SimpleDrawBoxCast(raycastEmitter.transform.position, halfExtents, boxRotation, attackDirection, raycastLength, Color.red);

        #endregion

        // Cooldown duration before the player can attack again
        yield return new WaitForSeconds(cooldown);

        // Resets the ability to move and attack
        canAttack = true;
        canMove = true;
    }

    // Method for Blocking Attacks Logic
    public virtual void Block()
    {
        // ...
        canMove = false;
        canAttack = false;
        isBlocking = true;

        //Collider thisCollider = this.gameObject.GetComponent<Collider>();
        IDamageable thisDamageable = this.GetComponent<IDamageable>();

        // ...
        thisDamageable.iVulnerable = false;
    }

    //// Coroutine for handling the blocking sequence with delay and cooldown
    //protected IEnumerator BlockSequence(IDamageable damageable, float cooldown)
    //{
    //    // Shileding Cooldown duration after blocking an attack
    //    yield return new WaitForSeconds(cooldown);

    //    // ...
    //    damageable.iBlock = true;
    //}

    #endregion

    // -------------------------- ANIMATIONS ---------------------------
    #region ANIMATION LOGICS

    // Method for Character Animation
    public void Animate(string animParamater, float inputValue, float transitionSmooth, float transitionCounter)
    {
        // - ("Name of the Animation Parameter", player.input value, transition smoothness, counter)
        animatorController.SetFloat(animParamater, inputValue, transitionSmooth, transitionCounter);
    }

    #endregion
}