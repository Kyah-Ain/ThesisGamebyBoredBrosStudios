using System.Collections; // Grants access to collections and data structures like ArrayList, Hashtable, etc.
using System.Collections.Generic; // Grants access to generic collections like List, Dictionary, etc.
using UnityEngine; // Grants access to Unity's core classes and functions, such as MonoBehaviour, GameObject, Transform, etc.

public class EnemyPoolMember : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------

    private EnemyPool pool; // Reference to the pool this object belongs to
    private EnemyFactory.EnemyType enemyType;  // Type classification for proper pool return

    // ------------------------- METHODS -------------------------

    // Sets the pool reference for this pool member
    public void SetPool(EnemyPool enemyPool)
    {
        // Assign the provided pool reference
        pool = enemyPool;
        Debug.Log($"Pool reference SET for {gameObject.name}: {(pool != null ? "SUCCESS" : "FAILED")}");
    }

    // Sets the enemy type for proper categorization in the pool
    public void SetEnemyType(EnemyFactory.EnemyType type)
    {
        // Assign the provided enemy type
        enemyType = type;
    }

    // Returns this object to its pool for reuse, or destroys it if no pool exists
    public void ReturnToPool()
    {
        Debug.Log($"Attempting to return {gameObject.name} to pool...");

        // Check if pool reference exists
        if (pool != null)
        {
            // Return to the associated pool
            pool.ReturnEnemy(this.gameObject, enemyType);
        }
        else
        {
            // Handle case where pool reference is missing
            HandleMissingPoolReference();
        }
    }

    // Searches for EnemyPoolMember component in GameObject hierarchy
    public static EnemyPoolMember FindInHierarchy(GameObject searchRoot)
    {
        // Check root object first for optimal performance
        EnemyPoolMember member = searchRoot.GetComponent<EnemyPoolMember>();
        if (member != null)
        {
            Debug.Log($"Found pool member on root: {searchRoot.name}");
            return member;
        }

        // Search children if not found on root
        member = searchRoot.GetComponentInChildren<EnemyPoolMember>();
        if (member != null)
        {
            Debug.Log($"Found pool member in child: {member.gameObject.name}");
            return member;
        }

        Debug.Log($"No pool member found in hierarchy of: {searchRoot.name}");
        return null;
    }

    // Handles the case where pool reference is missing with detailed diagnostics
    private void HandleMissingPoolReference()
    {
        Debug.LogError($"No pool reference! Destroying enemy: {gameObject.name}");
        Debug.LogError($"GameObject path: {GetGameObjectPath(this.gameObject)}");
        Debug.LogError($"Active in hierarchy: {gameObject.activeInHierarchy}");

        // Destroy object since it can't be pooled
        Destroy(gameObject);
    }

    // Generates full hierarchical path for debugging purposes
    private string GetGameObjectPath(GameObject obj)
    {
        // Sets initial path & current transform
        string path = obj.name;
        Transform current = obj.transform;

        // Traverse up the hierarchy to build full path
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        // Return the constructed path
        return path;
    }
}