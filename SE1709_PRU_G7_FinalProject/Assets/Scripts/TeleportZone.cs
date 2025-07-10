using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TeleportZone : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform targetPosition;

    [Header("Fade UI")]
    [SerializeField] private Image fadeImage;

    [Header("Fade Durations")]
    [SerializeField] private float flickerDuration = 0.2f;   // Thời gian mỗi lần chớp đen
    [SerializeField] private float fullFadeDuration = 0.4f;  // Tối hẳn để teleport
    [SerializeField] private float fadeBackDuration = 1.0f;  // Thời gian sáng lại

    private bool isTeleporting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTeleporting && other.CompareTag("Player"))
        {
            isTeleporting = true;
            StartCoroutine(FlickerAndTeleport(other.gameObject));
        }
    }

    IEnumerator FlickerAndTeleport(GameObject player)
    {
        // Chớp đen 2 lần
        for (int i = 0; i < 2; i++)
        {
            yield return StartCoroutine(Fade(0f, 1f, flickerDuration));
            yield return StartCoroutine(Fade(1f, 0f, flickerDuration));
        }

        // Tối hẳn
        yield return StartCoroutine(Fade(0f, 1f, fullFadeDuration));

        // Dịch chuyển
        player.transform.position = targetPosition.position;
        yield return new WaitForSeconds(0.2f);

        // Sáng lại
        yield return StartCoroutine(Fade(1f, 0f, fadeBackDuration));

        isTeleporting = false;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        Color color = fadeImage.color;
        float timer = 0f;

        while (timer < duration)
        {
            float alpha = Mathf.Lerp(from, to, timer / duration);
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(0f, 0f, 0f, to);
    }
}
