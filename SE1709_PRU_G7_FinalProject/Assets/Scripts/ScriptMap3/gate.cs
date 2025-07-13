using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class gate : MonoBehaviour
{
    public bool needToClose = false;
    [SerializeField] Transform Head;
    [SerializeField] Transform EndPoint;
    [SerializeField] Transform startPoint;
    [SerializeField] GameObject gateObject;
    public bool stop = false;
    public bool moveUp = false;
    public float moveSpeed = 2f;
    public float moveTime = 2f;

    private float timer = 0;
    private bool isOpening = false;

    void Update()
    {
        if (!isOpening) // Nếu không phải mở cửa, chạy timer đóng cửa
        {
            timer += Time.deltaTime;
            if (timer <= moveTime)
            {
                if (moveUp)
                {
                    transform.position += Vector3.up * moveSpeed * Time.deltaTime;
                }
                else
                {
                    transform.position += Vector3.down * moveSpeed * Time.deltaTime;
                }
            }
        }
    }

    // Phương thức mở cửa
    public void OpenGate()
    {
        Debug.Log("Mở cửa!");
        StopAllCoroutines(); // Dừng mọi coroutine đang chạy
        StartCoroutine(MoveGate(true));
    }

    // Phương thức đóng cửa
    public void CloseGate()
    {
        Debug.Log("Đóng cửa!");
        StopAllCoroutines();
        StartCoroutine(MoveGate(false));
    }

    // Coroutine di chuyển cửa
    private System.Collections.IEnumerator MoveGate(bool opening)
    {
        isOpening = opening;
        Vector3 targetPosition = opening ? Head.position : EndPoint.position;

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        isOpening = false;
    }

    // Phương thức hủy cửa khi boss chết
    public void DestroyGate()
    {
        Debug.Log("Boss đã chết, hủy cửa!");
        Destroy(gameObject);
    }
}
