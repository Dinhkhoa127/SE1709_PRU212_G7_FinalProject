using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class SkillProjectile:MonoBehaviour
    {
        public float speed = 10f;
        public int damage = 1;

        private Vector2 direction;

        public void SetDirection(Vector2 dir)
        {
            direction = dir.normalized;
        }

        void Update()
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
                Destroy(gameObject); // hủy vật thể sau khi trúng địch
            }
            else if (other.CompareTag("Ground"))
            {
                Destroy(gameObject); // hủy khi chạm tường hoặc nền
            }
        }

    }
}
