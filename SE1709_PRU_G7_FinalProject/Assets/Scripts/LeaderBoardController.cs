using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using static EndGameController;





public class LeaderController : MonoBehaviour
{
    [Header("Leaderboard UI")]
    [SerializeField] private Transform leaderboardContainer; // Assign in Inspector
    [SerializeField] private GameObject leaderboardRowPrefab; // Assign in Inspector

    // Helper to parse "hh:mm:ss" to seconds
    private int ParseTimeToSeconds(string time)
    {
        if (TimeSpan.TryParse(time, out var ts))
            return (int)ts.TotalSeconds;
        // fallback: try manual split
        var parts = time.Split(':');
        if (parts.Length == 3 &&
            int.TryParse(parts[0], out int h) &&
            int.TryParse(parts[1], out int m) &&
            int.TryParse(parts[2], out int s))
        {
            return h * 3600 + m * 60 + s;
        }
        return int.MaxValue; // If invalid, treat as slowest
    }

    public void LoadResult()
    {
        string path = Path.Combine(Application.persistentDataPath, "result1.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
            {
                Debug.LogWarning("result1.json is empty or not a valid object.");
                return;
            }
            ResultDataList dataList = JsonUtility.FromJson<ResultDataList>(json);

            // Clear old rows
            foreach (Transform child in leaderboardContainer)
                Destroy(child.gameObject);

            if (dataList.results != null && dataList.results.Length > 0)
            {
                // Sort by EnemiesKilled DESC, then PlayTime ASC
                var sorted = new List<ResultData>(dataList.results);
                sorted.Sort((a, b) =>
                {
                    int cmp = b.EnemiesKilled.CompareTo(a.EnemiesKilled);
                    if (cmp == 0)
                        cmp = ParseTimeToSeconds(a.PlayTime).CompareTo(ParseTimeToSeconds(b.PlayTime));
                    return cmp;
                });

                // Show top 5
                int count = Mathf.Min(5, sorted.Count);
                for (int i = 0; i < count; i++)
                {
                    var result = sorted[i];
                    GameObject row = Instantiate(leaderboardRowPrefab, leaderboardContainer);
                    var texts = row.GetComponentsInChildren<TextMeshProUGUI>();
                    if (texts.Length >= 3)
                    {
                        texts[0].text = result.Name;
                        texts[1].text = result.EnemiesKilled.ToString();
                        texts[2].text = result.PlayTime;
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No result1.json found to display leader data.");
        }
    }

    void Start()
    {
        LoadResult();
    }
}