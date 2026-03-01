using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteraction : Teleportation
{
    // ------------------------- VARIABLES -------------------------

    PrivatePrinceControls ppControls; // Reference to the PrivatePrinceControls script that handles the new input system controls

    public enum DoorType // List of options for the door condition type
    {
        OpenDoor, // Instantly triggerable just by entering the trigger area
        InteractDoor // Requires the player to interact with the door to enter
    }

    // Sets the default door type to an open door (the most use cases)
    public DoorType doorType = DoorType.InteractDoor;

    [SerializeField] private bool doorInteracted = false; // Flag to track if the door has been interacted 

    // ------------------------- UNITY METHODS -------------------------

    // Built-In Unity method that called when this script's gameObject is first loaded
    private void Awake()
    {
        SubscribeToInputEvents();
    }

    // Built-In Unity method that called when a gameObject with a Collider enters
    private void OnTriggerEnter(Collider actor)
    {
        // 1st: Evaluates if the portal is set to only teleport the player character
        if (portalType == PortalType.ForPlayerOnly) 
        {
            // Filters the trigger event to only respond to a non 'Player' tagged gameObject
            if (!actor.CompareTag("Player"))
            {
                return; // Exit the method early if the actor trying to access is not player
            }
        }

        // 2nd: Evaluates if the door is set to require player interaction to teleport
        if (doorType == DoorType.InteractDoor) 
        {
            // Filters the trigger event to only respond to a 'Player' tagged gameObject
            if (actor.CompareTag("Player"))
            {
                return; // Exit the method early if the actor requires interaction to teleport
            }
        }

        ResetDoorInteraction();

        // 3rd: Teleports the Character to the portal's destination position
        base.TeleportPlayer(actor.gameObject);
    }

    // ...
    private void OnTriggerStay(Collider actor)
    {
        if (doorType == DoorType.OpenDoor) return;

        Debug.Log("DoorInteraction: Player is within the door's trigger area, waiting for interaction...");

        if (doorInteracted)
        {
            Debug.Log("DoorInteraction: Door interaction detected, teleporting player...");

            // ...
            ResetDoorInteraction();

            // ...
            base.TeleportPlayer(actor.gameObject);
        }
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

        // Invokes the method when there's an input detected from the New Input System
        ppControls.Player.Interact.performed += OpenDoor;

        Debug.Log("ResponseHandler: Subscribed to input events");
    }

    // ...
    private void UnsubscribeFromInputEvents()
    {
        if (ppControls == null) return;

        ppControls.Player.Interact.performed -= OpenDoor;

        Debug.Log("ResponseHandler: Unsbscribed to input events");
    }

    // ------------------------- CUSTOM METHODS -------------------------

    // ...
    public void OpenDoor(InputAction.CallbackContext context) 
    {
        doorInteracted = true;
    }

    // ...
    public void ResetDoorInteraction()
    {
        doorInteracted = false;
    }

    // -------------------------- CLEANUP METHODS -------------------------

    //// Unity Built-in method called when this component is disabled
    //private void OnDisable()
    //{
    //    // Clean up subscriptions & Reset responsive state
    //    UnsubscribeFromInputEvents();
    //    doorInteracted = false;
    //}

    // Unity Built-in method called when this component is destroyed
    private void OnDestroy()
    {
        // Clean up subscriptions
        UnsubscribeFromInputEvents();
    }
}