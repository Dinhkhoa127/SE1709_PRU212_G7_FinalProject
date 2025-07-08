using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float detectionRange = 5f; // Khoảng cách phát hiện Player
    [SerializeField] private float attackRange = 1f;    // Khoảng cách tấn công
    [SerializeField] private float health = 3f;
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float healDelay = 3f; // Thời gian chờ trước khi hồi máu
    [SerializeField] private float healRate = 1f;  // Số máu hồi mỗi giây

    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayers;
    [SerializeField] private int attackDamage = 1;

    private Vector3 startPos;
    private bool moveRight = true;
    private bool facingRight = true;
    private Transform player;
    private Animator animator;
    private bool isAttacking = false;
    private float attackCooldown = 1.0f; // thời gian giữa các đòn tấn công
    private float lastAttackTime = -999f;
    private float lastTimeSawPlayer = -999f;
    private bool hasHealed = false;

    // Thêm biến tham chiếu đến EnemyHealthBar
    [SerializeField] public EnemyHealthBar healthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
        player = GameObject.FindGameObjectWithTag("Player").transform; // Đảm bảo HeroKnight có tag là "Player"
        animator = GetComponent<Animator>();
        health = maxHealth;
        // Khởi tạo thanh máu nếu có
        if (healthBar != null)
        {
            healthBar.Setup((int)maxHealth);
            healthBar.UpdateHealth((int)health);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            lastTimeSawPlayer = Time.time; // Cập nhật thời gian cuối cùng thấy player
            hasHealed = false; // Reset khi phát hiện player
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRange)
            {
                if (!isAttacking)
                {
                    // Đuổi theo player
                    Vector2 direction = (player.position - transform.position).normalized;
                    transform.Translate(new Vector2(direction.x, 0) * speed * Time.deltaTime);

                    // Lật hướng nếu cần
                    if ((direction.x > 0 && !facingRight) || (direction.x < 0 && facingRight))
                    {
                        Flip();
                    }
                    animator.SetBool("isRun", true);
                }
            }
            else
            {
                // Trong tầm tấn công
                animator.SetBool("isRun", false);
                if (!isAttacking && Time.time - lastAttackTime > attackCooldown)
                {
                    isAttacking = true;
                    animator.SetTrigger("isAttack");
                    Debug.Log("Enemy is attacking Player");
                    
                }
            }
        }
        else
        {
            if (!isAttacking)
            {
                Patrol();
                HealIfNeeded();
            }
        }
    }

    void Patrol()
    {
        // Đảm bảo hướng nhìn đúng với hướng di chuyển
        if (moveRight && !facingRight) Flip();
        if (!moveRight && facingRight) Flip();

        float leftBound = startPos.x - distance;
        float rightBound = startPos.x + distance;
        
        if (moveRight)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            if (transform.position.x >= rightBound)
            {
                moveRight = false;
                Flip(); // Quay đầu khi đến ranh giới phải
            }
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
            if (transform.position.x <= leftBound)
            {
                moveRight = true;
                Flip(); // Quay đầu khi đến ranh giới trái
            }
        }
    }

    void Flip()
    {
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
        facingRight = !facingRight;

        // Đổi vị trí attackPoint theo hướng
        if (attackPoint != null)
        {
            float newX = facingRight ? Mathf.Abs(attackPoint.localPosition.x) : -Mathf.Abs(attackPoint.localPosition.x);
            attackPoint.localPosition = new Vector3(newX, attackPoint.localPosition.y, attackPoint.localPosition.z);
        }
    }

    // Hàm này sẽ được gọi từ Animation Event ở cuối animation attack
    public void EndAttack()
    {
        isAttacking = false;
        lastAttackTime = Time.time;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        // Cập nhật thanh máu nếu có
        if (healthBar != null)
            healthBar.UpdateHealth((int)health);
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public void DealDamageToPlayer()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayers);
        foreach (Collider2D player in hitPlayers)
        {
            PlayerKnight playerScript = player.GetComponent<PlayerKnight>();
            if (playerScript != null)
            {
                //playerScript.TakeDamage(attackDamage);
                playerScript.TakePhysicalDamage(attackDamage);
            }
        }
    }
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    void HealIfNeeded()
    {
        if (health < maxHealth && Time.time - lastTimeSawPlayer > healDelay && !hasHealed)
        {
            health = maxHealth;
            hasHealed = true;
            // Cập nhật thanh máu UI khi hồi máu
            if (healthBar != null)
                healthBar.UpdateHealth((int)health);
            Debug.Log($"Enemy fully healed after {healDelay} seconds: current health = {health}");
        }
    }
}
