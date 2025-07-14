using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Data")]
    public string itemName;
    public int itemQuantity;
    
    [Header("Drag Settings")]
    public Canvas dragCanvas;
    public float dragAlpha = 0.6f;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Image itemIcon;
    private TextMeshProUGUI itemCount;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
        itemCount = transform.Find("ItemCount")?.GetComponent<TextMeshProUGUI>();
        
        // Find the main canvas if not assigned
        if (dragCanvas == null)
            dragCanvas = FindObjectOfType<Canvas>();
    }
    
    public void SetItemData(string name, int quantity)
    {
        itemName = name;
        itemQuantity = quantity;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Check if inventory is still open (prevent drag if inventory closed)
        if (GameManager.Instance != null && GameManager.Instance.currentGameState != GameManager.GameState.InventoryOpen)
        {
            Debug.Log("Drag cancelled - inventory not open");
            return;
        }
        
        // Only allow dragging equipment items
        if (ItemManager.Instance == null) return;
        
        ItemInfo itemInfo = ItemManager.Instance.GetItemInfo(itemName);
        if (itemInfo == null || itemInfo.itemType != ItemType.Equipment) return;
        
        // Store original position and parent
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        
        // Move to drag canvas for proper rendering
        transform.SetParent(dragCanvas.transform);
        transform.SetAsLastSibling();
        
        // Set drag appearance
        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;
        
        Debug.Log($"Started dragging {itemName}");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // Check if inventory is still open during drag
        if (GameManager.Instance != null && GameManager.Instance.currentGameState != GameManager.GameState.InventoryOpen)
        {
            Debug.Log("Drag interrupted - inventory closed");
            ForceCleanup();
            return;
        }
        
        rectTransform.anchoredPosition += eventData.delta / dragCanvas.scaleFactor;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Check if inventory is still open
        if (GameManager.Instance != null && GameManager.Instance.currentGameState != GameManager.GameState.InventoryOpen)
        {
            Debug.Log("End drag cancelled - inventory not open");
            ForceCleanup();
            return;
        }
        
        // Restore appearance
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Check if dropped on a valid equipment slot
        GameObject dropTarget = eventData.pointerEnter;
        EquipmentSlot equipmentSlot = null;
        
        if (dropTarget != null)
        {
            equipmentSlot = dropTarget.GetComponent<EquipmentSlot>();
            if (equipmentSlot == null)
                equipmentSlot = dropTarget.GetComponentInParent<EquipmentSlot>();
        }
        
        if (equipmentSlot != null)
        {
            // Try to equip the item
            if (TryEquipItem(equipmentSlot))
            {
                Debug.Log($"Successfully equipped {itemName}");
                // Don't return to original position - the item slot will be updated by InventoryUI
            }
            else
            {
                Debug.Log($"Failed to equip {itemName}");
                ReturnToOriginalPosition();
            }
        }
        else
        {
            // No valid drop target, return to original position
            ReturnToOriginalPosition();
        }
    }
    
    bool TryEquipItem(EquipmentSlot equipmentSlot)
    {
        if (ItemManager.Instance == null) return false;
        
        ItemInfo itemInfo = ItemManager.Instance.GetItemInfo(itemName);
        if (itemInfo == null || itemInfo.itemType != ItemType.Equipment) return false;
        
        // Check if item can be equipped in this slot
        if (itemInfo.equipmentType != equipmentSlot.allowedType) return false;
        
        // Get player reference
        PlayerKnight player = FindObjectOfType<PlayerKnight>();
        if (player == null) return false;
        
        // Check if player has this item
        if (player.GetItemQuantity(itemName) <= 0) return false;
        
        // Equip the item
        player.EquipItem(itemInfo);
        
        // Update inventory UI
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.UpdateUI();
        }
        
        return true;
    }
    
    void ReturnToOriginalPosition()
    {
        // Return to original parent and position
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
    
    /// <summary>
    /// Force cleanup drag state - called when inventory is closed suddenly
    /// </summary>
    public void ForceCleanup()
    {
        // Restore appearance
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Return to original position if we have valid references
        if (originalParent != null && rectTransform != null)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
        }
        
        // Clear drag state
        originalParent = null;
        originalPosition = Vector2.zero;
        
        Debug.Log($"Force cleaned up drag handler for {itemName}");
    }
    
    void OnDisable()
    {
        // Cleanup when object is disabled
        ForceCleanup();
    }
    
    void OnDestroy()
    {
        // Cleanup when object is destroyed
        ForceCleanup();
    }
} 