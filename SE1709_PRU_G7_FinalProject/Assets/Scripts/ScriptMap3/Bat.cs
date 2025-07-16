//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;

//public class Bat : Enemy1, IDamageable
//{
//    [SerializeField] private float detectionRange = 5f;
//    [SerializeField] private float attackRange = 3f;
//    [SerializeField] private float minPatrolDistance = 5f;
//    [SerializeField] private float maxPatrolDistance = 10f;
//    [SerializeField] private float patrolHeightVariation = 2f;
//    [SerializeField] private LayerMask obstacleLayer;
//    [SerializeField] private LayerMask playerLayer;
//    [SerializeField] private Transform attackPoint;
//    [SerializeField] private float attackRadius = 1f;
//    [SerializeField] private Image healthBar;

//    private Animator animator;
//    private Vector2 patrolTarget;
//    private bool movingRight = true;
//    private Vector2 startPos;

//    private Coroutine smoothCoroutine;
//    //protected override void Start()
//    //{
//    //    base.Start();
//    //    animator = GetComponent<Animator>();
//    //    startPos = transform.position;
//    //    player = GameObject.FindGameObjectWithTag("Player")?.transform;
//    //    currentHealth = Hp;

//    //    // Gán thanh máu (nếu chưa gán qua Inspector)
//    //    if (healthBar == null)
//    //    {
//    //        healthBar = transform.Find("Hp")?.GetComponent<Image>();
//    //    }

//    //    UpdateHealthBar();
//    //    SetNextPatrolTarget();
//    //}
//    protected override void Start()
//    {
//        base.Start();
//        animator = GetComponent<Animator>();
//        startPos = transform.position;
//        player = GameObject.FindGameObjectWithTag("Player")?.transform;
//        currentHealth = Hp; // Set full health khi spawn

//        // Kiểm tra và setup health bar
//        if (healthBar == null)
//        {
//            healthBar = transform.Find("Hp")?.GetComponent<Image>();
//            if (healthBar == null)
//            {
//                Debug.LogError("[Bat] Không tìm thấy health bar!");
//            }
//        }

//        // Set thanh máu đầy khi spawn
//        if (healthBar != null)
//        {
//            healthBar.fillAmount = 1f;
//        }

//        SetNextPatrolTarget();
//    }
//    void Update()
//    {
//        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

//        if (isAttacking && distanceToPlayer > attackRange)
//        {
//            isAttacking = false;
//            animator.ResetTrigger("Attack");
//        }

//        if (distanceToPlayer < detectionRange)
//        {
//            if (distanceToPlayer < attackRange && Time.time - lastAttackTime > attackCooldown)
//            {
//                Attack();
//            }
//            else
//            {
//                ChasePlayer();
//            }
//        }
//        else
//        {
//            Patrol();
//        }
//    }

//    //protected override void Attack()
//    //{
//    //    isAttacking = true;
//    //    animator.SetTrigger("Attack");
//    //    Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

//    //    foreach (Collider2D hit in hits)
//    //    {
//    //        IDamageable damageable = hit.GetComponent<IDamageable>();
//    //        if (damageable != null)
//    //        {
//    //            damageable.TakeDamage(PhysicalDame);
//    //        }
//    //    }
//    //    lastAttackTime = Time.time;
//    //}
//    protected override void Attack()
//    {
//        Debug.Log("[Bat] Bắt đầu tấn công!");
//        isAttacking = true;
//        animator.SetTrigger("Attack");

//        // Debug thông tin tấn công
//        Debug.Log($"[Bat] Physical Damage: {PhysicalDame}, Attack Range: {attackRadius}");

//        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
//        Debug.Log($"[Bat] Tìm thấy {hits.Length} colliders trong tầm đánh");

//        foreach (Collider2D hit in hits)
//        {
//            Debug.Log($"[Bat] Collider hit: {hit.gameObject.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");

//            // Check PlayerKnight trước
//            var player = hit.GetComponent<PlayerKnight>();
//            if (player != null)
//            {
//                Debug.Log($"[Bat] Tìm thấy PlayerKnight, máu trước khi đánh: {player.GetCurrentHealth()}");
//                player.TakePhysicalDamage((int)PhysicalDame);
//                Debug.Log($"[Bat] Máu player sau khi đánh: {player.GetCurrentHealth()}");
//                break;
//            }

//            //// Fallback sang IDamageable
//            //IDamageable damageable = hit.GetComponent<IDamageable>();
//            //if (damageable != null)
//            //{
//            //    Debug.Log("[Bat] Gây sát thương qua IDamageable interface");
//            //    damageable.TakeDamage(PhysicalDame);
//            //}
//        }

//        lastAttackTime = Time.time;
//    }

//    void ChasePlayer()
//    {
//        isAttacking = false;
//        animator.SetBool("isRuning", true);

//        Vector2 direction = (player.position - transform.position).normalized;
//        Flip(direction.x);
//        transform.position += (Vector3)direction * FlySpeed * Time.deltaTime;
//    }

