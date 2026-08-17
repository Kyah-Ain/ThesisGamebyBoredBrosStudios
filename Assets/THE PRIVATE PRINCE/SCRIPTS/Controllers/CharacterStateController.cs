using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using TMPro;
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject, Transform, Vector3, etc.

using UnityEngine.InputSystem; // Grants access to Unity's new Input System for handling player inputs

// Owns shared character state (booleans, references) and the mood system.
// Movement / Combat / Interaction scripts all read/write through this hub
// instead of duplicating state or reaching into each other directly.
[RequireComponent(typeof(CharacterController))] // Requires this GameObject to have a CharacterController component in order to function properly
public class CharacterStateController : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // Reference to the PlayerInput component for handling new input system actions and controls
    private PrivatePrinceControls ppControls;
    public PrivatePrinceControls Controls => ppControls; // Read-only access for Movement/Combat/Interaction scripts

    [Header("OBJECT REFERENCES")]
    [SerializeField] private CharacterController characController; // Reference to the CharacterController component for controlling character movement
    [SerializeField] private Animator animatorController; // Reference to the Animator component for controlling character animations
    [SerializeField] private GameObject spriteRoot; // Reference to the root GameObject that contains the characer sprites for flipping their facing direction
    //[SerializeField] private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component for handling sprite rendering and flipping

    public CharacterController CharacController => characController;
    public Animator AnimatorController => animatorController;
    public GameObject SpriteRoot => spriteRoot;

    [Header("CHARACTER ATTRIBUTES")]
    [SerializeField] private float gravity = -9.81f; // Gravity force applied to the character (Earth based gravity)
    [SerializeField] private float gravityMultiplier = 3f; // Multiplier to increase the effect of gravity for a more grounded feel
    [SerializeField] private float freefallVelocity = 0f; // Current vertical velocity of the character for applying gravity and simulating freefall

    public float FreefallVelocity => freefallVelocity; // Movement script needs this to compose its movement vector

    [Header("BOOLEANS")]
    public bool onCyberScan = false; // ...
    public bool inDialogue = false; // Indicates if the player is currently in a dialogue
    public bool canAttack = true; // Indicates if the player can perform an attack
    public bool canMove = true; // Indicates if the player can move
    public bool hasHit = false; // Indicates if the player has hit something with its attack
    public bool isBlocking = false; //  Indicates if the player is currently blocking an attack with their shield

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

    // Update is called once per frame
    // NOTE: Set Script Execution Order so this runs BEFORE PlayerMovement2Point5D and
    // PlayerCombat2Point5D — they depend on ApplyGravity() and InDialogue() having run first.
    private void Update()
    {
        // Early return if critical components are missing
        if (characController == null || animatorController == null) return;

        // ApplyGravity();

        InDialogue();
    }

    #endregion

    // ---------------------------- STATES -------------------------
    #region STATE LOGICS

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

    #endregion

    // --------------------------- GRAVITY ---------------------------
    #region GRAVITY LOGICS

    // Method for applying gravity to the character to simulate freefall and grounded movement
    public virtual void ApplyGravity()
    {
        // if (characController == null) return;

        // if (!characController.isGrounded)
        // {
        //     Debug.Log("Player is Falling!");

        //     freefallVelocity += gravity * gravityMultiplier * Time.deltaTime;
        //     characController.Move(new Vector3(0, freefallVelocity, 0) * Time.deltaTime);
        // }
        // else
        // {
        //     freefallVelocity = -1.0f; // Resets vertical velocity when grounded
        // }
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
}