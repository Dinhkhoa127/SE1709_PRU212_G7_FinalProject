//using System.Collections;
//using System.IO;
//using UnityEngine;
//using UnityEngine.UI;

//public class Boss : Enemy1, IDamageable
//{
//    public Transform attack_Point;
//    public float attackRadius = 2.5f;
//    [SerializeField] private float detectionRange = 30f;
//    [SerializeField] private LayerMask playerLayer;
//    [SerializeField] private AudioSource audioSource;
//    [SerializeField] private AudioClip attackSound;
//    [SerializeField] private AudioClip bossSound;
//    [SerializeField] private float soundInterval = 10f;
//    [SerializeField] private GameObject bulletPrefab;
//    [SerializeField] private Transform firePoint;
//    [SerializeField] private float circleFireInterval = 4f;
//    [SerializeField] private GameObject hpUI;
//    [SerializeField] private Image hpBar;
//    [SerializeField] public float hp = 1000;
//    [SerializeField] private GameObject bulletPrefab1;
//    [SerializeField] private Transform firePoint1;
//    [SerializeField] private float speedDan = 20f;
//    [SerializeField] private float vongTron = 20f;

//    private float currentHp;
//    private float fireDamageTimer = 0f;
//    private float fireDamageInterval = 1f;
//    private float circleFireTimer = 0f;
//    private int direction = 1;
//    private Animator animator;
//    private bool is_Chasing = false;
//    private bool isDead = false;
//    private int attackCount = 0;
//    private int maxComboBeforeFire = 4;
//    private string savePath;
//    private GateController gate;
//    private bool isAttackAnimationPlaying = false;  // Thêm biến kiểm tra animation tấn công
//    private float attackDamageDelay = 0.3f;        // Thời gian delay 
//    protected override void Start()
//    {
//        base.Start();
//        animator = GetComponent<Animator>();
//        player = GameObject.FindGameObjectWithTag("Player")?.transform;
//        currentHp = hp;

//        gate = FindAnyObjectByType<GateController>();

//        string directoryPath = Path.Combine(Application.persistentDataPath, "GameData");
//        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
//        savePath = Path.Combine(directoryPath, "bossData.json");

//        hpUI.SetActive(false);
//        InvokeRepeating(nameof(PlayBossSound), soundInterval, soundInterval);
//    }

//    private void Update()
//    {
//        if (player == null || isDead) return;

//        float distanceToPlayerX = Mathf.Abs(player.position.x - transform.position.x);

//        // Hiển thị/ẩn thanh máu
//        if (Vector2.Distance(transform.position, player.position) < detectionRange)
//            hpUI.SetActive(true);
//        else
//            hpUI.SetActive(false);

//        fireDamageTimer += Time.deltaTime;
//        circleFireTimer += Time.deltaTime;

//        if (circleFireTimer >= circleFireInterval)
//        {
//            FireCircle();
//            circleFireTimer = 0f;
//        }

//        if (fireDamageTimer >= fireDamageInterval)
//        {
//            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 5, playerLayer);
//            if (hits.Length > 0)
//                DealDamage(MagicDame, hits);

//            fireDamageTimer = 0;
//        }

//        if (distanceToPlayerX < 5f)
//        {
//            isChasing = false;
//            isAttacking = false;
//            animator.SetBool("isWalking", false);
//        }
//        else
//        {
//            if (PlayerInAttackRange())
//            {
//                if (Time.time - lastAttackTime >= attackCooldown)
//                    animator.SetTrigger("Attack");
//            }
//            else if (CheckInRange())
//            {
//                ChasePlayer();
//            }
//            else
//            {
//                animator.SetBool("isWalking", false);
//            }
//        }
//    }

//    protected override bool CheckInRange()
//    {
//        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
//        return distanceToPlayer < detectionRange;
//    }

//    protected bool PlayerInAttackRange()
//    {
//        float distanceToPlayer = Vector2.Distance(attack_Point.position, player.position);
//        return distanceToPlayer < attackRadius;
//    }

