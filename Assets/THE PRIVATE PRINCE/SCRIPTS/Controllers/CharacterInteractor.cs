using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.InputSystem;
 
// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class CharacterInteractor : MonoBehaviour
{
    // Reference to the PlayerInput component for handling new input system actions and controls
    private PrivatePrinceControls ppControls;

    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain

    // [Header("EVENTS")]
    // [SerializeField] UnityEvent onCyberScan;
    // [SerializeField] UnityEvent onDialogue;

    [Header("INTERACTIONS")]
    [Range(1, 100)] [SerializeField] float interactionRadius = 1f;
    [SerializeField] LayerMask interactableLayers; // Layer for obstacles that can block the cast
    [SerializeField] LayerMask obstacleLayers; // Layer for obstacles that can block the cast
    [SerializeField] GameObject interactIcon; // Icon that will pop up when near interactable object

    [Header("STATUS")]
    [SerializeField] bool inDialogue; // Indicates if the player is currently in a dialogue
    [SerializeField] bool isCyberScanning; // ...
 
    // ------------------------- UNITY METHODS -------------------------
    #region UNITY LOGICS
 
    // Awake is called when this script was first loaded
    private void Awake()
    {
        // Checks if our reference for the script was not set
        if(debuggerNiAin == null)
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();

        // Evaluates if there is controls initialized in the "GameplayInputManager"
        if (GameplayInputManager.Instance.Controls == null)
        {
            debuggerNiAin.Error("PlayerInputManager singleton not found! Make sure it exists in the scene.");
        }
        else 
        {
            // Accesses the controls from the PlayerInputManager singleton instance
            ppControls = GameplayInputManager.Instance.Controls;

            debuggerNiAin.Log($"New Input System was set: {ppControls}");
        }
    }

    // OnEnable is called when the object becomes enabled and active
    void OnEnable()
    {
        Subscribe();
    }

    // OnDisable is called when the object becomes disabled
    void OnDisable()
    {
        Unsubscribe();
    }

    // OnDestroy is called when the object is destroyed
    void OnDestroy()
    {
        Unsubscribe();
    }

    #endregion
 
    // ---------------------- PREPARATION METHODS -------------------------
    #region PREPARATION LOGICS

    // Method to subscribe to events as a listener
    void Subscribe()
    {
        // Proceeds only if the input control reference was successfully set
        if (ppControls == null) return;

        // Subscribes methods to the corresponding inputs set in New Input System
        ppControls.Player.Interact.performed += Interact;
        ppControls.Player.Cyberscan.performed += Interact;
    }

    // Method to unsubscribe from events 
    void Unsubscribe()
    {
        // Unsubscribes methods to the corresponding inputs set in New Input System
        ppControls.Player.Interact.performed -= Interact;
        ppControls.Player.Cyberscan.performed -= Interact;
    }

    #endregion

    // ------------------------- INTERACTIONS -------------------------
    #region INTERACTION LOGICS
 
    // Method to call for Interacting In-Game objects
    void Interact(InputAction.CallbackContext context)
    {
        // Gets the closest interactable object it can found
        Collider interacted = GetClosestInteractable();

        // Checks if the interactable object is really an interactable
        if (interacted.TryGetComponent(out IInteractable interactableObj))
        {
            // Interact the object
            interactableObj.Interact();
        }
    }

    // Method to call for evaluating the closest interactable object
    Collider GetClosestInteractable()
    {
        // Placeholder for the closest candidate (defaults to null)
        Collider closestInteractable = null;

        // Placeholder for the limit on determining the closest object
        float closestDistance = Mathf.Infinity;

        // Scans a specified radius "OverlapSphere(this.transform.position, interactionRadius)" 
        // Checks for objects included in the layers within those radius "(_, _, interactableLayers)"
        // Then adds it to the list if they were "Collider[] hits"
        Collider[] hits = Physics.OverlapSphere(this.transform.position, interactionRadius, interactableLayers);

        // Narrates through all scanned objects one by one
        foreach(Collider obj in hits)
        {
            // Calculates the acurate distance of the scanned object away from this object
            // Then converts the result into a single number to be stored as distance 
            // NOTE - "directionToTarget" contains "(x,y,z)" values
            Vector3 directionToObj = obj.transform.position - this.transform.position;
            float distanceToObj = directionToObj.magnitude; 

            // Shoots a laser from this "this.transform.position" object to the target "distanceToTarget" 
            // In the direction of the calculated result "directionToTarget.normalized"
            // And only proceeds the block if it hits an object inclued in "obstacleLayers"
            if (Physics.Raycast(this.transform.position, directionToObj.normalized, distanceToObj, obstacleLayers) ||
                TryGetComponent(out IInteractable interactable))
            {
                // Skip objects that are obstructed by another object or a wall
                continue;
            }

            // Keep track of which visible item is the absolute closest
            if (distanceToObj < closestDistance)
            {
                // Overwrites the current closest object & distance value with:
                // * the closest object found (for grabbing the closest object)
                // * closest object distance (for setting the bar lower to ignore farher objects)
                closestInteractable = obj;
                closestDistance = distanceToObj;
            }
        }

        // Returns the single closest interactable
        return closestInteractable;
    }
 
    #endregion

    // ---------------------------- STATES -------------------------
    #region STATE LOGICS

    // NOTE - I think methods like this shouldn't be called here
    // Method to check if the player is currently in dialogue
    public void InDialogue()
    {
        // NOTE - Add Invoke event here instead of status checker...

        // if (inDialogue)
        // {
        //     Debug.Log("Player in Dialogue!");

        //     // Stops the movement animation when in dialogue
        //     if (animatorController != null)
        //         animatorController.SetBool("isMoving", false);

        //     return true;
        // }

        // return false;
    }

    // ...
    public void InCyberScan()
    {
        // NOTE - Add Invoke event here instead of status checker...

        // // ...
        // onCyberScan = !onCyberScan;

        // // ...
        // if (onCyberScan == true)
        // {
        //     // Disables Movements
        //     canAttack = false;
        //     isBlocking = false;

        //     // Slows down time
        //     Time.timeScale = 0.5f;

        //     return;
        // }

        // // Disables Movements
        // canAttack = true;
        // isBlocking = true;

        // // Turn back normal time
        // Time.timeScale = 1f;

        // return;
    }

    #endregion

    // ------------------------- GIZMOS -------------------------
    #region GIZMO LOGICS

    // Method to visualize the interaction radius in the Scene view
    private void OnDrawGizmos()
    {
        // Sets the gizmo color for the interaction radius (ALL THE TIME)
        Gizmos.color = Color.yellow;

        // Draws a wireframe sphere matching the actual OverlapSphere check
        Gizmos.DrawWireSphere(this.transform.position, interactionRadius);
    }

    // Method to visualize system in the Scene view (ONLY when selecting the object)
    private void OnDrawGizmosSelected()
    {
        // Just Waiting for Your Logic...
    }

    #endregion
}