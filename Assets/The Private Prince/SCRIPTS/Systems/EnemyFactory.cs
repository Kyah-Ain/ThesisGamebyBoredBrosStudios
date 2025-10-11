using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.

public class EnemyFactory : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    public enum EnemyType
    {
        Guard,      // Stationary defensive enemy
        Roamer      // Patrol-based mobile enemy
    }

    [Header("PREFAB VARIANTS")]
    public GameObject roamerPrefab; // Prefab for roamer-type enemy
    public GameObject guardPrefab; // Prefab for guard-type enemy

    [Header("POOLING SETTINGS")]
    private EnemyPool enemyPool;  // Reference to the object pool manager
    public bool useObjectPooling = true; // Toggle for using pooling or direct instantiation

    // ------------------------- METHODS -------------------------

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to EnemyPool on the same GameObject
        enemyPool = GetComponent<EnemyPool>();

        // Validate critical dependency
        if (enemyPool == null)
        {
            Debug.LogError("EnemyPool component missing! Add EnemyPool to the same GameObject.");
        }
    }

    // Creates an enemy of specified type, using pooling if available
    public GameObject CreateEnemy(EnemyType type)
    {
        // PRIMARY PATH: Use object pooling for optimal performance
        if (enemyPool != null)
        {
            // Attempt to get enemy from the pool
            GameObject enemy = GetEnemyFromPool(type);
            if (enemy != null) return enemy;
        }

        // FALLBACK PATH: Direct instantiation if pooling fails
        return CreateEnemyDirectly(type);
    }

    // Gets an enemy from the pool and configures it
    private GameObject GetEnemyFromPool(EnemyType type)
    {
        // Request enemy from the pool
        GameObject enemy = enemyPool.GetEnemy(type);
        if (enemy == null) return null;

        // Setup pooled enemy components and reset state
        ConfigurePooledEnemy(enemy, type);
        return enemy;
    }

    // Configures pooled enemy with necessary components and resets state
    private void ConfigurePooledEnemy(GameObject enemy, EnemyType type)
    {
        // Locate pool member component in hierarchy
        EnemyPoolMember poolMember = enemy.GetComponentInChildren<EnemyPoolMember>();
        if (poolMember != null)
        {
            // Set up pooling references
            poolMember.SetEnemyType(type);
            poolMember.SetPool(enemyPool);
        }
        else
        {
            Debug.LogError($"No EnemyPoolMember found in {enemy.name} hierarchy!");
        }

        // Reset combat state for reused enemy
        ResetEnemyCombatState(enemy);
    }

    // Resets combat-related components to default state
    private void ResetEnemyCombatState(GameObject enemy)
    {
        // Get CombatManager component and reset its state
        CombatManager combat = enemy.GetComponentInChildren<CombatManager>();
        if (combat != null)
        {
            combat.ResetCombat();
        }
    }

    // Directly instantiates enemy without pooling
    private GameObject CreateEnemyDirectly(EnemyType type)
    {
        Debug.LogWarning("Using instantiation fallback - object pool unavailable");

        // Instantiate enemy prefab based on type specified
        GameObject newEnemy = InstantiateEnemyByType(type);
        if (newEnemy != null)
        {
            // Ensure new enemy has pooling components for future use
            ConfigureNewEnemy(newEnemy, type);
        }

        return newEnemy;
    }

    // Instantiates enemy prefab based on type
    private GameObject InstantiateEnemyByType(EnemyType type)
    {
        // NPC variants based on enemy type
        switch (type)
        {
            case EnemyType.Guard:
                return InstantiateWithValidation(guardPrefab, "Guard");

            case EnemyType.Roamer:
                return InstantiateWithValidation(roamerPrefab, "Roamer");

            default:
                Debug.LogError($"Unknown enemy type: {type}");
                return null;
        }
    }

    // Instantiates prefab with null check and error logging
    private GameObject InstantiateWithValidation(GameObject prefab, string enemyName)
    {
        // Check for null prefab reference
        if (prefab != null) return Instantiate(prefab);

        Debug.LogError($"{enemyName} prefab not assigned in inspector!");
        return null;
    }

    // Configures newly instantiated enemy with pooling components
    private void ConfigureNewEnemy(GameObject enemy, EnemyType type)
    {
        // Ensure enemy has pool membership component
        EnemyPoolMember poolMember = enemy.GetComponent<EnemyPoolMember>();
        if (poolMember == null)
        {
            // Add component if missing
            poolMember = enemy.AddComponent<EnemyPoolMember>();
        }

        // Set up pooling references
        poolMember.SetEnemyType(type);
        poolMember.SetPool(enemyPool);
    }
}