//    protected override void Patrol()
//    {
//        if (isAttacking) return;

//        animator.SetBool("isRuning", true);

//        if (Vector2.Distance(transform.position, patrolTarget) < 0.5f || IsObstacleAhead())
//        {
//            SetNextPatrolTarget();
//        }

//        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
//        Flip(direction.x);
//        transform.position += (Vector3)direction * FlySpeed * Time.deltaTime;
//    }

//    void SetNextPatrolTarget()
//    {
//        float patrolDistance = Random.Range(minPatrolDistance, maxPatrolDistance);
//        float heightOffset = Random.Range(-patrolHeightVariation, patrolHeightVariation);

//        movingRight = !movingRight;

//        patrolTarget = startPos + new Vector2(movingRight ? patrolDistance : -patrolDistance, heightOffset);
//    }

//    bool IsObstacleAhead()
//    {
//        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
//        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1f, obstacleLayer);
//        return hit.collider != null;
//    }

//    //void Flip(float directionX)
//    //{
//    //    if ((directionX > 0 && transform.localScale.x < 0) || (directionX < 0 && transform.localScale.x > 0))
//    //    {
//    //        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
//    //        if (healthBar != null)
//    //        {
//    //            healthBar.transform.localScale = new Vector3(-healthBar.transform.localScale.x, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
//    //        }
//    //    }
//    //}
//    // Sửa lại hàm Flip để xử lý health bar
//    void Flip(float directionX)
//    {
//        if ((directionX > 0 && transform.localScale.x < 0) ||
//            (directionX < 0 && transform.localScale.x > 0))
//        {
//            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

//            // Lật thanh máu theo hướng nhân vật
//            if (healthBar != null)
//            {
//                healthBar.transform.localScale = new Vector3(
//                    -healthBar.transform.localScale.x,
//                    healthBar.transform.localScale.y,
//                    healthBar.transform.localScale.z
//                );
//            }
//        }
//    }

//    protected override void Die()
//    {
//        StartCoroutine(ReturnToPoolAfterDelay());
//    }

//    private IEnumerator ReturnToPoolAfterDelay()
//    {
//        Enemy_Pool enemy_Pool = Object.FindFirstObjectByType<Enemy_Pool>();
//        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
//        ResetState();
//        enemy_Pool.ReturnToPool(gameObject);
//    }

//    //public void TakeDamage(float damage)
//    //{
//    //    currentHealth -= damage;
//    //    UpdateHealthBar();
//    //    if (currentHealth <= 0)
//    //    {
//    //        Die();
//    //    }
//    //}
//    public void TakeDamage(float damage)
//    {
//        currentHealth -= damage;
//        currentHealth = Mathf.Clamp(currentHealth, 0, Hp); // Giới hạn health hợp lệ

//        // Áp dụng smooth health bar
//        if (healthBar != null)
//        {
//            if (smoothCoroutine != null)
//                StopCoroutine(smoothCoroutine);

//            float targetFill = currentHealth / Hp;
//            smoothCoroutine = StartCoroutine(SmoothHealthBar(targetFill));
//        }
//        else
//        {
//            Debug.LogWarning("[Bat] healthBar chưa được gán trong Inspector!");
//        }

//        if (currentHealth <= 0)
//        {
//            Die();
//        }
//        else
//        {
//            animator.SetTrigger("Hurt");
//        }
//    }
//    void UpdateHealthBar()
//    {
//        if (healthBar != null)
//        {
//            healthBar.fillAmount = currentHealth / Hp;
//        }
//    }

//    private void OnDrawGizmos()
//    {
//        if (attackPoint != null)
//        {
//            Gizmos.color = Color.red;
//            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
//        }
//    }
//    // Thêm coroutine để smooth health bar
//    private IEnumerator SmoothHealthBar(float target)
//    {
//        float currentFill = healthBar.fillAmount;
//        float elapsedTime = 0f;
//        float duration = 0.5f; // Thời gian transition

//        while (elapsedTime < duration)
//        {
//            elapsedTime += Time.deltaTime;
//            healthBar.fillAmount = Mathf.Lerp(currentFill, target, elapsedTime / duration);
//            yield return null;
//        }

//        healthBar.fillAmount = target; // Đảm bảo đạt đúng giá trị cuối
//    }

