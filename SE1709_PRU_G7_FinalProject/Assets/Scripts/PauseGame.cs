using UnityEngine;

public class PauseGame : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject pausePanel; // Kéo Panel UI vào đây
    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false); // Ẩn panel lúc đầu
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
                ResumeGame();
            else
                Pause();
        }
    }

    void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
}
