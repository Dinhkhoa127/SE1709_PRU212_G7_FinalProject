using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller cho EndGame scene - xử lý restart và main menu
/// </summary>
/// 

[Serializable]
public class ResultData
{
    public string Name;
    public string PlayTime;
    public int EnemiesKilled;
 
}

public class EndGameController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;
    
    public TMPro.TextMeshProUGUI playTimeText;
    public TMPro.TextMeshProUGUI enemiesKilledText;
    [SerializeField] private TMP_InputField nameInputField;


    void Start()
    {
        // Setup button events
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (playTimeText != null)
        {
            float time = GameManager.Instance.totalPlayTime;
            int hours = Mathf.FloorToInt(time / 3600f);
            int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            playTimeText.text = $"{hours:00}:{minutes:00}:{seconds:00}";
        }
        if (enemiesKilledText != null)
        {
            int killed = GameManager.Instance.totalEnemiesKilled;
            //int total = GameManager.Instance.totalEnemiesInGame;
            //int totalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
            enemiesKilledText.text = $"{killed}";
        }

        if (nameInputField != null)
        {
            nameInputField.onSubmit.AddListener(OnNameSubmitted);
        }
    }
    private void OnNameSubmitted(string playerName)
    {
        SaveResultToJson(playerName);
    }
    private void SaveResultToJson(string playerName)
    {
        string path = Path.Combine(Application.persistentDataPath, "result1.json");
        var dataList = new ResultDataList();

        // Load existing data if file exists
        if (File.Exists(path))
        {
            string existingJson = File.ReadAllText(path);
            if (!string.IsNullOrWhiteSpace(existingJson) && existingJson.TrimStart().StartsWith("{"))
            {
                dataList = JsonUtility.FromJson<ResultDataList>(existingJson);
            }
        }

        var newResult = new ResultData
        {
            Name = playerName,
            PlayTime = playTimeText != null ? playTimeText.text : "",
            EnemiesKilled = (GameManager.Instance != null) ? GameManager.Instance.totalEnemiesKilled : 0,
        };

        var resultsList = new List<ResultData>();
        if (dataList.results != null)
            resultsList.AddRange(dataList.results);
        resultsList.Add(newResult);
        dataList.results = resultsList.ToArray();

        string json = JsonUtility.ToJson(dataList, true);
        File.WriteAllText(path, json);

        Debug.Log($"Result saved to {path}");
    }

    public void RestartGame()
    {
        if (AudioController.instance != null)
            AudioController.instance.PlayClickSound();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartFromEndGame();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Map1");
        }
    }

    public void GoToMainMenu()
    {
        if (AudioController.instance != null)
            AudioController.instance.PlayClickSound();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    public void QuitGame()
    {
        if (AudioController.instance != null)
            AudioController.instance.PlayClickSound();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
    public void BackLeaderBoard()
    {
        AudioController.instance?.PlayClickSound();
        GameManager.Instance.LoadScene("LeaderBoard");
    }
}