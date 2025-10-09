using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.

public class OneTimeTriggerLock : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // Defines how the spawner should be disabled after triggering
    public enum DisableSpawnerMode
    {
        JustDisableTrigger,    // Only disable the trigger component
        DisableEntireCollider, // Disable the entire collider
        DisableWholeGameObject // Disable the entire GameObject
    }

    [Header("LOCK SETTINGS")]
    public DisableSpawnerMode disableMode = DisableSpawnerMode.JustDisableTrigger;
    public float disableDelay = 0.15f; // Delay before disabling (seconds)

    private Collider spawnerCollider = null; // Reference to the collider component

    // ------------------------- METHODS -------------------------

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Get reference to collider component
        spawnerCollider = GetComponent<Collider>();
    }

    // Called when another collider enters the trigger
    private void OnTriggerEnter(Collider actor)
    {
        // Only process player triggers
        if (!actor.CompareTag("Player")) return;

        // Safety check for collider reference
        if (spawnerCollider == null) return;

        // Execute disable with optional delay
        if (disableDelay <= 0)
        {
            // Immediate disable
            DisableTarget(); 
        }
        else
        {
            // Delayed disable
            Invoke(nameof(DisableTarget), disableDelay); 
        }

        // Prevent retriggering by disabling this script
        this.enabled = false;
    }

    // Disables the target based on selected mode
    private void DisableTarget()
    {
        // Safety check for collider reference
        if (spawnerCollider == null) return;

        // Apply appropriate disable method
        switch (disableMode)
        {
            case DisableSpawnerMode.JustDisableTrigger:
                spawnerCollider.isTrigger = false;
                break;

            case DisableSpawnerMode.DisableEntireCollider:
                spawnerCollider.enabled = false;
                break;

            case DisableSpawnerMode.DisableWholeGameObject:
                gameObject.SetActive(false);
                break;
        }
    }
}