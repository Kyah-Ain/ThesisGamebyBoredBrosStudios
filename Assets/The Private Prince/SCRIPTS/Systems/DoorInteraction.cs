using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

// Manually creates an Event Method that accepts Parameter
//[Serializable] public class DoorEvent : UnityEvent<string> { }

public class DoorInteraction : Portal
{
    // ------------------------- EVENTS -------------------------

    [Header("REFERENCES")]
    [SerializeField] DebuggerNiAinPjls debuggerNiAin; // Custom debugging script from your dev Ain

    [SerializeField] GameObject passenger; // Refers to the teleport subject

    // ...
    public UnityEvent onEnterDoor;

    public UnityEvent doorVFX;
    public UnityEvent doorSFX;

    // ------------------------- VARIABLES -------------------------

    PrivatePrinceControls ppControls; // Reference to the PrivatePrinceControls script that handles the new input system controls

    public enum DoorType // List of options for the door condition type
    {
        OpenDoor, // Instantly triggerable just by entering the trigger area
        InteractDoor // Requires the player to interact with the door to enter
    }

    // Sets the default door type to an open door (the most use cases)
    public DoorType doorType = DoorType.InteractDoor;

    [SerializeField] private bool isInteractable = false; // Flag to track if the door has been interacted 

    // ------------------------- UNITY METHODS -------------------------

    // ...
    public void Awake()
    {
        // Checks if our reference for the script was not set
        if (debuggerNiAin == null)
            // If it is not, then set it automatically by looking for the script class from this object
            debuggerNiAin = this.GetComponent<DebuggerNiAinPjls>();

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
    }

    // Built-In Unity method that called when this script's gameObject is first loaded
    private void OnEnable()
    {
        Subscribe();

        //// Subscribes to the Delegate Events
        //onEnterDoor.AddListener(OnEnterDoor);

        // Subscribes the Neutralizer method to the onTeleportStart event
        onTeleportStart += Teleport;
        onTeleportFinish += PassengerReseat;
    }

    // Built-In Unity method that called when this script's gameObject is disabled
    private void OnDisable()
    {
        Unsubscribe();

        //// Unsubscribes to the Delegate Events
        //onEnterDoor.RemoveListener(OnEnterDoor);

        // Subscribes the Neutralizer method to the onTeleportStart event
        onTeleportStart -= Teleport;
        onTeleportFinish -= PassengerReseat;
    }

    // Built-In Unity method that called when this script's gameObject is destroyed
    private void OnDestroy()
    {
        Unsubscribe();

        // ...
        onTeleportStart = null;
        onTeleportFinish = null;
        onEnterDoor.RemoveAllListeners();
    }

    // ---------------------- PREPARATION METHODS -------------------------

    // Method to subscribe to events as a listener
    public void Subscribe()
    {
        // Proceeds only if the input control reference was successfully set
        if (ppControls == null) return;

        // SUBSCRIBE METHODS to the input action events
        ppControls.Player.Interact.performed += OpenDoor;
    }

    // Method to unsubscribe from events 
    public void Unsubscribe()
    {
        // Proceeds only if the input control reference was successfully set
        if (ppControls == null) return;

        // UNSUBSCRIBE METHODS to the input action events
        ppControls.Player.Interact.performed -= OpenDoor;
    }

    // ------------------------- COLLISIONS -------------------------

    // Built-In Unity method that called when a gameObject with a Collider enters
    private void OnTriggerEnter(Collider actor)
    {
        // Filters the trigger to only responds to 'Player' tagged requests
        if (!actor.CompareTag("Player")) return;

        passenger = actor.gameObject;

        if (doorType == DoorType.OpenDoor)
        {
            EnterDoor();

            return;
        }

        isInteractable = true;
    }

    // ...
    private void OnTriggerExit(Collider actor)
    {
        // Filters the trigger to only responds to 'Player' tagged requests
        if (!actor.CompareTag("Player")) return;

        isInteractable = false;

        passenger = null;
    }

    // ------------------------- CUSTOM METHODS -------------------------

    // ...
    void EnterDoor()
    {
        onEnterDoor?.Invoke();

        onTeleportStart?.Invoke(passenger, base.tpDestination);
        doorVFX?.Invoke();
        doorSFX?.Invoke();
    }

    // ...
    void OpenDoor(InputAction.CallbackContext context)
    {
        // ...
        if (isInteractable && doorType == DoorType.InteractDoor)
        {
            EnterDoor();
        }
    }

    // ...
    protected override void Teleport(GameObject passenger, Transform destination)
    {
        base.Teleport(passenger, destination);
    }
}