using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyOfDeath : Enemy1, IDamageable
{
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;

    private Animator animator;
    private int direction = 1;
    //private bool isChasing = false;
    private Coroutine smoothCoroutine;
    //protected override void Start()
    //{
    //    base.Start();
    //    animator = GetComponent<Animator>();
    //    player = GameObject.FindGameObjectWithTag("Player")?.transform;
    //}
    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Khởi tạo health
        currentHealth = Hp;

        // Kiểm tra và setup health bar
        if (healthBar != null)
        {
            healthBar.fillAmount = 1f;
            Debug.Log($"[EnemyOfDeath] Initialized - Max HP: {Hp}, Current: {currentHealth}");
        }
        else
        {
            Debug.LogError("[EnemyOfDeath] Health bar reference is missing!");
        }
    }
    void Update()
    {
        if (player == null) return;

        if (PlayerInAttackRange())
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                animator.SetTrigger("isAttack");
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
        isChasing = true;
        isAttacking = false;
        animator.SetBool("isRun", true);

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if ((directionToPlayer.x > 0 && direction < 0) || (directionToPlayer.x < 0 && direction > 0))
        {
            Flip();
        }

        transform.position += new Vector3(Mathf.Sign(directionToPlayer.x) * RunSpeed * Time.deltaTime, 0, 0);
    }

    //protected override void Attack()
    //{
    //    DealDamage();
    //    audioSource?.PlayOneShot(attackSound);
    //    lastAttackTime = Time.time;
    //}
    protected override void Attack()
    {
        Debug.Log("[EnemyOfDeath] Bắt đầu tấn công!");
        DealDamage();
        audioSource?.PlayOneShot(attackSound);
        lastAttackTime = Time.time;
        isAttacking = false; // Reset trạng thái tấn công
    }

    //private void DealDamage()
    //{
    //    Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
    //    foreach (Collider2D hit in hits)
    //    {
    //        IDamageable damageable = hit.GetComponent<IDamageable>();
    //        if (damageable != null)
    //        {
    //            damageable.TakeDamage(PhysicalDame);
    //            Debug.Log("EnemyOfDeath gây sát thương!");
    //        }
    //    }
    //}
    private void DealDamage()
    {
        Debug.Log($"[EnemyOfDeath] Đang thực hiện DealDamage(), attackRange: {attackRange}, playerLayer: {playerLayer.value}");

        // Debug vẽ sphere trong scene view
        Debug.DrawLine(attackPoint.position, attackPoint.position + Vector3.right * attackRange, Color.red, 1f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);
        Debug.Log($"[EnemyOfDeath] Tìm thấy {hits.Length} colliders trong tầm đánh");

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"[EnemyOfDeath] Collider hit: {hit.gameObject.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");

            // Check PlayerKnight trước
            var player = hit.GetComponent<PlayerKnight>();
            if (player != null)
            {
                Debug.Log($"[EnemyOfDeath] Tìm thấy PlayerKnight component, sát thương gây ra: {PhysicalDame}");
                Debug.Log($"[EnemyOfDeath] Máu player trước khi đánh: {player.GetCurrentHealth()}");

                player.TakePhysicalDamage((int)PhysicalDame);

                Debug.Log($"[EnemyOfDeath] Máu player sau khi đánh: {player.GetCurrentHealth()}");
                return;
            }
            else
            {
                Debug.Log("[EnemyOfDeath] Không tìm thấy PlayerKnight component trên object bị hit");
            }
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
    //    Invoke(nameof(ResetHurtState), 0.3f);
    //}
    public void TakeDamage(float damage)
    {
        // Debug trước khi nhận damage
        Debug.Log($"[EnemyOfDeath Health] Before Damage - Current: {currentHealth}, Max: {Hp}, HealthBar: {healthBar?.fillAmount}");

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, Hp); // Đảm bảo health không âm và không vượt quá max

        // Áp dụng smooth health bar
        if (healthBar != null)
        {
            if (smoothCoroutine != null)
                StopCoroutine(smoothCoroutine);

            float targetFill = currentHealth / Hp;
            smoothCoroutine = StartCoroutine(SmoothHealthBar(targetFill));
        }
        else
        {
            Debug.LogWarning("[EnemyOfDeath] healthBar chưa được gán trong Inspector!");
        }

        // Debug sau khi nhận damage
        Debug.Log($"[EnemyOfDeath Health] After Damage - Current: {currentHealth}, Max: {Hp}, Target Fill: {currentHealth / Hp}");

        if (currentHealth <= 0)
        {
            Debug.Log("[EnemyOfDeath] Enemy is dying!");
            Die();
        }
        else
        {
            animator.SetTrigger("Hurt");
            Invoke(nameof(ResetHurtState), 0.3f);
        }
    }

    // Thêm coroutine để smooth health bar
    private IEnumerator SmoothHealthBar(float target)
    {
        float currentFill = healthBar.fillAmount;
        float elapsedTime = 0f;
        float duration = 0.5f; // Thời gian transition

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            healthBar.fillAmount = Mathf.Lerp(currentFill, target, elapsedTime / duration);
            yield return null;
        }

        healthBar.fillAmount = target;
    }

    private void ResetHurtState()
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
            Debug.LogWarning("[EnemyOfDeath] Animator chưa được gán!");
        }

        yield return new WaitForSeconds(delay);
        ResetState();
        if (pool != null)
            pool.ReturnToPool(gameObject);
        else
            Debug.LogWarning("[EnemyOfDeath] Enemy_Pool chưa tồn tại trong scene!");
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

    // Bỏ hoàn toàn hành vi tuần tra
    protected override void Patrol() { }
}
