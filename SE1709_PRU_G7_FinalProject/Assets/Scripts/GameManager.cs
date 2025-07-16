using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq; // Added for .Where()

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
        SettingsOpen,  // Settings đang mở
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
    public GameObject settingsPanel;   // Thêm reference cho Settings Panel
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
    public Button settingsBackButton;  // Thêm button để đóng settings
    
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
    public float autoSaveInterval = 30f; // Auto-save mỗi 30 giây
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
        
        // Make ONLY Settings Panel persistent, not the entire MainMenu Canvas
        if (settingsPanel != null)
        {
            // Tạo Canvas riêng cho Settings để không cầm cả MainMenu
            CreatePersistentSettingsCanvas();
        }
        
        // Load player data if exists (chỉ load nếu có player)
        if (FindObjectOfType<PlayerKnight>() != null)
        {
            LoadGameData();
        }
        
        Debug.Log("GameManager initialized successfully!");
    }
    
    void CreatePersistentSettingsCanvas()
    {
        // Tạo Canvas mới chỉ cho Settings
        GameObject persistentSettingsCanvas = new GameObject("PersistentSettingsCanvas");
        Canvas canvas = persistentSettingsCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Đảm bảo hiện trên cùng
        
        // Add CanvasScaler và config đúng settings như MainMenu
        CanvasScaler canvasScaler = persistentSettingsCanvas.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;
        
        // Add GraphicRaycaster
        persistentSettingsCanvas.AddComponent<GraphicRaycaster>();
        
        // Copy Settings Panel sang Canvas mới
        GameObject newSettingsPanel = Instantiate(settingsPanel, persistentSettingsCanvas.transform);
        
        // Cập nhật reference để trỏ đến Settings Panel mới
        settingsPanel = newSettingsPanel;
        
        // Ẩn Settings Panel ban đầu
        settingsPanel.SetActive(false);
        
        // Làm persistent Canvas mới này
        DontDestroyOnLoad(persistentSettingsCanvas);
        
        // Setup lại button events cho Settings Panel mới
        SetupUIEvents();
        
        Debug.Log("Created persistent Settings Canvas separate from MainMenu with proper scaling!");
    }
    
    void SetupUIEvents()
    {
        // Setup pause menu buttons - tự động tìm trong Pause Panel
        if (pauseMenuPanel != null)
        {
            Button[] buttonsInPause = pauseMenuPanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttonsInPause)
            {
                string btnName = btn.name.ToLower();
                
                // Tìm Resume button
                if ((btnName.Contains("resume") || btnName.Contains("continue") || btnName.Contains("play")) && pauseResumeButton == null)
                {
                    pauseResumeButton = btn;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(ResumeGame);
                    Debug.Log($"Pause Resume button found and connected: {btn.name}");
                }
                
                // Tìm MainMenu button  
                if ((btnName.Contains("mainmenu") || btnName.Contains("main") || btnName.Contains("menu") || btnName.Contains("quit")) && pauseMainMenuButton == null)
                {
                    pauseMainMenuButton = btn;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.Log("Returning to MainMenu from Pause...");
                        SaveGameData(); // Save progress trước khi về MainMenu
                        LoadScene(mainMenuScene);
                    });
                    Debug.Log($"Pause MainMenu button found and connected: {btn.name}");
                }
            }
        }
        
        // Fallback: Manual assignment từ Inspector
        if (pauseResumeButton != null)
        {
            pauseResumeButton.onClick.RemoveAllListeners();
            pauseResumeButton.onClick.AddListener(ResumeGame);
        }
        if (pauseMainMenuButton != null)
        {
            pauseMainMenuButton.onClick.RemoveAllListeners();
            pauseMainMenuButton.onClick.AddListener(() => {
                Debug.Log("Returning to MainMenu from Pause...");
                SaveGameData(); // Save progress trước khi về MainMenu
                LoadScene(mainMenuScene);
            });
        }
            
        // Setup level complete buttons
        if (levelCompleteNextButton != null)
            levelCompleteNextButton.onClick.AddListener(LoadNextLevel);
        if (levelCompleteMainMenuButton != null)
            levelCompleteMainMenuButton.onClick.AddListener(() => LoadScene(mainMenuScene));
            
        // Setup settings button - tìm button trong Settings Panel
        if (settingsPanel != null)
        {
            // Tìm button Back/Close trong Settings Panel
            Button[] buttonsInSettings = settingsPanel.GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttonsInSettings)
            {
                string btnName = btn.name.ToLower();
                if (btnName.Contains("back") || btnName.Contains("close") || btnName.Contains("exit"))
                {
                    settingsBackButton = btn;
                    btn.onClick.RemoveAllListeners(); // Clear existing listeners
                    btn.onClick.AddListener(CloseSettings);
                    Debug.Log($"Settings back button found and connected: {btn.name}");
                    break;
                }
            }
        }
        
        // Fallback: nếu có settingsBackButton reference sẵn
        if (settingsBackButton != null)
        {
            settingsBackButton.onClick.RemoveAllListeners();
            settingsBackButton.onClick.AddListener(CloseSettings);
        }
    }
    #endregion

    #region Update Loop
    void Update()
    {
        HandleInput();
        HandleAutoSave();
        UpdateUI();
        if (currentGameState == GameState.Playing)
            totalPlayTime += Time.deltaTime;
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
            else if (currentGameState == GameState.SettingsOpen)
                CloseSettings(); // Đóng settings
        }
        
        // Mở Settings với phím P
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (currentGameState == GameState.Playing)
                OpenSettings();
            else if (currentGameState == GameState.SettingsOpen)
                CloseSettings();
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
            ShowCheckpointMessage($"Next auto-save: {remainingTime:F0}s");
        }
    }
    
    void HandleAutoSave()
    {
        if (!autoSaveEnabled) return;
        
        // Auto-save trong playing state hoặc khi inventory/shop mở (vì có thể mua/sử dụng items)
        bool shouldCountTimer = (currentGameState == GameState.Playing || 
                                currentGameState == GameState.InventoryOpen || 
                                currentGameState == GameState.ShopOpen ||
                                currentGameState == GameState.SettingsOpen);
        
        if (shouldCountTimer)
        {
            autoSaveTimer += Time.unscaledDeltaTime; // Dùng unscaledDeltaTime để hoạt động khi pause
            
            if (autoSaveTimer >= autoSaveInterval)
            {
                autoSaveTimer = 0f;
                SaveGameData();
                ShowCheckpointMessage("Auto Saved!");
            }
        }
        else
        {
            // Reset timer nếu không ở state thích hợp
            if (autoSaveTimer > 0)
            {
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
            
        if (settingsPanel != null)
            settingsPanel.SetActive(currentGameState == GameState.SettingsOpen);
            
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
            case GameState.SettingsOpen:
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
            currentGameState == GameState.ShopOpen ||
            currentGameState == GameState.SettingsOpen)
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
    
    public void OpenSettings()
    {
        if (currentGameState == GameState.Playing)
        {
            ChangeGameState(GameState.SettingsOpen);
            AudioController.instance?.PlayClickSound();
        }
    }
    
    public void CloseSettings()
    {
        if (currentGameState == GameState.SettingsOpen)
        {
            ChangeGameState(GameState.Playing);
            AudioController.instance?.PlayClickSound();
        }
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to MainMenu...");
        SaveGameData(); // Lưu progress trước khi về MainMenu
        AudioController.instance?.PlayClickSound();
        LoadScene(mainMenuScene);
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
        
        // Handle Pause Menu Panel - tự động tìm và setup
        if (pauseMenuPanel == null && scene.name != mainMenuScene)
        {
            // Tìm Pause Panel trong scene mới
            var allPanels = FindObjectsOfType<GameObject>(true).Where(g => 
                g.name.ToLower().Contains("pause") && 
                g.GetComponent<RectTransform>() != null).ToArray();
                
            if (allPanels.Length > 0)
            {
                pauseMenuPanel = allPanels[0];
                pauseMenuPanel.SetActive(false); // Ẩn pause menu ban đầu
                
                // Setup button events cho pause menu mới
                SetupUIEvents();
                
                Debug.Log($"Pause Menu Panel found: {pauseMenuPanel.name}");
            }
        }
        
        // Handle Settings Panel - tự động tìm và tạo persistent nếu chưa có
        if (settingsPanel == null || settingsPanel.transform.parent.name != "PersistentSettingsCanvas")
        {
            // Tìm Settings Panel trong scene mới
            var allPanels = FindObjectsOfType<GameObject>(true).Where(g => 
                g.name.ToLower().Contains("setting") && 
                g.GetComponent<RectTransform>() != null).ToArray();
                
            if (allPanels.Length > 0)
            {
                var foundSettingsPanel = allPanels[0];
                
                // Nếu chưa có persistent canvas, tạo mới
                if (settingsPanel == null)
                {
                    settingsPanel = foundSettingsPanel;
                    CreatePersistentSettingsCanvas();
                }
                
                Debug.Log($"Settings Panel found: {foundSettingsPanel.name}");
            }
        }
        
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
            }
            else if (currentScene == "EndGame" || currentScene == "MainMenu")
            {
                // Từ EndGame hoặc MainMenu - có thể cần load save
                shouldAutoLoad = true;
            }
            else if (currentScene == "MapRest")
            {
                // Luôn auto-load khi vào MapRest - đây là hub scene
                shouldAutoLoad = true;
            }
            else if (currentScene == lastCheckpointScene)
            {
                // Respawn tại checkpoint
                shouldAutoLoad = true;
            }
            else if (player.GetHealth() <= 0)
            {
                // Player chết - cần load để restore health
                shouldAutoLoad = true;
            }
            
            if (shouldAutoLoad)
            {
                LoadGameData();
            }
            else
            {
                // Đảm bảo equipment UI luôn hiển thị đúng khi không auto-load
                // Load equipped items từ save data để equipment UI có thể hiển thị
                PlayerData savedData = SaveManager.Load();
                if (savedData != null && savedData.equippedItems != null)
                {
                    player.LoadEquippedItems(savedData.equippedItems);
                    
                    // Force update equipment slots UI immediately
                    var equipmentSlotsUI = FindObjectOfType<EquipmentSlotsUI>();
                    if (equipmentSlotsUI != null)
                    {
                        equipmentSlotsUI.UpdateEquipmentDisplay();
                    }
                }
                
                // Schedule equipment UI update for next inventory open
                player.ScheduleEquipmentUIUpdate();
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

            // Save total play time
            PlayerPrefs.SetFloat("TotalPlayTime", totalPlayTime);
            PlayerPrefs.SetInt("TotalEnemiesKilled", totalEnemiesKilled);
            PlayerPrefs.SetInt("TotalEnemiesInGame", totalEnemiesInGame);
            
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

        // Load total play time
        totalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0f);
        totalEnemiesKilled = PlayerPrefs.GetInt("TotalEnemiesKilled", 0);
        totalEnemiesInGame = PlayerPrefs.GetInt("TotalEnemiesInGame", 0);
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
    /// Force load game data - chỉ dùng cho Continue Game hoặc F9
    /// </summary>
    
    /// <summary>
    /// Reset game data cho New Game - xóa save data và reset về default
    /// </summary>
    public void ResetGameForNewGame()
    {
        // Clear GameManager data
        lastCheckpointScene = "";
        lastCheckpointPosition = Vector3.zero;
        completedLevels.Clear();
        totalPlayTime = 0f;
        totalEnemiesKilled = 0;
        totalEnemiesInGame = 0;
        
        // Clear save data
        SaveManager.DeleteSave();
        
        // Clear GameManager specific PlayerPrefs
        PlayerPrefs.DeleteKey("LastCheckpointScene");
        PlayerPrefs.DeleteKey("LastCheckpointX");
        PlayerPrefs.DeleteKey("LastCheckpointY");
        PlayerPrefs.DeleteKey("LastCheckpointZ");
        PlayerPrefs.DeleteKey("CompletedLevels");
        PlayerPrefs.DeleteKey("TotalPlayTime");
        PlayerPrefs.DeleteKey("TotalEnemiesKilled");
        PlayerPrefs.DeleteKey("TotalEnemiesInGame");
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
               currentGameState == GameState.SettingsOpen ||
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

    public float totalPlayTime = 0f;
    public int totalEnemiesKilled = 0;
    public int totalEnemiesInGame = 0; // Sẽ cộng dồn khi load từng map

    public void OnEnemyKilled()
    {
        totalEnemiesKilled++;
    }

    public Dictionary<string, int> enemiesPerMap = new Dictionary<string, int>();
} 