using System.Collections;
using UnityEngine;

public class GateController : MonoBehaviour
{
    public Transform closedPosition;
    public Transform openPosition;
    public float moveSpeed = 2f;

    private bool isMoving = false;
    private bool isClosed = false;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;

    public void CloseGate()
    {
        if (!isMoving)
        {
            isMoving = true;
            audioSource.PlayOneShot(attackSound);
            StartCoroutine(MoveGate(closedPosition.position, true));
        }
    }

    public void OpenGate()
    {
        if (!isMoving)
        {
            isMoving = true;
            audioSource.PlayOneShot(attackSound);
            StartCoroutine(MoveGate(openPosition.position, false));
        }
    }


    IEnumerator MoveGate(Vector3 targetPosition, bool closing)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        isClosed = closing;
        isMoving = false;
    }

    public bool IsClosed()
    {
        return isClosed;
    }
}
