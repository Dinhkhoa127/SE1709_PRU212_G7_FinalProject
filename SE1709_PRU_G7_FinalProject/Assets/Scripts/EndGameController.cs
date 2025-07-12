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
    
    void Start()
    {
        // Setup button events
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
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