using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleportation : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    public Transform portalDestination; // The destination portal could teleport to

    public enum PortalType // List of options for the portal condition type
    {
        ForPlayerOnly, // Only teleports the player character
        ForAllCharacters // Teleports any character that enters the portal
    }

    // Sets the default portal type to only teleport the player character (the most use cases)
    public PortalType portalType = PortalType.ForPlayerOnly; 

    // ------------------------- UNITY METHODS -------------------------

    // Built-In Unity method that called when a gameObject with a Collider enters
    private void OnTriggerEnter(Collider actor)
    {
        if (portalType == PortalType.ForPlayerOnly) 
        {
            // Filters the trigger event to only respond to a 'Player' tagged gameObject
            if (!actor.CompareTag("Player"))
            {
                return; // Exit the method early if the actor is not the player
            }
        } 

        // Teleport the a Character to the portal's destination position
        TeleportPlayer(actor.gameObject);
    }

    // Method to handle teleporting the character to the destination portal's position
    protected virtual void TeleportPlayer(GameObject passenger) 
    {
        // Try to get the CharacterController component from the passenger GameObject
        // - this is important to prevent issues if the character contains CharacterController 
        CharacterController actorController = passenger.GetComponent<CharacterController>();

        // Evalutes if the passenger has a CharacterController that needs to disable before teleporting
        if (actorController)
        {
            // Disable the CharacterController to avoid collision issues during teleportation
            actorController.enabled = false;

            // Teleport the player to the destination portal's position
            passenger.transform.position = portalDestination.position;

            // Re-enable the CharacterController after teleportation
            actorController.enabled = true;
        }
        else 
        {
            // Teloports a Character with no CharacterController (like an Enemy, NPC, and such)
            passenger.transform.position = portalDestination.position;
        }

        Debug.Log($"Teleported {passenger.name} to {portalDestination.position}");
    }
}