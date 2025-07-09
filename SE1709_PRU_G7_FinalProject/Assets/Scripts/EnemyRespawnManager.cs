using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRespawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnPointData
    {
        public Transform spawnPoint;
        [HideInInspector] public GameObject currentEnemy;
    }

    public GameObject enemyPrefab;
    public float respawnDelay = 3f; // Thời gian chờ respawn (giây)
    public List<SpawnPointData> spawnPoints = new List<SpawnPointData>();

    void Start()
    {
        // Spawn enemy ở tất cả các điểm
        foreach (var sp in spawnPoints)
        {
            SpawnEnemyAtPoint(sp);
        }
    }

    void SpawnEnemyAtPoint(SpawnPointData sp)
    {
        GameObject enemy = Instantiate(enemyPrefab, sp.spawnPoint.position, Quaternion.identity);
        sp.currentEnemy = enemy;

        // Gắn script theo dõi chết
        EnemyRespawnWatcher watcher = enemy.AddComponent<EnemyRespawnWatcher>();
        watcher.manager = this;
        watcher.spawnPointData = sp;
    }

    // Hàm này được gọi khi enemy chết
    public void OnEnemyDied(SpawnPointData sp)
    {
        StartCoroutine(RespawnAfterDelay(sp));
    }

    IEnumerator RespawnAfterDelay(SpawnPointData sp)
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnEnemyAtPoint(sp);
    }
}

// Script này sẽ gắn vào mỗi enemy để báo về manager khi bị destroy
public class EnemyRespawnWatcher : MonoBehaviour
{
    public EnemyRespawnManager manager;
    public EnemyRespawnManager.SpawnPointData spawnPointData;

    void OnDestroy()
    {
        // Chỉ gọi khi game đang chạy (tránh gọi khi dừng Play Mode)
        if (manager != null && Application.isPlaying)
        {
            manager.OnEnemyDied(spawnPointData);
        }
    }
}
