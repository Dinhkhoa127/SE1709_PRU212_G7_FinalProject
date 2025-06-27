using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Endgame()
    {
        SceneManager.LoadScene("Endgame");
    }
    public void StartGameMap1()
    {
        SceneManager.LoadScene("Map1");
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }
    public void StartGameMap2()
    {
        SceneManager.LoadScene("Map2");
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }
    public void StartGameMap3()
    {
        SceneManager.LoadScene("Map3");
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }
    public void StartGameMap4()
    {
        SceneManager.LoadScene("Map4");
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }
    public void StartGameMap5()
    {
        SceneManager.LoadScene("Map5");
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }
    public void ExampleMap()
    {
        SceneManager.LoadScene("Example");
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game has been closed.");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");

    }
    public void Instruction()
    {
        SceneManager.LoadScene("Instruction");
    }
    public void Leaderboard()
    {
        SceneManager.LoadScene("Leaderboard");
    }
}
