using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MenuController - Quản lý navigation giữa các scenes từ menu
/// Đã được update để tích hợp với GameManager system
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("Audio Feedback")]
    [Tooltip("Play click sound khi chuyển scene")]
    public bool playClickSound = true;
    
    #region Game Maps
    /// <summary>
    /// Load Map1 - Level đầu tiên
    /// </summary>
    public void StartGameMap1()
    {
        LoadGameScene("Map1");
    }
    
    /// <summary>
    /// Load Map2 - Level thứ hai
    /// </summary>
    public void StartGameMap2()
    {
        LoadGameScene("Map2");
    }
    
    /// <summary>
    /// Load Map3 - Level thứ ba
    /// </summary>
    public void StartGameMap3()
    {
        LoadGameScene("Map3");
    }
    

    
    /// <summary>
    /// Load MapRest - Map nghỉ ngơi, mua đồ, upgrade
    /// </summary>
    public void StartMapRest()
    {
        LoadGameScene("MapRest");
    }
    
    /// <summary>
    /// NEW GAME - Start fresh game trong MapRest
    /// </summary>
    public void NewGame()
    {
        PlayClickSound();
        
        if (GameManager.Instance != null)
        {
            // Reset game state cho new game
            GameManager.Instance.ResetGameForNewGame();
            GameManager.Instance.LoadScene("MapRest");
        }
        else
        {
            // Fallback
            LoadGameScene("MapRest");
        }
        
        Debug.Log("NEW GAME: Starting fresh in MapRest");
    }
    
    /// <summary>
    /// CONTINUE GAME - Load saved data và vào MapRest
    /// </summary>
    public void ContinueGame()
    {
        PlayClickSound();
        
        // Check if save data exists
        PlayerData saveData = SaveManager.Load();
        if (saveData == null)
        {
            Debug.LogWarning("No save data found! Starting new game instead.");
            NewGame(); // No save data, start new game
            return;
        }
        
        if (GameManager.Instance != null)
        {
            // Load saved checkpoint scene nếu có, otherwise MapRest
            string targetScene = "MapRest";
            if (!string.IsNullOrEmpty(GameManager.Instance.lastCheckpointScene))
            {
                targetScene = GameManager.Instance.lastCheckpointScene;
            }
            
            // Load game data và chuyển scene
            GameManager.Instance.LoadGameData();
            GameManager.Instance.LoadScene(targetScene);
        }
        else
        {
            // Fallback
            LoadGameScene("MapRest");
        }
        
        Debug.Log($"CONTINUE GAME: Loading saved data and going to scene");
    }
    
    /// <summary>
    /// Load Example Map - Map test/demo
    /// </summary>
    public void ExampleMap()
    {
        LoadGameScene("Example");
    }
    #endregion
    
    #region Navigation Scenes
    /// <summary>
    /// Về MainMenu
    /// </summary>
    public void MainMenu()
    {
        LoadMenuScene("MainMenu");
    }
    
    /// <summary>
    /// Mở scene hướng dẫn
    /// </summary>
    public void Instruction()
    {
        LoadMenuScene("Instruction");
    }
    
    /// <summary>
    /// Mở scene bảng xếp hạng
    /// </summary>
    public void Leaderboard()
    {
        LoadMenuScene("Leaderboard");
    }
    
    /// <summary>
    /// Load EndGame scene
    /// </summary>
    public void EndGame()
    {
        LoadMenuScene("EndGame");
    }
    #endregion
    
    #region Game Control
    /// <summary>
    /// Thoát game
    /// </summary>
    public void QuitGame()
    {
        PlayClickSound();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
            // Fallback nếu không có GameManager
            Application.Quit();
            Debug.Log("Game has been closed.");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
    
    /// <summary>
    /// Pause game (nếu đang trong gameplay)
    /// </summary>
    public void PauseGame()
    {
        PlayClickSound();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
    }
    
    /// <summary>
    /// Resume game (nếu đang pause)
    /// </summary>
    public void ResumeGame()
    {
        PlayClickSound();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }
    #endregion
    
    #region Helper Methods
    /// <summary>
    /// Load game scene với GameManager integration
    /// </summary>
    private void LoadGameScene(string sceneName)
    {
        PlayClickSound();
        
        if (GameManager.Instance != null)
        {
            // Sử dụng GameManager để load scene (có auto-save, loading screen, etc.)
            GameManager.Instance.LoadScene(sceneName);
        }
        else
        {
            // Fallback nếu không có GameManager
            Debug.LogWarning("GameManager not found! Using fallback scene loading.");
            
            // Reset time scale nếu bị pause
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
            }
            
            SceneManager.LoadScene(sceneName);
        }
    }
    
    /// <summary>
    /// Load menu scene (không cần auto-save)
    /// </summary>
    private void LoadMenuScene(string sceneName)
    {
        PlayClickSound();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(sceneName);
        }
        else
        {
            // Fallback nếu không có GameManager
            SceneManager.LoadScene(sceneName);
        }
    }
    
    /// <summary>
    /// Play click sound effect
    /// </summary>
    private void PlayClickSound()
    {
        if (playClickSound && AudioController.instance != null)
        {
            AudioController.instance.PlayClickSound();
        }
    }
    #endregion
    
    #region Public Utility Methods
    /// <summary>
    /// Load specific scene by name - có thể gọi từ UI hoặc scripts khác
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (IsGameplayScene(sceneName))
        {
            LoadGameScene(sceneName);
        }
        else
        {
            LoadMenuScene(sceneName);
        }
    }
    
    /// <summary>
    /// Check xem scene có phải là gameplay scene không
    /// </summary>
    private bool IsGameplayScene(string sceneName)
    {
        string[] gameplayScenes = { "Map1", "Map2", "Map3", "MapRest", "Example" };
        
        foreach (string gameScene in gameplayScenes)
        {
            if (sceneName == gameScene)
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Get current scene name
    /// </summary>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    /// <summary>
    /// Check if currently in main menu
    /// </summary>
    public bool IsInMainMenu()
    {
        return GetCurrentSceneName() == "MainMenu";
    }
         #endregion
}
