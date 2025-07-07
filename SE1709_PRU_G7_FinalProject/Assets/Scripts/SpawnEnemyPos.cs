using UnityEngine;

public class SpawnEnemyPos : MonoBehaviour
{
    public GameObject enemyPrefab;

    public Transform[] areaASpawnPoints;
    //public Transform[] areaBSpawnPoints;
    //public Transform[] areaCSpawnPoints;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Khu vực A: spawn 1 enemy ở vị trí ngẫu nhiên
        SpawnEnemies(areaASpawnPoints, 1);

        //// Khu vực B: spawn 2 enemy ở 2 vị trí ngẫu nhiên (không trùng)
        //SpawnEnemies(areaBSpawnPoints, 2);

        //// Khu vực C: spawn 3 enemy ở 3 vị trí ngẫu nhiên (không trùng)
        //SpawnEnemies(areaCSpawnPoints, 3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void SpawnEnemies(Transform[] spawnPoints, int enemyCount)
    {
        // Tạo mảng index để tránh spawn trùng vị trí
        System.Collections.Generic.List<int> usedIndexes = new System.Collections.Generic.List<int>();
        for (int i = 0; i < enemyCount; i++)
        {
            int idx;
            do
            {
                idx = Random.Range(0, spawnPoints.Length);
            } while (usedIndexes.Contains(idx));
            usedIndexes.Add(idx);

            Instantiate(enemyPrefab, spawnPoints[idx].position, Quaternion.identity);
        }
    }
}
