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

    // ------------------------- OPTIONAL METHODS -------------------------

    // Built-In Unity method that allows you to draw gizmos in the editor for visualization
    private void OnDrawGizmos()
    {
        if (tpDestination == null) return;

        // 1. Sets the gizmo's wireframe pen color
        // 2. Draws wireframe to the specified location (VISIBLE ON EDITOR ONLY)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(tpDestination.position, 0.5f);
        Gizmos.DrawWireCube(this.transform.position, this.transform.localScale);

        // 1. Sets the gizmo's wireframe pen color
        // 2. Gets and stores the forward direction of the portal destination
        // 3. Draws a ray to visualize the facing direction of the portal (VISIBLE ON EDITOR ONLY)
        Gizmos.color = Color.white;
        var faceDirection = tpDestination.TransformDirection(Vector3.forward) * tpOffset;
        Gizmos.DrawRay(tpDestination.position, faceDirection);
    }
}