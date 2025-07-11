using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryPanel; // Kéo InventoryPanel vào đây trong Inspector

    public static bool IsInventoryOpen = false;
    public static InventoryManager Instance;

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
            inventoryPanel.SetActive(false);
            IsInventoryOpen = false;
            Debug.Log("Inventory UI closed");
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
