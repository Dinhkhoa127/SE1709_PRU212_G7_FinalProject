using UnityEngine;

public class NPCController : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float distance = 2f;
    private Animator animator;
    private Vector3 startPos;
    private bool moveRight = true;
    private bool facingRight = true;
    [SerializeField] private Transform uiCanvas; // Kéo FPrompt vào đây

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
                animator.SetBool("isWalk", false);
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
                animator.SetBool("isWalk", false);
            }
        }
    }

    void Flip()
    {
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
        facingRight = !facingRight;

        // Đảo lại scale.x của FPrompt để text luôn đúng chiều
        if (uiCanvas != null)
        {
            Vector3 canvasScale = uiCanvas.localScale;
            canvasScale.x *= -1;
            uiCanvas.localScale = canvasScale;

            // Đảo lại localPosition.x để text luôn đúng vị trí trên đầu NPC
            Vector3 pos = uiCanvas.localPosition;
            pos.x *= -1;
            uiCanvas.localPosition = pos;
        }
    }
}
