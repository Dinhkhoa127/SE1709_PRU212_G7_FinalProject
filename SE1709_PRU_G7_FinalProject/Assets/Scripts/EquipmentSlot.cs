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
        player = FindObjectOfType<PlayerKnight>();
        ClearSlot();
    }
    
    public void Setup(EquipmentType type)
    {
        allowedType = type;
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
        
        Debug.Log($"🔧 EQUIPPING: {item.itemName} to {allowedType} slot");
        Debug.Log($"🎨 Item sprite available: {item.itemSprite != null}");
        
        if (itemIcon != null)
        {
            if (item.itemSprite != null)
            {
                itemIcon.sprite = item.itemSprite;
                itemIcon.color = Color.white; // Visible
                Debug.Log($"✅ Set sprite for {item.itemName} successfully");
            }
            else
            {
                // Fallback: tìm sprite từ ItemManager
                if (ItemManager.Instance != null)
                {
                    var originalItem = ItemManager.Instance.GetItemInfo(item.itemName);
                    if (originalItem != null && originalItem.itemSprite != null)
                    {
                        itemIcon.sprite = originalItem.itemSprite;
                        itemIcon.color = Color.white;
                        Debug.Log($"✅ Found fallback sprite for {item.itemName}");
                    }
                    else
                    {
                        itemIcon.sprite = null;
                        itemIcon.color = new Color(1, 1, 1, 0);
                        Debug.LogWarning($"⚠️ No sprite found for {item.itemName}");
                    }
                }
                else
                {
                    itemIcon.sprite = null;
                    itemIcon.color = new Color(1, 1, 1, 0);
                    Debug.LogWarning($"⚠️ ItemManager not found, cannot display {item.itemName}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ itemIcon is null for {allowedType} slot");
        }
        
        if (itemNameText != null)
            itemNameText.text = item.itemName;
        
        Debug.Log($"✅ Equipment UI updated for {item.itemName} in {allowedType} slot");
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
            Debug.Log($"Cannot equip {itemInfo.itemName} in {allowedType} slot");
            return;
        }
        
        // Get player reference
        if (player == null) player = FindObjectOfType<PlayerKnight>();
        if (player == null) return;
        
        // Check if player has this item
        if (player.GetItemQuantity(dragHandler.itemName) <= 0) return;
        
        // Equip the item
        player.EquipItem(itemInfo);
        
        Debug.Log($"Equipped {itemInfo.itemName} to {allowedType} slot");
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Handle click to unequip
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