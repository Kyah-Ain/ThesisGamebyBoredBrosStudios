using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class VoidLimiter : Portal
{
    // ------------------------- EVENTS -------------------------

    // DELEGATE EVENTS: Short Method Setup
    public event Action <GameObject, Transform> onHitVoid; // Method to subscribe to when the character hits the void

    // ------------------------- VARIABLES -------------------------

    [SerializeField] private Transform spawnPoint; // Spawn location to teleport to
    [SerializeField] private float voidLimitY = -40f; // Y position threshold to consinder what is void

    // -------------------------- UNITY METHODS -------------------------

    // Built-In Unity method that called when this script's gameObject is first loaded
    private void OnEnable()
    {
        // Subscribes to the Delegate Events
        onHitVoid += Teleport;
        onTeleportFinish += base.PassengerReseat;
    }

    // Built-In Unity method that called when this script's gameObject is disabled
    private void OnDisable()
    {
        // Unsubscribes to the Delegate Events
        onHitVoid -= Teleport;
        onTeleportFinish -= base.PassengerReseat;
    }

    // Built-In Unity method that called when this script's gameObject is destroyed
    private void OnDestroy()
    {
        // ...
        onHitVoid = null;
        onTeleportFinish = null;
    }

    // ...
    private void Awake()
    {
        // ...
        if (spawnPoint == null)
        {
            // Finds the spawn point in the scene by tag
            // - make sure to set the tag in the inspector
            spawnPoint = GameObject.FindWithTag("SpawnPoint").transform;

            // ...
            if (spawnPoint == null) 
            {
                Debug.LogError($"Spawn point not found! " +
                    $"Manually assign it on the Inspector instead!");
            }
        }
    }

    // ...
    private void Update()
    {
        // ...
        if (spawnPoint != null)
        {
            // Sets the teleport destination to the spawn point if it exists
            tpDestination = spawnPoint;

            Debug.Log($"Currently at: {this.transform.position.y}ft");

            // ...
            if (this.transform.position.y <= voidLimitY) 
            {
                Debug.LogWarning($"Character has fallen below the void limit! " +
                    $"Teleporting to spawn point...");

                // ...
                if (this.gameObject.CompareTag("Enemy"))
                {
                    // Destroys the enemy if it hits the void
                    Destroy(this.gameObject);
                }
                else
                {
                    // ...
                    onHitVoid?.Invoke(this.gameObject, tpDestination);
                }
            }
        }
    }

    // --------------------------- INHERITED METHODS -------------------------

    // Method to handle teleporting the character to the destination portal's position
    protected override void Teleport(GameObject passenger, Transform destination)
    {
        // ...
        base.Teleport(passenger, destination);
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