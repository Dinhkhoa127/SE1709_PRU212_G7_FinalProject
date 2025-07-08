using UnityEngine;
using UnityEngine.UI;

public class PlayerBlockBarUI : MonoBehaviour
{
    [SerializeField] public Slider blockSlider;
    private PlayerKnight player;
    private float lastMaxBlockStamina = -1f;

    void Start()
    {
        player = FindObjectOfType<PlayerKnight>();
        UpdateSliderValues();
    }

    void Update()
    {
        if (player != null && blockSlider != null)
        {
            // Nếu maxBlockStamina thay đổi thì cập nhật lại maxValue
            if (player.MaxBlockStamina != lastMaxBlockStamina)
            {
                UpdateSliderValues();
            }
            blockSlider.value = player.BlockStamina;
            blockSlider.interactable = !player.IsBlockOnCooldown;
        }
    }

    void UpdateSliderValues()
    {
        if (player != null && blockSlider != null)
        {
            lastMaxBlockStamina = player.MaxBlockStamina;
            blockSlider.maxValue = lastMaxBlockStamina;
            blockSlider.value = player.BlockStamina;
        }
    }
}
