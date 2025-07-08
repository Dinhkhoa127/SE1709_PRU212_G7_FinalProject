using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider healthSlider; // Kéo Slider vào đây trong Inspector

    // Gọi khi khởi tạo hoặc khi Enemy đổi max máu
    public void Setup(int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    // Gọi mỗi khi máu thay đổi
    public void UpdateHealth(int currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
}
