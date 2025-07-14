using System.Collections.Generic;
using UnityEngine;

public class Enemy_Pool : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        public int poolSize;
        public string poolId;
    }

    [SerializeField] private List<EnemySpawnData> enemyTypes;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var enemyType in enemyTypes)
        {
            Queue<GameObject> enemyPool = new Queue<GameObject>();

            for (int i = 0; i < enemyType.poolSize; i++)
            {
                GameObject enemy = Instantiate(enemyType.enemyPrefab);

                // Assign the pool ID to the enemy
                PooledEnemy pooledEnemy = enemy.GetComponent<PooledEnemy>();
                if (pooledEnemy == null)
                {
                    pooledEnemy = enemy.AddComponent<PooledEnemy>();
                }
                pooledEnemy.poolId = enemyType.poolId;

                enemy.SetActive(false);
                enemyPool.Enqueue(enemy);
            }

            poolDictionary.Add(enemyType.poolId, enemyPool);
        }
    }

    // Method to get the enemy types list
    public List<EnemySpawnData> GetEnemyTypes()
    {
        return enemyTypes;
    }

    public GameObject GetEnemy(GameObject enemyPrefab, Vector3 spawnPosition)
    {
        // Find the poolId for the given prefab
        string poolId = null;
        foreach (var enemyType in enemyTypes)
        {
            if (enemyType.enemyPrefab == enemyPrefab)
            {
                poolId = enemyType.poolId;
                break;
            }
        }

        if (poolId == null)
        {
            Debug.LogWarning($"Pool doesn't contain enemy type: {enemyPrefab.name}");
            return null;
        }

        // Only spawn if there are available enemies in the pool
        if (poolDictionary.ContainsKey(poolId) && poolDictionary[poolId].Count > 0)
        {
            GameObject enemy = poolDictionary[poolId].Dequeue();
            enemy.transform.position = spawnPosition;
            enemy.SetActive(true);
            return enemy;
        }

        // If no enemies available in the pool, don't spawn
        Debug.Log($"No available enemies in pool for: {enemyPrefab.name}");
        return null;
    }

    public void ReturnToPool(GameObject enemy)
    {
        PooledEnemy pooledEnemy = enemy.GetComponent<PooledEnemy>();
        if (pooledEnemy == null)
        {
            Debug.LogWarning("Enemy doesn't have PooledEnemy component");
            return;
        }

        if (!poolDictionary.ContainsKey(pooledEnemy.poolId))
        {
            Debug.LogWarning($"Pool doesn't contain enemy type with ID: {pooledEnemy.poolId}");
            return;
        }

        enemy.SetActive(false);
        poolDictionary[pooledEnemy.poolId].Enqueue(enemy);
    }
}