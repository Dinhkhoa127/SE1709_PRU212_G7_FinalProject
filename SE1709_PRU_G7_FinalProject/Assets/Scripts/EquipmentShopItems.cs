using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EquipmentShopItems", menuName = "Shop/Equipment Shop Items")]
public class EquipmentShopItems : ScriptableObject
{
    [Header("Equipment Items for Sale")]
    public List<ItemInfo> equipmentItems = new List<ItemInfo>();
    
    [Header("Equipment Categories")]
    [Tooltip("Items sẽ được categorize theo equipment type")]
    public List<ItemInfo> helmets = new List<ItemInfo>();
    public List<ItemInfo> chestArmor = new List<ItemInfo>();
    public List<ItemInfo> legArmor = new List<ItemInfo>();
    public List<ItemInfo> boots = new List<ItemInfo>();
    public List<ItemInfo> weapons = new List<ItemInfo>();
    public List<ItemInfo> shields = new List<ItemInfo>();
    public List<ItemInfo> rings = new List<ItemInfo>();
    public List<ItemInfo> necklaces = new List<ItemInfo>();
    
    void OnValidate()
    {
        // Auto-populate equipmentItems list from categories
        equipmentItems.Clear();
        equipmentItems.AddRange(helmets);
        equipmentItems.AddRange(chestArmor);
        equipmentItems.AddRange(legArmor);
        equipmentItems.AddRange(boots);
        equipmentItems.AddRange(weapons);
        equipmentItems.AddRange(shields);
        equipmentItems.AddRange(rings);
        equipmentItems.AddRange(necklaces);
    }
    
    public List<ItemInfo> GetEquipmentItems()
    {
        return equipmentItems;
    }
    
    public List<ItemInfo> GetItemsByType(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Helmet: return helmets;
            case EquipmentType.Chest: return chestArmor;
            case EquipmentType.Leg: return legArmor;
            case EquipmentType.Boot: return boots;
            case EquipmentType.Weapon: return weapons;
            case EquipmentType.Shield: return shields;
            case EquipmentType.Ring: return rings;
            case EquipmentType.Necklace: return necklaces;
            default: return new List<ItemInfo>();
        }
    }
} 