//}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Bat : Enemy1, IDamageable
{
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float minPatrolDistance = 5f;
    [SerializeField] private float maxPatrolDistance = 10f;
    [SerializeField] private float patrolHeightVariation = 2f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 1f;
    // [SerializeField] private new Image healthBar; // Sử dụng 'new' để ẩn healthBar của Enemy1

    private Animator animator;
    private Vector2 patrolTarget;
    private bool movingRight = true;
    private Vector2 startPos;

    private Coroutine smoothCoroutine;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        startPos = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentHealth = Hp; // Set full health khi spawn

        // Kiểm tra và setup health bar
        if (healthBar == null)
        {
            healthBar = transform.Find("Hp")?.GetComponent<Image>();
            if (healthBar == null)
            {
                Debug.LogError("[Bat] Không tìm thấy health bar!");
            }
        }

        // Set thanh máu đầy khi spawn
        if (healthBar != null)
        {
            healthBar.fillAmount = 1f;
        }

        SetNextPatrolTarget();
    }
    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (isAttacking && distanceToPlayer > attackRange)
        {
            isAttacking = false;
            animator.ResetTrigger("Attack");
        }

        if (distanceToPlayer < detectionRange)
        {
            if (distanceToPlayer < attackRange && Time.time - lastAttackTime > attackCooldown)
            {
                Attack();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            Patrol();
        }
    }

    protected override void Attack()
    {
        Debug.Log("[Bat] Bắt đầu tấn công!");
        isAttacking = true;
        animator.SetTrigger("Attack");

        Debug.Log($"[Bat] Physical Damage: {PhysicalDame}, Attack Range: {attackRadius}");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        Debug.Log($"[Bat] Tìm thấy {hits.Length} colliders trong tầm đánh");

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"[Bat] Collider hit: {hit.gameObject.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");

            var player = hit.GetComponent<PlayerKnight>();
            if (player != null)
            {
                Debug.Log($"[Bat] Tìm thấy PlayerKnight, máu trước khi đánh: {player.GetCurrentHealth()}");
                player.TakePhysicalDamage((int)PhysicalDame);
                Debug.Log($"[Bat] Máu player sau khi đánh: {player.GetCurrentHealth()}");
                break;
            }
        }

        lastAttackTime = Time.time;
    }

    void ChasePlayer()
    {
        isAttacking = false;
        animator.SetBool("isRuning", true);

        Vector2 direction = (player.position - transform.position).normalized;
        Flip(direction.x);
        transform.position += (Vector3)direction * FlySpeed * Time.deltaTime;
    }

    protected override void Patrol()
    {
        if (isAttacking) return;

        animator.SetBool("isRuning", true);

        if (Vector2.Distance(transform.position, patrolTarget) < 0.5f || IsObstacleAhead())
        {
            SetNextPatrolTarget();
        }

        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        Flip(direction.x);
        transform.position += (Vector3)direction * FlySpeed * Time.deltaTime;
    }

    void SetNextPatrolTarget()
    {
        float patrolDistance = Random.Range(minPatrolDistance, maxPatrolDistance);
        float heightOffset = Random.Range(-patrolHeightVariation, patrolHeightVariation);

        movingRight = !movingRight;

        patrolTarget = startPos + new Vector2(movingRight ? patrolDistance : -patrolDistance, heightOffset);
    }

    bool IsObstacleAhead()
    {
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1f, obstacleLayer);
        return hit.collider != null;
    }

    void Flip(float directionX)
    {
        if ((directionX > 0 && transform.localScale.x < 0) ||
            (directionX < 0 && transform.localScale.x > 0))
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

            if (healthBar != null)
            {
                healthBar.transform.localScale = new Vector3(
                    -healthBar.transform.localScale.x,
                    healthBar.transform.localScale.y,
                    healthBar.transform.localScale.z
                );
            }
        }
    }

    protected override void Die()
    {
        StartCoroutine(ReturnToPoolAfterDelay());
    }

    //private IEnumerator ReturnToPoolAfterDelay()
    //{
    //    Enemy_Pool enemy_Pool = Object.FindFirstObjectByType<Enemy_Pool>();
    //    yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    //    ResetState();
    //    enemy_Pool.ReturnToPool(gameObject);
    //}
    private IEnumerator ReturnToPoolAfterDelay()
    {
        Enemy_Pool enemy_Pool = Object.FindFirstObjectByType<Enemy_Pool>();

        float delay = 1.5f; // Giá trị mặc định nếu animator null
        if (animator != null)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.length > 0)
                delay = stateInfo.length;
        }
        else
        {
            Debug.LogWarning("[Bat] Animator chưa được gán!");
        }

        yield return new WaitForSeconds(delay);
        ResetState();
        if (enemy_Pool != null)
            enemy_Pool.ReturnToPool(gameObject);
        else
            Debug.LogWarning("[Bat] Enemy_Pool chưa tồn tại trong scene!");
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, Hp);

        if (healthBar != null)
        {
            if (smoothCoroutine != null)
                StopCoroutine(smoothCoroutine);

            float targetFill = currentHealth / Hp;
            smoothCoroutine = StartCoroutine(SmoothHealthBar(targetFill));
        }
        else
        {
            Debug.LogWarning("[Bat] healthBar chưa được gán trong Inspector!");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("Hurt");
        }
    }
    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / Hp;
        }
    }

    private void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
    private IEnumerator SmoothHealthBar(float target)
    {
        float currentFill = healthBar.fillAmount;
        float elapsedTime = 0f;
        float duration = 0.5f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            healthBar.fillAmount = Mathf.Lerp(currentFill, target, elapsedTime / duration);
            yield return null;
        }

        healthBar.fillAmount = target;
    }
}