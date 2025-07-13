using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy1 : MonoBehaviour
{

    [SerializeField] protected float WalkSpeed = 2f;
    [SerializeField] protected float RunSpeed = 4f;
    [SerializeField] protected float FlySpeed = 5f;
    [SerializeField] protected float Hp = 100f;
    [SerializeField] protected float Mana = 100f;
    [SerializeField] protected float Defend = 0;
    [SerializeField] protected float PhysicalDame = 0;
    [SerializeField] protected float AttackSpeed = 0;
    [SerializeField] protected float AttackRange = 30;
    [SerializeField] protected float IsAttackRange = 30;
    [SerializeField] protected float MagicDame = 0;
    [SerializeField] protected float PatrolRange = 10f;

    [SerializeField] protected float attackCooldown = 0.5f;
    [SerializeField] protected float lastAttackTime = 0f;

    [SerializeField] protected Image healthBar;

    public float currentHealth = 0;
    public float expereince = 10;

    protected Rigidbody2D rb;

    public Vector2 spawnPosition;


    protected Transform player;

    protected bool isChasing = false;
    protected bool isAttacking = false;
    [SerializeField] protected Transform enemyEye;

    protected virtual void Start()
    {
        spawnPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentHealth = Hp;
    }


    protected abstract void Attack();
    protected abstract void Patrol();
    protected abstract void Die();


    protected virtual bool CheckInRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= AttackRange;
    }


    protected void DealDamageToPlayer()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, AttackRange, LayerMask.GetMask("Player")); // hoặc gán layer qua serialized field
        foreach (Collider2D playerCol in hitPlayers)
        {
            PlayerKnight playerScript = playerCol.GetComponent<PlayerKnight>();
            if (playerScript != null)
            {
                playerScript.TakePhysicalDamage((int)PhysicalDame); // hoặc bạn dùng TakeDamage
                Debug.Log("Enemy1 hit Player!");
            }
        }
    }

    protected virtual void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    protected void FaceToward(Vector2 direction)
    {
        float temp = transform.position.x - enemyEye.position.x;

        if (rb.linearVelocity.x < 0)
        {
            if (temp > 0)
            {
                return;
            }
            else
            {
                Flip();
                return;
            }

        }
        if (rb.linearVelocity.x > 0)
        {
            if (temp < 0)
            {
                return;
            }
            else
            {
                Flip();
                return;
            }
        }


    }
    public virtual void ResetState()
    {
        currentHealth = Hp;
        gameObject.SetActive(false);
        healthBar.fillAmount = 1;
    }


}