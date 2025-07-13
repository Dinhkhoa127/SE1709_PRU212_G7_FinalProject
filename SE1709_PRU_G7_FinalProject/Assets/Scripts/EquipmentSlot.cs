using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EquipmentSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image slotIcon;
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    
    [Header("Slot Settings")]
    public Sprite emptySlotSprite;
    public EquipmentType allowedType;
    
    private ItemInfo currentItem;
    private PlayerKnight player;
    
    void Start()
    {
        RefreshPlayerReference();
        ClearSlot();
    }
    
    public void RefreshPlayerReference()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerKnight>();
        }
    }
    
    public void Setup(EquipmentType type)
    {
        allowedType = type;
        RefreshPlayerReference();
        ClearSlot();
    }
    
    public void ClearSlot()
    {
        currentItem = null;
        
        if (slotIcon != null && emptySlotSprite != null)
            slotIcon.sprite = emptySlotSprite;
        
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.color = new Color(1, 1, 1, 0); // Transparent
        }
        
        if (itemNameText != null)
            itemNameText.text = "";
    }
    
    public void EquipItem(ItemInfo item)
    {
        if (item == null || item.itemType != ItemType.Equipment) return;
        
        currentItem = item;
        
        if (itemIcon != null)
        {
            Sprite spriteToUse = null;
            
            // Try to find sprite from multiple sources
            if (item.itemSprite != null)
            {
                spriteToUse = item.itemSprite;
            }
            
            if (spriteToUse == null && ItemManager.Instance != null)
            {
                var originalItem = ItemManager.Instance.GetItemInfo(item.itemName);
                if (originalItem != null && originalItem.itemSprite != null)
                {
                    spriteToUse = originalItem.itemSprite;
                }
            }
            
            if (spriteToUse == null)
            {
                var equipmentShopManager = FindObjectOfType<EquipmentShopManager>();
                if (equipmentShopManager?.equipmentShopItems != null)
                {
                    foreach (var shopItem in equipmentShopManager.equipmentShopItems.GetEquipmentItems())
                    {
                        if (shopItem.itemName == item.itemName && shopItem.itemSprite != null)
                        {
                            spriteToUse = shopItem.itemSprite;
                            break;
                        }
                    }
                }
            }
            
            if (spriteToUse == null)
            {
                var allItems = Resources.LoadAll<ItemInfo>("");
                foreach (var resourceItem in allItems)
                {
                    if (resourceItem.itemName == item.itemName && resourceItem.itemSprite != null)
                    {
                        spriteToUse = resourceItem.itemSprite;
                        break;
                    }
                }
            }
            
            // Apply the sprite or clear if none found
            if (spriteToUse != null)
            {
                itemIcon.sprite = spriteToUse;
                itemIcon.color = Color.white;
            }
            else
            {
                itemIcon.sprite = null;
                itemIcon.color = new Color(1, 1, 1, 0);
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ itemIcon is null for {allowedType} slot");
        }
        
        if (itemNameText != null)
            itemNameText.text = item.itemName;
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // Get the drag handler from the dragged object
        InventoryItemDragHandler dragHandler = eventData.pointerDrag?.GetComponent<InventoryItemDragHandler>();
        if (dragHandler == null) return;
        
        // Get item info
        if (ItemManager.Instance == null) return;
        ItemInfo itemInfo = ItemManager.Instance.GetItemInfo(dragHandler.itemName);
        
        if (itemInfo == null || itemInfo.itemType != ItemType.Equipment) return;
        
        // Check if item can be equipped in this slot
        if (itemInfo.equipmentType != allowedType) 
        {
            return;
        }
        
        // Refresh player reference
        RefreshPlayerReference();
        if (player == null) return;
        
        // Check if player has this item
        if (player.GetItemQuantity(dragHandler.itemName) <= 0) return;
        
        // Equip the item
        player.EquipItem(itemInfo);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Handle click to unequip
        RefreshPlayerReference();
        
        if (currentItem != null && player != null)
        {
            player.UnequipItem(allowedType);
        }
    }
    
    public ItemInfo GetEquippedItem()
    {
        return currentItem;
    }
}

public enum EquipmentType
{
    Helmet,
    Chest,
    Leg,
    Boot,
    Weapon,
    Shield,
    Ring,
    Necklace
} 