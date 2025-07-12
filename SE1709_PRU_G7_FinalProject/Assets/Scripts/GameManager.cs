using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// GameManager - Quản lý toàn bộ game flow, states, UI, và data persistence
/// Sử dụng Singleton pattern để truy cập từ mọi nơi trong game
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton Pattern
    public static GameManager Instance;
    
    void Awake()
    {
        // Đảm bảo chỉ có 1 GameManager duy nhất trong game
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Không bị destroy khi chuyển scene
            InitializeGame();
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate GameManager
        }
    }
    #endregion

    #region Game States
    public enum GameState
    {
        MainMenu,      // Ở main menu
        Playing,       // Đang chơi game
        Paused,        // Game bị pause
        InventoryOpen, // Inventory đang mở
        ShopOpen,      // Shop đang mở
        GameOver,      // Player chết
        LevelComplete, // Hoàn thành level
        Loading        // Đang loading
    }
    
    [Header("Game State")]
    public GameState currentGameState = GameState.MainMenu;
    private GameState previousGameState; // Để restore lại state trước đó
    #endregion

    #region UI References
    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public GameObject levelCompletePanel;
    public GameObject loadingPanel;
    public GameObject hudPanel; // Health, Mana bars, etc.
    
    [Header("UI Text Elements")]
    public TextMeshProUGUI levelCompleteText;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI checkpointText; // Hiển thị "Checkpoint Saved!"
    
    [Header("UI Buttons")]
    public Button pauseResumeButton;
    public Button pauseMainMenuButton;
    public Button levelCompleteNextButton;
    public Button levelCompleteMainMenuButton;
    
    [Header("EndGame Scene")]
    [Tooltip("Tên scene EndGame để load khi player chết")]
    public string endGameScene = "EndGame";
    #endregion

    #region Player & Data Management
    [Header("Player & Save System")]
    public PlayerKnight player;
    public Vector3 lastCheckpointPosition;
    public string lastCheckpointScene;
    public List<string> completedLevels = new List<string>();
    public bool autoSaveEnabled = true;
    public float autoSaveInterval = 10f; // Auto-save mỗi 10 giây (để test dễ hơn)
    private float autoSaveTimer = 0f;
    #endregion

    #region Level & Scene Management
    [Header("Scene Management")]
    public string[] gameScenes = { "Map1", "Map2", "Map3", "MapRest" };
    public string mainMenuScene = "MainMenu";
    public string currentLevel = "";
    private bool isLoadingScene = false;
    #endregion

    #region Events & Delegates
    // Events để các systems khác có thể subscribe
    public delegate void GameStateChangedDelegate(GameState newState, GameState oldState);
    public static event GameStateChangedDelegate OnGameStateChanged;
    
    public delegate void PlayerDeathDelegate();
    public static event PlayerDeathDelegate OnPlayerDeath;
    
    public delegate void LevelCompleteDelegate(string levelName);
    public static event LevelCompleteDelegate OnLevelComplete;
    #endregion

    #region Initialization
    void InitializeGame()
    {
        // Subscribe to scene events
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        
        // Initialize UI button events
        SetupUIEvents();
        
        // Load player data if exists (chỉ load nếu có player)
        if (FindObjectOfType<PlayerKnight>() != null)
        {
            LoadGameData();
        }
        
        Debug.Log("GameManager initialized successfully!");
    }
    
    void SetupUIEvents()
    {
        // Setup pause menu buttons
        if (pauseResumeButton != null)
            pauseResumeButton.onClick.AddListener(ResumeGame);
        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(() => LoadScene(mainMenuScene));
            
        // Setup level complete buttons
        if (levelCompleteNextButton != null)
            levelCompleteNextButton.onClick.AddListener(LoadNextLevel);
        if (levelCompleteMainMenuButton != null)
            levelCompleteMainMenuButton.onClick.AddListener(() => LoadScene(mainMenuScene));
    }
    #endregion

    #region Update Loop
    void Update()
    {
        HandleInput();
        HandleAutoSave();
        UpdateUI();
    }
    
    void HandleInput()
    {
        // Pause/Resume với ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentGameState == GameState.Playing)
                PauseGame();
            else if (currentGameState == GameState.Paused)
                ResumeGame();
            else if (currentGameState == GameState.InventoryOpen || currentGameState == GameState.ShopOpen)
                ResumeGame(); // Đóng inventory/shop
        }
        
        // Quick save với F5 (giữ lại tính năng cũ)
        if (Input.GetKeyDown(KeyCode.F5) && currentGameState == GameState.Playing)
        {
            SaveGameData();
            ShowCheckpointMessage("Quick Save!");
        }
        
        // Quick load với F9
        if (Input.GetKeyDown(KeyCode.F9))
        {
            ForceLoadGameData();
            ShowCheckpointMessage("Game Loaded!");
        }
        
        // Debug auto-save status với F8
        if (Input.GetKeyDown(KeyCode.F8))
        {
            float remainingTime = autoSaveInterval - autoSaveTimer;
            Debug.Log($"🔍 AUTO-SAVE STATUS:");
            Debug.Log($"  ⏰ Timer: {autoSaveTimer:F1}s / {autoSaveInterval}s");
            Debug.Log($"  ⏳ Next save in: {remainingTime:F1}s");
            Debug.Log($"  🎮 Game State: {currentGameState}");
            Debug.Log($"  ✅ Auto-save enabled: {autoSaveEnabled}");
            ShowCheckpointMessage($"Next auto-save: {remainingTime:F0}s");
        }
    }
    
    void HandleAutoSave()
    {
        if (!autoSaveEnabled) return;
        
        // Auto-save trong playing state hoặc khi inventory/shop mở (vì có thể mua/sử dụng items)
        bool shouldCountTimer = (currentGameState == GameState.Playing || 
                                currentGameState == GameState.InventoryOpen || 
                                currentGameState == GameState.ShopOpen);
        
        if (shouldCountTimer)
        {
            autoSaveTimer += Time.unscaledDeltaTime; // Dùng unscaledDeltaTime để hoạt động khi pause
            
            // Debug auto-save timer mỗi 10 giây để track
            if (Mathf.FloorToInt(autoSaveTimer) % 10 == 0 && Mathf.FloorToInt(autoSaveTimer) > 0)
            {
                float remainingTime = autoSaveInterval - autoSaveTimer;
                if (remainingTime > 0)
                {
                    Debug.Log($"⏰ AUTO-SAVE: {remainingTime:F0}s until next save (State: {currentGameState})");
                }
            }
            
            if (autoSaveTimer >= autoSaveInterval)
            {
                autoSaveTimer = 0f;
                Debug.Log($"🔔 AUTO-SAVE TRIGGERED: {autoSaveInterval}s elapsed (State: {currentGameState})");
                SaveGameData();
                ShowCheckpointMessage("Auto Saved!");
            }
        }
        else
        {
            // Reset timer nếu không ở state thích hợp
            if (autoSaveTimer > 0)
            {
                Debug.Log($"⏸️ AUTO-SAVE PAUSED: Incompatible game state ({currentGameState})");
                autoSaveTimer = 0f;
            }
        }
    }
    
    void UpdateUI()
    {
        // Update UI elements based on current state
        if (loadingPanel != null)
            loadingPanel.SetActive(currentGameState == GameState.Loading);
            
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(currentGameState == GameState.Paused);
            
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(currentGameState == GameState.LevelComplete);
            
        if (hudPanel != null)
            hudPanel.SetActive(currentGameState == GameState.Playing);
    }
    #endregion

    #region Game State Management
    public void ChangeGameState(GameState newState)
    {
        if (currentGameState == newState) return;
        
        GameState oldState = currentGameState;
        previousGameState = currentGameState;
        currentGameState = newState;
        
        // Handle time scale based on state
        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
            case GameState.InventoryOpen:
            case GameState.ShopOpen:
            case GameState.GameOver:
            case GameState.LevelComplete:
                Time.timeScale = 0f;
                break;
            case GameState.Loading:
                Time.timeScale = 1f; // Keep time running for loading animations
                break;
        }
        
        // Fire event
        OnGameStateChanged?.Invoke(newState, oldState);
        
        Debug.Log($"Game State changed: {oldState} -> {newState}");
    }
    
    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
        {
            ChangeGameState(GameState.Paused);
            AudioController.instance?.PlayClickSound();
        }
    }
    
    public void ResumeGame()
    {
        if (currentGameState == GameState.Paused || 
            currentGameState == GameState.InventoryOpen || 
            currentGameState == GameState.ShopOpen)
        {
            ChangeGameState(GameState.Playing);
            AudioController.instance?.PlayClickSound();
        }
    }
    
    public void OpenInventory()
    {
        if (currentGameState == GameState.Playing)
        {
            ChangeGameState(GameState.InventoryOpen);
        }
    }
    
    public void CloseInventory()
    {
        if (currentGameState == GameState.InventoryOpen)
        {
            ChangeGameState(GameState.Playing);
        }
    }
    
    public void OpenShop()
    {
        if (currentGameState == GameState.Playing)
        {
            ChangeGameState(GameState.ShopOpen);
        }
    }
    
    public void CloseShop()
    {
        if (currentGameState == GameState.ShopOpen)
        {
            ChangeGameState(GameState.Playing);
        }
    }
    #endregion

    #region Player Death & Game Over
    public void OnPlayerDied()
    {
        if (currentGameState == GameState.GameOver) return; // Prevent multiple calls
        
        // Auto-save before death để preserve progress
        SaveGameData();
        Debug.Log("💀 AUTO-SAVE: Player died - progress saved!");
        
        ChangeGameState(GameState.GameOver);
        
        // Fire death event
        OnPlayerDeath?.Invoke();
        
        AudioController.instance?.PlayDeathSound();
        
        Debug.Log("Player died - Loading EndGame scene");
        
        // Load EndGame scene thay vì hiện UI panel
        StartCoroutine(LoadEndGameCoroutine());
    }
    
    IEnumerator LoadEndGameCoroutine()
    {
        // Đợi một chút cho death animation
        yield return new WaitForSecondsRealtime(2f);
        
        // Save current progress before going to EndGame
        SaveGameData();
        
        // Load EndGame scene
        LoadScene(endGameScene);
    }
    
    public void RestartLevel()
    {
        SaveGameData(); // Save current progress before restart
        StartCoroutine(RestartLevelCoroutine());
    }
    
    IEnumerator RestartLevelCoroutine()
    {
        ChangeGameState(GameState.Loading);
        
        yield return new WaitForSecondsRealtime(0.5f); // Small delay for UX
        
        // Reload current scene
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene);
    }
    
    // Hàm này có thể gọi từ EndGame scene để restart
    public void RestartFromEndGame()
    {
        // Load lại data và về scene cuối cùng chơi
        LoadGameData();
        if (!string.IsNullOrEmpty(lastCheckpointScene))
        {
            LoadScene(lastCheckpointScene);
        }
        else
        {
            LoadScene("Map1"); // Default scene
        }
    }
    #endregion

    #region Level Completion & Progression
    public void CompleteLevel(string levelName = "")
    {
        if (string.IsNullOrEmpty(levelName))
            levelName = SceneManager.GetActiveScene().name;
            
        currentLevel = levelName;
        
        // Add to completed levels if not already completed
        if (!completedLevels.Contains(levelName))
        {
            completedLevels.Add(levelName);
        }
        
        // Always save when completing level
        SaveGameData();
        Debug.Log($"🎉 AUTO-SAVE: Level {levelName} completed and saved!");
        
        ChangeGameState(GameState.LevelComplete);
        
        if (levelCompleteText != null)
            levelCompleteText.text = $"Level {levelName} Complete!\nWell Done!";
            
        // Fire level complete event
        OnLevelComplete?.Invoke(levelName);
        
        Debug.Log($"Level completed: {levelName}");
    }
    
    public void LoadNextLevel()
    {
        string nextLevel = GetNextLevel(currentLevel);
        if (!string.IsNullOrEmpty(nextLevel))
        {
            LoadScene(nextLevel);
        }
        else
        {
            // No more levels - game completed!
            LoadScene("EndGame");
        }
    }
    
    string GetNextLevel(string currentLevel)
    {
        for (int i = 0; i < gameScenes.Length - 1; i++)
        {
            if (gameScenes[i] == currentLevel)
                return gameScenes[i + 1];
        }
        return ""; // No next level
    }
    #endregion

    #region Scene Management
    public void LoadScene(string sceneName)
    {
        if (isLoadingScene) return; // Prevent multiple loads
        
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    
    IEnumerator LoadSceneCoroutine(string sceneName)
    {
        isLoadingScene = true;
        ChangeGameState(GameState.Loading);
        
        if (loadingText != null)
            loadingText.text = $"Loading {sceneName}...";
        
        // Save before changing scene - CRITICAL for inventory persistence
        if (currentGameState != GameState.MainMenu && player != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log($"💾 AUTO-SAVE: Saving progress before leaving {currentScene}...");
            SaveGameData();
            Debug.Log($"✅ AUTO-SAVE: Progress saved! Items: {player.inventory.Count}, Gold: {player.gold}");
        }
        
        yield return new WaitForSecondsRealtime(0.5f); // Minimum loading time for UX
        
        // Load scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            // Update loading progress if needed
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            if (loadingText != null)
                loadingText.text = $"Loading {sceneName}... {progress * 100:F0}%";
            yield return null;
        }
        
        isLoadingScene = false;
        
        // Set appropriate state based on scene
        if (sceneName == mainMenuScene)
            ChangeGameState(GameState.MainMenu);
        else
            ChangeGameState(GameState.Playing);
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");   
        
        // Find player in new scene
        if (scene.name != mainMenuScene)
        {
            player = FindObjectOfType<PlayerKnight>();
            if (player != null)
            {
                // Restore checkpoint position if applicable
                if (!string.IsNullOrEmpty(lastCheckpointScene) && scene.name == lastCheckpointScene)
                {
                    player.transform.position = lastCheckpointPosition;
                }
                
                // Always try to load saved stats when player is found
                // (LoadGameData will handle cases where no save exists)
                StartCoroutine(LoadPlayerDataAfterFrame());
            }
        }
        
        // Update current level
        currentLevel = scene.name;
    }
    
    // Load player data sau 1 frame để đảm bảo player đã được initialize
    System.Collections.IEnumerator LoadPlayerDataAfterFrame()
    {
        yield return null; // Wait 1 frame
        if (player != null)
        {
            // 🧠 SMART AUTO-LOAD - Chỉ load khi thực sự cần thiết
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            // Check if this is a fresh start or respawn situation
            bool shouldAutoLoad = false;
            
            // Auto-load trong các trường hợp sau:
            if (player.inventory.Count == 0 && player.gold == 0 && player.GetMaxHealth() <= 100)
            {
                // Player mới hoặc chưa có data gì
                shouldAutoLoad = true;
                Debug.Log("🔄 AUTO-LOAD: Fresh player detected");
            }
            else if (currentScene == "EndGame" || currentScene == "MainMenu")
            {
                // Từ EndGame hoặc MainMenu - có thể cần load save
                shouldAutoLoad = true;
                Debug.Log("🔄 AUTO-LOAD: Coming from EndGame/MainMenu");
            }
            else if (currentScene == "MapRest")
            {
                // Luôn auto-load khi vào MapRest - đây là hub scene
                shouldAutoLoad = true;
                Debug.Log("🏠 AUTO-LOAD: Entering MapRest - loading all saved progress");
            }
            else if (currentScene == lastCheckpointScene)
            {
                // Respawn tại checkpoint
                shouldAutoLoad = true;
                Debug.Log("♻️ AUTO-LOAD: Respawn at checkpoint");
            }
            else if (player.GetHealth() <= 0)
            {
                // Player chết - cần load để restore health
                shouldAutoLoad = true;
                Debug.Log("💀 AUTO-LOAD: Player dead - restoring health");
            }
            
            if (shouldAutoLoad)
            {
                Debug.Log("🔄 AUTO-LOADING save data...");
                LoadGameData();
            }
            else
            {
                Debug.Log("⚠️ AUTO-LOAD SKIPPED - Preserving current player state");
                Debug.Log($"📦 Current inventory: {player.inventory.Count} items, Gold: {player.gold}");
                
                // Chỉ force update UI để sync với current state
                player.ForceUpdateInventoryUI();
            }
        }
    }
    
    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"Scene unloaded: {scene.name}");
    }
    #endregion

    #region Checkpoint System
    public void SetCheckpoint(Vector3 position)
    {
        lastCheckpointPosition = position;
        lastCheckpointScene = SceneManager.GetActiveScene().name;
        SaveGameData(); // Save checkpoint
        ShowCheckpointMessage("Checkpoint Saved!");
        
        Debug.Log($"Checkpoint set at {position} in {lastCheckpointScene}");
    }
    
    public void RespawnAtCheckpoint()
    {
        if (player != null && lastCheckpointPosition != Vector3.zero)
        {
            player.transform.position = lastCheckpointPosition;
            player.LoadGame(); // Restore player stats
            ChangeGameState(GameState.Playing);
            ShowCheckpointMessage("Respawned at Checkpoint!");
        }
    }
    
    void ShowCheckpointMessage(string message)
    {
        if (checkpointText != null)
        {
            checkpointText.text = message;
            checkpointText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideCheckpointMessage));
            Invoke(nameof(HideCheckpointMessage), 2f);
        }
    }
    
    void HideCheckpointMessage()
    {
        if (checkpointText != null)
            checkpointText.gameObject.SetActive(false);
    }
    #endregion

    #region Save/Load System Integration
    void SaveGameData()
    {
        if (player != null)
        {
            // DETAILED DEBUG - Trước khi save
            Debug.Log("💾 === STARTING SAVE PROCESS ===");
            Debug.Log($"📊 Current Player State: Gold: {player.gold}, Inventory: {player.inventory.Count} items");
            
            // Debug equipped items trước khi save
            int equippedCount = 0;
            foreach (var kvp in player.equippedItems)
            {
                if (kvp.Value != null)
                {
                    equippedCount++;
                    Debug.Log($"⚔️ Equipped: {kvp.Value.itemName} in {kvp.Key} slot");
                }
            }
            Debug.Log($"🛡️ Total equipped items: {equippedCount}");
            
            // Use existing PlayerKnight save system
            player.SaveGame();
            
            // Save GameManager specific data
            PlayerPrefs.SetString("LastCheckpointScene", lastCheckpointScene);
            PlayerPrefs.SetFloat("LastCheckpointX", lastCheckpointPosition.x);
            PlayerPrefs.SetFloat("LastCheckpointY", lastCheckpointPosition.y);
            PlayerPrefs.SetFloat("LastCheckpointZ", lastCheckpointPosition.z);
            
            // Save completed levels
            string completedLevelsString = string.Join(",", completedLevels);
            PlayerPrefs.SetString("CompletedLevels", completedLevelsString);
            
            PlayerPrefs.Save();
            
            Debug.Log($"✅ GAMEMANAGER SAVE COMPLETED: Gold: {player.gold}, Inventory: {player.inventory.Count} items, Equipment: {equippedCount}");
            Debug.Log("💾 === SAVE PROCESS FINISHED ===");
        }
        else
        {
            Debug.LogWarning("⚠️ Cannot save - Player not found!");
        }
    }
    
    public void LoadGameData()
    {
        if (player != null)
        {
            // Use existing PlayerKnight load system
            player.LoadGame();
        }
        
        // Load GameManager specific data
        lastCheckpointScene = PlayerPrefs.GetString("LastCheckpointScene", "");
        lastCheckpointPosition = new Vector3(
            PlayerPrefs.GetFloat("LastCheckpointX", 0),
            PlayerPrefs.GetFloat("LastCheckpointY", 0),
            PlayerPrefs.GetFloat("LastCheckpointZ", 0)
        );
        
        // Load completed levels
        string completedLevelsString = PlayerPrefs.GetString("CompletedLevels", "");
        if (!string.IsNullOrEmpty(completedLevelsString))
        {
            completedLevels = new List<string>(completedLevelsString.Split(','));
        }
    }
    
    /// <summary>
    /// Force load game data - chỉ dùng cho Continue Game hoặc F9
    /// </summary>
    public void ForceLoadGameData()
    {
        Debug.Log("🔄 FORCE LOADING GAME DATA...");
        LoadGameData();
        Debug.Log("✅ Force load completed");
    }
    
    /// <summary>
    /// Reset game data cho New Game - xóa save data và reset về default
    /// </summary>
    public void ResetGameForNewGame()
    {
        // Clear GameManager data
        lastCheckpointScene = "";
        lastCheckpointPosition = Vector3.zero;
        completedLevels.Clear();
        
        // Clear save data
        SaveManager.DeleteSave();
        
        // Clear GameManager specific PlayerPrefs
        PlayerPrefs.DeleteKey("LastCheckpointScene");
        PlayerPrefs.DeleteKey("LastCheckpointX");
        PlayerPrefs.DeleteKey("LastCheckpointY");
        PlayerPrefs.DeleteKey("LastCheckpointZ");
        PlayerPrefs.DeleteKey("CompletedLevels");
        PlayerPrefs.Save();
        
        Debug.Log("Game data reset for NEW GAME");
    }
    #endregion

    #region Public Utility Methods
    public bool IsLevelCompleted(string levelName)
    {
        return completedLevels.Contains(levelName);
    }
    
    public bool IsGameplayState()
    {
        return currentGameState == GameState.Playing;
    }
    
    public bool IsUIState()
    {
        return currentGameState == GameState.InventoryOpen || 
               currentGameState == GameState.ShopOpen || 
               currentGameState == GameState.Paused;
    }
    
    public void QuitGame()
    {
        SaveGameData();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    #endregion

    #region Cleanup
    void OnDestroy()
    {
        // Unsubscribe from events
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    #endregion
} 