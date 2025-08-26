using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour // Assign to 2D game object
{
    [Header("Key Settings")]
    public string keyID; // Assign key ID in Inspector

    private void OnTriggerEnter(Collider other) // Add Collider (not Collider2D) to game object set to Trigger; ensure player has Rigidbody
    {
        if (other.CompareTag("Player")) // Assign Player tag to player
        {
            Debug.Log($"Collected {keyID}.");
            KeyHolder holder = other.GetComponent<KeyHolder>(); // Assign KeyHolder.cs to player
            if (holder != null) // Prevent bugging out if KeyHolder.cs is not assigned to player
            {
                holder.CollectKey(keyID); // Add key to list of collected keys made in KeyHolder.cs

                // Disable collider and visuals; Disabling game object may bug save files
                GetComponent<Collider>().enabled = false; // Collider disabled
                SpriteRenderer renderer = GetComponent<SpriteRenderer>(); // Get visual component of key
                if (renderer != null) renderer.enabled = false; // Visuals disabled
            }
        }
    }
}
