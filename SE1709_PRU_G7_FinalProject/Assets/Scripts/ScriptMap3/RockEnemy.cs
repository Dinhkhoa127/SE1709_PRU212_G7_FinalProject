using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    //[SerializeField] private new Image healthBar;
    private Coroutine smoothCoroutine;

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
        if (player == null || attackPoint == null)
        {
            Debug.Log("[RockEnemy] Player hoặc attackPoint là null!");
            return false;
        }

        float distanceToPlayer = Vector2.Distance(attackPoint.position, player.position);
        bool inRange = distanceToPlayer <= attackRange;
        
        // Debug để kiểm tra
        if (inRange)
        {
            Debug.Log($"Player trong tầm đánh! Khoảng cách: {distanceToPlayer:F2}");
        }
        return inRange;
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
        Debug.Log("RockEnemy bắt đầu tấn công!");
        DealDamage();
        audioSource?.PlayOneShot(attackSound);
        lastAttackTime = Time.time;
        isAttacking = false;
    }

    private void DealDamage()
    {
        //Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        //foreach (Collider2D hit in hits)
        //{
        //    IDamageable damageable = hit.GetComponent<IDamageable>();
        //    if (damageable != null)
        //    {
        //        damageable.TakeDamage(PhysicalDame);
        //        Debug.Log("RockEnemy gây sát thương!");
        //    }
        //}

        Debug.Log($"[RockEnemy] Đang thực hiện DealDamage(), attackRange: {attackRange}, playerLayer: {playerLayer.value}");

        // Debug vẽ sphere trong scene view
        Debug.DrawLine(attackPoint.position, attackPoint.position + Vector3.right * attackRange, Color.red, 1f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        Debug.Log($"[RockEnemy] Tìm thấy {hits.Length} colliders trong tầm đánh");

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"[RockEnemy] Collider hit: {hit.gameObject.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");

            var player = hit.GetComponent<PlayerKnight>();
            if (player != null)
            {
                Debug.Log($"[RockEnemy] Tìm thấy PlayerKnight component, sát thương gây ra: {PhysicalDame}");
                Debug.Log($"[RockEnemy] Máu player trước khi đánh: {player.GetCurrentHealth()}");

                player.TakePhysicalDamage((int)PhysicalDame);

                Debug.Log($"[RockEnemy] Máu player sau khi đánh: {player.GetCurrentHealth()}");
                return;
            }
            else
            {
                Debug.Log("[RockEnemy] Không tìm thấy PlayerKnight component trên object bị hit");
            }

            // Backup check với IDamageable
            //IDamageable damageable = hit.GetComponent<IDamageable>();
            //if (damageable != null)
            //{
            //    Debug.Log("[RockEnemy] Tìm thấy IDamageable, gây sát thương");
            //    damageable.TakeDamage(PhysicalDame);
            //}
        }
    }

    //public void TakeDamage(float damage)
    //{
    //    currentHealth -= damage;
    //    healthBar.fillAmount = currentHealth / Hp;

    //    if (currentHealth <= 0)
    //    {
    //        Die();
    //    }

    //    animator.SetTrigger("Hurt");
    //    Invoke(nameof(ResetHurt), 0.3f);
    //}
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, Hp);

        // Gọi hiệu ứng mượt nếu có thanh máu
        if (healthBar != null)
        {
            if (smoothCoroutine != null)
                StopCoroutine(smoothCoroutine);

            float target = currentHealth / Hp;
            smoothCoroutine = StartCoroutine(SmoothHealthBar(target));
        }
        else
        {
            Debug.LogWarning("[RockEnemy] healthBar chưa được gán trong Inspector!");
        }

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

    //private IEnumerator ReturnToPoolAfterDelay()
    //{
    //    Enemy_Pool pool = Object.FindFirstObjectByType<Enemy_Pool>();
    //    yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    //    ResetState();
    //    pool.ReturnToPool(gameObject);
    //}

    private IEnumerator ReturnToPoolAfterDelay()
    {
        Enemy_Pool pool = Object.FindFirstObjectByType<Enemy_Pool>();

        float delay = 1.5f; // Giá trị mặc định nếu animator null
        if (animator != null)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.length > 0)
                delay = stateInfo.length;
        }
        else
        {
            Debug.LogWarning("[RockEnemy] Animator chưa được gán!");
        }

        yield return new WaitForSeconds(delay);
        ResetState();
        if (pool != null)
            pool.ReturnToPool(gameObject);
        else
            Debug.LogWarning("[RockEnemy] Enemy_Pool chưa tồn tại trong scene!");
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
    private IEnumerator SmoothHealthBar(float target)
    {
        float currentFill = healthBar.fillAmount;
        float elapsedTime = 0f;
        float duration = 0.5f; // Duration for the smooth transition

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            healthBar.fillAmount = Mathf.Lerp(currentFill, target, elapsedTime / duration);
            yield return null;
        }

        healthBar.fillAmount = target;
    }
}
