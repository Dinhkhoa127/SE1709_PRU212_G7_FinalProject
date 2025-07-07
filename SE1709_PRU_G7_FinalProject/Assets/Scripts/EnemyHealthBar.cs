using UnityEngine;
using System.Collections.Generic;

public class EnemyHealthBar : MonoBehaviour
{
    public GameObject healthUnitPrefab; // Prefab ô máu màu đỏ (foreground)
    public GameObject healthBackgroundPrefab; // Prefab ô máu màu đen (background)
    public float spacing = 18f; // Khoảng cách giữa các ô máu
    private List<GameObject> healthUnits = new List<GameObject>(); // Ô màu đỏ
    private List<GameObject> healthBackgrounds = new List<GameObject>(); // Ô màu đen

    public void Setup(int maxHealth)
    {
        // Xóa các ô cũ nếu có
        foreach (var unit in healthUnits)
            Destroy(unit);
        foreach (var bg in healthBackgrounds)
            Destroy(bg);
        healthUnits.Clear();
        healthBackgrounds.Clear();

        // Tính toán vị trí bắt đầu để căn giữa
        float totalWidth = (maxHealth - 1) * spacing;
        float startX = -totalWidth / 2f;

        // Tạo các ô mới
        for (int i = 0; i < maxHealth; i++)
        {
            Vector2 position = new Vector2(startX + i * spacing, 0);
            
            // Tạo ô nền (màu đen)
            if (healthBackgroundPrefab != null)
            {
                GameObject background = Instantiate(healthBackgroundPrefab, transform);
                RectTransform bgRt = background.GetComponent<RectTransform>();
                bgRt.anchoredPosition = position;
                healthBackgrounds.Add(background);
            }
            
            // Tạo ô máu (màu đỏ) - đặt sau để nằm trên ô nền
            GameObject unit = Instantiate(healthUnitPrefab, transform);
            RectTransform rt = unit.GetComponent<RectTransform>();
            rt.anchoredPosition = position;
            healthUnits.Add(unit);
        }
    }

    public void UpdateHealth(int currentHealth)
    {
        for (int i = 0; i < healthUnits.Count; i++)
        {
            // Chỉ ẩn/hiện ô màu đỏ (foreground)
            // Ô màu đen (background) luôn hiển thị
            healthUnits[i].SetActive(i < currentHealth);
        }
    }
}
