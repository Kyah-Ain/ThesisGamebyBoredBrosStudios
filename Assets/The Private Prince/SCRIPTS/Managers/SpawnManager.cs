using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    [Header("SPAWNER SETTINGS")]
    public Transform[] spawnPoints; // Array of spawn locations in the scene
    [SerializeField]private EnemyFactory _factory; // Reference to factory that creates enemies

    void Start()
    {
        // Get the EnemyFactory component on the same GameObject
        if (_factory == null) 
        {
            _factory = GetComponent<EnemyFactory>();
        }

        // Check if factory component exists, log error if missing
        if (_factory == null)
        {
            Debug.LogError("EnemyFactory component missing!");
            return;
        }

        // Log readiness status with number of available spawn points
        Debug.Log($"Spawner ready with {spawnPoints.Length} spawn points");
    }

    // FOR TESTING: Keyboard shortcuts to spawn enemies manually
    void Update()
    {
        // Keyboard shortcut 1: Spawn guards at all spawn points
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnAtAllPoints(EnemyFactory.EnemyType.Guard);
        }

        // Keyboard shortcut 2: Spawn roamers at all spawn points
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnAtAllPoints(EnemyFactory.EnemyType.Roamer);
        }

        // Keyboard shortcut 3: Spawn mixed wave alternating between guard and roamer
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnMixedWave();
        }
    }

    // Trigger-based spawning when player enters the collider area
    void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player
        if (other.CompareTag("Player"))
        {
            // Spawns random mixed wave when player enters trigger
            //SpawnMixedWave();

            // Spawns roamers only when player enters trigger
            SpawnAtAllPoints(EnemyFactory.EnemyType.Roamer);
        }
    }

    // Method to spawn one enemy at EACH spawn point with the same type
    public void SpawnAtAllPoints(EnemyFactory.EnemyType enemyType)
    {
        // Validate if spawning is possible before proceeding
        if (!IsReadyToSpawn()) return;

        // Iterate through all spawn points and spawn enemies
        foreach (Transform spawnPoint in spawnPoints)
        {
            SpawnEnemyAtPoint(enemyType, spawnPoint);
        }

        // Log the spawn operation details
        Debug.Log($"Spawned {spawnPoints.Length} {enemyType} enemies (1 per spawn point)");
    }

    // Method to spawn mixed enemies across all spawn points
    public void SpawnMixedWave()
    {
        // Validate if spawning is possible before proceeding
        if (!IsReadyToSpawn()) return;

        // Loop through spawn points with index to alternate enemy types
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // Alternate between Guard and Roamer based on even/odd index
            EnemyFactory.EnemyType enemyType = (i % 2 == 0) ?
            EnemyFactory.EnemyType.Guard :
            EnemyFactory.EnemyType.Roamer;

            SpawnEnemyAtPoint(enemyType, spawnPoints[i]);
        }

        // Log the mixed wave spawn operation
        Debug.Log($"Spawned mixed wave across {spawnPoints.Length} spawn points");
    }

    // Method to spawn single enemy at specific point with NavMesh validation
    private void SpawnEnemyAtPoint(EnemyFactory.EnemyType enemyType, Transform spawnPoint)
    {
        // Check if spawn point is on NavMesh before spawning
        if (!NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            Debug.LogError($"Spawn point {spawnPoint.name} is NOT on NavMesh!");
            return;
        }

        // Use factory to create the requested enemy type
        GameObject enemy = _factory.CreateEnemy(enemyType);

        // If enemy was successfully created, set up its position and navigation
        if (enemy != null)
        {
            SetupNavMeshEnemy(enemy, spawnPoint);
            Debug.Log($"Spawned {enemyType} at {spawnPoint.name}");
        }
    }

    // Method to handle NavMeshAgent setup and positioning
    private void SetupNavMeshEnemy(GameObject enemy, Transform spawnPoint)
    {
        // Get the NavMeshAgent component if it exists
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            // Use Warp for NavMesh agents to ensure proper positioning
            agent.Warp(spawnPoint.position);
            agent.transform.rotation = spawnPoint.rotation;
            agent.isStopped = false;
            agent.ResetPath();
        }
        else
        {
            // Fallback positioning for enemies without NavMeshAgent
            enemy.transform.position = spawnPoint.position;
            enemy.transform.rotation = spawnPoint.rotation;
        }
    }

    // Method to validate readiness to spawn enemies
    private bool IsReadyToSpawn()
    {
        // Check if factory reference is available
        if (_factory == null)
        {
            Debug.LogError("Factory is null!");
            return false;
        }

        // Check if spawn points are assigned
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return false;
        }

        // All checks passed, ready to spawn
        return true;
    }

    // Visualize spawn points in editor with NavMesh status coloring
    void OnDrawGizmosSelected()
    {
        // Only draw gizmos if spawn points array exists
        if (spawnPoints != null)
        {
            // Iterate through all spawn points
            foreach (Transform spawn in spawnPoints)
            {
                // Only process valid spawn points
                if (spawn != null)
                {
                    // Check NavMesh status and set color accordingly
                    if (NavMesh.SamplePosition(spawn.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                    {
                        Gizmos.color = Color.green; // Green = valid spawn point on NavMesh
                    }
                    else
                    {
                        Gizmos.color = Color.red; // Red = invalid spawn point not on NavMesh
                    }
                    // Draw visual indicators for spawn points
                    Gizmos.DrawWireSphere(spawn.position, 0.5f);
                    Gizmos.DrawSphere(spawn.position, 0.3f);
                }
            }
        }
    }
}