//    protected void ChasePlayer()
//    {
//        isChasing = true;
//        isAttacking = false;
//        animator.SetBool("isWalking", true);

//        Vector3 directionToPlayer = (player.position - transform.position).normalized;

//        if ((directionToPlayer.x > 0 && direction < 0) || (directionToPlayer.x < 0 && direction > 0))
//        {
//            Flip();
//        }

//        transform.position += new Vector3(Mathf.Sign(directionToPlayer.x) * RunSpeed * Time.deltaTime, 0, 0);
//    }

//    //protected override void Attack()
//    //{
//    //    isAttacking = true;
//    //    isChasing = false;
//    //    audioSource.PlayOneShot(attackSound);
//    //    lastAttackTime = Time.time;
//    //    attackCount++;

//    //    Collider2D[] hits = Physics2D.OverlapCircleAll(attack_Point.position, attackRadius, playerLayer);
//    //    DealDamage(PhysicalDame, hits);

//    //    if (attackCount >= maxComboBeforeFire)
//    //    {
//    //        attackCount = 0;
//    //        Fire();
//    //    }
//    //}
//    protected override void Attack()
//    {
//        Debug.Log($"[Boss] Bắt đầu tấn công! Combo: {attackCount + 1}/{maxComboBeforeFire}");

//        isAttacking = true;
//        isChasing = false;

//        // Debug thông tin tấn công
//        Debug.Log($"[Boss] Physical Damage: {PhysicalDame}, Attack Range: {attackRadius}");

//        Collider2D[] hits = Physics2D.OverlapCircleAll(attack_Point.position, attackRadius, playerLayer);
//        Debug.Log($"[Boss] Tìm thấy {hits.Length} colliders trong tầm đánh");

//        DealDamage(PhysicalDame, hits);

//        audioSource?.PlayOneShot(attackSound);
//        lastAttackTime = Time.time;
//        attackCount++;

//        if (attackCount >= maxComboBeforeFire)
//        {
//            Debug.Log("[Boss] Hoàn thành combo! Bắt đầu bắn đạn!");
//            attackCount = 0;
//            Fire();
//        }
//    }

//    //private void DealDamage(float damage, Collider2D[] hits)
//    //{
//    //    foreach (Collider2D hit in hits)
//    //    {
//    //        IDamageable damageable = hit.GetComponent<IDamageable>();
//    //        if (damageable != null)
//    //            damageable.TakeDamage(damage);
//    //    }
//    //}
//    private void DealDamage(float damage, Collider2D[] hits)
//    {
//        Debug.Log($"[Boss] Đang thực hiện DealDamage(), Damage: {damage}, Hits: {hits.Length}");

//        foreach (Collider2D hit in hits)
//        {
//            Debug.Log($"[Boss] Collider hit: {hit.gameObject.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");

//            // Check PlayerKnight trước
//            var player = hit.GetComponent<PlayerKnight>();
//            if (player != null)
//            {
//                Debug.Log($"[Boss] Tìm thấy PlayerKnight, máu trước khi đánh: {player.GetCurrentHealth()}");

//                // Phân biệt loại damage
//                if (damage == PhysicalDame)
//                {
//                    player.TakePhysicalDamage((int)damage);
//                    Debug.Log($"[Boss] Gây {damage} sát thương vật lý!");
//                }
//                else if (damage == MagicDame)
//                {
//                    player.TakeMagicDamage((int)damage);
//                    Debug.Log($"[Boss] Gây {damage} sát thương phép!");
//                }

//                Debug.Log($"[Boss] Máu player sau khi đánh: {player.GetCurrentHealth()}");
//                return;
//            }

//            // Fallback sang IDamageable
//            IDamageable damageable = hit.GetComponent<IDamageable>();
//            if (damageable != null)
//            {
//                Debug.Log($"[Boss] Gây {damage} sát thương qua IDamageable!");
//                damageable.TakeDamage(damage);
//            }
//        }
//    }

//    protected override void Die()
//    {
//        isDead = true;
//        BossData data = new BossData { health = 0, isDead = true };
//        File.WriteAllText(savePath, JsonUtility.ToJson(data));

