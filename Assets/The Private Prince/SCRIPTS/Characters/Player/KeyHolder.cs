using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyHolder : MonoBehaviour // Assign script to player component with Collider
{
    private HashSet<string> collectedKeys = new HashSet<string>(); // Create new empty list of collected keys for player

    public void CollectKey(string keyID) // Used in Key.cs
    {
        if (!collectedKeys.Contains(keyID))
        {
            collectedKeys.Add(keyID); // Add key if not yet collected
            Debug.Log($"Collected key: {keyID}");
        }
    }

    public bool HasKeys(List<string> keys) // Used in Door.cs, needs a list of required keys for a door
    {
        foreach (string key in keys)
        {
            if (!collectedKeys.Contains(key)) // If player's list does not have all the keys requested from list, return false
                return false;
        }
        return true; // Else return true
    }
}
