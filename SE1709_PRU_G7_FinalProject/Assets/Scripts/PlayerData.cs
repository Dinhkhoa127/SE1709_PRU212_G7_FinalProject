using System;
using System.Collections.Generic;

/// <summary>
/// Lưu trữ toàn bộ dữ liệu cần thiết của Player để save/load.
/// </summary>
[Serializable]
public class PlayerData
{
    // Base stats (không bao gồm equipment bonuses)
    public int maxHealth;
    public int health;
    public int maxArmorShield;
    public int currentArmorShield;
    public int maxMagicShield;
    public int currentMagicShield;
    public int maxMana;
    public int currentMana;
    public int attackDamage;
    public float maxBlockStamina;
    public float blockStamina;
    public int gold; // Thêm vàng
    public string currentStage; // Màn hiện tại (hoặc string stageName)
    public List<string> learnedSkills = new List<string>(); // Kỹ năng đã học
    public List<ItemData> inventory = new List<ItemData>();
    
    // Equipment system
    public List<EquippedItemData> equippedItems = new List<EquippedItemData>();
}

/// <summary>
/// Lưu trữ thông tin về một item đã được trang bị
/// </summary>
[Serializable]
public class EquippedItemData
{
    public EquipmentType equipmentType;
    public string itemName;
    public int attackBonus;
    public int armorBonus;
    public int magicResistBonus;
    public int healthBonus;
    public int manaBonus;
    
    public EquippedItemData() { }
    
    public EquippedItemData(EquipmentType type, ItemInfo item)
    {
        equipmentType = type;
        itemName = item.itemName;
        attackBonus = item.attackBonus;
        armorBonus = item.armorBonus;
        magicResistBonus = item.magicResistBonus;
        healthBonus = item.healthBonus;
        manaBonus = item.manaBonus;
    }
}
