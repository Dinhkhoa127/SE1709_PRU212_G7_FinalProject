using System;
using System.Collections.Generic;

/// <summary>
/// Lưu trữ toàn bộ dữ liệu cần thiết của Player để save/load.
/// </summary>
[Serializable]
public class PlayerData
{
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
}
