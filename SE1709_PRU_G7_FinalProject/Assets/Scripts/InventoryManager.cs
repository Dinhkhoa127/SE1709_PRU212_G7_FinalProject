using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryPanel; // Kéo InventoryPanel vào đây trong Inspector

    public static bool IsInventoryOpen = false;
    public static InventoryManager Instance;
    private PlayerKnight PlayerKnight;

    void Awake()
    {
        // Singleton pattern nhưng không DontDestroyOnLoad (để reset mỗi scene)
        Instance = this;
    }

    void Start()
    {
        // Auto-find InventoryPanel nếu reference bị mất
        if (inventoryPanel == null)
        {
            TryFindInventoryPanel();
        }

        // Đảm bảo inventory panel bị ẩn khi start
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            IsInventoryOpen = false;
        }

        // Subscribe to GameManager state changes
        if (GameManager.Instance != null)
        {
            GameManager.OnGameStateChanged += OnGameStateChanged;
        }
    }

    void TryFindInventoryPanel()
    {
        // Tìm theo tên GameObject
        GameObject foundPanel = GameObject.Find("InventoryPanel");
        if (foundPanel == null)
        {
            foundPanel = GameObject.Find("Inventory Panel");
        }
        if (foundPanel == null)
        {
            foundPanel = GameObject.Find("Inventory");
        }

        // Tìm theo Canvas children
        if (foundPanel == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                Transform found = canvas.transform.Find("InventoryPanel");
                if (found == null) found = canvas.transform.Find("Inventory Panel");
                if (found == null) found = canvas.transform.Find("Inventory");

                if (found != null)
                {
                    foundPanel = found.gameObject;
                    break;
                }
            }
        }

        if (foundPanel != null)
        {
            inventoryPanel = foundPanel;
            Debug.Log($"Auto-found InventoryPanel: {foundPanel.name}");
        }
        else
        {
            Debug.LogWarning("InventoryPanel not found! Please assign manually in Inspector or ensure it exists in scene.");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe when destroyed
        if (GameManager.Instance != null)
        {
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }

    void Update()
    {
        // Chỉ handle input nếu GameManager cho phép
        if (GameManager.Instance == null) return;
        
        // Safety check: Cleanup drag operations if inventory is closed but drag handlers exist
        if (!IsInventoryOpen && GameManager.Instance.currentGameState != GameManager.GameState.InventoryOpen)
        {
            // Check for any stuck drag handlers
            InventoryItemDragHandler[] dragHandlers = FindObjectsOfType<InventoryItemDragHandler>();
            if (dragHandlers.Length > 0)
            {
                foreach (var dragHandler in dragHandlers)
                {
                    if (dragHandler != null)
                    {
                        dragHandler.ForceCleanup();
                    }
                }
            }
        }
        
        //if (PlayerKnight.IsAttackLockedScene())
        //{
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (GameManager.Instance.currentGameState == GameManager.GameState.Playing)
                {
                    OpenInventory();
                }
                else if (GameManager.Instance.currentGameState == GameManager.GameState.InventoryOpen)
                {
                    CloseInventory();
                }
            }
            
            // Emergency cleanup hotkey: Ctrl+R
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("🚨 EMERGENCY CLEANUP TRIGGERED");
                ForceCleanupDragOperations();
            }
        //}
    }

    void OnGameStateChanged(GameManager.GameState newState, GameManager.GameState oldState)
    {
        // Sync UI với GameManager state
        switch (newState)
        {
            case GameManager.GameState.InventoryOpen:
                ShowInventoryUI();
                break;
            case GameManager.GameState.Playing:
                if (oldState == GameManager.GameState.InventoryOpen)
                {
                    HideInventoryUI();
                }
                break;
            case GameManager.GameState.Paused:
            case GameManager.GameState.SettingsOpen:
            case GameManager.GameState.ShopOpen:
                // Cleanup drag operations when switching to other UI states
                if (oldState == GameManager.GameState.InventoryOpen)
                {
                    CleanupDragOperations();
                }
                break;
        }
    }

    public void OpenInventory()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OpenInventory();
        }
        else
        {
            // Fallback
            ShowInventoryUI();
        }
    }

    public void CloseInventory()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CloseInventory();
        }
        else
        {
            // Fallback
            HideInventoryUI();
        }
    }

    void ShowInventoryUI()
    {
        if (inventoryPanel != null)
        {
            // Preventive cleanup before showing inventory
            CleanupDragOperations();
            
            inventoryPanel.SetActive(true);
            IsInventoryOpen = true;
            Debug.Log("Inventory UI opened");
        }
        else
        {
            Debug.LogWarning("InventoryPanel reference is null! Please assign in Inspector.");
        }
    }
    
    void HideInventoryUI()
    {
        if (inventoryPanel != null)
        {
            // CRITICAL: Cleanup any ongoing drag operations before hiding
            CleanupDragOperations();
            
            inventoryPanel.SetActive(false);
            IsInventoryOpen = false;
            Debug.Log("Inventory UI closed");
        }
    }
    
    /// <summary>
    /// Cleanup any ongoing drag operations to prevent UI sticking
    /// </summary>
    void CleanupDragOperations()
    {
        // Find all drag handlers and force cleanup
        InventoryItemDragHandler[] dragHandlers = FindObjectsOfType<InventoryItemDragHandler>();
        foreach (var dragHandler in dragHandlers)
        {
            if (dragHandler != null)
            {
                dragHandler.ForceCleanup();
            }
        }
        
        // Reset any canvas groups that might be stuck
        CanvasGroup[] canvasGroups = FindObjectsOfType<CanvasGroup>();
        foreach (var cg in canvasGroups)
        {
            if (cg != null && (cg.alpha < 1f || !cg.blocksRaycasts))
            {
                cg.alpha = 1f;
                cg.blocksRaycasts = true;
            }
        }
        
        // Force stop any ongoing drag operations by simulating mouse up
        if (Input.GetMouseButton(0))
        {
            // This will help end any stuck drag operations
            Canvas.ForceUpdateCanvases();
        }
        
        Debug.Log("Cleaned up drag operations");
    }
    
    /// <summary>
    /// Public method to manually cleanup drag operations - can be called from anywhere
    /// </summary>
    public static void ForceCleanupDragOperations()
    {
        if (Instance != null)
        {
            Instance.CleanupDragOperations();
        }
        else
        {
            // Static fallback cleanup
            InventoryItemDragHandler[] dragHandlers = FindObjectsOfType<InventoryItemDragHandler>();
            foreach (var dragHandler in dragHandlers)
            {
                if (dragHandler != null)
                {
                    dragHandler.ForceCleanup();
                }
            }
            
            CanvasGroup[] canvasGroups = FindObjectsOfType<CanvasGroup>();
            foreach (var cg in canvasGroups)
            {
                if (cg != null && (cg.alpha < 1f || !cg.blocksRaycasts))
                {
                    cg.alpha = 1f;
                    cg.blocksRaycasts = true;
                }
            }
            
            Debug.Log("Static cleanup completed");
        }
    }

    // Public method để other scripts có thể toggle inventory
    public void ToggleInventory()
    {
        if (IsInventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();

        }
    }
}
