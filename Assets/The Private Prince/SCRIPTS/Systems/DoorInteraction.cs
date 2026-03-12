using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

// Manually creates an Event Method that accepts Parameter
[Serializable] public class DoorEvent : UnityEvent<string> { }

public class DoorInteraction : Portal
{
    // ------------------------- EVENTS -------------------------

    // ...
    public DoorEvent onEnterDoor;
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

    // Built-In Unity method that called when this script's gameObject is first loaded
    private void OnEnable()
    {
        SubscribeToInputEvents();

        // Subscribes to the Delegate Events
        onEnterDoor.AddListener(OnEnterDoor);

        // Subscribes the Neutralizer method to the onTeleportStart event
        onTeleportStart += Teleport;
        onTeleportFinish += PassengerReseat;
    }

    // Built-In Unity method that called when this script's gameObject is disabled
    private void OnDisable()
    {
        UnsubscribeFromInputEvents();

        // Unsubscribes to the Delegate Events
        onEnterDoor.RemoveListener(OnEnterDoor);

        // Subscribes the Neutralizer method to the onTeleportStart event
        onTeleportStart -= Teleport;
        onTeleportFinish -= PassengerReseat;
    }

    // Built-In Unity method that called when this script's gameObject is destroyed
    private void OnDestroy()
    {
        SubscribeToInputEvents();

        // ...
        onTeleportStart = null;
        onTeleportFinish = null;
        onEnterDoor.RemoveAllListeners();
    }

    // Built-In Unity method that called when a gameObject with a Collider enters
    private void OnTriggerEnter(Collider actor)
    {
        isInteractable = true;
    }

    // ...
    private void OnTriggerStay(Collider actor)
    {
        if (!actor.CompareTag("Player")) return;

        // Check if ppControls is null and try to get it again
        if (ppControls == null)
        {
            SubscribeToInputEvents();
        }

        // ...
        if (isInteractable && 
           (ppControls.Player.Interact.WasPerformedThisFrame() || 
            doorType == DoorType.OpenDoor))
        {
            onTeleportStart?.Invoke(actor.gameObject, base.tpDestination);
            doorVFX?.Invoke();
            doorSFX?.Invoke();
        }
    }

    // ...
    private void OnTriggerExit(Collider actor)
    {
        isInteractable = false;
    }

    // ------------------------- EVENT METHODS -------------------------

    // ...
    private void SubscribeToInputEvents()
    {
        // Get the reference to the PrivatePrinceControls script that handles the new input system controls
        ppControls = GameplayInputManager.Instance?.Controls;

        if (ppControls == null) return;

        // Unsubscribe first to prevent double or multiple subscriptions 
        UnsubscribeFromInputEvents();

        Debug.Log("ResponseHandler: Subscribed to input events");
    }

    // ...
    private void UnsubscribeFromInputEvents()
    {
        if (ppControls == null) return;

        Debug.Log("ResponseHandler: Unsbscribed to input events");
    }


    // ------------------------- CUSTOM METHODS -------------------------

    // ...
    protected override void Teleport(GameObject passenger, Transform destination)
    {
        base.Teleport(passenger, destination);

        onEnterDoor?.Invoke(null);
    }

    // ...
    public void OnEnterDoor(string message = "") { Debug.Log(message); }
}