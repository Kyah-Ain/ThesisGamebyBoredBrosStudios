using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions
using UnityEngine.AI; // Grants access to Unity's AI and Navigation system

public class EnemyPool : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    // Represents a single pool of enemy objects
    [System.Serializable]
    public class Pool
    {
        public EnemyFactory.EnemyType enemyType;           // Type of enemies in this pool
        public GameObject prefab;                          // Prefab to instantiate
        public int initialSize;                            // Starting pool size
        public Queue<GameObject> availableObjects = new Queue<GameObject>();  // Ready-to-use objects
        public List<GameObject> allObjects = new List<GameObject>();          // All objects in pool
    }

    [Header("POOL CONFIGURATION")]
    public Pool[] pools; // Configure each enemy type with its own pool settings

    // Dictionary for quick pool lookup by enemy type
    private Dictionary<EnemyFactory.EnemyType, Pool> poolDictionary = new Dictionary<EnemyFactory.EnemyType, Pool>();
    private Transform poolParent;  // Parent transform to keep hierarchy organized

    // ------------------------- METHODS -------------------------

    // Called when the script instance is being loaded
    void Awake()
    {
        // Initialize pool system
        CreatePoolParent();
        InitializePools();
    }

    // Creates parent object to organize pooled enemies in hierarchy
    private void CreatePoolParent()
    {
        // Create a new GameObject to hold pooled enemies and set as child
        poolParent = new GameObject("EnemyPool").transform;
        poolParent.SetParent(this.transform);
    }

    // Initializes all configured pools with their initial objects
    private void InitializePools()
    {
        // Setup each pool defined in inspector
        foreach (Pool pool in pools)
        {
            // Validate pool configuration
            poolDictionary[pool.enemyType] = pool;
            PreWarmPool(pool);
        }

        Debug.Log($"Object Pool initialized with {pools.Length} pool types");
    }

    // Pre-creates initial pool objects to avoid runtime instantiation
    private void PreWarmPool(Pool pool)
    {
        // Create initial pool objects
        for (int i = 0; i < pool.initialSize; i++)
        {
            // Instantiate, configure new enemy, and add to pool
            GameObject enemy = CreateNewEnemy(pool.prefab);
            pool.availableObjects.Enqueue(enemy);
            pool.allObjects.Add(enemy);
        }
    }

    // Retrieves an enemy from the pool, creating new ones if pool is empty
    public GameObject GetEnemy(EnemyFactory.EnemyType enemyType)
    {
        // Validate pool existence
        if (!poolDictionary.ContainsKey(enemyType))
        {
            Debug.LogError($"No pool configured for enemy type: {enemyType}");
            return null;
        }

        // Get the appropriate pool
        Pool pool = poolDictionary[enemyType];
        Debug.Log($"GetEnemy: {enemyType}, Available: {pool.availableObjects.Count}");

        // Reuse existing or create new enemy
        return pool.availableObjects.Count > 0 ? ReusePooledEnemy(pool) : CreateAndExpandPool(pool);
    }

    // Returns an enemy to its pool for future reuse
    public void ReturnEnemy(GameObject enemy, EnemyFactory.EnemyType enemyType)
    {
        // Validate pool existence
        if (!poolDictionary.ContainsKey(enemyType))
        {
            Debug.LogError($"No pool found for enemy type: {enemyType}");
            return;
        }

        Debug.Log($"Returning enemy to pool: {enemy.name}, Type: {enemyType}");

        // Reset enemy state and return to pool
        ResetEnemyState(enemy);
        ReturnToAvailablePool(enemy, enemyType);
    }

    // Gets count of available (inactive) enemies in pool
    public int GetAvailableCount(EnemyFactory.EnemyType enemyType)
    {
        return poolDictionary.ContainsKey(enemyType) ? poolDictionary[enemyType].availableObjects.Count : 0;
    }

    // Reuses existing pooled enemy and activates it
    private GameObject ReusePooledEnemy(Pool pool)
    {
        // Dequeue and activate enemy
        GameObject enemy = pool.availableObjects.Dequeue();
        enemy.SetActive(true);

        Debug.Log($"Reusing pooled enemy: {enemy.name}");
        return enemy;
    }

    // Creates new enemy when pool is empty (dynamic pool expansion)
    private GameObject CreateAndExpandPool(Pool pool)
    {
        Debug.Log($"Pool empty, creating new enemy: {pool.enemyType}");

        // Instantiate, configure, and add to pool
        GameObject enemy = CreateNewEnemy(pool.prefab);
        pool.allObjects.Add(enemy);
        enemy.SetActive(true);

        return enemy;
    }

    // Creates new enemy instance and configures it for pooling
    private GameObject CreateNewEnemy(GameObject prefab)
    {
        // Instantiate enemy and set inactive
        GameObject enemy = Instantiate(prefab, poolParent);
        enemy.SetActive(false);

        Debug.Log($"Creating new enemy: {enemy.name}");

        // Setup pooling components
        ConfigureEnemyForPooling(enemy);
        return enemy;
    }

    // Ensures enemy has proper pool membership configuration
    private void ConfigureEnemyForPooling(GameObject enemy)
    {
        // Find or add pool member component in hierarchy
        EnemyPoolMember poolMember = enemy.GetComponentInChildren<EnemyPoolMember>();
        if (poolMember == null)
        {
            // Add component if missing
            poolMember = enemy.AddComponent<EnemyPoolMember>();
            Debug.Log($"Added EnemyPoolMember to: {enemy.name}");
        }
        else
        {
            Debug.Log($"Found EnemyPoolMember on: {poolMember.gameObject.name}");
        }

        // Set pool reference
        poolMember.SetPool(this);
    }

    // Resets enemy to default state before returning to pool
    private void ResetEnemyState(GameObject enemy)
    {
        // Deactivate and reparent
        enemy.SetActive(false);
        enemy.transform.SetParent(poolParent);

        // Reset combat system
        ResetCombatComponents(enemy);

        // Reset navigation
        ResetNavigationComponents(enemy);

        // Clear transform
        enemy.transform.position = Vector3.zero;
        enemy.transform.rotation = Quaternion.identity;
    }

    // Resets health and combat status for pool reuse
    private void ResetCombatComponents(GameObject enemy)
    {
        // Reset health to max
        CombatManager combat = enemy.GetComponentInChildren<CombatManager>();
        if (combat != null)
        {
            // Set health to max
            combat.health = combat.maxHealth;
            Debug.Log($"Reset health for: {enemy.name}");
        }
    }

    // Resets navigation components to default state
    private void ResetNavigationComponents(GameObject enemy)
    {
        // Reset NavMeshAgent path and stop movement
        NavMeshAgent agent = enemy.GetComponentInChildren<NavMeshAgent>();
        if (agent != null)
        {
            // Clear path and stop agent
            agent.ResetPath();
            agent.isStopped = true;
            Debug.Log($"Reset NavMeshAgent for: {enemy.name}");
        }
    }

    // Returns enemy to available queue for future reuse
    private void ReturnToAvailablePool(GameObject enemy, EnemyFactory.EnemyType enemyType)
    {
        Pool pool = poolDictionary[enemyType];
        pool.availableObjects.Enqueue(enemy);
        Debug.Log($"Returned {enemyType} to pool. Available: {pool.availableObjects.Count}");
    }

    // ------------------------- DEBUGGING METHODS -------------------------

    // Logs current pool status for debugging
    public void DebugPoolStatus()
    {
        // Log summary of each pool's status
        foreach (var kvp in poolDictionary)
        {
            // Get pool and counts
            Pool pool = kvp.Value;
            int activeCount = pool.allObjects.Count - pool.availableObjects.Count;
            Debug.Log($"{kvp.Key}: Total={pool.allObjects.Count}, Active={activeCount}, Available={pool.availableObjects.Count}");
        }
    }

    // Comprehensive pool diagnostics with reference validation
    public void DebugAllPoolReferences()
    {
        Debug.Log("=== COMPREHENSIVE POOL DIAGNOSTICS ===");

        // Log each pool's total objects
        foreach (var kvp in poolDictionary)
        {
            // Get pool
            Pool pool = kvp.Value;
            Debug.Log($"Pool {kvp.Key}: {pool.allObjects.Count} total objects");

            // Validate each object in pool
            ValidatePoolObjects(pool);
        }
    }

    // Validates all objects in pool for proper configuration
    private void ValidatePoolObjects(Pool pool)
    {
        // Check each object in the pool
        for (int i = 0; i < pool.allObjects.Count; i++)
        {
            // Store reference
            GameObject enemy = pool.allObjects[i];

            // Validate reference if not null
            if (enemy == null)
            {
                Debug.LogError($"Object {i}: NULL REFERENCE");
                continue;
            }

            // Check for EnemyPoolMember component
            EnemyPoolMember poolMember = enemy.GetComponent<EnemyPoolMember>();
            if (poolMember == null)
            {
                Debug.LogError($"{enemy.name}: MISSING EnemyPoolMember COMPONENT");
            }
            else
            {
                Debug.Log($"{enemy.name}: Properly configured with EnemyPoolMember");
            }
        }
    }
}