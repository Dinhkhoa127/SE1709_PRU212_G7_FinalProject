using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    [SerializeField] public Slider healthSlider;
    private PlayerKnight player;
    private int lastMaxHealth = -1;

    void Start()
    {
        player = FindObjectOfType<PlayerKnight>();
        UpdateSliderValues();
    }

    void Update()
    {
        if (player != null && healthSlider != null)
        {
            // Nếu maxHealth thay đổi thì cập nhật lại maxValue
            if (player.GetMaxHealth() != lastMaxHealth)
            {
                UpdateSliderValues();
            }
            healthSlider.value = player.GetHealth();
        }
    }

    void UpdateSliderValues()
    {
        if (player != null && healthSlider != null)
        {
            lastMaxHealth = player.GetMaxHealth();
            healthSlider.maxValue = lastMaxHealth;
            healthSlider.value = player.GetHealth();
        }
    }
}
