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

        // Add pool reference to enemy for returning later
        EnemyPoolMember poolMember = enemy.GetComponent<EnemyPoolMember>();
        if (poolMember == null)
            poolMember = enemy.AddComponent<EnemyPoolMember>();

        poolMember.SetPool(this);

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

        if (pool.availableObjects.Count > 0)
        {
            GameObject enemy = pool.availableObjects.Dequeue();
            enemy.SetActive(true);
            return enemy;
        }
        else
        {
            // Expand pool if empty
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

        // Reset enemy state
        enemy.SetActive(false);
        enemy.transform.SetParent(poolParent);

        // Reset health if exists
        CombatManager combat = enemy.GetComponent<CombatManager>();
        if (combat != null)
        {
            combat.health = combat.maxHealth;
        }

        // Reset NavMeshAgent if exists
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        Pool pool = poolDictionary[enemyType];
        pool.availableObjects.Enqueue(enemy);

        Debug.Log($"Returned {enemyType} to pool. Available: {pool.availableObjects.Count}");
    }

    public int GetAvailableCount(EnemyFactory.EnemyType enemyType)
    {
        return poolDictionary.ContainsKey(enemyType) ? poolDictionary[enemyType].availableObjects.Count : 0;
    }
}