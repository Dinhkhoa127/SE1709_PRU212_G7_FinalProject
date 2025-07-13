using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlotHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color validDropColor = Color.green;
    public Color invalidDropColor = Color.red;
    
    private Image slotImage;
    private EquipmentSlot equipmentSlot;
    private bool isDraggedOver = false;
    
    void Awake()
    {
        slotImage = GetComponent<Image>();
        equipmentSlot = GetComponent<EquipmentSlot>();
        
        if (slotImage == null)
            slotImage = GetComponentInChildren<Image>();
    }
    
    void Start()
    {
        SetNormalColor();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // Check if dragged item is valid for this slot
            InventoryItemDragHandler dragHandler = eventData.pointerDrag.GetComponent<InventoryItemDragHandler>();
            if (dragHandler != null && IsValidDropTarget(dragHandler))
            {
                SetColor(validDropColor);
            }
            else
            {
                SetColor(invalidDropColor);
            }
            isDraggedOver = true;
        }
        else
        {
            SetColor(highlightColor);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        SetNormalColor();
        isDraggedOver = false;
    }
    
    void SetNormalColor()
    {
        SetColor(normalColor);
    }
    
    void SetColor(Color color)
    {
        if (slotImage != null)
        {
            slotImage.color = color;
        }
    }
    
    bool IsValidDropTarget(InventoryItemDragHandler dragHandler)
    {
        if (equipmentSlot == null || ItemManager.Instance == null) return false;
        
        ItemInfo itemInfo = ItemManager.Instance.GetItemInfo(dragHandler.itemName);
        if (itemInfo == null || itemInfo.itemType != ItemType.Equipment) return false;
        
        return itemInfo.equipmentType == equipmentSlot.allowedType;
    }
    
    void Update()
    {
        // Reset color if drag ended while over this slot
        if (isDraggedOver && Input.GetMouseButtonUp(0))
        {
            SetNormalColor();
            isDraggedOver = false;
        }
    }
} 