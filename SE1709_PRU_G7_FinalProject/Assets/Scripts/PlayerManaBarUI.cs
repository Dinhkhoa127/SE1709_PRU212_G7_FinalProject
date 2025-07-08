using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script
{
    public class PlayerManaBarUI : MonoBehaviour
    {
        [SerializeField] public Slider manaSlider;
        private PlayerKnight player;
        private int lastMaxMana = -1;

        void Start()
        {
            player = FindObjectOfType<PlayerKnight>();
            UpdateSliderValues();
        }

        void Update()
        {
            if (player != null && manaSlider != null)
            {
                // Nếu maxMana thay đổi thì cập nhật lại maxValue
                if (player.GetMaxMana() != lastMaxMana)
                {
                    UpdateSliderValues();
                }

                // Cập nhật giá trị thanh mana theo thời gian thực
                manaSlider.value = player.GetCurrentMana();
            }
        }

        void UpdateSliderValues()
        {
            if (player != null && manaSlider != null)
            {
                lastMaxMana = player.GetMaxMana();
                manaSlider.maxValue = lastMaxMana;
                manaSlider.value = player.GetCurrentMana();
            }
        }
    }
}
