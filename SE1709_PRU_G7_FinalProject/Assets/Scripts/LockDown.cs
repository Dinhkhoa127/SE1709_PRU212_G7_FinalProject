using UnityEngine;

public class LockDown : MonoBehaviour
{
    [SerializeField] GameObject lockGround;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            lockGround.SetActive(true);
        }
    }
}
