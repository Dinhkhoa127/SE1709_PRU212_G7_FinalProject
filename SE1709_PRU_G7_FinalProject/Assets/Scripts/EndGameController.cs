using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller cho EndGame scene - xử lý restart và main menu
/// </summary>
public class EndGameController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;
    
    public TMPro.TextMeshProUGUI playTimeText;
    public TMPro.TextMeshProUGUI enemiesKilledText;
    
    void Start()
    {
        // Setup button events
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (playTimeText != null)
        {
            float time = GameManager.Instance.totalPlayTime;
            int hours = Mathf.FloorToInt(time / 3600f);
            int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            playTimeText.text = $"Play Time: {hours:00}:{minutes:00}:{seconds:00}";
        }
        if (enemiesKilledText != null)
        {
            int killed = GameManager.Instance.totalEnemiesKilled;
            int total = GameManager.Instance.totalEnemiesInGame;
            enemiesKilledText.text = $"Enemies Defeated: {killed}/{total}";
        }
    }
    
    public void RestartGame()
    {
        // Sử dụng GameManager để restart
        if (GameManager.Instance != null)
        {
            AudioController.instance?.PlayClickSound();
            GameManager.Instance.RestartFromEndGame();
        }
        else
        {
            // Fallback nếu không có GameManager
            UnityEngine.SceneManagement.SceneManager.LoadScene("Map1");
        }
    }
    
    public void GoToMainMenu()
    {
        // Load main menu
        if (GameManager.Instance != null)
        {
            AudioController.instance?.PlayClickSound();
            GameManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            // Fallback nếu không có GameManager
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
    
    public void QuitGame()
    {
        AudioController.instance?.PlayClickSound();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
            // Fallback quit
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
} 