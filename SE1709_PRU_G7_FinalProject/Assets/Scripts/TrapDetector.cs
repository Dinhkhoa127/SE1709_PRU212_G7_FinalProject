using UnityEngine;

public class TrapDetector : MonoBehaviour
{
    private bool isDead = false;
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (isDead) return;
        if (collider.CompareTag("Player"))
        {
            isDead = true;
            // Gọi đúng script PlayerKnight và dùng Die()
            PlayerKnight player = collider.GetComponent<PlayerKnight>();
            if (player != null)
            {
                player.SendMessage("Die");
            }
        }
    }
}
