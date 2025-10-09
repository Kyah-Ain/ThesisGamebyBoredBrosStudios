using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions

public class EnemySpawnTrigger : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // Defines how enemies should be spawned when triggered
    public enum SpawnMode
    {
        AllPointsSameType,  // Spawn same enemy type at all points
        MixedWave           // Spawn different enemy types across points
    }

    [Header("SPAWN CONFIGURATION")]
    public SpawnMode spawnMode = SpawnMode.AllPointsSameType; // Selected spawn mode
    public EnemyFactory.EnemyType enemyType = EnemyFactory.EnemyType.Roamer; // Enemy type for uniform spawning

    [Header("SPAWN POINTS")]
    public Transform[] spawnPoints; // Unique spawn points for this trigger

    [Header("REFERENCES")]
    public SpawnManager spawnManager; // Reference to spawn manager component

    // ------------------------- METHODS -------------------------

    // Start is called before the first frame update
    private void Start()
    {
        // Auto-find SpawnManager if not assigned
        if (spawnManager == null)
        {
            // Store reference to the first SpawnManager found in the scene
            spawnManager = FindObjectOfType<SpawnManager>();
        }

        // Log warning if still not found
        if (spawnManager == null)
        {
            Debug.LogError("SpawnManager not found! Assign it in the inspector or make sure one exists in the scene.");
        }
    }

    // Trigger-based spawning when player enters the collider area
    void OnTriggerEnter(Collider other)
    {
        // Only trigger for player objects
        if (other.CompareTag("Player"))
        {
            TriggerSpawning();
        }
    }

    // Handles the spawning logic when trigger is activated
    public void TriggerSpawning()
    {
        // Validate spawn manager availability
        if (spawnManager == null)
        {
            Debug.LogError("Cannot spawn enemies: SpawnManager is null!");
            return;
        }

        // Validate spawn points assignment
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned to this trigger!");
            return;
        }

        // Execute appropriate spawn method based on mode
        switch (spawnMode)
        {
            case SpawnMode.AllPointsSameType:
                spawnManager.SpawnAtPoints(spawnPoints, enemyType);
                Debug.Log($"Spawned {enemyType} at {spawnPoints.Length} specific points");
                break;

            case SpawnMode.MixedWave:
                spawnManager.SpawnMixedWaveAtPoints(spawnPoints);
                Debug.Log($"Spawned mixed wave at {spawnPoints.Length} specific points");
                break;
        }
    }

    // Visualize spawn points and trigger area in editor
    void OnDrawGizmosSelected()
    {
        // Draw trigger bounds with color coding based on spawn mode
        Gizmos.color = spawnMode == SpawnMode.MixedWave ? Color.yellow : Color.blue;
        if (TryGetComponent<Collider>(out Collider collider))
        {
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }

        // Draw spawn points and connections to trigger
        if (spawnPoints != null)
        {
            foreach (Transform spawn in spawnPoints)
            {
                if (spawn != null)
                {
                    // Draw spawn point indicator
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(spawn.position, 0.5f);
                    Gizmos.DrawSphere(spawn.position, 0.3f);

                    // Draw connection line from trigger to spawn point
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(transform.position, spawn.position);
                }
            }
        }
    }
}