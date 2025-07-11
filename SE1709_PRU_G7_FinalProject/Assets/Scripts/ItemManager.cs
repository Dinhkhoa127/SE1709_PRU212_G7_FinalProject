using UnityEngine;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour
{
    [Header("Item Database")]
    public List<ItemInfo> allItems = new List<ItemInfo>();
    
    public static ItemManager Instance;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public ItemInfo GetItemInfo(string itemName)
    {
        return allItems.Find(item => item.itemName == itemName);
    }
} 