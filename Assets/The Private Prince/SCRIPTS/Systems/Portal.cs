using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Portal : Teleportation
{
    // ------------------------- EVENTS -------------------------

    // DELEGATE EVENTS: Long Method Setup
    public delegate void OnTeleportStart(GameObject passenger, Transform destination); // Required method datatype to be stored
    protected OnTeleportStart onTeleportStart; // Method to subscribe to when the teleportation process starts (e.g., when a character enters the portal trigger)

    public delegate void OnTeleportFinish(GameObject passenger, CharacterController controller = null, Rigidbody rb = null); // Required method datatype to be stored
    protected OnTeleportFinish onTeleportFinish; // Method to subscribe to when the teleportation process finishes (e.g., after the character has been teleported and needs to be reset)

    // ------------------------- VARIABLES -------------------------

    private enum PortalType // List of options for the portal condition type
    {
        ForPlayerOnly, // Only teleports the player character
        ForAllCharacters // Teleports any character that enters the portal
    }

    // Sets the default portal type to only teleport the player character (the most use cases)
    private PortalType portalType = PortalType.ForPlayerOnly;

    // -------------------------- UNITY METHODS -------------------------

    // Built-In Unity method that called when this script's gameObject is first loaded
    private void OnEnable()
    {
        // Subscribes the Neutralizer method to the onTeleportStart event
        onTeleportStart += Teleport;
        onTeleportFinish += PassengerReseat;
    }

    // Built-In Unity method that called when this script's gameObject is disabled
    private void OnDisable()
    {
        // Unsubscribes the Neutralizer method from the onTeleportStart event to prevent memory leaks
        onTeleportStart -= Teleport;
        onTeleportFinish -= PassengerReseat;
    }

    // Built-In Unity method that called when this script's gameObject is destroyed
    private void OnDestroy()
    {
        // ...
        onTeleportStart = null;
        onTeleportFinish = null;
    }

    // Built-In Unity method that called when a gameObject with a Collider enters
    private void OnTriggerEnter(Collider actor)
    {
        // ...
        if (portalType == PortalType.ForPlayerOnly)
        {
            // Filters the trigger event to only respond to a 'Player' tagged gameObject
            if (!actor.CompareTag("Player"))
            {
                return; // Exit the method early if the actor is not the player
            }
        }

        // Invoke the onTeleportStart event, passing the actor's gameObject as a parameter
        onTeleportStart?.Invoke(actor.gameObject, tpDestination);
    }

    // --------------------------- INHERITED METHODS -------------------------

    // Method to handle teleporting the character to the destination portal's position
    protected override void Teleport(GameObject passenger, Transform destination)
    {
        // Try to get the CharacterController component from the passenger GameObject
        // - this is important to prevent issues if the character contains CharacterController 
        CharacterController actorController = passenger.GetComponent<CharacterController>();

        // Store velocity if needed (for Rigidbody characters)
        Rigidbody rigidBody = passenger.GetComponent<Rigidbody>();

        // Evalutes if the passenger has a CharacterController that needs to disable before teleporting
        if (actorController != null)
        {
            // Disable the CharacterController first to avoid collision issues during teleportation
            actorController.enabled = false;
        }

        // ...
        base.ApplyTeleportPosition(passenger, destination);

        // ...
        onTeleportFinish?.Invoke(passenger, actorController, rigidBody);
    }

    // --------------------------- RESET METHODS -------------------------

    // Method to handle resetting the passenger's state after teleportation
    protected virtual void PassengerReseat(GameObject passenger, CharacterController controller = null, Rigidbody rb = null)
    {
        // ...
        if (controller != null)
        {
            // ... 
            controller.enabled = true;
            controller = null;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            controller = null;
        }

        passenger = null;
    }
}