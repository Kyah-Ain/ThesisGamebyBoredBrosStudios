using UnityEngine;

public class EnemyPoolMember : MonoBehaviour
{
    private EnemyPool pool;
    private EnemyFactory.EnemyType enemyType;
    private GameObject rootPoolObject; // Track the root object

    public void SetPool(EnemyPool enemyPool)
    {
        pool = enemyPool;
    }

    public void SetEnemyType(EnemyFactory.EnemyType type)
    {
        enemyType = type;
    }

    // NEW: Set the root object this component belongs to
    public void SetRootObject(GameObject root)
    {
        rootPoolObject = root;
    }

    // Call this when enemy is defeated
    public void ReturnToPool()
    {
        if (pool != null)
        {
            // Return the ROOT object, not this child object
            GameObject objectToReturn = rootPoolObject != null ? rootPoolObject : this.gameObject;
            pool.ReturnEnemy(objectToReturn, enemyType);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to find pool member in hierarchy AND get the root
    public static (EnemyPoolMember member, GameObject root) FindInHierarchy(GameObject searchRoot)
    {
        EnemyPoolMember member = searchRoot.GetComponent<EnemyPoolMember>();
        if (member != null) return (member, searchRoot);

        member = searchRoot.GetComponentInChildren<EnemyPoolMember>();
        if (member != null)
        {
            // Find the actual enemy root (not the pool parent)
            GameObject enemyRoot = FindEnemyRoot(member.transform);
            return (member, enemyRoot);
        }

        member = searchRoot.GetComponentInParent<EnemyPoolMember>();
        if (member != null)
        {
            GameObject enemyRoot = FindEnemyRoot(member.transform);
            return (member, enemyRoot);
        }

        return (null, null);
    }

    // Helper method to find the enemy root (ignoring pool parent)
    private static GameObject FindEnemyRoot(Transform start)
    {
        Transform current = start;

        // Go up the hierarchy until we find the pool parent or reach top
        while (current.parent != null)
        {
            // Check if parent is the pool (has EnemyPool component)
            if (current.parent.GetComponent<EnemyPool>() != null)
            {
                // This current transform is the enemy root
                return current.gameObject;
            }
            current = current.parent;
        }

        // If no pool parent found, return the topmost object
        return current.gameObject;
    }
}