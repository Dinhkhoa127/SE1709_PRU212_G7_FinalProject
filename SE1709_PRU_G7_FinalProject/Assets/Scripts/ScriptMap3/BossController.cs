using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour, IDamageable
{
    [SerializeField] private float circleFireInterval = 4f;  // Thời gian giữa các lần bắn đạn vòng tròn
[SerializeField] private GameObject bulletPrefab1;       // Prefab của đạn
[SerializeField] private Transform firePoint1;           // Vị trí bắn đạn
[SerializeField] private float vongTron = 20f;          // Tốc độ đạn vòng tròn
private float circleFireTimer = 0f;                     // Đếm thời gian giữa các lần bắn
    [SerializeField] private float Hp = 1000f;   // Máu tối đa của boss
    [SerializeField] private GameObject miniEnemy;
    private float currentHealth; // Máu hiện tại
    private float maxHp;// Lưu trữ máu tối đa

    public static bool IsBossDefeated { get; private set; }  // Trạng thái boss đã bị đánh bai

    [Header("Di chuyển")]
    public float minPatrolDistance = 0.5f;// Khoảng cách tuần tra tối thiểu
    public float maxPatrolDistance = 6f;// Khoảng cách tuần tra tối đa
    public float patrolHeightVariation = 3f; // Độ cao thay đổi khi tuần tra
    public float moveSpeed = 2f;
    private Vector2 patrolTarget;
    private bool isMovingToTarget = false;
    private bool isFacingRight = true;
    private Vector2 startPosition;
    private Rigidbody2D rb;


    [Header("Phạm vi & Phát hiện")]
    public LayerMask groundLayer;// Layer mặt đất để check va chạm
    public Transform groundCheck;// Điểm check mặt đất
    public float groundCheckDistance = 0.5f;// Khoảng cách check mặt đất
    public GameObject hpUI;
    public float checkPlayerDistance = 20f;        // Khoảng cách hiện thanh máu
    public float checkPlayerDistanceSound = 25f;// Khoảng cách phát âm thanh

    [Header("Tấn công")]
    public Transform player;
    public float attackRange = 3f;
    public float diveAttackRange = 5f;
    public float diveSpeed = 10f;
    private bool isDiving = false;
    private bool isAttacking = false;
    public float damge = 15f;
    public float damgeFire = 30f;

    [Header("Tăng cường khi mất máu")]
    public float enragedThreshold = 0.3f;
    public float enragedSpeedMultiplier = 1.5f;
    private bool isEnraged = false;

    
    [SerializeField] private Image healthBar;

    [Header("Điểm tấn công")]
    public Transform attackPoint;
    public float PainAttack = 1f;
    public int attackDamage = 20;
    public LayerMask playerLayer;

    private Animator animator;
    private int attackCount = 0;
    public GameObject effectFire;
    public Transform attackPoint2;
    public Transform attackPoint3;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip checkPlayerSound;
    public AudioClip attackSound;

    private Coroutine smoothCoroutine;
    //private void Start()
    //{
    //    rb = GetComponent<Rigidbody2D>();
    //    startPosition = transform.position;
    //    player = GameObject.FindGameObjectWithTag("Player")?.transform;
    //    animator = GetComponent<Animator>();
    //    SetNextPatrolTarget();
    //    effectFire.SetActive(false);
    //    currentHealth = Hp;
    //    maxHp = Hp; // Gán giá trị tối đa
    //    UpdateHp();
    //    hpUI.SetActive(false);
    //    audioSource = GetComponent<AudioSource>();
    //    IsBossDefeated = false;
    //}
    // Cập nhật Start() để khởi tạo health đúng cách
private void Start()
{
    rb = GetComponent<Rigidbody2D>();
    startPosition = transform.position;
    player = GameObject.FindGameObjectWithTag("Player")?.transform;
    animator = GetComponent<Animator>();
    SetNextPatrolTarget();
    effectFire.SetActive(false);
    
    // Khởi tạo health
    currentHealth = Hp;
    maxHp = Hp;
    
    // Setup health bar
    if (healthBar != null)
    {
        healthBar.fillAmount = 1f;
        Debug.Log($"[BossController] Initialized - Max HP: {Hp}, Current: {currentHealth}");
    }
    else
    {
        Debug.LogError("[BossController] Health bar reference is missing!");
    }
    
    hpUI.SetActive(false);
    audioSource = GetComponent<AudioSource>();
    IsBossDefeated = false;
}
    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        circleFireTimer += Time.deltaTime;

        // ✔️ Boss phát hiện Player để hiện HP UI
        if (distanceToPlayer <= checkPlayerDistance)
        {
            hpUI.SetActive(true);
        }
        else
        {
            hpUI.SetActive(false);
        }

        if (IsBossDefeated)
        {
            hpUI.SetActive(false);
            return;
        }

        // ✔️ Boss chọn hành vi: tấn công gần, lao tới, hoặc đi tuần
        if (!isDiving && !isAttacking)
        {
            if (distanceToPlayer <= attackRange)
            {
                StartCoroutine(AttackPlayer());
            }
            else if (distanceToPlayer <= diveAttackRange)
            {
                StartCoroutine(DiveAttack());
            }
            else
            {
                RandomPatrol();
            }
        }

        // ✔️ Bắn đạn vòng tròn theo thời gian
        //if (circleFireTimer >= circleFireInterval && Vector2.Distance(transform.position, player.position) <= checkPlayerDistance)
        //{
        //    int skill = Random.Range(0, 3); // 0 = FireCircle, 1 = SinhEnemy, 2 = HoiMau

        //    if (skill == 0)
        //    {
        //        FireCircle();
        //    }
        //    else if (skill == 1)
        //    {
        //        SinhEnemy();
        //    }
        //    else
        //    {
        //        HoiMau(15f);
        //    }

        //    circleFireTimer = 0f;
        //}
        // Trong phương thức Update(), sửa đoạn chọn skill ngẫu nhiên:
        if (circleFireTimer >= circleFireInterval && Vector2.Distance(transform.position, player.position) <= checkPlayerDistance)
        {
            int skill = Random.Range(0, 2); // Đổi từ (0, 3) thành (0, 2)

            if (skill == 0)
            {
                FireCircle(); // Skill bắn đạn vòng tròn
            }
            else
            {
                HoiMau(15f); // Skill hồi máu
            }

            circleFireTimer = 0f;
        }




        // ✔️ Kích hoạt chế độ Enrage khi máu thấp
        if (!isEnraged && currentHealth / Hp <= enragedThreshold)
        {
            EnrageMode();
        }

        // ✔️ Phát âm thanh nếu Player trong phạm vi âm thanh
        if (distanceToPlayer <= checkPlayerDistanceSound)
        {
            if (audioSource != null && checkPlayerSound != null)
            {
                if (!audioSource.isPlaying) // tránh lặp liên tục
                {
                    audioSource.volume = 0.1f;
                    audioSource.PlayOneShot(checkPlayerSound);
                }
            }
        }
    }
    private void HoiMau(float hpAmount)
    {
        if (IsBossDefeated) return;

        currentHealth = Mathf.Min(currentHealth + hpAmount, maxHp);
        animator.SetTrigger("skill_1"); // hoặc animation hồi máu nếu có
        UpdateHp();
        Debug.Log("Boss hồi máu: +" + hpAmount);
    }
    private void SinhEnemy()
    {
        Instantiate(miniEnemy, transform.position, Quaternion.identity);
    }
    //public void FireCircle()
    //{
    //    const int bulletCount = 24;
    //    float angleStep = 360f / bulletCount;

    //    for (int i = 0; i < bulletCount; i++)
    //    {
    //        float angle = i * angleStep;
    //        Vector3 bulletDirection = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0);

    //        GameObject bullet = Instantiate(bulletPrefab1, firePoint1.position, Quaternion.identity);
    //        EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
    //        enemyBullet.SetMovementDirection(bulletDirection.normalized * vongTron);
    //    }
    //}
    public void FireCircle()
    {
        const int bulletCount = 24;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Vector3 bulletDirection = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0);

            GameObject bullet = Instantiate(bulletPrefab1, firePoint1.position, Quaternion.identity);
            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();

            // Thiết lập damage cho đạn
            enemyBullet.SetDamage(damgeFire); // Thêm dòng này
            enemyBullet.SetMovementDirection(bulletDirection.normalized * vongTron);
        }
    }
    //Di chuyển
    private void RandomPatrol()
    {

        if (animator != null)
        {
            animator.SetTrigger("walk");
        }
        if (!isMovingToTarget || Vector2.Distance(transform.position, patrolTarget) < 0.5f || IsObstacleAhead())
        {
            SetNextPatrolTarget();
        }

        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
        FlipBoss(direction.x);
    }

    // Xác định điểm đến ngẫu nhiên**
    private void SetNextPatrolTarget()
    {
        float patrolDistance = Random.Range(minPatrolDistance, maxPatrolDistance);
        float heightOffset = Random.Range(-patrolHeightVariation, patrolHeightVariation);

        isFacingRight = !isFacingRight;

        patrolTarget = startPosition + new Vector2(isFacingRight ? patrolDistance : -patrolDistance, heightOffset);
        isMovingToTarget = true;
    }

    //Phát hiện va chạm với Ground để quay đầu
    private bool IsObstacleAhead()
    {
        return Physics2D.Raycast(groundCheck.position, Vector2.right, groundCheckDistance, groundLayer);
    }



    private IEnumerator AttackPlayer()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            if (attackCount < 5)
            {
                animator.SetTrigger("skill_1");
                attackCount++;
            }
            else
            {
                animator.SetTrigger("skill_2");
                effectFire.transform.position = attackPoint3.position;
                Vector2 directionToPlayer = (player.position - effectFire.transform.position).normalized;

                // Xoay effectFire để hướng về phía người chơi
                float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
                effectFire.transform.rotation = Quaternion.Euler(0, 0, angle);
                effectFire.SetActive(true);
                attackCount = 0;

                yield return new WaitForSeconds(1f);
                effectFire.SetActive(false);
            }
        }

        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    public void AudioAttack()
    {

        audioSource.volume = 0.5f; // Giảm âm lượng
        audioSource.PlayOneShot(attackSound);

    }



    public void AttackPlayer2()
    {
        Collider2D playerHit = Physics2D.OverlapCircle(attackPoint.position, PainAttack, playerLayer);
        Collider2D playerHit2 = Physics2D.OverlapCircle(attackPoint2.position, PainAttack, playerLayer);

        if (playerHit != null || playerHit2 != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, PainAttack, playerLayer);
            DealDamage(damge, hits);
            Debug.Log("Player bị trúng đòn!");
        }
    }




    // Lao tới tấn công**
    private IEnumerator DiveAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("run");
        }
        isDiving = true;

        Vector2 direction = new Vector2(player.position.x, player.position.y) - (Vector2)transform.position;
        direction.Normalize();
        FlipBoss(direction.x);
        rb.linearVelocity = direction * diveSpeed;

        yield return new WaitForSeconds(0.2f);
        rb.linearVelocity = Vector2.zero;


        isDiving = false;
    }

    private void EnrageMode()
    {
        isEnraged = true;
        moveSpeed *= enragedSpeedMultiplier;
        diveSpeed *= enragedSpeedMultiplier;
    }

    private void FlipBoss(float directionX)
    {
        if ((directionX > 0 && transform.localScale.x < 0) || (directionX < 0 && transform.localScale.x > 0))
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnDrawGizmos()
    {
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(rb.position, attackRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rb.position, diveAttackRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, PainAttack);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint2.position, PainAttack);

            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(attackPoint3.position, PainAttack);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(rb.position, checkPlayerDistanceSound);
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(attackPoint3.position, checkPlayerDistance);
        }
    }

    //public void TakeDamage(float damage)
    //{
    //    currentHealth -= damage;
    //    animator.SetTrigger("hit_2");
    //    UpdateHp();
    //    if (currentHealth <= 0)
    //    {
    //        Die();
    //    }
    //}
    public void TakeDamage(float damage)
    {
        if (IsBossDefeated) return;

        // Debug trước khi nhận damage
        Debug.Log($"[BossController Health] Before Damage - Current: {currentHealth}, Max: {Hp}, HealthBar: {healthBar?.fillAmount}");

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, Hp);

        // Áp dụng smooth health bar
        if (healthBar != null)
        {
            if (smoothCoroutine != null)
                StopCoroutine(smoothCoroutine);

            float targetFill = currentHealth / Hp;
            smoothCoroutine = StartCoroutine(SmoothHealthBar(targetFill));
        }

        // Debug sau khi nhận damage
        Debug.Log($"[BossController Health] After Damage - Current: {currentHealth}, Max: {Hp}, Target Fill: {currentHealth / Hp}");

        animator.SetTrigger("hit_2");

        if (currentHealth <= 0)
        {
            Debug.Log("[BossController] Boss is dying!");
            Die();
        }
    }

    //private void DealDamage(float dame, Collider2D[] hits)
    //{
    //    //Collider2D[] hits = Physics2D.OverlapCircleAll(attack_Point.position, attackRadius, playerLayer);
    //    foreach (Collider2D hit in hits)
    //    {
    //        IDamageable damageable = hit.GetComponent<IDamageable>();
    //        if (damageable != null)
    //        {
    //            damageable.TakeDamage(dame);
    //            Debug.Log("Take Dame");
    //            return;
    //        }
    //    }

    //}
    private void DealDamage(float damage, Collider2D[] hits)
    {
        Debug.Log($"[BossController] Đang thực hiện DealDamage(), Damage: {damage}, Hits: {hits.Length}");

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"[BossController] Collider hit: {hit.gameObject.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");

            // Check PlayerKnight trước
            var player = hit.GetComponent<PlayerKnight>();
            if (player != null)
            {
                Debug.Log($"[BossController] Tìm thấy PlayerKnight, máu trước khi đánh: {player.GetCurrentHealth()}");

                if (damage == damge)
                {
                    player.TakePhysicalDamage((int)damage);
                    Debug.Log($"[BossController] Gây {damage} sát thương vật lý!");
                }
                else if (damage == damgeFire)
                {
                    player.TakeMagicDamage((int)damage);
                    Debug.Log($"[BossController] Gây {damage} sát thương phép!");
                }

                Debug.Log($"[BossController] Máu player sau khi đánh: {player.GetCurrentHealth()}");
                return;
            }

            // Fallback sang IDamageable
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Debug.Log($"[BossController] Gây {damage} sát thương qua IDamageable!");
                damageable.TakeDamage(damage);
            }
        }
    }

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


    public void UpdateHp()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / Hp;
        }
    }


    public void Die()
    {
        IsBossDefeated = true;
        animator.SetTrigger("death");

        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        StopAllCoroutines();
        effectFire.SetActive(false);
        Destroy(gameObject, 2f);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            IDamageable player = collision.gameObject.GetComponent<IDamageable>();
            if (player != null)
            {
                player.TakeDamage(10);
            }


            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                playerRb.AddForce(knockbackDirection * 500f); // Điều chỉnh lực hất ra theo ý muốn
            }
        }
    }
}
