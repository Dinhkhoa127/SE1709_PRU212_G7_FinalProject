using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class FPromptFollower : MonoBehaviour
    {
        public Transform targetNPC; // Gắn NPC vào đây trong Inspector
        public Vector3 offset = new Vector3(0, 1.5f, 0); // Đặt vị trí text phía trên đầu NPC

        void Update()
        {
            if (targetNPC != null)
            {
                transform.position = targetNPC.position + offset;

                // Luôn giữ text hướng phải (không bị lật theo NPC)
                Vector3 scale = transform.localScale;
                scale.x = 1;
                transform.localScale = scale;
            }
        }
    }
}
