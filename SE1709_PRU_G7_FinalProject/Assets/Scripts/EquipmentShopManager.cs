using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class EquipmentShopManager : MonoBehaviour
{
    [Header("Grid & Prefab")]
    public Transform gridParent; // ShopGridPanel
    public GameObject shopItemSlotPrefab;
    public List<ItemInfo> shopItems; // Danh sách equipment bán

    [Header("Detail Panel")]
    public Image detailIcon;
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailPriceText;    // Text hiển thị giá đơn vị
    public TextMeshProUGUI detailDescriptionText; // Text hiển thị mô tả và hiệu ứng
    public TextMeshProUGUI totalPriceText;     // Text hiển thị tổng giá
    public TMP_InputField quantityInput;
    public Button buyButton;

    [Header("UI Feedback")]
    public TextMeshProUGUI feedbackText; // Text hiển thị thông báo mua hàng
    public TextMeshProUGUI playerGoldText; // Text hiển thị số vàng hiện có

    [Header("Equipment Items")]
    public EquipmentShopItems equipmentShopItems;

    private ItemInfo currentItem;
    private PlayerKnight player;

    void Start()
    {
        player = FindObjectOfType<PlayerKnight>();
        if (buyButton != null)
            buyButton.onClick.AddListener(BuyItem);
        if (quantityInput != null)
            quantityInput.onValueChanged.AddListener(delegate { UpdateTotalPrice(); });
        
        // Load equipment items
        if (equipmentShopItems != null)
        {
            shopItems = equipmentShopItems.GetEquipmentItems();
            
            // Auto-add equipment items to ItemManager if not already there
            if (ItemManager.Instance != null)
            {
                foreach (var item in shopItems)
                {
                    // Check if item already exists in ItemManager
                    if (ItemManager.Instance.GetItemInfo(item.itemName) == null)
                    {
                        ItemManager.Instance.allItems.Add(item);
                        Debug.Log($"Added {item.itemName} to ItemManager");
                    }
                }
            }
            else
            {
                Debug.LogWarning("ItemManager not found! Equipment items won't display in inventory.");
            }
        }
        
        UpdateUI();
        UpdatePlayerGoldDisplay();
    }

    public void UpdateUI()
    {
        Debug.Log($"EquipmentShopManager: Bắt đầu UpdateUI, có {shopItems.Count} items");
        
        if (gridParent == null) return;
        
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (var item in shopItems)
        {
            Debug.Log($"EquipmentShopManager: Đang tạo slot cho {item.itemName}");
            var slot = Instantiate(shopItemSlotPrefab, gridParent);
            var icon = slot.transform.Find("ItemChild").GetComponent<Image>();
            if (icon != null)
                icon.sprite = item.itemSprite;
            
            // Thêm Button component nếu chưa có
            var btn = slot.GetComponent<Button>();
            if (btn == null)
                btn = slot.AddComponent<Button>();
            btn.onClick.AddListener(() => ShowItemDetail(item));
            Debug.Log($"EquipmentShopManager: Đã tạo slot cho {item.itemName}");
        }
    }

    public void ShowItemDetail(ItemInfo item)
    {
        currentItem = item;
        if (detailIcon != null)
            detailIcon.sprite = item.itemSprite;
        if (detailNameText != null)
            detailNameText.text = item.itemName;
        if (detailPriceText != null)
            detailPriceText.text = $"Price: {item.basePrice} Gold";
        
        // Hiển thị description + stats cho equipment
        if (detailDescriptionText != null)
        {
            string description = item.description;
            if (item.itemType == ItemType.Equipment)
            {
                description += "\n\nStats:";
                if (item.attackBonus > 0) description += $"\nAttack: +{item.attackBonus}";
                if (item.armorBonus > 0) description += $"\nArmor: +{item.armorBonus}";
                if (item.magicResistBonus > 0) description += $"\nMagic Resist: +{item.magicResistBonus}";
                if (item.healthBonus > 0) description += $"\nHealth: +{item.healthBonus}";
                if (item.manaBonus > 0) description += $"\nMana: +{item.manaBonus}";
            }
            detailDescriptionText.text = description;
        }
        
        if (quantityInput != null)
            quantityInput.text = "1";
        UpdateTotalPrice();
    }

    void BuyItem()
    {
        if (currentItem == null) return;
        int amount = 1;
        if (quantityInput != null)
            int.TryParse(quantityInput.text, out amount);
        if (amount < 1) amount = 1;
        int totalPrice = currentItem.basePrice * amount;
        
        if (player.gold >= totalPrice)
        {
            player.gold -= totalPrice;
            player.AddItem(currentItem.itemName, amount);
            player.SaveGame();
            Debug.Log($"Đã mua {amount} {currentItem.itemName} với giá {totalPrice} vàng!");
            
            // Hiển thị thông báo thành công
            if (feedbackText != null)
                feedbackText.text = $"Purchased {amount} {currentItem.itemName} for {totalPrice} Gold!";
            
            UpdatePlayerGoldDisplay();
            ClearFeedback();
        }
        else
        {
            Debug.Log("Không đủ vàng để mua vật phẩm!");
            
            // Hiển thị thông báo không đủ tiền
            if (feedbackText != null)
                feedbackText.text = "Not enough Gold!";
            
            ClearFeedback();
        }
    }

    public void UpdateTotalPrice()
    {
        if (currentItem == null) return;
        int amount = 1;
        if (quantityInput != null)
            int.TryParse(quantityInput.text, out amount);
        
        // Validation: đảm bảo số lượng >= 1
        if (amount < 1) 
        {
            amount = 1;
            if (quantityInput != null)
                quantityInput.text = "1";
        }
        
        // Validation: giới hạn số lượng tối đa (tùy chọn)
        int maxAffordable = player.gold / currentItem.basePrice;
        if (amount > maxAffordable && maxAffordable > 0)
        {
            amount = maxAffordable;
            if (quantityInput != null)
                quantityInput.text = maxAffordable.ToString();
        }
        
        int totalPrice = currentItem.basePrice * amount;
        if (totalPriceText != null)
            totalPriceText.text = $"Total: {totalPrice} Gold";
        
        // Đổi màu nút Buy nếu không đủ tiền
        if (buyButton != null)
        {
            buyButton.interactable = (player.gold >= totalPrice);
        }
    }

    void UpdatePlayerGoldDisplay()
    {
        if (playerGoldText != null && player != null)
            playerGoldText.text = $"Gold: {player.gold}";
    }

    void ClearFeedback()
    {
        if (feedbackText != null)
        {
            CancelInvoke(nameof(HideFeedback));
            Invoke(nameof(HideFeedback), 3f); // Ẩn thông báo sau 3 giây
        }
    }

    void HideFeedback()
    {
        if (feedbackText != null)
            feedbackText.text = "";
    }
} 