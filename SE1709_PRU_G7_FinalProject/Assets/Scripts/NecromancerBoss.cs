using System.Collections;
using UnityEngine;

public class NecromancerBoss : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] public float detectionRange = 6f;
    [SerializeField] public float attackRange = 1.5f;
    [SerializeField] public float moveSpeed = 2f;
    [SerializeField] public int attack1Damage = 4;
    [SerializeField] public int attack2Damage = 6;
    [SerializeField] public float attackCooldown = 2f;

    [Header("Health Settings")]
    [SerializeField] public float maxHealth = 15f;
    [SerializeField] public float healDelay = 5f;
    [SerializeField] public EnemyHealthBar healthBar;
    [SerializeField] public GameObject healthBarSlider;

    [Header("Teleport Settings")]
    [SerializeField] private Transform teleportLeft;
    [SerializeField] private Transform teleportRight;

    [Header("Attack Points")]
    [SerializeField] Transform attack1Point;
    [SerializeField] Transform attack2Point;
    public LayerMask playerLayer;

    private GameObject player;
    private Animator animator;
    private Rigidbody2D rb;

    private float health;
    private float lastAttackTime;
    private float lastTimeSawPlayer = -999f;
    private bool isFacingRight = true;
    private bool hasHealed = false;
    private bool isAttacking = false;
    private bool isHurting = false;
    private bool isGrounded = false;

    private int currentAttackType = 1;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        health = maxHealth;
        if (healthBar != null)
        {
            healthBar.Setup((int)maxHealth);
            healthBar.UpdateHealth((int)health);
        }
    }

    void Update()
    {
        if (player == null || isHurting) return;


        float distance = Vector2.Distance(transform.position, player.transform.position);

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (isAttacking && !state.IsName("Attack1") && !state.IsName("Attack2"))
        {
            isAttacking = false;
        }

        if ((player.transform.position.x < transform.position.x && isFacingRight) ||
            (player.transform.position.x > transform.position.x && !isFacingRight))
        {
            Flip();
        }

        if (distance <= detectionRange)
        {
            lastTimeSawPlayer = Time.time;
            hasHealed = false;

            if (distance > attackRange)
            {
                if (!isAttacking)
                {
                    animator.SetBool("isRun", true);
                    MoveToPlayer();
                }
                else
                {
                    animator.SetBool("isRun", false);
                }
            }
            else
            {
                animator.SetBool("isRun", false);

                if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
                {
                    isAttacking = true;
                    currentAttackType = Random.Range(1, 3);

                    if (currentAttackType == 1)
                        animator.SetTrigger("Attack1");
                    else
                        animator.SetTrigger("Attack2");

                    lastAttackTime = Time.time;
                }
            }
        }
        else
        {
            animator.SetBool("isRun", false);
            HealIfNeeded();
        }
    }

    void MoveToPlayer()
    {
        Vector2 target = new Vector2(player.transform.position.x, rb.position.y);
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPos);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void DealDamage1()
    {
        if (attack1Point == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attack1Point.position, 2.2f, playerLayer);
        foreach (Collider2D hit in hits)
        {
            PlayerKnight p = hit.GetComponent<PlayerKnight>();
            if (p != null)
            {
                p.TakePhysicalDamage(attack1Damage);
            }
        }
    }

    public void DealDamage2()
    {
        if (attack2Point == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attack2Point.position, 1.8f, playerLayer);
        foreach (Collider2D hit in hits)
        {
            PlayerKnight p = hit.GetComponent<PlayerKnight>();
            if (p != null)
            {
                p.TakePhysicalDamage(attack2Damage);
            }
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        if (!isHurting && health > 0)
        {
            isHurting = true;
            //animator.SetTrigger("Hurt");
        }

        StartCoroutine(HandleHurtAndTeleport());

        if (healthBar != null)
            healthBar.UpdateHealth((int)health);

        if (health <= 0)
        {
            Die();
        }
    }

    public void EndHurt()
    {
        isHurting = false;
    }

    void Die()
    {
        Debug.Log("Necromancer is dead");

        this.enabled = false;
        healthBarSlider.SetActive(false);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        animator.SetTrigger("Death");
        Destroy(gameObject, 2.5f);
    }

    void HealIfNeeded()
    {
        if (health < maxHealth && Time.time - lastTimeSawPlayer > healDelay && !hasHealed)
        {
            health = maxHealth;
            hasHealed = true;
            if (healthBar != null)
                healthBar.UpdateHealth((int)health);
        }
    }

    IEnumerator TeleportBehindPlayer()
    {
        // 1. Hiệu ứng nhấp nháy
        yield return StartCoroutine(TeleportEffect());

        if (player == null) yield break;

        // 2. Tính vị trí phía sau lưng player
        bool playerFacingRight = player.transform.localScale.x > 0;
        float offsetX = playerFacingRight ? -1.5f : 1.5f; // Đằng sau player
        Vector2 teleportPos = new Vector2(player.transform.position.x + offsetX, transform.position.y);
        transform.position = teleportPos;

        // 3. Lật mặt boss đúng hướng về phía Player
        if ((player.transform.position.x < transform.position.x && isFacingRight) ||
            (player.transform.position.x > transform.position.x && !isFacingRight))
        {
            Flip();
        }
    }

    IEnumerator HandleHurtAndTeleport()
    {
        // Giả sử animation Hurt dài khoảng 0.4 giây
        yield return new WaitForSeconds(0.4f);

        yield return StartCoroutine(TeleportBehindPlayer());

        isHurting = false; // Kết thúc trạng thái hurt (có thể gọi từ AnimationEvent thay thế)
    }


    IEnumerator TeleportEffect()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color originalColor = sr.color;

        // Nhấp nháy trắng xám 2 lần
        for (int i = 0; i < 2; i++)
        {
            sr.color = new Color(1f, 1f, 1f, 0.5f); // Trắng mờ
            yield return new WaitForSeconds(0.05f);
            sr.color = new Color(0.6f, 0.6f, 0.6f, 0.8f); // Xám mờ
            yield return new WaitForSeconds(0.05f);
        }

        sr.color = originalColor;
    }


    void OnDrawGizmosSelected()
    {
        if (attack1Point != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attack1Point.position, 2.2f);
        }

        if (attack2Point != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attack2Point.position, 1.8f);
        }
    }
}
