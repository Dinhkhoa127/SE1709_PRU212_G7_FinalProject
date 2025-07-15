using UnityEngine;
using UnityEngine.UI;

public class butterflycontroller : Enemy1 , IDamageable
{
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float minPatrolDistance = 5f;
    [SerializeField] private float maxPatrolDistance = 10f;
    [SerializeField] private float patrolHeightVariation = 2f;
    //[SerializeField] protected Image healthBar;

    // [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private LayerMask obstacleLayer;

    private Animator animator;
    // private bool isAttacking = false;
    // private float lastAttackTime = 0f;
    private Vector2 patrolTarget;
    private bool movingRight = true;
    private Vector2 startPos;
    public GameObject bullet;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        startPos = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        SetNextPatrolTarget();
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (isAttacking && distanceToPlayer > attackRange)
        {
            isAttacking = false;
            animator.ResetTrigger("attack");
        }

        if (distanceToPlayer < detectionRange)
        {
            if (distanceToPlayer < attackRange && Time.time - lastAttackTime > attackCooldown)
            {
                Attack();
            }
            
        }
        else
        {
            Patrol();
        }
    }
    void Shoot()
    {
        if (bullet == null || firePoint == null || player == null)
        {
            Debug.LogError("Thiếu giá trị: Bullet, FirePoint hoặc Player chưa được gán!");
            return;
        }

        GameObject newbullet = Instantiate(bullet, firePoint.position, firePoint.rotation);
        Bullet bulletComponent = newbullet.GetComponent<Bullet>();

        if (bulletComponent != null)
        {
            bulletComponent.SetTarget(player);
            Debug.Log("Đã bắn đạn thành công!");
        }
        else
        {
            Debug.LogError("Lỗi: Prefab đạn không có script Bullet!");
        }
    }


    protected override void Attack()
    {
        isAttacking = true;
        animator.SetTrigger("attack");
        lastAttackTime = Time.time;    
        Shoot();
    }

    void ChasePlayer()
    {
        isAttacking = false;
        animator.SetBool("isMoving", true);

        Vector2 direction = (player.position - transform.position).normalized;
        Flip(direction.x);
        transform.position += (Vector3)direction * FlySpeed * Time.deltaTime;
    }

    protected override void Patrol()
    {
        if (isAttacking) return;

        animator.SetBool("isMoving", true);

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
        if ((directionX > 0 && transform.localScale.x < 0) || (directionX < 0 && transform.localScale.x > 0))
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            healthBar.transform.localScale = new Vector3(-healthBar.transform.localScale.x, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
        }
    }

    protected override void Die()
    {
        Destroy(gameObject);
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
        
    }
}
