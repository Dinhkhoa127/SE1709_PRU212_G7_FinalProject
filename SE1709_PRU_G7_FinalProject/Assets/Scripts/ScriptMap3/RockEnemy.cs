using System.Collections;
using UnityEngine;

public class RockEnemy : Enemy1, IDamageable
{
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;

    private Animator animator;
    private int direction = 1;
    private bool is_Chasing = false;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        if (PlayerInAttackRange())
        {
            if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
            {
                animator.SetTrigger("Attack");
                isAttacking = true; // Ngăn spam animation
            }
        }
        else if (CheckInRange())
        {
            ChasePlayer();
        }
        else
        {
            animator.SetBool("isRun", false);
        }
    }

    protected override bool CheckInRange()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= detectionRange;
    }

    private bool PlayerInAttackRange()
    {
        float distanceToPlayer = Vector2.Distance(attackPoint.position, player.position);
        return distanceToPlayer <= attackRange;
    }

    protected virtual void ChasePlayer()
    {
        is_Chasing = true;
        isAttacking = false;
        animator.SetBool("isRun", true);

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        if ((directionToPlayer.x > 0 && direction < 0) || (directionToPlayer.x < 0 && direction > 0))
        {
            Flip();
        }

        transform.position += new Vector3(Mathf.Sign(directionToPlayer.x) * RunSpeed * Time.deltaTime, 0, 0);
    }

    // ✅ Gọi từ Animation Event
    protected override void Attack()
    {
        DealDamage();
        audioSource?.PlayOneShot(attackSound);
        lastAttackTime = Time.time;
        isAttacking = false; // Reset để cho phép tấn công lần sau
    }

    private void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(PhysicalDame);
                Debug.Log("RockEnemy gây sát thương!");
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthBar.fillAmount = currentHealth / Hp;

        if (currentHealth <= 0)
        {
            Die();
        }

        animator.SetTrigger("Hurt");
        Invoke(nameof(ResetHurt), 0.3f);
    }

    void ResetHurt()
    {
        animator.ResetTrigger("Hurt");
        animator.SetBool("isHurt", false);
    }

    protected override void Die()
    {
        animator.SetTrigger("Die");
        StartCoroutine(ReturnToPoolAfterDelay());
    }

    private IEnumerator ReturnToPoolAfterDelay()
    {
        Enemy_Pool pool = Object.FindFirstObjectByType<Enemy_Pool>();
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        ResetState();
        pool.ReturnToPool(gameObject);
    }

    protected override void Flip()
    {
        direction *= -1;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        healthBar.transform.localScale = new Vector3(-healthBar.transform.localScale.x, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
    }

    private void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    protected override void Patrol() { }
}
