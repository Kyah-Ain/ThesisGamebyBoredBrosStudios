using UnityEngine;

public class EnemyPoolMember : MonoBehaviour
{
    private EnemyPool pool;
    private EnemyFactory.EnemyType enemyType;

    public void SetPool(EnemyPool enemyPool)
    {
        pool = enemyPool;
    }

    public void SetEnemyType(EnemyFactory.EnemyType type)
    {
        enemyType = type;
    }

    // Call this when enemy is defeated
    public void ReturnToPool()
    {
        if (pool != null)
        {
            pool.ReturnEnemy(this.gameObject, enemyType);
        }
        else
        {
            Destroy(gameObject); // Fallback if no pool
        }
    }
}