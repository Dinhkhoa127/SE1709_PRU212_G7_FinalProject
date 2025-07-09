using UnityEngine;

public class NPCController : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float distance = 2f;
    private Animator animator;

    private Vector3 startPos;
    private bool moveRight = true;
    private bool facingRight = true;

    void Start()
    {
        startPos = transform.position;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Patrol();
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
            animator.SetBool("isWalk", true);
            if (transform.position.x >= rightBound)
            {
                moveRight = false;
                Flip();
                animator.SetBool("isWalk", false); // Đứng lại khi đổi hướng
            }
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
            animator.SetBool("isWalk", true);
            if (transform.position.x <= leftBound)
            {
                moveRight = true;
                Flip();
                animator.SetBool("isWalk", false); // Đứng lại khi đổi hướng
            }
        }
    }

    void Flip()
    {
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
        facingRight = !facingRight;
    }
}
