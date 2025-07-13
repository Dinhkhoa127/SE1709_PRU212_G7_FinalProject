using UnityEngine;

public class Bullet2 : MonoBehaviour
{
    [SerializeField] private float damage = 10f;  // Sát thương gây ra
    [SerializeField] private float lifeTime = 3f; // Thời gian tồn tại của viên đạn
    [SerializeField] private float speed = 10f;   // Tốc độ đạn

    private Vector2 direction; // Hướng bay của viên đạn

    void Start()
    {
        Destroy(gameObject, lifeTime); // Hủy viên đạn sau khi hết thời gian
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    public void SetTarget(Transform target)
    {
        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            IDamageable player = collision.GetComponent<IDamageable>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            Destroy(gameObject); // Hủy viên đạn sau khi va chạm
        }
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject); // Hủy viên đạn nếu chạm vào mặt đất
        }
    }
}
