using UnityEngine;

public class DiePoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.CompareTag("Player"))
        //{
        //    IDamageable damageable = collision.GetComponent<IDamageable>();
        //    if (damageable != null)
        //    {
        //        damageable.TakeDamage(9999);
        //    }
        //}
        if (collision.CompareTag("Player"))
        {
            // Lấy component PlayerKnight
            var player = collision.GetComponent<PlayerKnight>();
            if (player != null)
            {
                // Gây sát thương cực lớn để đảm bảo chết ngay
                player.TakeDamage(99999);
                Debug.Log("[DiePoint] Player died!");
                return;
            }

            // Fallback nếu không tìm thấy PlayerKnight component
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(99999);
            }
        }
    }
}

