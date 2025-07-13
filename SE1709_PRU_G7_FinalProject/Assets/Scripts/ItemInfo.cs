using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item Info")]
public class ItemInfo : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite itemSprite;
    public int basePrice;
    
    [Header("Item Effects")]
    public ItemType itemType;
    public int effectValue; // Health restore amount, Mana restore amount, etc.
    
    [Header("Equipment Settings")]
    public EquipmentType equipmentType; // Chỉ dùng khi itemType = Equipment
    public int attackBonus = 0;
    public int armorBonus = 0;
    public int magicResistBonus = 0;
    public int healthBonus = 0;
    public int manaBonus = 0;
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description;
    
    [Header("Shop Settings")]
    public bool isAvailableInShop = true;
    public int maxStackSize = 99;
    
    // Helper method để tạo description tự động cho equipment
    public string GetEquipmentDescription()
    {
        if (itemType != ItemType.Equipment) return description;
        
        string equipDesc = description + "\n\n";
        if (attackBonus > 0) equipDesc += $"Attack: +{attackBonus}\n";
        if (armorBonus > 0) equipDesc += $"Armor: +{armorBonus}\n";
        if (magicResistBonus > 0) equipDesc += $"Magic Resist: +{magicResistBonus}\n";
        if (healthBonus > 0) equipDesc += $"Health: +{healthBonus}\n";
        if (manaBonus > 0) equipDesc += $"Mana: +{manaBonus}\n";
        
        return equipDesc;
    }
}

public enum ItemType
{
    HealthPotion,
    ManaPotion,
    Equipment,
    Consumable,
    Key
} 