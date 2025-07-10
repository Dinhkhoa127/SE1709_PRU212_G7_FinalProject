using UnityEngine;

public class TrapDetector : MonoBehaviour
{
    [SerializeField] private bool killOnTouch = true;      // Nếu true thì gọi Die(), false thì chỉ trừ máu
    [SerializeField] private int damageAmount = 1;        // Lượng máu bị trừ nếu không chết liền

    private bool isDead = false;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (isDead) return;

        if (collider.CompareTag("Player"))
        {
            PlayerKnight player = collider.GetComponent<PlayerKnight>();
            if (player != null)
            {
                if (killOnTouch)
                {
                    isDead = true;
                    player.SendMessage("Die"); // Giết ngay
                }
                else
                {
                    player.SendMessage("TakeDamage", damageAmount); // Gọi hàm trừ máu
                }
            }
        }
    }
}
