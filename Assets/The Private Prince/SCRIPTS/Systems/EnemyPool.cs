using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public EnemyFactory.EnemyType enemyType;
        public GameObject prefab;
        public int initialSize;
        public Queue<GameObject> availableObjects = new Queue<GameObject>();
        public List<GameObject> allObjects = new List<GameObject>();
    }

    [Header("POOL SETTINGS")]
    public Pool[] pools;

    private Dictionary<EnemyFactory.EnemyType, Pool> poolDictionary = new Dictionary<EnemyFactory.EnemyType, Pool>();
    private Transform poolParent;

    void Awake()
    {
        poolParent = new GameObject("EnemyPool").transform;
        poolParent.SetParent(this.transform);

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (Pool pool in pools)
        {
            poolDictionary[pool.enemyType] = pool;

            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject enemy = CreateNewEnemy(pool.prefab);
                pool.availableObjects.Enqueue(enemy);
                pool.allObjects.Add(enemy);
            }
        }

        Debug.Log($"Object Pool initialized with {pools.Length} pool types");
    }

    private GameObject CreateNewEnemy(GameObject prefab)
    {
        GameObject enemy = Instantiate(prefab, poolParent);
        enemy.SetActive(false);

        Debug.Log($"Creating new enemy: {enemy.name}");

        // Add EnemyPoolMember to the root object
        EnemyPoolMember poolMember = enemy.GetComponent<EnemyPoolMember>();
        if (poolMember == null)
        {
            poolMember = enemy.AddComponent<EnemyPoolMember>();
        }

        poolMember.SetPool(this);
        // EnemyType will be set when the enemy is actually spawned

        return enemy;
    }

    public GameObject GetEnemy(EnemyFactory.EnemyType enemyType)
    {
        if (!poolDictionary.ContainsKey(enemyType))
        {
            Debug.LogError($"No pool found for enemy type: {enemyType}");
            return null;
        }

        Pool pool = poolDictionary[enemyType];
        Debug.Log($"GetEnemy: {enemyType}, Available: {pool.availableObjects.Count}");

        if (pool.availableObjects.Count > 0)
        {
            GameObject enemy = pool.availableObjects.Dequeue();
            enemy.SetActive(true);

            // FIXED DEBUG: Just log that we're reusing
            Debug.Log($"Reusing pooled enemy: {enemy.name}");

            return enemy;
        }
        else
        {
            Debug.Log($"Pool empty for {enemyType}, creating new enemy");
            GameObject enemy = CreateNewEnemy(pool.prefab);
            pool.allObjects.Add(enemy);
            enemy.SetActive(true);
            return enemy;
        }
    }

    public void ReturnEnemy(GameObject enemy, EnemyFactory.EnemyType enemyType)
    {
        if (!poolDictionary.ContainsKey(enemyType))
        {
            Debug.LogError($"No pool found for enemy type: {enemyType}");
            return;
        }

        Debug.Log($"Returning enemy to pool: {enemy.name}, Type: {enemyType}");

        // Reset enemy state
        enemy.SetActive(false);
        enemy.transform.SetParent(poolParent);

        // Reset components - find them anywhere in the hierarchy
        CombatManager combat = enemy.GetComponentInChildren<CombatManager>();
        if (combat != null)
        {
            combat.health = combat.maxHealth;
            Debug.Log($"Reset health for {enemy.name}");
        }

        NavMeshAgent agent = enemy.GetComponentInChildren<NavMeshAgent>();
        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
            Debug.Log($"Reset NavMeshAgent for {enemy.name}");
        }

        // Reset position and rotation
        enemy.transform.position = Vector3.zero;
        enemy.transform.rotation = Quaternion.identity;

        Pool pool = poolDictionary[enemyType];
        pool.availableObjects.Enqueue(enemy);

        Debug.Log($"Returned {enemyType} to pool. Available: {pool.availableObjects.Count}");
    }

    public int GetAvailableCount(EnemyFactory.EnemyType enemyType)
    {
        return poolDictionary.ContainsKey(enemyType) ? poolDictionary[enemyType].availableObjects.Count : 0;
    }

    // DEBUG METHOD - Add this to see what's happening
    public void DebugPoolStatus()
    {
        foreach (var kvp in poolDictionary)
        {
            Pool pool = kvp.Value;
            Debug.Log($"{kvp.Key}: Total={pool.allObjects.Count}, Active={pool.allObjects.Count - pool.availableObjects.Count}, Available={pool.availableObjects.Count}");
        }
    }

    // Add this method to EnemyPool to check all references
    public void DebugAllPoolReferences()
    {
        Debug.Log("=== DEBUGGING ALL POOL REFERENCES ===");

        foreach (var kvp in poolDictionary)
        {
            Pool pool = kvp.Value;
            Debug.Log($"Pool {kvp.Key}: {pool.allObjects.Count} total objects");

            for (int i = 0; i < pool.allObjects.Count; i++)
            {
                GameObject enemy = pool.allObjects[i];
                if (enemy == null)
                {
                    Debug.LogError($"  Object {i}: NULL REFERENCE");
                    continue;
                }

                EnemyPoolMember poolMember = enemy.GetComponent<EnemyPoolMember>();
                if (poolMember == null)
                {
                    Debug.LogError($"  {enemy.name}: NO EnemyPoolMember COMPONENT");
                }
                else
                {
                    // We can't check the private pool field directly, but we can see if it errors when returning
                    Debug.Log($"  {enemy.name}: Has EnemyPoolMember component");
                }
            }
        }
    }
}