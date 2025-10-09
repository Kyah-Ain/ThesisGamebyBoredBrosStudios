using UnityEngine;

public class EnemyPoolMember : MonoBehaviour
{
    private EnemyPool pool;
    private EnemyFactory.EnemyType enemyType;

    public void SetPool(EnemyPool enemyPool)
    {
        pool = enemyPool;
        Debug.Log($"Pool reference SET for {gameObject.name}: {pool != null}");
    }

    public void SetEnemyType(EnemyFactory.EnemyType type)
    {
        enemyType = type;
    }

    public void ReturnToPool()
    {
        Debug.Log($"Attempting to return {gameObject.name} to pool. Pool reference: {pool != null}");

        if (pool != null)
        {
            pool.ReturnEnemy(this.gameObject, enemyType);
        }
        else
        {
            Debug.LogError($"No pool reference! Destroying enemy: {gameObject.name}");
            Debug.LogError($"GameObject path: {GetGameObjectPath(this.gameObject)}");
            Debug.LogError($"Active in hierarchy: {gameObject.activeInHierarchy}");
            Destroy(gameObject);
        }
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }

    public static EnemyPoolMember FindInHierarchy(GameObject searchRoot)
    {
        EnemyPoolMember member = searchRoot.GetComponent<EnemyPoolMember>();
        if (member != null)
        {
            Debug.Log($"Found pool member on root: {searchRoot.name}");
            return member;
        }

        member = searchRoot.GetComponentInChildren<EnemyPoolMember>();
        if (member != null)
        {
            Debug.Log($"Found pool member in children: {member.gameObject.name} of {searchRoot.name}");
            return member;
        }

        Debug.Log($"No pool member found in hierarchy of: {searchRoot.name}");
        return null;
    }
}