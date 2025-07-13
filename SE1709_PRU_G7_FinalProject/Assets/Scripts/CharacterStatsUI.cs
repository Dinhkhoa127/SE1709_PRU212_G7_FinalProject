using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterStatsUI : MonoBehaviour
{
    [Header("Character Avatar")]
    public Image characterAvatarImage;
    public Sprite defaultCharacterSprite;
    
    [Header("Health Stats")]
    public TextMeshProUGUI healthText;
    public Slider healthSlider;
    
    [Header("Mana Stats")]
    public TextMeshProUGUI manaText;
    public Slider manaSlider;
    
    [Header("Defense Stats")]
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI magicResistText;
    
    [Header("Combat Stats")]
    public TextMeshProUGUI attackDamageText;
    public TextMeshProUGUI blockStaminaText;
    public Slider blockStaminaSlider;
    
    [Header("Currency")]
    public TextMeshProUGUI goldText;
    
    [Header("Level Info")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI currentStageText;
    
    private PlayerKnight player;
    
    void Start()
    {
        player = FindObjectOfType<PlayerKnight>();
        
        // Set default character avatar
        if (characterAvatarImage != null && defaultCharacterSprite != null)
        {
            characterAvatarImage.sprite = defaultCharacterSprite;
        }
    }
    
    void OnEnable()
    {
        UpdateCharacterStats();
    }
    
    void Update()
    {
        // Update stats real-time khi inventory mở
        if (gameObject.activeInHierarchy)
        {
            UpdateCharacterStats();
        }
    }
    
    public void UpdateCharacterStats()
    {
        if (player == null) return;
        
        // Update Health
        if (healthText != null)
            healthText.text = $"Health: {player.GetMaxHealth()}";
        if (healthSlider != null)
        {
            healthSlider.maxValue = player.GetMaxHealth();
            healthSlider.value = player.GetHealth();
        }
        
        // Update Mana
        if (manaText != null)
            manaText.text = $"Mana: {player.GetMaxMana()}";
        if (manaSlider != null)
        {
            manaSlider.maxValue = player.GetMaxMana();
            manaSlider.value = player.GetCurrentMana();
        }
        
        // Update Defense (now shows total including equipment)
        if (armorText != null)
            armorText.text = $"Armor: {player.GetCurrentArmorShield()}";
        if (magicResistText != null)
            magicResistText.text = $"Magic Resist: {player.GetCurrentMagicShield()}";
        
        // Update Combat Stats (now shows total including equipment)
        if (attackDamageText != null)
            attackDamageText.text = $"Attack: {player.GetAttackDamage()}";
        if (blockStaminaText != null)
            blockStaminaText.text = $"Stamina: {player.MaxBlockStamina:F0}";
        if (blockStaminaSlider != null)
        {
            blockStaminaSlider.maxValue = player.MaxBlockStamina;
            blockStaminaSlider.value = player.BlockStamina;
        }
        
        // Update Currency
        if (goldText != null)
            goldText.text = $"Gold: {player.gold}";
        
        // Update Level Info
        if (levelText != null)
            levelText.text = $"Level: 1"; // Có thể thêm level system sau
        if (currentStageText != null)
            currentStageText.text = $"Stage: {player.currentStage}";
    }
    
    // Helper method để hiển thị equipment bonuses (optional)
    public void ShowEquipmentBonuses()
    {
        if (player == null) return;
        
        string bonusText = "Equipment Bonuses:\n";
        
        // Loop through all equipped items and show their bonuses
        System.Array equipmentTypes = System.Enum.GetValues(typeof(EquipmentType));
        foreach (EquipmentType equipType in equipmentTypes)
        {
            ItemInfo equippedItem = player.GetEquippedItem(equipType);
            if (equippedItem != null)
            {
                bonusText += $"{equipType}: {equippedItem.itemName}\n";
                if (equippedItem.attackBonus > 0) bonusText += $"  +{equippedItem.attackBonus} Attack\n";
                if (equippedItem.armorBonus > 0) bonusText += $"  +{equippedItem.armorBonus} Armor\n";
                if (equippedItem.magicResistBonus > 0) bonusText += $"  +{equippedItem.magicResistBonus} Magic Resist\n";
                if (equippedItem.healthBonus > 0) bonusText += $"  +{equippedItem.healthBonus} Health\n";
                if (equippedItem.manaBonus > 0) bonusText += $"  +{equippedItem.manaBonus} Mana\n";
            }
        }
        
        Debug.Log(bonusText);
    }
} 