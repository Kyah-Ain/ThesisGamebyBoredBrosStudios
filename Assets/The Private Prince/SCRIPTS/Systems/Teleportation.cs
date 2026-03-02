using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Teleportation : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [SerializeField] protected Transform tpDestination; // The destination portal could teleport to
    [SerializeField] protected float tpOffset = 0.5f; // Offset to prevent teleporting into the destination portal's collider

    // --------------------------- TP METHODS -------------------------

    // Abstract method - derived classes MUST implement this
    protected abstract void Teleport(GameObject passenger, Transform destination);

    // Protected helper method that derived classes can use
    protected virtual void ApplyTeleportPosition(GameObject passenger, Transform destination)
    {
        // ...
        Vector3 faceDirection = destination.TransformDirection(Vector3.forward) * tpOffset;
        passenger.transform.position = destination.position + faceDirection;

        // ...
        Physics.SyncTransforms();

        Debug.Log($"Teleported {passenger.name} to {destination.position}");
    }
}