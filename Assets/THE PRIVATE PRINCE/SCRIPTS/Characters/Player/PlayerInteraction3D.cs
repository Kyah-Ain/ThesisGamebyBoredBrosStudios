using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.InputSystem;

// Required DebuggerNiAinPjls.cs for this to be able to monitor debugs, otherwise use the old one
[RequireComponent(typeof(DebuggerNiAinPjls))]
public class PlayerInteraction3D : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("REFERENCES")]
    private PrivatePrinceControls ppControls; // Reference to the Action Map created from Unity's New Input System
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain

    [Header("EVENTS")]
    [SerializeField] UnityEvent onInteracting;
    [SerializeField] UnityEvent onUnInteracting;

    [Header("INTERACTIONS (for Debug Only, DO NOT put anything here)")]
    [SerializeField] List<GameObject> interactablesInRange = new(); // List of objects that can be interacted
    [SerializeField] GameObject interactionAtHand; // Refers to the object that is currently being interacted 
    [SerializeField] bool isInteracting; // Determines if the player is currently interacting, to avoid multiple interactions
 
    // ----------------------- UNITY METHODS -------------------------

    // Awake is called when the object was first loaded, before Start()
    void Awake()
    {
        // Checks if our reference for the script was not set
        if(debuggerNiAin == null)
        {
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();
        }

        // Evaluates if an InputManager instance exists in the scene (for reference)
        if (GameplayInputManager.Instance == null)
        {
            debuggerNiAin.Log("GameplayInputManager Instance is NULL!");

            return;
        }

        // Automatically sets the 'Initialized' input control maps from InputManager 
        if (GameplayInputManager.Instance.Controls == null)
        {
            debuggerNiAin.Log("GameplayInputManager Controls is NULL!");

            return;
        }

        // Prepared the controls to be ready for use  
        ppControls = GameplayInputManager.Instance.Controls;

        debuggerNiAin.Log($"Game Controls was successfully set: {ppControls}");
    }

    // Start is called before the first frame Update()
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

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

    // ---------------------- PREPARATION METHODS -------------------------

    // Method to subscribe to events as a listener
    void Subscribe()
    {
        // Proceeds only if the input control reference was successfully set
        if (ppControls == null) return;

        // Subscribes methods to the corresponding inputs set in New Input System
        ppControls.Player.Interact.performed += PerformInteractCast;
        ppControls.UserNavigation.Interact.performed += PerformInteractCast;
    }

    // Method to unsubscribe from events 
    void Unsubscribe()
    {
        // Unsubscribes methods to the corresponding inputs set in New Input System
        ppControls.Player.Interact.performed -= PerformInteractCast;
        ppControls.UserNavigation.Interact.performed -= PerformInteractCast;
    }

    // ----------------------- INTERACTION METHODS -------------------------

    // Method to scan for interactable objects in the player's vicinity
    public void OnTriggerEnter(Collider obj)
    {
        // Proceeds to execute the code below if the object as two things:
        // 1) object has implemented the 'IInteractable' interface
        // 2) object hasn't been added to the list yet, ensuring no duplicates
        if (obj.TryGetComponent(out IInteractable interactable) &&
            !interactablesInRange.Contains(obj.gameObject) &&
            obj.gameObject != this.gameObject) 
        {
            // Adds the object in the interactable range
            interactablesInRange.Add(obj.gameObject);
        }

        // // Adds the object in the interactable range
        // interactablesInRange.Add(obj.gameObject);
    }

    // Method to remove interactable objects from the player's vicinity when they exit the trigger area
    public void OnTriggerExit(Collider obj)
    {
        // Checks if the object to remove was inside the list to begin with
        if (interactablesInRange.Contains(obj.gameObject))
        {
            // Removes the object in the interactable range
            interactablesInRange.Remove(obj.gameObject);
        }
    }

    // Method to execute an interaction
    void PerformInteractCast(InputAction.CallbackContext context)
    {
        // Evaluates if there's even interactables in range to begin with
        if (interactablesInRange == null || interactablesInRange.Count <= 0) return;

        // Placeholder for the closest interactable in range
        float minDistance = Mathf.Infinity;
        
        // Iterates through all object in the list, evaluates them one by one
        foreach(GameObject obj in interactablesInRange)
        {
            // Calcutes for the distance of each item in the list, how far it is from the player
            float distance = Vector3.Distance(this.transform.position, obj.transform.position);

            // Checks for the closest object to the player
            if (distance < minDistance)
            {
                minDistance = distance;
                interactionAtHand = obj;
            }
        }

        // Checks if the object was interactable to begin with
        if (interactionAtHand != null)
        {
            // Gets the Interactable reference of the closest object found
            IInteractable interactable = interactionAtHand.GetComponent<IInteractable>();

            // Flips the value of the bool to its opposite counterpart
            isInteracting = !isInteracting;

            // Automatically un-interact if you interact while in the middle of interaction
            if (isInteracting == false)
            {
                // Un-Interact the object
                interactable.UnInteract();

                // Brodcast the un-interaction
                onUnInteracting?.Invoke();

                return;
            }

            // Interact the object
            interactable.Interact();

            // Brodcast the interaction
            onInteracting?.Invoke();
        }
    }
}