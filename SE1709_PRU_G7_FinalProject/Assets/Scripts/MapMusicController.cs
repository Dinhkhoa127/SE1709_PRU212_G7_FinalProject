using UnityEngine;

public class MapMusicController : MonoBehaviour
{
    private AudioClip currentMapClip;

    void Start()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log("[MapMusicController] Start() in scene: " + sceneName);

        if (AudioController.instance != null)
        {
            switch (sceneName)
            {
                case "Map1":
                    currentMapClip = AudioController.instance.map1Music;
                    break;
                case "Map2":
                    currentMapClip = AudioController.instance.map2Music;
                    break;
                case "Map3":
                    currentMapClip = AudioController.instance.map3Music;
                    break;
                case "MapRest":
                case "Example":
                    currentMapClip = AudioController.instance.restMapMusic;
                    break;
                case "MainMenu":
                case "EndGame":
                case "Instruction":
                case "LeaderBoard":
                    currentMapClip = AudioController.instance.menu;
                    break;
                default:
                    Debug.Log("No music assigned for: " + sceneName);
                    break;
            }

            // Nếu có nhạc phù hợp thì gọi PlayMapMusic
            if (currentMapClip != null)
            {
                AudioController.instance.PlayMapMusic(currentMapClip);
            }
        }
        else
        {
            Debug.LogWarning("AudioController.instance is null!");
        }

    }
}