using System.Collections.Generic;
using UnityEngine;

public class PaladinBoss : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] public float detectionRange = 6f;
    [SerializeField] public float attackRange = 1.5f;
    [SerializeField] public float moveSpeed = 2f;
    [SerializeField] public int attack1Damage = 5;
    [SerializeField] public int attack2Damage = 3;
    [SerializeField] public float attackCooldown = 2f;
    [SerializeField] public float attackSwitchInterval = 5f;

    [Header("Health Settings")]
    [SerializeField] public float maxHealth = 20f;
    [SerializeField] public float healDelay = 5f;
    [SerializeField] public EnemyHealthBar healthBar;
    [SerializeField] public GameObject healthBarSlider;

    [Header("Attack Points")]
    [SerializeField] Transform attack1Point;
    [SerializeField] List<Transform> attack2Points;
    public LayerMask playerLayer;

    private GameObject player;
    private Animator animator;
    private Rigidbody2D rb;

    private float health;
    private float lastAttackTime;
    private float attackTypeTimer;
    private float lastTimeSawPlayer = -999f;
    private bool isFacingRight = true;
    private bool hasHealed = false;
    private bool isAttacking = false;
    private bool isHurting = false;

    private int currentAttackType = 1;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        attackTypeTimer = Time.time;

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

        // Nếu attack animation đã kết thúc thì tắt isAttacking
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (isAttacking && !state.IsName("Attack1") && !state.IsName("Attack2"))
        {
            isAttacking = false;
        }

        // Flip hướng
        if ((player.transform.position.x < transform.position.x && isFacingRight) ||
            (player.transform.position.x > transform.position.x && !isFacingRight))
        {
            Flip();
        }

        // Chuyển đổi kiểu attack mỗi 5s
        if (Time.time - attackTypeTimer >= attackSwitchInterval)
        {
            currentAttackType = (currentAttackType == 1) ? 2 : 1;
            attackTypeTimer = Time.time;
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
                    animator.SetBool("isRun", false); // Nếu đang attack, không chạy
                }
            }
            else
            {
                animator.SetBool("isRun", false);

                if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
                {
                    isAttacking = true;
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

        Collider2D[] hits = Physics2D.OverlapCircleAll(attack1Point.position, 1.8f, playerLayer);
        foreach (Collider2D hit in hits)
        {
            PlayerKnight p = hit.GetComponent<PlayerKnight>();
            if (p != null)
            {
                Debug.Log($"Hit player with Attack1 at {attack1Point.position}");
                p.TakePhysicalDamage(attack1Damage);
            }
        }
    }

    public void DealDamage2()
    {
        foreach (Transform point in attack2Points)
        {
            if (point == null) continue;

            Collider2D[] hits = Physics2D.OverlapCircleAll(point.position, 1.8f, playerLayer);
            foreach (Collider2D hit in hits)
            {
                PlayerKnight p = hit.GetComponent<PlayerKnight>();
                if (p != null)
                {
                    Debug.Log($"Hit player at {point.position} with {point.name}");
                    p.TakePhysicalDamage(attack2Damage);
                }
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
            animator.SetTrigger("Hurt");
        }

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
        Debug.Log("Paladin is dead");

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
        Destroy(gameObject, 7.5f);
    }

    void HealIfNeeded()
    {
        if (health < maxHealth && Time.time - lastTimeSawPlayer > healDelay && !hasHealed)
        {
            health = maxHealth;
            hasHealed = true;
            if (healthBar != null)
                healthBar.UpdateHealth((int)health);
            Debug.Log($"Boss healed after {healDelay}s: {health}/{maxHealth}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attack1Point != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attack1Point.position, 1.8f);
        }

        if (attack2Points != null)
        {
            Gizmos.color = Color.magenta;
            foreach (Transform point in attack2Points)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 1.8f);
            }
        }
    }
}
