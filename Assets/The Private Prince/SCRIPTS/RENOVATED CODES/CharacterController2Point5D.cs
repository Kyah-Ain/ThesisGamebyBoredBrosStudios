using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using Unity.Burst.CompilerServices;
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject, Transform, Vector3, etc.

[RequireComponent(typeof(CharacterController))] // Requires this GameObject to have a CharacterController component in order to function properly

public class CharacterController2Point5D : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    [SerializeField] private CharacterController characController; // Reference to the CharacterController component for controlling character movement
    [SerializeField] private Animator animatorController; // Reference to the Animator component for controlling character animations
    [SerializeField] private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component for handling sprite rendering and flipping

    [Header("CHARACTER ATTRIBUTES")]
    [SerializeField] private float horizontal; // Placeholder for horizontal movement inputs
    [SerializeField] private float vertical; // Placeholder for vertical movement inputs
    [SerializeField] private float movementSpeed = 6f; // Speed at which the character moves

    [Header("COMBAT ATTRIBUTES")]
    [SerializeField] private int attackDamage = 1; // Amount of damage dealt per attack

    [Header("INTERACTIONS")]
    [SerializeField] private Vector3 attackBoxCastSize = new Vector3(0.5f, 0.5f, 0.5f); // Defines the size of the attack box cast
    [SerializeField] private Vector3 interactionBoxSize = new Vector3(0.1f, 1f, 0.5f); // -joseph

    [SerializeField] private LayerMask obstacleLayer; // Layer for obstacles that can block attacks

    [Space(8f)] // Adds spacing in the Inspector

    [SerializeField] private Transform raycastEmitter; // Point from which the raycast will be emitted
    [SerializeField] private float raycastLength = 2f; // Defines how long the raycast would be

    [Space(8f)] // Adds spacing in the Inspector

    [SerializeField] private GameObject interactIcon; // Icon that will pop up when near interactable object

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

            if (spriteRenderer != null) return;

            // Assigns the gameObject's "Sprite Renderer" automatically to this script
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
        if (Input.GetButton("Fire1"))
            Attack();

        // Interact Key
        if (Input.GetKeyUp(KeyCode.E))
            CheckInteraction();
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

    // ---------------------------- COMBATS ---------------------------
    #region COMBAT LOGICS

    // Handles raycasting for Interaction and Combat
    protected virtual void Attack()
    {
        Debug.Log("Player performed attack");

        // Gets the half dimension of the full attack box size
        Vector3 halfExtents = attackBoxCastSize / 2f;

        // Sets the direction the character is facing from the Sprite Renderer's flip logic
        bool isFacingLeft = spriteRenderer.flipX;

        // Gets the direction of which way the character should be attacking
        Vector3 attackDirection = isFacingLeft ? Vector3.left : Vector3.right ;

        // Sets the current rotation of the box to follow the character's rotation
        Quaternion boxRotation = this.transform.rotation;

        // Variable to store information about what the BoxCast has hit
        RaycastHit hitInfo;

        // Performs the BoxCast and stores whether it hit something or not (it only stores the first hit)
        bool hasHit = Physics.BoxCast(
            raycastEmitter.transform.position, // Starting Point
            halfExtents, // HALF the box dimensions
            attackDirection, // Direction on where to cast the box
            out hitInfo, // Information about what was hit
            boxRotation, // Current rotation of the box
            raycastLength // The max distance the boxCast could reach
        );

        // Evaluates if the BoxCast has hit something
        if (hasHit && hitInfo.collider.gameObject.CompareTag("Enemy")) 
        {
            // Transforms the hit object into a damageable object if it implements IDamageable
            IDamageable damageable = hitInfo.collider.GetComponent<IDamageable>();

            // Transforms the hit object into a knockable object if it implements IKnockable
            IKnockable knockable = hitInfo.collider.GetComponent<IKnockable>();

            // 
            if (damageable != null)
            {
                // ...
                damageable.TakeDamage(attackDamage);

                if (knockable != null)
                {
                    // Applies knockback to the target if it implements IKnockable
                    knockable.KnockBack(this.transform, hitInfo.transform);
                }
            }
        }

        // Visualizes the BoxCast in the Scene View for debugging (uses the static class from DebugBoxCastbyArian.cs)
        DebugBoxCast.SimpleDrawBoxCast(raycastEmitter.transform.position, halfExtents, boxRotation, attackDirection, raycastLength, Color.red);
    }

    #endregion

    // --------------------------- MOVEMENT ---------------------------
    #region MOVEMENT LOGICS

    // Method for Character Movement Logic
    public void Move()
    {
        // Get's the Horizontal & Vertical Value from Unity's Input System
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        // Computes for the direction by merging horizontal & vertical positions
        // - ".normalized" so that moving diagonally would make us not move faster
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Evaluates if there is a movement
        // ".magnitude" to compute for the distance 
        if (direction.magnitude >= 0.1f)
        {
            // Animates the character when moving
            //Animate("Input Magnitude", direction.magnitude, 0.05f, Time.deltaTime);

            // Controls the "Character Controller" of a Unity game object
            characController.Move(direction * movementSpeed * Time.deltaTime);

            // Determines the direction the character is facing
            if (horizontal < 0f)
            {
                // Flips the sprite to face left if the horizontal input is negative
                spriteRenderer.flipX = true;
            }
            else if (horizontal > 0f)
            {
                // Resets the sprite to face right if the horizontal input is positive 
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            // Animates the character when NOT moving
            //Animate("Input Magnitude", 0f, 0.05f, Time.deltaTime);
        }
    }

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