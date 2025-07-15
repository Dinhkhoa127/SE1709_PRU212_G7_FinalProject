/*using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour, IDamageable
{
    [Header("Combat Stats")]
    [SerializeField] private float hp = 100f;
    [SerializeField] private float physicalDame = 10f;
    [SerializeField] private float magicDame = 5f;
    [SerializeField] private float attackCooldown = 0.5f;
    private float lastAttackTime = 0f;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    private int direction = 1;
    private Vector2 spawnPosition;

    [Header("Detection & Attack")]
    [SerializeField] private float attackRadius = 2.5f;
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playerLayer;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip bossSound;
    [SerializeField] private float soundInterval = 10f;

    [Header("UI")]
    [SerializeField] private Image healthBar;

    private float currentHealth;
    private float fireDamageTimer = 0f;
    private float fireDamageInterval = 1f;
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    private bool isChasing = false;
    private bool isAttacking = false;

    private GateController gate;
    private string savePath;

    private void Start()
    {
        spawnPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        gate = FindAnyObjectByType<GateController>();

        currentHealth = hp;

        string directoryPath = Path.Combine(Application.persistentDataPath, "GameData");
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        savePath = Path.Combine(directoryPath, "bossData.json");

        //LoadData();

        InvokeRepeating(nameof(PlayBossSound), soundInterval, soundInterval);
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayerX = Mathf.Abs(player.position.x - groundCheck.position.x);
        fireDamageTimer += Time.deltaTime;

        if (fireDamageTimer >= fireDamageInterval)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, 5, playerLayer);
            if (hits.Length > 0)
            {
                DealDamage(magicDame, hits);
                Debug.Log("Boss Fire Damage Triggered");
            }
            fireDamageTimer = 0f;
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
                {
                    animator.SetTrigger("Attack");
                }
            }
            else if (CheckInRange())
            {
                ChasePlayer();
            }
            else
            {
                Patrol();
            }
        }
    }

    private void Patrol()
    {
        isChasing = false;
        isAttacking = false;
        animator.SetBool("isWalking", true);

        bool isGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        Vector2 obstacleCheckDir = direction > 0 ? Vector2.right : Vector2.left;
        bool isObstacleAhead = Physics2D.Raycast(groundCheck.position, obstacleCheckDir, 6f, groundLayer);

        if (!isGroundAhead || isObstacleAhead)
        {
            Flip();
        }

        transform.position += new Vector3(direction * walkSpeed * Time.deltaTime, 0, 0);
    }

    private void ChasePlayer()
    {
        isChasing = true;
        isAttacking = false;
        animator.SetBool("isWalking", true);

        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        if ((dirToPlayer.x > 0 && direction < 0) || (dirToPlayer.x < 0 && direction > 0))
        {
            Flip();
        }

        transform.position += new Vector3(Mathf.Sign(dirToPlayer.x) * runSpeed * Time.deltaTime, 0, 0);
    }

    private bool CheckInRange()
    {
        return Vector2.Distance(groundCheck.position, player.position) < detectionRange;
    }

    private bool PlayerInAttackRange()
    {
        return Vector2.Distance(attackPoint.position, player.position) < attackRadius;
    }

    private void Attack()
    {
        isAttacking = true;
        isChasing = false;
        audioSource.PlayOneShot(attackSound);
        lastAttackTime = Time.time;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        DealDamage(physicalDame, hits);
    }

    private void DealDamage(float damage, Collider2D[] hits)
    {
        foreach (var hit in hits)
        {
            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
                Debug.Log("Boss dealt damage to player.");
                return;
            }
        }
    }

    private void Flip()
    {
        direction *= -1;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        healthBar.transform.localScale = new Vector3(-healthBar.transform.localScale.x, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
    }

    private void Die()
    {
        BossData data = new BossData { health = 0, isDead = true };
        File.WriteAllText(savePath, JsonUtility.ToJson(data));
        animator.SetTrigger("Die");
        gate.OpenGate();
        FindAnyObjectByType<Enemy_Spawner>()?.PlayerRespawned();
        StartCoroutine(ReturnToPoolAfterDelay());
    }

    private IEnumerator ReturnToPoolAfterDelay()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        gameObject.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthBar.fillAmount = currentHealth / hp;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void PlayBossSound()
    {
        if (audioSource != null && bossSound != null)
        {
            audioSource.PlayOneShot(bossSound);
        }
    }

    public void ResetBossState()
    {
        currentHealth = hp;
        healthBar.fillAmount = 1f;
        gameObject.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, detectionRange);
        }
    }
}

[System.Serializable]
public class BossData
{
    public float health;
    public bool isDead;
}
*/