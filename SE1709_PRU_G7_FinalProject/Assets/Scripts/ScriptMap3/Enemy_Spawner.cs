using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Spawner : MonoBehaviour
{
    public List<Transform> _spawnPoints;
    public float _spawnInterval = 3f;
    private Enemy_Pool enemyPool;
    private bool initialSpawnComplete = false;
    private bool shouldRespawn = false;

    void Start()
    {
        StartCoroutine(InitializeAndSpawn());
    }

    // This method will be called when player dies and respawns
    public void PlayerRespawned()
    {
        Boss boss = FindAnyObjectByType<Boss>();
        if (boss != null)
        {
            boss.ResetBossState();
        }
        shouldRespawn = true;
        StartCoroutine(SpawnEnemiesOnPlayerRespawn());
    }

    IEnumerator SpawnEnemiesOnPlayerRespawn()
    {
        if (shouldRespawn && initialSpawnComplete)
        {
            SpawnEnemies();
            shouldRespawn = false;
        }
        yield return null;
    }

    IEnumerator InitializeAndSpawn()
    {
        yield return new WaitForSeconds(0.1f);
        enemyPool = FindAnyObjectByType<Enemy_Pool>();
        if (enemyPool == null)
        {
            Debug.LogError("Enemy_Pool not found in the scene!");
            yield break;
        }

        // Initial spawn when game starts
        SpawnEnemies();
        initialSpawnComplete = true;
    }

    void SpawnEnemies()
    {
        if (enemyPool == null) return;
        List<Enemy_Pool.EnemySpawnData> enemySpawnList = enemyPool.GetEnemyTypes();
        for (int i = 0; i < enemySpawnList.Count; i++)
        {
            var enemyData = enemySpawnList[i];
            int spawnPointIndex = i % _spawnPoints.Count;
            Vector3 basePosition = _spawnPoints[spawnPointIndex].position;

            for (int j = 0; j < enemyData.poolSize; j++)
            {
                float randomX = Random.Range(-10f, 10f);

                Vector3 spawnPosition = basePosition + new Vector3(randomX, 0, 0);
                enemyPool.GetEnemy(enemyData.enemyPrefab, spawnPosition);
            }
        }
    }
}