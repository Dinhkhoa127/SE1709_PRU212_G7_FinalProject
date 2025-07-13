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
        Debug.Log($"🚀 PlayerHealthBarUI Start: player={player != null}, healthSlider={healthSlider != null}");
        
        if (healthSlider == null)
        {
            Debug.LogError($"❌ HealthSlider is null! Please assign it in Inspector.");
        }
        
        UpdateSliderValues();
    }

    private int lastCurrentHealth = -1;
    
    void Update()
    {
        if (player != null && healthSlider != null)
        {
            int currentMaxHealth = player.GetMaxHealth();
            int currentHealth = player.GetHealth();
            
            // Cập nhật khi có thay đổi
            if (currentMaxHealth != lastMaxHealth || currentHealth != lastCurrentHealth)
            {
                UpdateSliderValues();
                Debug.Log($"HealthBar updated - MaxHP: {currentMaxHealth}, HP: {currentHealth}");
            }
        }
    }

    public void UpdateSliderValues()
    {
        if (player != null && healthSlider != null)
        {
            lastMaxHealth = player.GetMaxHealth();
            lastCurrentHealth = player.GetHealth();
            healthSlider.maxValue = lastMaxHealth;
            healthSlider.value = lastCurrentHealth;
        }
    }
}
