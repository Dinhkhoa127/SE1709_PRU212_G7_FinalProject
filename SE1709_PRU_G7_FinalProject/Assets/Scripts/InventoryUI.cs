using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public PlayerKnight player;
    public Transform gridParent; // Gán là GridPanel
    public GameObject itemSlotPrefab;
    public Sprite healthPotionSprite;
    public Sprite manaPotionSprite;

    void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (var item in player.inventory)
        {
            // Chỉ hiển thị items có số lượng > 0
            if (item.quantity <= 0) continue;
            
            var slot = Instantiate(itemSlotPrefab, gridParent);
            // Gán icon
            var icon = slot.transform.Find("ItemIcon").GetComponent<Image>();
            if (item.itemName == "Health Potion") // Sửa tên cho đồng nhất
                icon.sprite = healthPotionSprite;
            else if (item.itemName == "Mana Potion") // Sửa tên cho đồng nhất
                icon.sprite = manaPotionSprite;
            // Gán số lượng
            var countText = slot.transform.Find("ItemCount").GetComponent<TextMeshProUGUI>();
            countText.text = "x" + item.quantity;
        }
    }
}
