using System;
using UnityEngine;
using UnityEngine.UI;

public class enemywalkercontroller : Enemy1, IDamageable
{
    private int direction = 1;
    private Animator animator;
    private float lastAttackTime;
    private Vector2 startPosition; // Lưu vị trí bắt đầu

    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;
    // healthBar đã được kế thừa từ Enemy1, không cần khai báo lại
    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        startPosition = transform.position; // Lưu vị trí bắt đầu
    }

    void Update()
    {
        if (player != null && CheckInRange())
        {
            FacePlayer(); // Xoay hướng enemy về phía player
            ShootAtPlayer();
        }
        else
        {
            Patrol();
        }
    }

    protected override void Patrol()
    {
        animator.SetBool("isMoving", true);
        transform.position += new Vector3(direction * WalkSpeed * Time.deltaTime, 0, 0);

        // Kiểm tra nếu enemy di chuyển quá phạm vi được đặt trước
        if (Mathf.Abs(transform.position.x - startPosition.x) >= PatrolRange)
        {
            Flip();
        }
    }

    private void ShootAtPlayer()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger("attack");

            GameObject newBullet = Instantiate(bullet, firePoint.position, Quaternion.identity);
            Rigidbody2D rb = newBullet.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 directionToPlayer = (player.position - firePoint.position).normalized;
                rb.linearVelocity = directionToPlayer * bulletSpeed;
            }

            // Đảm bảo sau khi bắn, animation chuyển về trạng thái di chuyển
            Invoke(nameof(ResetToMove), 0.5f);
        }
    }

    protected override bool CheckInRange()
    {
        return Vector2.Distance(transform.position, player.position) <= AttackRange;
    }

    protected override void Flip()
    {
        direction *= -1;
        transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
        healthBar.transform.localScale = new Vector3(-healthBar.transform.localScale.x, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
    }

    private void FacePlayer()
    {
        if (player != null)
        {
            float directionToPlayer = player.position.x - transform.position.x;
            if ((directionToPlayer > 0 && direction < 0) || (directionToPlayer < 0 && direction > 0))
            {
                Flip();
            }
        }
    }

    private void ResetToMove()
    {
        animator.ResetTrigger("attack");
        animator.SetBool("isMoving", true);
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
        gameObject.SetActive(false);
    }

    protected override void Attack()
    {
        throw new NotImplementedException();
    }
}
