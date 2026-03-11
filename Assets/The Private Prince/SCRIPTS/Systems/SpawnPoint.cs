using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    [SerializeField] protected float moveOffset = 0f; // Offset to prevent teleporting into the destination portal's collider

    // --------------------------- TP METHODS -------------------------

    // Protected helper method that derived classes can use
    public void MoveSpawnPoint(Transform objToAttachedTo)
    {
        // ...
        Vector3 faceDirection = objToAttachedTo.TransformDirection(Vector3.forward) * moveOffset;
        this.transform.position = objToAttachedTo.position + faceDirection;

        // ...
        Physics.SyncTransforms();

        Debug.Log($"Teleported {this.name} to {objToAttachedTo.position}");
    }
}