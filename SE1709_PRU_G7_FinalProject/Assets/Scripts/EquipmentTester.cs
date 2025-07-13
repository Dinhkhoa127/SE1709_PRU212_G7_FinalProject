using UnityEngine;

public class EquipmentTester : MonoBehaviour
{
    [Header("Test Settings")]
    public PlayerKnight player;
    
    void Update()
    {
        // Test Equipment với số 8, 9, 0, - (Minus)
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            TestEquipItem(); // Helmet với debug logs chi tiết
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            TestEquipSword();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            TestEquipArmor();
        }
        
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            ClearAllEquipment();
        }
    }
    
    void TestEquipItem()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerKnight>();
            if (player == null)
            {
                Debug.LogError("PlayerKnight not found!");
                return;
            }
        }
        
        // Create legendary helmet with HIGH health bonus (+50 HP)
        ItemInfo testHelmet = CreateTestHelmet();
        
        // Add to inventory first
        player.AddItem(testHelmet.itemName, 1);
        
        // Then equip it
        player.EquipItem(testHelmet);
        
        Debug.Log($"=== EQUIPMENT TEST ===");
        Debug.Log($"Equipped: {testHelmet.itemName}");
        Debug.Log($"Health Bonus: +{testHelmet.healthBonus}");
        Debug.Log($"Player MaxHealth: {player.GetMaxHealth()}");
        Debug.Log($"Player Current Health: {player.GetCurrentHealth()}");
        Debug.Log($"Player Attack: {player.GetAttackDamage()}");
        Debug.Log($"Player Armor: {player.GetCurrentArmorShield()}");
        Debug.Log($"======================");
    }
    
    ItemInfo CreateTestHelmet()
    {
        ItemInfo helmet = ScriptableObject.CreateInstance<ItemInfo>();
        helmet.itemName = "Legendary Helmet";
        helmet.itemType = ItemType.Equipment;
        helmet.equipmentType = EquipmentType.Helmet;
        helmet.basePrice = 100;
        helmet.attackBonus = 2;
        helmet.armorBonus = 5;
        helmet.magicResistBonus = 3;
        helmet.healthBonus = 50; // TĂNG MẠNH để dễ thấy
        helmet.manaBonus = 20;
        helmet.description = "A legendary helmet that greatly increases your vitality.";
        
        return helmet;
    }
    
    ItemInfo CreateTestSword()
    {
        ItemInfo sword = ScriptableObject.CreateInstance<ItemInfo>();
        sword.itemName = "Legendary Sword";
        sword.itemType = ItemType.Equipment;
        sword.equipmentType = EquipmentType.Weapon;
        sword.basePrice = 150;
        sword.attackBonus = 15;
        sword.armorBonus = 2;
        sword.magicResistBonus = 1;
        sword.healthBonus = 30; // THÊM HEALTH BONUS
        sword.manaBonus = 10;
        sword.description = "A legendary sword that empowers the wielder with vitality.";
        
        return sword;
    }
    
    ItemInfo CreateTestArmor()
    {
        ItemInfo armor = ScriptableObject.CreateInstance<ItemInfo>();
        armor.itemName = "Dragon Scale Armor";
        armor.itemType = ItemType.Equipment;
        armor.equipmentType = EquipmentType.Chest;
        armor.basePrice = 500;
        armor.attackBonus = 5;
        armor.armorBonus = 20;
        armor.magicResistBonus = 15;
        armor.healthBonus = 100; // HEALTH BONUS CỰC CAO
        armor.manaBonus = 50;
        armor.description = "Legendary dragon scale armor that greatly increases vitality.";
        
        return armor;
    }
    
    [ContextMenu("Test Equip Helmet")]
    public void TestEquipHelmet()
    {
        if (player == null) player = FindObjectOfType<PlayerKnight>();
        
        ItemInfo helmet = CreateTestHelmet();
        player.AddItem(helmet.itemName, 1);
        player.EquipItem(helmet);
    }
    
    [ContextMenu("Test Equip Sword")]
    public void TestEquipSword()
    {
        if (player == null) player = FindObjectOfType<PlayerKnight>();
        
        ItemInfo sword = CreateTestSword();
        player.AddItem(sword.itemName, 1);
        player.EquipItem(sword);
        
        Debug.Log($"=== SWORD TEST ===");
        Debug.Log($"Equipped: {sword.itemName}");
        Debug.Log($"Attack Bonus: +{sword.attackBonus}");
        Debug.Log($"Health Bonus: +{sword.healthBonus}");
        Debug.Log($"Player MaxHealth: {player.GetMaxHealth()}");
        Debug.Log($"Player Attack: {player.GetAttackDamage()}");
        Debug.Log($"==================");
    }
    
    [ContextMenu("Test Equip Armor")]
    public void TestEquipArmor()
    {
        if (player == null) player = FindObjectOfType<PlayerKnight>();
        
        ItemInfo armor = CreateTestArmor();
        player.AddItem(armor.itemName, 1);
        player.EquipItem(armor);
        
        Debug.Log($"=== ARMOR TEST ===");
        Debug.Log($"Equipped: {armor.itemName}");
        Debug.Log($"Health Bonus: +{armor.healthBonus}");
        Debug.Log($"Armor Bonus: +{armor.armorBonus}");
        Debug.Log($"Player MaxHealth: {player.GetMaxHealth()}");
        Debug.Log($"Player Armor: {player.GetCurrentArmorShield()}");
        Debug.Log($"==================");
    }
    
    [ContextMenu("Clear All Equipment")]
    public void ClearAllEquipment()
    {
        if (player == null) player = FindObjectOfType<PlayerKnight>();
        
        System.Array equipmentTypes = System.Enum.GetValues(typeof(EquipmentType));
        foreach (EquipmentType equipType in equipmentTypes)
        {
            player.UnequipItem(equipType);
        }
        
        Debug.Log($"=== CLEARED ALL EQUIPMENT ===");
        Debug.Log($"Player MaxHealth: {player.GetMaxHealth()}");
        Debug.Log($"Player Attack: {player.GetAttackDamage()}");
        Debug.Log($"==============================");
    }
    
    void Start()
    {
        // Hiển thị hướng dẫn test với key mới
        Debug.Log($"=== EQUIPMENT TESTER CONTROLS ===");
        Debug.Log($"8 - Test Equip Helmet (+50 HP)");
        Debug.Log($"9 - Test Equip Sword (+30 HP)");
        Debug.Log($"0 - Test Equip Armor (+100 HP)");
        Debug.Log($"- (Minus) - Clear All Equipment");
        Debug.Log($"==================================");
    }
} 