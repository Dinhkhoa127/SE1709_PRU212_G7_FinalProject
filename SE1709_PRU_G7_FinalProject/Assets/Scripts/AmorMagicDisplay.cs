using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Script
{
    public class AmorMagicDisplay: MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI armorText;
        [SerializeField] private TextMeshProUGUI magicShieldText;

        private PlayerKnight player;

        void Start()
        {
            player = FindObjectOfType<PlayerKnight>();
        }

        void Update()
        {
            if (player != null)
            {
                armorText.text = $"Armor: {player.GetCurrentArmorShield()}";
                magicShieldText.text = $"Magic Resist: {player.GetCurrentMagicShield()}";
            }
        }
    }
}
