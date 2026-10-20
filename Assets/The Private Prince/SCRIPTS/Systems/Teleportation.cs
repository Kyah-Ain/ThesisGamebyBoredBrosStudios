using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleportation : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [Header("TELEPORTATION SETTINGS")]
    [SerializeField] Transform tpPassenger; // The subject to teleport
    [SerializeField] Transform teleportTo; // The destination of the teleportation 

    [Header("TRIGGER ENTER SETTINGS")]
    [SerializeField] string requiredTag = "Player"; // Only objects with this tag can trigger this (leave empty to accept any object)
    
    // ----------------------- UNITY METHODS ------------------------
    #region UNITY METHODS
    
    // OnTriggerEnter is called when another collider enters this object's trigger collider
    void OnTriggerEnter(Collider actor)
    {
        /* NOTE:
            This object needs a Collider with 'Is Trigger' checked, and either this object
            or the object entering needs a Rigidbody, otherwise this will never be called.
        */
        
        // If a tag is required, ignore any object that doesn't have it
        if (!string.IsNullOrEmpty(requiredTag) && !actor.CompareTag(requiredTag)) return;

        // Sets the player as the teleportation passenger
        tpPassenger = actor.transform;
    }
    
    // OnTriggerEnter is called when another collider exits this object's trigger collider
    void OnTriggerExit(Collider actor)
    {
        // If a tag is required, ignore any object that doesn't have it
        if (!string.IsNullOrEmpty(requiredTag) && !actor.CompareTag(requiredTag)) return;

        // Resets the Trigger for re-use
        tpPassenger = null;
    }
    
    #endregion
    
    // --------------------------- TP METHODS -------------------------

    // Method to teleport an object to one place to another
    public void Teleport()
    {
        // Only use teleport if there's something to teleport
        if (tpPassenger != null)
        {
            Teleport(tpPassenger);
        }
    }

    // Method to transport a gameObject from its position to the target location
    public void Teleport(Transform passenger)
    {
        // Only proceeds if there's passenger and destination to teleport to
        if (passenger == null || teleportTo == null)
        {
            Debug.LogWarning("Passenger or destination is missing!", this);
            return;
        }

        // Override the objects position and rotation to match the target destination
        Vector3 newPosition = teleportTo.position;
        Quaternion newRotation = Quaternion.Euler(0f, teleportTo.eulerAngles.y, 0f);

        // Logic for CharacterController (disable, move, re-enable)
        if (passenger.TryGetComponent(out CharacterController controller))
        {
            controller.enabled = false;
            passenger.SetPositionAndRotation(newPosition, newRotation);
            controller.enabled = true;
        }
        // Logic for Physics-driven Rigidbody (move the body directly and clear leftover motion)
        else if (passenger.TryGetComponent(out Rigidbody rb) && !rb.isKinematic)
        {
            rb.position = newPosition;
            rb.rotation = newRotation;
            rb.velocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        // Anything else (plain object, kinematic Rigidbody, collider only)
        else
        {
            passenger.SetPositionAndRotation(newPosition, newRotation);
            Physics.SyncTransforms(); // only matters if you query physics this same frame
        }
        
        // Clears the tpPassenger when teleporting is done (don't wait for OnTriggerExit)
        if (passenger == tpPassenger) tpPassenger = null;
    }
}