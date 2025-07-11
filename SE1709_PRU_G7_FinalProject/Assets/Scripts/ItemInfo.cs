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
    
    [Header("Description")]
    [TextArea(3, 5)]
    public string description;
    
    [Header("Shop Settings")]
    public bool isAvailableInShop = true;
    public int maxStackSize = 99;
}

public enum ItemType
{
    HealthPotion,
    ManaPotion,
    Equipment,
    Consumable,
    Key
} 