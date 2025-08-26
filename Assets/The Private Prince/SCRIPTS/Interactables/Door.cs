using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour // Assign script to an empty game object child of 3D object; 3D game object handles blocking the player collider, child handles door requirements
{
    [Header("Door Requirements")]
    public List<string> requiredKeys; // Assign required keys in Inspector

    private bool isUnlocked = false; // Start locked (i.e. blocking the player from walking through)
    [SerializeField] private Collider solidCollider; // Door has Collider not set to trigger to block player collider if player does not have required keys
    [SerializeField] private Renderer doorRenderer; // 3D objects come with Mesh Renderer upon creation


    private void OnTriggerEnter(Collider other) // Empty child of the door has Collider set to trigger to handle player interaction; ensure player has Rigidbody
    {
        if (isUnlocked) return; // No action done if door is unlocked
        if (other.CompareTag("Player")) // Assign Player tag to player
        {
            KeyHolder holder = other.GetComponent<KeyHolder>(); // Assign KeyHolder.cs to player component with collider
            if (holder != null && holder.HasKeys(requiredKeys)) // Prevent bugging out if KeyHolder.cs is not assigned to player
            {
                Debug.Log("Door unlocked.");
                UnlockDoor(); // Disable door collider and visuals if list of required keys are met
            }
            else // Keep the player blocked off by door if list of required keys are not met
            {
                Debug.Log("You don't have all the keys to unlock this door...");
            }
        }
    }

    private void UnlockDoor() // Disables door collider and visuals
    {
        isUnlocked = true; // Set to true to not disturb OnTriggerEnter as much

        // Disabling game object instead may bug save files
        if (solidCollider != null) solidCollider.enabled = false; // Collider disabled
        if (doorRenderer != null) doorRenderer.enabled = false; // Visuals disabled
    }
}
