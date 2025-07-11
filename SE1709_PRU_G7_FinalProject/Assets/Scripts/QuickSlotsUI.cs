using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickSlotsUI : MonoBehaviour
{
    [Header("Health Slot")]
    public Image healthIcon;
    public TextMeshProUGUI healthCount;
    public GameObject healthSlot;
    
    [Header("Mana Slot")]
    public Image manaIcon;
    public TextMeshProUGUI manaCount;
    public GameObject manaSlot;
    
    [Header("Item Names")]
    public string healthPotionName = "Health Potion";
    public string manaPotionName = "Mana Potion";
    
    private PlayerKnight player;
    
    void Start()
    {
        player = FindObjectOfType<PlayerKnight>();
        UpdateSlots();
    }
    
    void Update()
    {
        // Cập nhật slots mỗi frame để đảm bảo real-time
        UpdateSlots();
    }
    
    public void UpdateSlots()
    {
        if (player == null) return;
        
        // Cập nhật Health Slot
        int healthCount = player.GetItemQuantity(healthPotionName);
        UpdateSlot(healthSlot, healthIcon, this.healthCount, healthCount);
        
        // Cập nhật Mana Slot
        int manaCount = player.GetItemQuantity(manaPotionName);
        UpdateSlot(manaSlot, manaIcon, this.manaCount, manaCount);
    }
    
    void UpdateSlot(GameObject slot, Image icon, TextMeshProUGUI countText, int quantity)
    {
        if (quantity > 0)
        {
            // Có item - hiển thị slot
            slot.SetActive(true);
            icon.color = Color.white; // Icon sáng
            countText.text = quantity.ToString();
            countText.color = Color.white;
        }
        else
        {
            // Không có item - làm mờ slot
            slot.SetActive(true); // Vẫn hiển thị để player biết có slot
            icon.color = new Color(1f, 1f, 1f, 0.3f); // Icon mờ
            countText.text = "0";
            countText.color = new Color(1f, 1f, 1f, 0.5f); // Text mờ
        }
    }
    
    // Phương thức để các script khác gọi cập nhật
    public void ForceUpdate()
    {
        UpdateSlots();
    }
} 