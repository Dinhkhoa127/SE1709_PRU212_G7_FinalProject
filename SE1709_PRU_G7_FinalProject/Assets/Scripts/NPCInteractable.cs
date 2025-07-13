using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCInteractable : MonoBehaviour
{
    public enum NPCType { Statue, Shop, Item, Training, Exit, MapPortal, EquipmentShop, Chest }
    public NPCType npcType = NPCType.Statue;
    public GameObject shopPanel;
    [Tooltip("FPrompt là GameObject chứa TextMeshPro, sẽ hiện trên đầu NPC khi lại gần.")]
    public GameObject fPrompt;
    private bool playerInRange = false;
    private PlayerKnight playerKnight;

    [Header("Shop Item Settings")]
    public string itemName = "HealthPotion"; // Tên vật phẩm bán
    public int itemPrice = 3;               // Giá mỗi vật phẩm
    public int buyAmount = 1;                // Số lượng mua mỗi lần


    [Header("Training Area Settings")]
    [Tooltip("Scene name để load khi vào training area")]
    public string trainingSceneName = "Example";
    [Tooltip("Text hiển thị khi player đến gần training area")]
    public string trainingPromptText = "Come To Training Area";

    [Header("Exit Portal Settings")]
    [Tooltip("Scene name để load khi exit")]
    public string exitSceneName = "MapRest";
    [Tooltip("Text hiển thị khi player đến gần exit")]
    public string exitPromptText = "Press F - Return to Village";

    [Header("Map Portal Settings")]
    [Tooltip("Scene name để load khi vào map portal (Map1, Map2, Map3)")]
    public string mapPortalSceneName = "Map1";
    [Tooltip("Text hiển thị khi player đến gần map portal")]
    public string mapPortalPromptText = "Press F - Enter Dungeon";

    [Header("Equipment Shop Settings")]
    [Tooltip("Text hiển thị khi player đến gần equipment shop")]
    public string equipmentShopPromptText = "Press F - Equipment Shop";

    [Header("Chest Settings")]
    public int goldAmount = 300;
    private bool isOpened = false;
    [Tooltip("Animator để mở hòm khi tương tác")]
    public string openChest = "Press F - Open Chest";

    private Animator animator;


    void Start()
    {
        // Auto-setup can be done here if needed
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // Hiển thị prompt với custom text cho training area
            if (fPrompt != null) 
            {
                fPrompt.SetActive(true);
                
                // Update prompt text for training area, exit, map portal, or equipment shop
                if (npcType == NPCType.Training || npcType == NPCType.Exit || npcType == NPCType.MapPortal || npcType == NPCType.EquipmentShop || npcType == NPCType.Chest)
                {
                    string promptText = "";
                    switch (npcType)
                    {
                        case NPCType.Training:
                            promptText = trainingPromptText;
                            break;
                        case NPCType.Exit:
                            promptText = exitPromptText;
                            break;
                        case NPCType.MapPortal:
                            promptText = mapPortalPromptText;
                            break;
                        case NPCType.EquipmentShop:
                            promptText = equipmentShopPromptText;
                            break;
                        case NPCType.Chest:
                            promptText = openChest;
                            break;

                    }

                    // Try TextMeshPro first
                    var tmpText = fPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (tmpText != null)
                    {
                        tmpText.text = promptText;
                    }
                    else
                    {
                        // Fallback to legacy UI Text
                        var uiText = fPrompt.GetComponentInChildren<UnityEngine.UI.Text>();
                        if (uiText != null)
                        {
                            uiText.text = promptText;
                        }
                    }
                }
            }
            
            // Lưu lại tham chiếu PlayerKnight để dùng khi bấm F
            playerKnight = other.GetComponent<PlayerKnight>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (fPrompt != null) fPrompt.SetActive(false);
            playerKnight = null;
            
            // Tự động đóng shop khi rời khỏi NPC (works for both Shop and EquipmentShop)
            if (shopPanel != null && shopPanel.activeSelf)
            {
                shopPanel.SetActive(false);
                Time.timeScale = 1f;
            }
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            switch (npcType)
            {
                case NPCType.Statue:
                    if (playerKnight != null)
                    {
                        playerKnight.Heal(playerKnight.GetMaxHealth());
                        playerKnight.RestoreFullMana();
                        playerKnight.RestoreFullStamina();
                        Debug.Log("Player được tượng thần ban phước và hồi đầy máu!");
                        playerKnight.SaveGame(); // Tự động lưu sau khi hồi máu
                        
                        // Schedule equipment UI update for next inventory open
                        playerKnight.ScheduleEquipmentUIUpdate();
                    }
                    if (fPrompt != null) fPrompt.SetActive(false);
                    break;
                case NPCType.Shop:


                    if (shopPanel != null)
                    {
                        bool isActive = shopPanel.activeSelf;
                        if (fPrompt != null) fPrompt.SetActive(false);
                        if (isActive)
                        {
                            // Nếu đang mở thì đóng
                            shopPanel.SetActive(false);
                            Time.timeScale = 1f;
                            if (fPrompt != null) fPrompt.SetActive(true); // hiện lại F để tương tác tiếp
                        }
                        else
                        {
                            // Nếu đang tắt thì mở
                            shopPanel.SetActive(true);
                            Time.timeScale = 0f;
                            if (fPrompt != null) fPrompt.SetActive(false); // ẩn F khi shop đang mở
                        }
                    }
                    break;
                case NPCType.Item:
                    Debug.Log("Mua vật phẩm từ NPC item!");
                    if (playerKnight != null)
                    {
                        // Kiểm tra đủ tiền
                        int totalPrice = itemPrice * buyAmount;
                        if (playerKnight.gold >= totalPrice)
                        {
                            playerKnight.gold -= totalPrice;
                            playerKnight.AddItem(itemName, buyAmount); // Viết hàm này ở PlayerKnight.cs
                            playerKnight.SaveGame();
                            Debug.Log($"Đã mua {buyAmount} {itemName} với giá {totalPrice} vàng!");
                            // Có thể hiện thông báo UI ở đây
                        }
                        else
                        {
                            Debug.Log("Không đủ vàng để mua vật phẩm!");
                            // Có thể hiện thông báo UI ở đây
                        }
                    }
                    if (shopPanel != null)
                    {
                        bool isActive = shopPanel.activeSelf;
                        if (fPrompt != null) fPrompt.SetActive(false);
                        if (isActive)
                        {
                            // Nếu đang mở thì đóng
                            shopPanel.SetActive(false);
                            Time.timeScale = 1f;
                            if (fPrompt != null) fPrompt.SetActive(true); // hiện lại F để tương tác tiếp
                        }
                        else
                        {
                            // Nếu đang tắt thì mở
                            shopPanel.SetActive(true);
                            Time.timeScale = 0f;
                            if (fPrompt != null) fPrompt.SetActive(false); // ẩn F khi shop đang mở
                        }
                    }
                    break;
                    
                case NPCType.Training:
                    Debug.Log("Entering Training Area...");
                    if (fPrompt != null) fPrompt.SetActive(false);
                    
                    // Auto-save trước khi chuyển scene
                    if (playerKnight != null)
                    {
                        playerKnight.SaveGame();
                    }
                    
                    // Load training scene thông qua GameManager
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.LoadScene(trainingSceneName);
                    }
                    else
                    {
                        // Fallback nếu không có GameManager
                        SceneManager.LoadScene(trainingSceneName);
                    }
                    
                    Debug.Log($"Loading training scene: {trainingSceneName}");
                    break;
                    
                case NPCType.Exit:
                    Debug.Log("Using Exit Portal...");
                    if (fPrompt != null) fPrompt.SetActive(false);
                    
                    // Auto-save trước khi chuyển scene
                    if (playerKnight != null)
                    {
                        playerKnight.SaveGame();
                    }
                    
                    // Load exit scene thông qua GameManager
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.LoadScene(exitSceneName);
                    }
                    else
                    {
                        // Fallback nếu không có GameManager
                        SceneManager.LoadScene(exitSceneName);
                    }
                    
                    Debug.Log($"Loading exit scene: {exitSceneName}");
                    break;

                case NPCType.MapPortal:
                    Debug.Log("Entering Map Portal...");
                    if (fPrompt != null) fPrompt.SetActive(false);
                    
                    // Auto-save trước khi chuyển scene
                    if (playerKnight != null)
                    {
                        playerKnight.SaveGame();
                        
                        // Set checkpoint tại MapRest trước khi vào dungeon
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.SetCheckpoint(playerKnight.transform.position);
                        }
                    }
                    
                    // Load map scene thông qua GameManager
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.LoadScene(mapPortalSceneName);
                    }
                    else
                    {
                        // Fallback nếu không có GameManager
                        SceneManager.LoadScene(mapPortalSceneName);
                    }
                    
                    Debug.Log($"Loading map scene: {mapPortalSceneName}");
                    break;

                case NPCType.EquipmentShop:
                    // Handle Equipment Shop exactly like normal Shop
                    if (shopPanel != null)
                    {
                        bool isActive = shopPanel.activeSelf;
                        if (fPrompt != null) fPrompt.SetActive(false);
                        if (isActive)
                        {
                            // Nếu đang mở thì đóng
                            shopPanel.SetActive(false);
                            Time.timeScale = 1f;
                            if (fPrompt != null) fPrompt.SetActive(true); // hiện lại F để tương tác tiếp
                        }
                        else
                        {
                            // Nếu đang tắt thì mở
                            shopPanel.SetActive(true);
                            Time.timeScale = 0f;
                            if (fPrompt != null) fPrompt.SetActive(false); // ẩn F khi shop đang mở
                        }
                    }
                    break;

                case NPCType.Chest:
                    if (!isOpened && playerKnight != null)
                    {
                        playerKnight.gold += goldAmount;
                        playerKnight.SaveGame();
                        isOpened = true;
                        Debug.Log($"Player nhận được {goldAmount} vàng từ hòm!");

                        if (fPrompt != null)
                            fPrompt.SetActive(false);

                        if (animator != null)
                            animator.SetTrigger("isOpen");

                        Collider2D col = GetComponent<Collider2D>();
                        if (col != null) col.enabled = false;
                    }
                    break;

            }
        }
    }
}
