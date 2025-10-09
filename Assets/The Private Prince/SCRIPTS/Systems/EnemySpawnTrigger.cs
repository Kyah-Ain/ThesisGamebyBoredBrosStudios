using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    public enum SpawnMode
    {
        AllPointsSameType,
        MixedWave
    }

    [Header("SPAWN CONFIGURATION")]
    [Tooltip("Choose which spawn method to call when triggered")]
    public SpawnMode spawnMode = SpawnMode.AllPointsSameType;

    [Tooltip("Only used when SpawnMode is AllPointsSameType")]
    public EnemyFactory.EnemyType enemyType = EnemyFactory.EnemyType.Roamer;

    [Header("SPAWN POINTS")]
    [Tooltip("Assign specific spawn points for this trigger")]
    public Transform[] spawnPoints; // Unique spawn points for this trigger

    [Header("REFERENCES")]
    [Tooltip("Drag your SpawnManager here or it will auto-find")]
    public SpawnManager spawnManager;

    private void Start()
    {
        // Auto-find SpawnManager if not assigned
        if (spawnManager == null)
        {
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
        if (other.CompareTag("Player"))
        {
            TriggerSpawning();
        }
    }

    public void TriggerSpawning()
    {
        if (spawnManager == null) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        // Use temporary spawn points without modifying the manager's permanent ones
        switch (spawnMode)
        {
            case SpawnMode.AllPointsSameType:
                spawnManager.SpawnAtPoints(spawnPoints, enemyType);
                break;
            case SpawnMode.MixedWave:
                spawnManager.SpawnMixedWaveAtPoints(spawnPoints);
                break;
        }
    }

    // Visualize spawn points in editor
    void OnDrawGizmosSelected()
    {
        // Draw trigger bounds
        Gizmos.color = spawnMode == SpawnMode.MixedWave ? Color.yellow : Color.blue;
        if (TryGetComponent<Collider>(out Collider collider))
        {
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }

        // Draw spawn points and connections
        if (spawnPoints != null)
        {
            foreach (Transform spawn in spawnPoints)
            {
                if (spawn != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(spawn.position, 0.5f);
                    Gizmos.DrawSphere(spawn.position, 0.3f);

                    // Draw line from trigger to spawn point
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(transform.position, spawn.position);
                }
            }
        }
    }
}