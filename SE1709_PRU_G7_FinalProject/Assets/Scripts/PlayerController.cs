using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //public float moveSpeed = 5f;
    //public float jumpForce = 2f;
    //public Transform attackPoint;
    //public float attackRange = 0.5f;
    //public LayerMask enemyLayers;

    //private Rigidbody2D rb;
    //private Animator animator;
    //private bool isGrounded = true;
    //private bool isDead = false;

    //void Start()
    //{
    //    rb = GetComponent<Rigidbody2D>();
    //    animator = GetComponent<Animator>();
    //}

    //void Update()
    //{
    //    if (isDead) return;

    //    Move();
    //    Jump();
    //    Attack();
    //}

    //void Move()
    //{
    //    float moveInput = Input.GetAxisRaw("Horizontal");
    //    rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

    //    animator.SetFloat("Speed", Mathf.Abs(moveInput));

    //    if (moveInput > 0)
    //        transform.localScale = new Vector3(1, 1, 1);
    //    else if (moveInput < 0)
    //        transform.localScale = new Vector3(-1, 1, 1);
    //}

    //void Jump()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
    //    {
    //        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    //        animator.SetTrigger("Jump");
    //    }
    //}

    //void Attack()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        animator.SetTrigger("Attack");

    //        //Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
    //        //foreach (Collider2D enemy in hitEnemies)
    //        //{
    //        //    Debug.Log("Hit " + enemy.name);
    //        //    // enemy.GetComponent<Enemy>().TakeDamage(damage);
    //        //}
    //    }
    //}

    //public void Die()
    //{
    //    isDead = true;
    //    rb.linearVelocity = Vector2.zero;
    //    animator.SetTrigger("Die");
    //    // Có thể thêm: Disable collider, tắt script di chuyển, v.v.
    //}

    //void OnDrawGizmosSelected()
    //{
    //    if (attackPoint == null) return;

    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    //}

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        isGrounded = true;
    //        animator.SetBool("IsGrounded", true);
    //    }
    //}

    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        isGrounded = false;
    //        animator.SetBool("IsGrounded", false);
    //    }
    //}
}