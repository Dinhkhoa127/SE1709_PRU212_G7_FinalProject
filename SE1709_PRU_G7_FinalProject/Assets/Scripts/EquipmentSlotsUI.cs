using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EquipmentSlotsUI : MonoBehaviour
{
    [Header("Equipment Slots")]
    public EquipmentSlot helmetSlot;
    public EquipmentSlot chestSlot;
    public EquipmentSlot legSlot;
    public EquipmentSlot bootSlot;
    public EquipmentSlot weaponSlot;
    public EquipmentSlot shieldSlot;
    public EquipmentSlot ringSlot;
    public EquipmentSlot necklaceSlot;
    
    private PlayerKnight player;
    
    void Start()
    {
        RefreshPlayerReference();
        InitializeSlots();
    }
    
    void OnEnable()
    {
        // Always update when UI becomes active - NO DELAY
        RefreshPlayerReference();
        UpdateEquipmentDisplay();
    }
    
    void RefreshPlayerReference()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerKnight>();
        }
    }
    
    void InitializeSlots()
    {
        // Setup each equipment slot
        if (helmetSlot != null) helmetSlot.Setup(EquipmentType.Helmet);
        if (chestSlot != null) chestSlot.Setup(EquipmentType.Chest);
        if (legSlot != null) legSlot.Setup(EquipmentType.Leg);
        if (bootSlot != null) bootSlot.Setup(EquipmentType.Boot);
        if (weaponSlot != null) weaponSlot.Setup(EquipmentType.Weapon);
        if (shieldSlot != null) shieldSlot.Setup(EquipmentType.Shield);
        if (ringSlot != null) ringSlot.Setup(EquipmentType.Ring);
        if (necklaceSlot != null) necklaceSlot.Setup(EquipmentType.Necklace);
    }
    
    public void UpdateEquipmentDisplay()
    {
        // Always refresh player reference first
        RefreshPlayerReference();
        
        if (player == null) return;
        
        // Update each equipment slot with equipped items
        UpdateSlotDisplay(helmetSlot, EquipmentType.Helmet);
        UpdateSlotDisplay(chestSlot, EquipmentType.Chest);
        UpdateSlotDisplay(legSlot, EquipmentType.Leg);
        UpdateSlotDisplay(bootSlot, EquipmentType.Boot);
        UpdateSlotDisplay(weaponSlot, EquipmentType.Weapon);
        UpdateSlotDisplay(shieldSlot, EquipmentType.Shield);
        UpdateSlotDisplay(ringSlot, EquipmentType.Ring);
        UpdateSlotDisplay(necklaceSlot, EquipmentType.Necklace);
    }
    
    void UpdateSlotDisplay(EquipmentSlot slot, EquipmentType equipType)
    {
        if (slot == null) return;
        
        // Refresh player reference in slot as well
        slot.RefreshPlayerReference();
        
        ItemInfo equippedItem = player.GetEquippedItem(equipType);
        if (equippedItem != null)
        {
            slot.EquipItem(equippedItem);
        }
        else
        {
            slot.ClearSlot();
        }
    }
    
    // Method to handle clicking on equipment slots (for testing)
    public void OnEquipmentSlotClick(EquipmentSlot slot)
    {
        if (slot == null) return;
        
        RefreshPlayerReference();
        if (player == null) return;
        
        ItemInfo currentItem = slot.GetEquippedItem();
        if (currentItem != null)
        {
            // Unequip item
            player.UnequipItem(slot.allowedType);
        }
    }
} 