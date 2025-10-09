using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    public enum EnemyType { Guard, Roamer } // Types of enemies this factory can create

    [Header("PREFAB REFERENCES")]
    private EnemyPool enemyPool;

    public GameObject guardPrefab; // Stationary enemy prefab
    public GameObject roamerPrefab; // Roaming enemy prefab

    [Header("POOLING SETTINGS")]
    public bool useObjectPooling = true;

    void Start()
    {
        enemyPool = GetComponent<EnemyPool>();

        if (enemyPool == null)
        {
            Debug.LogError("EnemyPool component missing! Add EnemyPool to the same GameObject.");
        }
    }

    // Factory method to create enemies based on behaviour type
    public GameObject CreateEnemy(EnemyType type)
    {
        // Use object pool if available
        if (enemyPool != null)
        {
            GameObject enemy = enemyPool.GetEnemy(type);
            if (enemy != null)
            {
                // FIND THE POOL MEMBER ANYWHERE IN HIERARCHY
                EnemyPoolMember poolMember = enemy.GetComponentInChildren<EnemyPoolMember>();
                if (poolMember != null)
                {
                    poolMember.SetEnemyType(type);
                    poolMember.SetPool(enemyPool); // ← Ensure pool reference is set!
                }
                else
                {
                    Debug.LogError($"No EnemyPoolMember found anywhere in {enemy.name} hierarchy!");
                }

                // Reset combat state
                CombatManager combat = enemy.GetComponentInChildren<CombatManager>();
                if (combat != null)
                {
                    combat.ResetCombat();
                }

                return enemy;
            }
        }

        // Fallback to instantiation if pool fails - USE YOUR ORIGINAL CODE
        Debug.LogWarning("Using instantiation fallback - consider setting up object pool");

        // Instantiate the appropriate prefab based on the requested type
        GameObject newEnemy = null;
        switch (type)
        {
            // If the requested type is Guard, then instantiate a stationary enemy
            case EnemyType.Guard:
                if (guardPrefab != null)
                    newEnemy = Instantiate(guardPrefab);
                else
                    Debug.LogError("Guard prefab not assigned!");
                break;

            // If the requested type is Roamer, then instantiate a roaming enemy
            case EnemyType.Roamer:
                if (roamerPrefab != null)
                    newEnemy = Instantiate(roamerPrefab);
                else
                    Debug.LogError("Roamer prefab not assigned!");
                break;
        }

        // Set up pool member for newly instantiated enemy
        if (newEnemy != null)
        {
            EnemyPoolMember poolMember = newEnemy.GetComponent<EnemyPoolMember>();
            if (poolMember == null)
            {
                poolMember = newEnemy.AddComponent<EnemyPoolMember>();
            }
            poolMember.SetEnemyType(type);
            poolMember.SetPool(enemyPool);
        }

        return newEnemy;
    }
}