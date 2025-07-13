using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Vector3 movementDirection;

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        if (movementDirection == Vector3.zero) return;
        transform.position += movementDirection * Time.deltaTime;
    }

    public void SetMovementDirection(Vector3 direction)
    {
        movementDirection = direction;

        // Flip sprite nếu bay trái
        if (movementDirection.x < 0f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * -1f;
            transform.localScale = scale;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Gây sát thương nếu có interface IDamageable
            IDamageable target = other.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(10); // Sát thương tuỳ chỉnh
            }

            Destroy(gameObject); // Biến mất sau va chạm
        }
    }
}
