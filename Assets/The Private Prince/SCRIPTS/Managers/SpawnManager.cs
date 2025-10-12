using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions
using UnityEngine.AI; // Grants access to Unity's AI and Navigation system

public class SpawnManager : MonoBehaviour
{
    [Header("SPAWNER SETTINGS")]
    public Transform[] spawnPoints; // Array of spawn locations in the scene
    [SerializeField] private EnemyFactory _factory; // Reference to factory that creates enemies

    [Header("NAVMESH SETTINGS")]
    [Tooltip("How far to search for valid NavMesh position when spawning")]
    public float spawnSearchRadius = 3.0f; // Configurable spawn position search radius

    [Tooltip("How far to search for NavMesh in debug visualizations")]
    public float debugSearchRadius = 5.0f; // Configurable debug search radius

    [Tooltip("Distance threshold to show adjustment warnings")]
    public float adjustmentWarningThreshold = 0.1f; // Minimum distance to trigger adjustment warnings

    [Header("DEBUG SETTINGS")]
    public bool enableDebugLogs = true; // Toggle for detailed spawn debugging

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
        Debug.Log($"Spawner ready with {spawnPoints.Length} spawn points (Search Radius: {spawnSearchRadius})");

        // Debug all spawn points on start
        if (enableDebugLogs)
        {
            DebugAllSpawnPoints();
        }
    }

    // FOR TESTING: Keyboard shortcuts to spawn enemies manually
    void Update()
    {
        // Debug pool references with P key
        if (Input.GetKeyDown(KeyCode.P))
        {
            EnemyPool enemyPool = GetComponent<EnemyPool>();
            if (enemyPool != null)
            {
                enemyPool.DebugAllPoolReferences();
            }
        }

        // Debug specific spawn point with O key
        if (Input.GetKeyDown(KeyCode.O))
        {
            DebugSpecificSpawnPoint(0); // Debug first spawn point
        }

        // Increase search radius with [ key
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            spawnSearchRadius += 1.0f;
            Debug.Log($"Spawn search radius increased to: {spawnSearchRadius}");
        }

        // Decrease search radius with ] key
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            spawnSearchRadius = Mathf.Max(0.5f, spawnSearchRadius - 1.0f);
            Debug.Log($"Spawn search radius decreased to: {spawnSearchRadius}");
        }

        // Keyboard shortcut 1: Spawn guards at all spawn points
        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnAtAllPoints(EnemyFactory.EnemyType.Guard);
        }

        // Keyboard shortcut 2: Spawn roamers at all spawn points
        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnAtAllPoints(EnemyFactory.EnemyType.Roamer);
        }

        // Keyboard shortcut 3: Spawn mixed wave alternating between guard and roamer
        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnMixedWave();
        }
    }

    // Method to spawn one enemy at EACH spawn point with the same type
    public void SpawnAtAllPoints(EnemyFactory.EnemyType enemyType)
    {
        // Validate if spawning is possible before proceeding
        if (!IsReadyToSpawn()) return;

        // Just spawn at all points and let the pool handle creating new ones if needed
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

    // Method to spawn single enemy at specific point with enhanced NavMesh validation
    public void SpawnEnemyAtPoint(EnemyFactory.EnemyType enemyType, Transform spawnPoint)
    {
        // Enhanced spawn point validation with detailed debugging
        Vector3 validatedPosition = ValidateAndGetSpawnPosition(spawnPoint);

        // Use factory to create the requested enemy type
        GameObject enemy = _factory.CreateEnemy(enemyType);

        // If enemy was successfully created, set up its position and navigation
        if (enemy != null)
        {
            SetupNavMeshEnemy(enemy, validatedPosition, spawnPoint.rotation);
            if (enableDebugLogs)
            {
                Debug.Log($"Spawned {enemyType} at {spawnPoint.name} -> Position: {validatedPosition}");
            }
        }
    }

    // Enhanced method to validate spawn position and get NavMesh-corrected position
    private Vector3 ValidateAndGetSpawnPosition(Transform spawnPoint)
    {
        // Detailed spawn point debugging
        if (enableDebugLogs)
        {
            DebugSpawnPoint(spawnPoint, spawnPoint.name);
        }

        // Check if spawn point is on NavMesh with CONFIGURABLE search radius
        if (NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, spawnSearchRadius, NavMesh.AllAreas))
        {
            float distance = Vector3.Distance(spawnPoint.position, hit.position);
            if (distance > adjustmentWarningThreshold)
            {
                Debug.LogWarning($"Spawn point {spawnPoint.name} adjusted by {distance:F2} units to fit NavMesh (Radius: {spawnSearchRadius})");
                Debug.Log($"Original: {spawnPoint.position} -> Corrected: {hit.position}");
            }
            return hit.position; // Return the NavMesh-corrected position
        }
        else
        {
            Debug.LogError($"Spawn point {spawnPoint.name} is NOT on NavMesh! (Search Radius: {spawnSearchRadius}). Using original position.");
            return spawnPoint.position; // Fallback to original position
        }
    }

    // Enhanced method to handle NavMeshAgent setup and positioning
    private void SetupNavMeshEnemy(GameObject enemy, Vector3 position, Quaternion rotation)
    {
        // Get the NavMeshAgent component if it exists
        NavMeshAgent agent = enemy.GetComponentInChildren<NavMeshAgent>();
        if (agent != null)
        {
            // Ensure agent is enabled and use Warp for proper NavMesh positioning
            agent.enabled = true;
            agent.Warp(position);
            agent.transform.rotation = rotation;
            agent.isStopped = false;
            agent.ResetPath();

            if (enableDebugLogs)
            {
                Debug.Log($"NavMeshAgent positioned at: {agent.transform.position}, OnNavMesh: {agent.isOnNavMesh}");
            }
        }
        else
        {
            // Fallback positioning for enemies without NavMeshAgent
            enemy.transform.position = position;
            enemy.transform.rotation = rotation;
            Debug.LogWarning($"Enemy {enemy.name} has no NavMeshAgent, using direct positioning");
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

    // ------------------------- DEBUGGING METHODS -------------------------

    // Comprehensive spawn point debugging method
    public void DebugSpawnPoint(Transform spawnPoint, string pointName)
    {
        Debug.Log($"=== SPAWN POINT DEBUG: {pointName} ===");
        Debug.Log($"World Position: {spawnPoint.position}");
        Debug.Log($"Local Position: {spawnPoint.localPosition}");
        Debug.Log($"Spawn Search Radius: {spawnSearchRadius}");

        // Check NavMesh availability with CONFIGURABLE debug search radius
        if (NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, debugSearchRadius, NavMesh.AllAreas))
        {
            float distance = Vector3.Distance(spawnPoint.position, hit.position);
            Debug.Log($"✓ Found NavMesh at distance: {distance:F2} (Max: {debugSearchRadius})");
            Debug.Log($"✓ NavMesh Position: {hit.position}");
            Debug.Log($"✓ NavMesh Area: {hit.mask}");

            // Visualize the difference in Scene view
            Debug.DrawLine(spawnPoint.position, hit.position, Color.yellow, 10f);
            Debug.DrawRay(hit.position, Vector3.up * 3, Color.green, 10f);
        }
        else
        {
            Debug.LogError($"✗ NO NavMesh found within {debugSearchRadius} units of spawn point!");
            Debug.DrawRay(spawnPoint.position, Vector3.up * 5, Color.red, 10f);
        }

        // Check for colliders that might be blocking
        Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, 2f);
        Debug.Log($"Colliders nearby: {colliders.Length}");
        foreach (Collider col in colliders)
        {
            Debug.Log($" - {col.name} (Layer: {LayerMask.LayerToName(col.gameObject.layer)})");
        }
        Debug.Log("=== END DEBUG ===");
    }

    // Debug all spawn points in the array
    private void DebugAllSpawnPoints()
    {
        Debug.Log($"=== SPAWN POINT ANALYSIS ({spawnPoints.Length} points) ===");
        Debug.Log($"Search Radius: {spawnSearchRadius}, Debug Radius: {debugSearchRadius}");

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                bool isValid = NavMesh.SamplePosition(spawnPoints[i].position, out NavMeshHit hit, spawnSearchRadius, NavMesh.AllAreas);
                float distance = isValid ? Vector3.Distance(spawnPoints[i].position, hit.position) : 0f;
                Debug.Log($"Point {i}: {spawnPoints[i].name} - Valid: {isValid} - Adjust Distance: {distance:F2} - Position: {spawnPoints[i].position}");
            }
            else
            {
                Debug.LogError($"Point {i}: NULL REFERENCE!");
            }
        }
        Debug.Log("=== END ANALYSIS ===");
    }

    // Debug specific spawn point by index
    public void DebugSpecificSpawnPoint(int index)
    {
        if (index >= 0 && index < spawnPoints.Length && spawnPoints[index] != null)
        {
            DebugSpawnPoint(spawnPoints[index], $"Index_{index}_{spawnPoints[index].name}");
        }
        else
        {
            Debug.LogError($"Invalid spawn point index: {index}");
        }
    }

    // Visualize spawn points in editor with enhanced NavMesh status coloring
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
                    // Check NavMesh status and set color accordingly with CONFIGURABLE search radius
                    if (NavMesh.SamplePosition(spawn.position, out NavMeshHit hit, spawnSearchRadius, NavMesh.AllAreas))
                    {
                        float distance = Vector3.Distance(spawn.position, hit.position);
                        if (distance < adjustmentWarningThreshold)
                        {
                            Gizmos.color = Color.green; // Green = perfectly on NavMesh
                        }
                        else
                        {
                            Gizmos.color = Color.yellow; // Yellow = adjusted to NavMesh
                            // Draw line showing adjustment
                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(spawn.position, hit.position);
                            Gizmos.color = Color.yellow;
                        }
                    }
                    else
                    {
                        Gizmos.color = Color.red; // Red = invalid spawn point not on NavMesh
                    }
                    // Draw visual indicators for spawn points
                    Gizmos.DrawWireSphere(spawn.position, 0.5f);
                    Gizmos.DrawSphere(spawn.position, 0.3f);

                    // Draw search radius visualization
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(spawn.position, spawnSearchRadius);
                }
            }
        }
    }

    // Public methods for external triggers
    public void SpawnAtPoints(Transform[] points, EnemyFactory.EnemyType enemyType)
    {
        foreach (Transform spawnPoint in points)
        {
            SpawnEnemyAtPoint(enemyType, spawnPoint);
        }
    }

    public void SpawnMixedWaveAtPoints(Transform[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            EnemyFactory.EnemyType enemyType = (i % 2 == 0) ?
            EnemyFactory.EnemyType.Guard : EnemyFactory.EnemyType.Roamer;
            SpawnEnemyAtPoint(enemyType, points[i]);
        }
    }

    // Public method to adjust search radius at runtime
    public void SetSpawnSearchRadius(float newRadius)
    {
        spawnSearchRadius = Mathf.Max(0.1f, newRadius);
        Debug.Log($"Spawn search radius set to: {spawnSearchRadius}");
    }

    // Public method to adjust debug search radius at runtime
    public void SetDebugSearchRadius(float newRadius)
    {
        debugSearchRadius = Mathf.Max(0.1f, newRadius);
        Debug.Log($"Debug search radius set to: {debugSearchRadius}");
    }
}