using UnityEngine;

public class DarkBolt : MonoBehaviour
{
    public int damage = 30;
    private Collider2D col;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip clip;
    public float lifeTime = 5f;
    void Start()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false; // Tắt collider ban đầu
        audioSource = GetComponent<AudioSource>();
        Destroy(gameObject, lifeTime);
    }
    public void SkillThunder()
    {
        audioSource.PlayOneShot(clip);
    }
    public void EnableCollider() // Gọi từ Animation Event
    {
        col.enabled = true;
    }

    public void DisableCollider() // Gọi khi kết thúc Animation
    {
        col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Untagged"))
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log("Player mất " + damage + " máu do Dark Bolt!");
            }
            Destroy(gameObject); // Xoá Dark Bolt sau khi gây sát thương
        }
    }
}
