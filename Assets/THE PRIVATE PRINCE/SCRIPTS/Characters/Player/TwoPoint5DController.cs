using System.Collections; // Grants access to collecitons structures like ArrayLists and Hashtables
using System.Collections.Generic; // Grants access to collections structures like Lists and Dictionaries
using UnityEngine; // Grants access to Unity's core classes and functions like MonoBehaviour, GameObject

using UnityEngine.InputSystem;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise you can use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
// Requires this GameObject to have a CharacterController component for movement
[RequireComponent(typeof(CharacterController))]

public class TwoPoint5DController : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain

    [Header("CORE")]
    [SerializeField] CharacterController controller; // Reference to the component that controls character movement
    [SerializeField] Animator animator; // Reference to the component that animates 
    // [SerializeField] SpriteRenderer spriteRoot; // Reference to the component that provides 2D-Visuals
    [SerializeField] GameObject spriteRoot; // Reference to the component that holds the visuals
    
    [Header("SETTINGS")]
    [SerializeField] float movementSpeed = 1f; // Speed at which the character moves
    [SerializeField] [Range(0f,10f)] float gravityMultiplier = 0f; // Intensity of the Gravity Multiplier applied to the character 
    
    [Header("STATUS")]
    [SerializeField] [ReadOnly] bool isCharacterFrozen; // Tracker if the character should be freezed or not (useful for dialogue events)
    [SerializeField] [ReadOnly] float gravity = -9.81f; // Gravity force applied to the character (Earth based gravity)
    [SerializeField] [ReadOnly] float freefallVelocity = -1.0f; // Speed at which the character fall
    [SerializeField] [ReadOnly] Vector2 inputVector; // The current input value/coordinates
    
    // ----------------------- UNITY METHODS -------------------------
    #region UNITY METHODS
    
    // Awake is called when this script was first initialized & loaded
    void Awake()
    {
        #region INITIALIZAIONS
        
            // Checks if our reference was null or not been set
            if (debuggerNiAin == null)
            {
                // If it is not, then set it automatically by looking for the script class from this object
                debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
                
                // Logs a missing component if still not found
                if (debuggerNiAin == null) MissingComponentLog("debuggerNiAin");
            }
            
            // Checks if our reference was null or not been set
            if (spriteRoot == null)
            {
                // If it is not, then set it automatically by looking for the script class from this object
                // spriteRoot = this.GetComponent<SpriteRenderer>();
                spriteRoot = this.gameObject; 
                
                // Logs a missing component if still not found
                if (spriteRoot == null) MissingComponentLog("spriteRoot");
            }

        #endregion
    }
    
    // OnEnable is called when the object becomes enabled and active
    void OnEnable()
    {
        Subscribe();
    }

    // OnDisable is called when the object becomes disabled
    void OnDisable()
    {
        UnSubscribe();
    }

    // Start is called once before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    private void Update()
    {
        MoveCharacter();
    }

    #endregion
    
    // ----------------------- EVENT METHODS -------------------------
    #region EVENT METHODS

    // Method to subscribe your local method to an event trigger
    protected virtual void Subscribe()
    {
        // Set subscriptions of these methods to an event
        // Left (Event Call) += Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onMovement += ApplyMovement;
        // GameEventsManager.Instance.inputEvents.onInteract += ;
    }

    // Method to UnSubscribe your local method to an event trigger
    protected virtual void UnSubscribe()
    {
        // UnSubscribe them methods from an event
        // Left (Event Call) -= Right (Method that would be called)
        GameEventsManager.Instance.inputEvents.onMovement -= ApplyMovement;
        // GameEventsManager.Instance.inputEvents.onInteract -= ;
    }
    
    #endregion
    
    // --------------------- CHARACTER METHODS ---------------------------
    #region CHARACTER METHODS
    
    // Method for moving the Character
    void MoveCharacter()
    {
        // Only proceeds if the character is unfrozen
        if (!isCharacterFrozen)
        {
            ApplyGravity();
        
            // Combines the horizontal and depth input with the current vertical velocity
            Vector3 movement = new Vector3(
                inputVector.x, // Horizontal Input
                freefallVelocity, // Vertical Input
                inputVector.y // Depth Input (if there's any)
            );

            ApplyAnimation();
        
            // Applies calculated movement to the character
            controller.Move(movement * (movementSpeed * Time.deltaTime));
        }
    }

    // Method to stop the character
    public void StopCharacter()
    {
        ToggleFreeze(true);
        
        // Resets vertical velocity 
        freefallVelocity = -1.0f;
            
        // Combines the horizontal and depth input with the current vertical velocity
        Vector3 movement = new Vector3(
            0, // Horizontal Input
            0, // Vertical Input
            0 // Depth Input (if there's any)
        );

        // Animates the character as NOT moving
        animator.SetBool("isMoving", false);
            
        // Applies calculated movement to the character
        controller.Move(movement);
    }

    // Method for receiving movement inputs
    void ApplyMovement(InputAction.CallbackContext context)
    {
        // Reads Input from the Event-Based New Input System
        inputVector = context.ReadValue<Vector2>();
    }
    
    // Method for applying gravity to the character (simulating freefall and grounded movement)
    public virtual void ApplyGravity()
    {
        // Checks if the CharacterController reference exists
        if (controller == null) return;

        // Checks if the character is currently in the air
        if (!controller.isGrounded)
        {
            // Gradually increases the downward velocity based on gravity
            freefallVelocity += gravity * gravityMultiplier * Time.deltaTime;
        }
        else
        {
            // Resets vertical velocity when grounded
            freefallVelocity = -1.0f;
        }
    }
    
    // Method for animating the character
    void ApplyAnimation()
    {
        // Evaluates if there is a movement
        if (inputVector.x != 0 || inputVector.y != 0f)
        {
            #region OBJECTS BASED FLIP LOGIC
            
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
                            Mathf.Abs(currentScale.x),
                            currentScale.y,
                            currentScale.z
                        );
                    }
                    else if (inputVector.x > 0f)
                    {
                        // Flips the sprite root to face right
                        spriteRoot.transform.localScale = new Vector3(
                            -Mathf.Abs(currentScale.x),
                            currentScale.y,
                            currentScale.z
                        );
                    }
                }
            
            #endregion

            #region SPRITE BASED FLIP LOGIC

                // // Flip sprite based on direction if spriteRoot exists
                // if (characSprites != null)
                // {
                //     // Determines the direction the character is facing
                //     if (inputVector.x < 0f)
                //     {
                //         // Flips the sprite root to face left 
                //         foreach (SpriteRenderer sprite in characSprites)
                //         {
                //             sprite.flipX = true;
                //         }
                //     }
                //     else if (inputVector.x > 0f)
                //     {
                //         // Flips the sprite root to face right
                //         foreach (SpriteRenderer sprite in characSprites)
                //         {
                //             sprite.flipX = false;
                //         }
                //     }
                // }

            #endregion
 
            // Animates the character when moving
            animator.SetBool("isMoving", true);
        }
        else
        {
            // Animates the character when NOT moving
            animator.SetBool("isMoving", false);
        }
    }

    #endregion
    
    // ------------------------- HELPERS -------------------------

    // Method to switch freeze condition
    public void ToggleFreeze(bool condition)
    {
        isCharacterFrozen = condition;
    }

    // ------------------------- DEBUGGER -------------------------

    // Method to log missing components
    void MissingComponentLog(string component)
    {
        Debug.LogError($"MISSING COMPONENT: Cannot find the \"{component}\"");
    }
}