//        animator.SetTrigger("Die");
//        gate.OpenGate();
//        FindAnyObjectByType<Enemy_Spawner>()?.PlayerRespawned();
//        hpUI.SetActive(false);
//        StartCoroutine(ReturnToPoolAfterDelay());
//    }

//    private IEnumerator ReturnToPoolAfterDelay()
//    {
//        yield return new WaitForSeconds(1.5f);
//        gameObject.SetActive(false);
//    }

//    //public void TakeDamage(float damage)
//    //{
//    //    if (isDead) return;

//    //    currentHp -= damage;
//    //    hpBar.fillAmount = currentHp / hp;

//    //    if (currentHp <= 0)
//    //    {
//    //        Die();
//    //    }
//    //}
//    // Thêm smooth health bar transition
//    private Coroutine smoothCoroutine;

//    public void TakeDamage(float damage)
//    {
//        if (isDead) return;

//        // Debug trước khi nhận damage
//        Debug.Log($"[Boss Health] Before Damage - Current: {currentHp}, Max: {hp}, HealthBar: {hpBar?.fillAmount}");

//        currentHp -= damage;
//        currentHp = Mathf.Clamp(currentHp, 0, hp);

//        // Áp dụng smooth health bar
//        if (hpBar != null)
//        {
//            if (smoothCoroutine != null)
//                StopCoroutine(smoothCoroutine);

//            float targetFill = currentHp / hp;
//            smoothCoroutine = StartCoroutine(SmoothHealthBar(targetFill));
//        }

//        // Debug sau khi nhận damage
//        Debug.Log($"[Boss Health] After Damage - Current: {currentHp}, Max: {hp}, Target Fill: {currentHp / hp}");

//        if (currentHp <= 0)
//        {
//            Debug.Log("[Boss] Boss is dying!");
//            Die();
//        }
//    }

//    private IEnumerator SmoothHealthBar(float target)
//    {
//        float currentFill = hpBar.fillAmount;
//        float elapsedTime = 0f;
//        float duration = 0.5f; // Thời gian transition

//        while (elapsedTime < duration)
//        {
//            elapsedTime += Time.deltaTime;
//            hpBar.fillAmount = Mathf.Lerp(currentFill, target, elapsedTime / duration);
//            yield return null;
//        }

//        hpBar.fillAmount = target;
//    }

//    private void PlayBossSound()
//    {
//        if (audioSource != null && bossSound != null)
//            audioSource.PlayOneShot(bossSound);
//    }

//    public void ResetBossState()
//    {
//        currentHp = hp;
//        hpBar.fillAmount = 1;
//    }

//    private void OnDrawGizmos()
//    {
//        if (attack_Point != null)
//        {
//            Gizmos.color = Color.red;
//            Gizmos.DrawWireSphere(attack_Point.position, attackRadius);
//        }
//    }

//    protected override void Flip()
//    {
//        direction *= -1;
//        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
//        hpUI.transform.localScale = new Vector3(-hpUI.transform.localScale.x, hpUI.transform.localScale.y, hpUI.transform.localScale.z);
//    }

//    public void Fire()
//    {
//        if (player != null)
//        {
//            Vector3 directionToPlayer = player.position - firePoint.position;
//            directionToPlayer.Normalize();

//            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
//            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
//            enemyBullet.SetMovementDirection(directionToPlayer * speedDan);
//        }
//    }

//    public void FireCircle()
//    {
//        const int bulletCount = 12;
//        float angleStep = 360f / bulletCount;

//        for (int i = 0; i < bulletCount; i++)
//        {
//            float angle = i * angleStep;
//            Vector3 bulletDirection = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0);

//            GameObject bullet = Instantiate(bulletPrefab1, firePoint1.position, Quaternion.identity);
//            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
//            enemyBullet.SetMovementDirection(bulletDirection.normalized * vongTron);
//        }
//    }

