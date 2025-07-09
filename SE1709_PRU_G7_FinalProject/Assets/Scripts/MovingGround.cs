using UnityEngine;

public class MovingGround : MonoBehaviour
{
    [SerializeField] public float speed = 2f;
    [SerializeField] public float moveDistance = 5f;

    [SerializeField] private Vector2 moveDirection = new Vector2(1, 0);

    /// <summary>
    ///  Trái ↔ Phải (1, 0), Lên ↕ Xuống (0, 1), Chéo lên phả (1, 1), Chéo xuống trái (-1, 1)
    /// </summary>

    private Vector3 startPos;
    private bool movingForward = true;

    private Vector3 lastPosition;
    public Vector3 DeltaMovement { get; private set; }

    void Start()
    {
        startPos = transform.position;
        lastPosition = transform.position;

        // Đảm bảo direction được chuẩn hoá (length = 1)
        moveDirection = moveDirection.normalized;
    }

    void Update()
    {
        float moveStep = speed * Time.deltaTime;
        Vector3 direction3D = new Vector3(moveDirection.x, moveDirection.y, 0);

        transform.Translate((movingForward ? direction3D : -direction3D) * moveStep);

        DeltaMovement = transform.position - lastPosition;
        lastPosition = transform.position;

        if (Vector3.Distance(startPos, transform.position) >= moveDistance)
        {
            movingForward = !movingForward;
            startPos = transform.position;
        }
    }
}
