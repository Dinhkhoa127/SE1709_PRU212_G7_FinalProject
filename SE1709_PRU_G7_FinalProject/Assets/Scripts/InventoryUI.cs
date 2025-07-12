using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Player Reference")]
    public PlayerKnight player;
    
    [Header("Inventory Grid")]
    public Transform gridParent; // Gán là GridPanel
    public GameObject itemSlotPrefab;
    public Sprite healthPotionSprite;
    public Sprite manaPotionSprite;

    [Header("Character Stats Panel")]
    public CharacterStatsUI characterStatsUI;
    
    [Header("Equipment Slots Panel")]
    public EquipmentSlotsUI equipmentSlotsUI;
    
    [Header("UI Panels")]
    public GameObject leftPanel; // Panel chứa character stats và equipment
    public GameObject rightPanel; // Panel chứa inventory grid

    void OnEnable()
    {
        UpdateUI();
        UpdateCharacterPanel();
    }

    public void UpdateUI()
    {
        // Update inventory grid
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (var item in player.inventory)
        {
            // Chỉ hiển thị items có số lượng > 0
            if (item.quantity <= 0) continue;
            
            var slot = Instantiate(itemSlotPrefab, gridParent);
            
            // Gán icon - sử dụng ItemManager để lấy ItemInfo
            var icon = slot.transform.Find("ItemIcon").GetComponent<Image>();
            if (icon != null)
            {
                // Tìm ItemInfo từ ItemManager để lấy sprite
                if (ItemManager.Instance != null)
                {
                    var itemInfo = ItemManager.Instance.GetItemInfo(item.itemName);
                    if (itemInfo != null && itemInfo.itemSprite != null)
                    {
                        icon.sprite = itemInfo.itemSprite;
                    }
                    else
                    {
                        // Fallback cho items cũ
                        if (item.itemName == "Health Potion")
                            icon.sprite = healthPotionSprite;
                        else if (item.itemName == "Mana Potion")
                            icon.sprite = manaPotionSprite;
                        else
                            Debug.LogWarning($"No sprite found for item: {item.itemName}");
                    }
                }
                else
                {
                    // Fallback nếu không có ItemManager
                    if (item.itemName == "Health Potion")
                        icon.sprite = healthPotionSprite;
                    else if (item.itemName == "Mana Potion")
                        icon.sprite = manaPotionSprite;
                    else
                        Debug.LogWarning($"ItemManager not found, cannot display: {item.itemName}");
                }
            }
            
            // Gán số lượng
            var countText = slot.transform.Find("ItemCount").GetComponent<TextMeshProUGUI>();
            if (countText != null)
                countText.text = "x" + item.quantity;
                
            // Thêm drag handler cho equipment items
            if (ItemManager.Instance != null)
            {
                var itemInfo = ItemManager.Instance.GetItemInfo(item.itemName);
                if (itemInfo != null && itemInfo.itemType == ItemType.Equipment)
                {
                    var dragHandler = slot.GetComponent<InventoryItemDragHandler>();
                    if (dragHandler == null)
                        dragHandler = slot.AddComponent<InventoryItemDragHandler>();
                    dragHandler.SetItemData(item.itemName, item.quantity);
                    Debug.Log($"Added drag handler for equipment: {item.itemName}");
                }
            }
        }
        
        // Update character stats panel
        UpdateCharacterPanel();
    }
    
    void UpdateCharacterPanel()
    {
        // Update character stats if available
        if (characterStatsUI != null)
        {
            characterStatsUI.UpdateCharacterStats();
        }
        
        // Update equipment display if available
        if (equipmentSlotsUI != null)
        {
            equipmentSlotsUI.UpdateEquipmentDisplay();
        }
    }
    
    // Method to toggle panels for different views
    public void TogglePanels()
    {
        // Could be used for responsive design or different inventory modes
        if (leftPanel != null) leftPanel.SetActive(!leftPanel.activeSelf);
        if (rightPanel != null) rightPanel.SetActive(!rightPanel.activeSelf);
    }
}
