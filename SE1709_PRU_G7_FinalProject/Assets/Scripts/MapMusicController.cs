using UnityEngine;

public class MapMusicController : MonoBehaviour
{
    void Start()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log("[MapMusicController] Start() in scene: " + sceneName);
        if (AudioController.instance != null)
        {
            Debug.Log("Found AudioController, playing music for: " + sceneName);
            switch (sceneName)
            {
                case "Map1":
                    AudioController.instance.PlayMap1Music();
                    break;
                case "Map2":
                    AudioController.instance.PlayMap2Music();
                    break;
                case "Map3":
                    AudioController.instance.PlayMap3Music();
                    break;
                case "MapRest":
                    AudioController.instance.PlayRestMapMusic();
                    break;
                case "Example":
                    AudioController.instance.PlayRestMapMusic();
                    break;
                case "MainMenu":
                    AudioController.instance.PlayMenuMusic();
                    break;
                case "EndGame":
                    AudioController.instance.PlayMenuMusic();
                    break;
                case "Instruction":
                    AudioController.instance.PlayMenuMusic();
                    break;
                case "LeaderBoard":
                    AudioController.instance.PlayMenuMusic();
                    break;
                default:
                    Debug.Log("No music assigned for: " + sceneName);
                    break;
            }
        }
        else
        {
            Debug.LogWarning("AudioController.instance is null!");
        }
    }
}