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
    [SerializeField] private SpriteRenderer spriteRenderer; // ...

    //[SerializeField] private GameObject attackBox; // ...
    [SerializeField] private Collider attackBox; // ...

    [Header("CHARACTER ATTRIBUTES")]
    [SerializeField] private float horizontal; // ...
    [SerializeField] private float vertical; // ...
    [SerializeField] private float movementSpeed = 6f; // Speed at which the character moves

    [Header("INTERACTIONS")]
    //[SerializeField] private Transform raycastEmitter; // Point from which the raycast will be emitted
    [SerializeField] private float raycastLength = 2f; // Defines how long the raycast would be

    [SerializeField] private GameObject interactIcon; // Icon that will pop up when near interactable object

    [Header("COMBAT STATS")]
    [SerializeField] private Bounds attackBounds; // Defines the bounds of the forward punch attack
    [SerializeField] private LayerMask obstacleLayer; // Layer for obstacles that can block attacks
    [SerializeField] private LayerMask enemyLayer; // Layer for enemies
    //[SerializeField] private float punchRadius = 1f; // Radius of the forward punch attack

    private Vector3 boxSize = new Vector3(0.1f, 1f, 0.5f); // -joseph

    [Header("BOOLEANS")]
    [SerializeField] private bool isFacingRight = true;

    // ------------------------- UNITY METHODS -------------------------

    // Awake is called before all frame updates
    private void Awake()
    {
        // Evaluates if there's no existing "Character Controller" component on the object
        if (characController == null)
        {
            Debug.Log($"Character Controller was set: {characController}");

            if (characController != null) return;

            // Assigns the gameObject's "Character Controller" autmatically to this script
            characController = GetComponent<CharacterController>();

            if (animatorController != null) return;

            // Assigns the gameObject's "Animator Controller" automatically to this script
            animatorController = GetComponent<Animator>();

            if (spriteRenderer != null) return;

            // ...
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
        interactIcon.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        // Calls the method that handles character movement
        Move();

        // TEST (SHOULD BE REMOVED SOON)
        Attack();

        // Interact Key
        if (Input.GetKeyUp(KeyCode.E))
            CheckInteraction();
    }

    // ------------------------- INTERACTIONS -------------------------

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
        Collider[] hits = Physics.OverlapBox(transform.position, boxSize / 2f, transform.rotation);

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

    // Handles raycasting for Interaction and Combat
    protected virtual void Attack()
    {
        //// ...
        //bool isFacingRight = !spriteRenderer.flipX;

        //// Establishes the raycast's origin and direction
        //Vector3 rayOrigin = raycastEmitter.transform.position;
        //Vector3 rayDirection = isFacingRight ? Vector3.right : Vector3.left;

        //// Creates the ray and visualizes it in the Scene view
        //Ray interactionRay = new Ray(rayOrigin, rayDirection);
        //Debug.DrawRay(rayOrigin, rayDirection * interactRaycast, Color.blue);

        //// Get CombatMomentum reference at the start
        //CombatMomentum playerCombatMomentum = GetComponent<CombatMomentum>();

        // ...
        Vector3 attackDirection = Vector3.left;

        //// ...
        //if (spriteRenderer.flipX)
        //{
        //    // Left Attack
        //    attackDirection = Vector3.left;
        //}
        //else
        //{
        //    // Right Attack
        //    attackDirection = Vector3.right;
        //}

        Debug.Log("Player performed attack");

        //  ...
        attackBounds = attackBox.bounds;

        // ...
        Vector3 halfExtents = attackBounds.extents;

        //// Position attack box based on direction
        //attackBox.transform.position = transform.position + attackDirection * raycastLength;

        // ...
        if (Physics.BoxCast(this.transform.position, halfExtents, attackDirection, out RaycastHit hitInfo, this.transform.rotation, 10f))
        {
            Debug.Log($"Custom hitbox detected: {hitInfo.collider.name}");
        }

        //// Attacks when Mouse Left Click button is pressed (with or without hitting something)
        //if (Input.GetButtonDown("Fire1"))
        //{
        //    Debug.Log("Player performed attack");

        //    //  ...
        //    attackBounds = attackBox.bounds;

        //    // ...
        //    Vector3 halfExtents = attackBounds.extents;

        //    if (Physics.BoxCast(this.transform.position, halfExtents, this.transform.forward, out RaycastHit hitInfo, this.transform.rotation, 10f)) 
        //    {
        //        Debug.Log($"Custom hitbox detected: {hitInfo.collider.name}");
        //    }
        //}

        // Debugger Calls
        DrawAttackBounds(attackBounds, Color.green);
    }

    // --------------------------- MOVEMENT ---------------------------

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

            // ...
            if (horizontal < 0f)
            {
                spriteRenderer.flipX = true;
            }
            else 
            {
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            // Animates the character when NOT moving
            //Animate("Input Magnitude", 0f, 0.05f, Time.deltaTime);
        }
    }

    // -------------------------- ANIMATIONS ---------------------------

    // Method for Character Animation
    public void Animate(string animParamater, float inputValue, float transitionSmooth, float transitionCounter)
    {
        // - ("Name of the Animation Parameter", player.input value, transition smoothness, counter)
        animatorController.SetFloat(animParamater, inputValue, transitionSmooth, transitionCounter);
    }

    // ------------------------- DEBUGGERS -------------------------

    void DrawAttackBounds(Bounds bounds, Color color)
    {
        Debug.DrawLine(bounds.min, new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), color);
        Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), color);
        Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), color);
        Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), bounds.min, color);
    }
}
