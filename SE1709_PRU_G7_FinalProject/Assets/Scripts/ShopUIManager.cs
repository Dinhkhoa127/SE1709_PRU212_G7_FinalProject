using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopUIManager : MonoBehaviour
{
    [Header("Grid & Prefab")]
    public Transform gridParent; // ShopGridPanel
    public GameObject shopItemSlotPrefab;
    public List<ItemInfo> shopItems; // Danh sách vật phẩm bán

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

    private ItemInfo currentItem;
    private PlayerKnight player;

    void Start()
    {
        player = FindObjectOfType<PlayerKnight>();
        buyButton.onClick.AddListener(BuyItem);
        quantityInput.onValueChanged.AddListener(delegate { UpdateTotalPrice(); });
        UpdateUI();
        UpdatePlayerGoldDisplay();
    }

    public void UpdateUI()
    {
        Debug.Log($"ShopUIManager: Bắt đầu UpdateUI, có {shopItems.Count} items");
        
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (var item in shopItems)
        {
            if (!item.isAvailableInShop) continue; // Chỉ hiển thị item có thể bán
            
            Debug.Log($"ShopUIManager: Đang tạo slot cho {item.itemName}");
            var slot = Instantiate(shopItemSlotPrefab, gridParent);
            var icon = slot.transform.Find("ItemChild").GetComponent<Image>();
            icon.sprite = item.itemSprite;
            // Thêm Button component nếu chưa có
            var btn = slot.GetComponent<Button>();
            if (btn == null)
                btn = slot.AddComponent<Button>();
            btn.onClick.AddListener(() => ShowItemDetail(item));
            Debug.Log($"ShopUIManager: Đã tạo slot cho {item.itemName}");
        }
    }

    public void ShowItemDetail(ItemInfo item)
    {
        currentItem = item;
        detailIcon.sprite = item.itemSprite;
        detailNameText.text = item.itemName;
        detailPriceText.text = $"Price: {item.basePrice} Gold";
        
        // Chỉ hiển thị description từ ItemInfo
        if (detailDescriptionText != null)
        {
            detailDescriptionText.text = item.description;
        }
        
        quantityInput.text = "1";
        UpdateTotalPrice();
    }

    string GetItemEffectDescription(ItemInfo item)
    {
        switch (item.itemType)
        {
            case ItemType.HealthPotion:
                return $"Heal: {item.effectValue} HP";
            case ItemType.ManaPotion:
                return $"Restore: {item.effectValue} MP";
            case ItemType.Consumable:
                return $"Effect: {item.effectValue}";
            default:
                return "";
        }
    }

    void BuyItem()
    {
        if (currentItem == null) return;
        int amount = 1;
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
        int.TryParse(quantityInput.text, out amount);
        
        // Validation: đảm bảo số lượng >= 1
        if (amount < 1) 
        {
            amount = 1;
            quantityInput.text = "1";
        }
        
        // Validation: giới hạn số lượng tối đa (tùy chọn)
        int maxAffordable = player.gold / currentItem.basePrice;
        if (amount > maxAffordable && maxAffordable > 0)
        {
            amount = maxAffordable;
            quantityInput.text = maxAffordable.ToString();
        }
        
        int totalPrice = currentItem.basePrice * amount;
        totalPriceText.text = $"Total: {totalPrice} Gold";
        
        // Đổi màu nút Buy nếu không đủ tiền
        if (buyButton != null)
        {
            buyButton.interactable = (player.gold >= totalPrice);
        }
    }

    void UpdatePlayerGoldDisplay()
    {
        if (playerGoldText != null)
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


