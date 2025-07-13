using UnityEngine;
public class TrapController : MonoBehaviour
{
    //public int damage = 10;
    //public float damageInterval = 1.0f;
    //private float lastDamageTime = 0f;

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    IDamageable damageable = collision.GetComponent<IDamageable>();

    //    if (damageable != null && Time.time >= lastDamageTime + damageInterval)
    //    {
    //        Vector2 direction = new Vector2(Random.Range(-2, 2), 50);
    //        collision.GetComponent<Rigidbody2D>().AddForce(direction * 1);
    //        damageable.TakeDamage(damage);
    //        lastDamageTime = Time.time;
    //        Debug.Log("Take dame");
    //    }
    //}
    [SerializeField] private float physicalDamage = 10f;  // Sát thýõng v?t l?
    [SerializeField] private float damageInterval = 1.0f; // Th?i gian gi?a các l?n gây damage
    private float lastDamageTime = 0f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Time.time < lastDamageTime + damageInterval) return;

        if (collision.CompareTag("Player"))
        {
            // Ýu tiên x? l? PlayerKnight trý?c
            var player = collision.GetComponent<PlayerKnight>();
            if (player != null)
            {
                player.TakePhysicalDamage((int)physicalDamage);
                lastDamageTime = Time.time;
                Debug.Log($"[Trap] Dealt {physicalDamage} physical damage to player!");
                return;
            }

            // Fallback cho IDamageable
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(physicalDamage);
                lastDamageTime = Time.time;
                Debug.Log($"[Trap] Dealt {physicalDamage} damage through IDamageable!");
            }
        }
    }
}