//    protected override void Patrol() { } // Không tuần tra
//}
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Boss : Enemy1, IDamageable
{
    public Transform attack_Point;
    public float attackRadius = 2.5f;
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip bossSound;
    [SerializeField] private float soundInterval = 10f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float circleFireInterval = 4f;
    [SerializeField] private GameObject hpUI;
    [SerializeField] private Image bossHpBar;
    [SerializeField] private GameObject bulletPrefab1;
    [SerializeField] private Transform firePoint1;
    [SerializeField] private float speedDan = 20f;
    [SerializeField] private float vongTron = 20f;

    private float currentHp;
    private float fireDamageTimer = 0f;
    private float fireDamageInterval = 1f;
    private float circleFireTimer = 0f;
    private int direction = 1;
    private Animator animator;
    private bool isDead = false;
    private int attackCount = 0;
    private int maxComboBeforeFire = 4;
    private string savePath;
    private GateController gate;
    private float attackDamageDelay = 0.3f;
    private Coroutine smoothCoroutine;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentHp = Hp;

        gate = FindAnyObjectByType<GateController>();

        string directoryPath = Path.Combine(Application.persistentDataPath, "GameData");
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
        savePath = Path.Combine(directoryPath, "bossData.json");

        hpUI.SetActive(false);
        InvokeRepeating(nameof(PlayBossSound), soundInterval, soundInterval);
    }

    private void Update()
    {
        if (player == null || isDead) return;

        float distanceToPlayerX = Mathf.Abs(player.position.x - transform.position.x);

        if (Vector2.Distance(transform.position, player.position) < detectionRange)
            hpUI.SetActive(true);
        else
            hpUI.SetActive(false);

        fireDamageTimer += Time.deltaTime;
        circleFireTimer += Time.deltaTime;

        if (circleFireTimer >= circleFireInterval)
        {
            FireCircle();
            circleFireTimer = 0f;
        }

        if (fireDamageTimer >= fireDamageInterval)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 5, playerLayer);
            if (hits.Length > 0)
                DealDamage(MagicDame, hits);

            fireDamageTimer = 0;
        }

        if (distanceToPlayerX < 5f)
        {
            isChasing = false;
            isAttacking = false;
            animator.SetBool("isWalking", false);
        }
        else
        {
            if (PlayerInAttackRange())
            {
                if (Time.time - lastAttackTime >= attackCooldown)
                    animator.SetTrigger("Attack");
            }
            else if (CheckInRange())
            {
                ChasePlayer();
            }
            else
            {
                animator.SetBool("isWalking", false);
            }
        }
    }

    protected override bool CheckInRange()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer < detectionRange;
    }

    protected bool PlayerInAttackRange()
    {
        float distanceToPlayer = Vector2.Distance(attack_Point.position, player.position);
        return distanceToPlayer < attackRadius;
    }

    protected void ChasePlayer()
    {
        isChasing = true;
        isAttacking = false;
        animator.SetBool("isWalking", true);

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        if ((directionToPlayer.x > 0 && direction < 0) || (directionToPlayer.x < 0 && direction > 0))
        {
            Flip();
        }

        transform.position += new Vector3(Mathf.Sign(directionToPlayer.x) * RunSpeed * Time.deltaTime, 0, 0);
    }

    protected override void Attack()
    {
        Debug.Log($"[Boss] Bắt đầu tấn công! Combo: {attackCount + 1}/{maxComboBeforeFire}");

        isAttacking = true;
        isChasing = false;

        Debug.Log($"[Boss] Physical Damage: {PhysicalDame}, Attack Range: {attackRadius}");

        Collider2D[] hits = Physics2D.OverlapCircleAll(attack_Point.position, attackRadius, playerLayer);
        Debug.Log($"[Boss] Tìm thấy {hits.Length} colliders trong tầm đánh");

        DealDamage(PhysicalDame, hits);

        audioSource?.PlayOneShot(attackSound);
        lastAttackTime = Time.time;
        attackCount++;

        if (attackCount >= maxComboBeforeFire)
        {
            Debug.Log("[Boss] Hoàn thành combo! Bắt đầu bắn đạn!");
            attackCount = 0;
            Fire();
        }
    }

    private void DealDamage(float damage, Collider2D[] hits)
    {
        Debug.Log($"[Boss] Đang thực hiện DealDamage(), Damage: {damage}, Hits: {hits.Length}");

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"[Boss] Collider hit: {hit.gameObject.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}");

            var player = hit.GetComponent<PlayerKnight>();
            if (player != null)
            {
                Debug.Log($"[Boss] Tìm thấy PlayerKnight, máu trước khi đánh: {player.GetCurrentHealth()}");

                if (damage == PhysicalDame)
                {
                    player.TakePhysicalDamage((int)damage);
                    Debug.Log($"[Boss] Gây {damage} sát thương vật lý!");
                }
                else if (damage == MagicDame)
                {
                    player.TakeMagicDamage((int)damage);
                    Debug.Log($"[Boss] Gây {damage} sát thương phép!");
                }

                Debug.Log($"[Boss] Máu player sau khi đánh: {player.GetCurrentHealth()}");
                return;
            }

            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Debug.Log($"[Boss] Gây {damage} sát thương qua IDamageable!");
                damageable.TakeDamage(damage);
            }
        }
    }

    protected override void Die()
    {
        isDead = true;
        BossData data = new BossData { health = 0, isDead = true };
        File.WriteAllText(savePath, JsonUtility.ToJson(data));

        animator.SetTrigger("Die");
        gate.OpenGate();
        FindAnyObjectByType<Enemy_Spawner>()?.PlayerRespawned();
        hpUI.SetActive(false);
        StartCoroutine(ReturnToPoolAfterDelay());
    }

    private IEnumerator ReturnToPoolAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        Debug.Log($"[Boss Health] Before Damage - Current: {currentHp}, Max: {Hp}, HealthBar: {bossHpBar?.fillAmount}");

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, Hp);

        if (bossHpBar != null)
        {
            if (smoothCoroutine != null)
                StopCoroutine(smoothCoroutine);

            float targetFill = currentHp / Hp;
            smoothCoroutine = StartCoroutine(SmoothHealthBar(targetFill));
        }

        Debug.Log($"[Boss Health] After Damage - Current: {currentHp}, Max: {Hp}, Target Fill: {currentHp / Hp}");

        if (currentHp <= 0)
        {
            Debug.Log("[Boss] Boss is dying!");
            Die();
        }
    }

    private IEnumerator SmoothHealthBar(float target)
    {
        float currentFill = bossHpBar.fillAmount;
        float elapsedTime = 0f;
        float duration = 0.5f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            bossHpBar.fillAmount = Mathf.Lerp(currentFill, target, elapsedTime / duration);
            yield return null;
        }

        bossHpBar.fillAmount = target;
    }

    private void PlayBossSound()
    {
        if (audioSource != null && bossSound != null)
            audioSource.PlayOneShot(bossSound);
    }

    public void ResetBossState()
    {
        currentHp = Hp;
        bossHpBar.fillAmount = 1;
    }

    private void OnDrawGizmos()
    {
        if (attack_Point != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attack_Point.position, attackRadius);
        }
    }

    protected override void Flip()
    {
        direction *= -1;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        hpUI.transform.localScale = new Vector3(-hpUI.transform.localScale.x, hpUI.transform.localScale.y, hpUI.transform.localScale.z);
    }

    public void Fire()
    {
        if (player != null)
        {
            Vector3 directionToPlayer = player.position - firePoint.position;
            directionToPlayer.Normalize();

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
            enemyBullet.SetMovementDirection(directionToPlayer * speedDan);
        }
    }

    public void FireCircle()
    {
        const int bulletCount = 12;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Vector3 bulletDirection = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0);

            GameObject bullet = Instantiate(bulletPrefab1, firePoint1.position, Quaternion.identity);
            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
            enemyBullet.SetMovementDirection(bulletDirection.normalized * vongTron);
        }
    }

    protected override void Patrol() { }
}
