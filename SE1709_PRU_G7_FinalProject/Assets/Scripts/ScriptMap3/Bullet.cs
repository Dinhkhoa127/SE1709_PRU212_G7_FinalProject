using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 10;
    private Transform target;
    private Vector2 moveDirection; // Lưu hướng bay
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            moveDirection = (target.position - transform.position).normalized;
        }
        else
        {
            moveDirection = transform.right; // Đạn bay thẳng nếu không có mục tiêu
        }
    }

    void Update()
    {
        rb.linearVelocity = (Vector3)moveDirection * speed;

        // Xoay đạn theo hướng bay
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Kiểm tra xem player có script IDamageable không
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            Destroy(gameObject); // Hủy đạn sau khi gây sát thương
        }
    }
}
