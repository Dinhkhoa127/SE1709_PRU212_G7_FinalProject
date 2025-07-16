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
            float volume = 0.9f;
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
                    volume = 0.1f;
                    break;
                case "MainMenu":
                case "EndGame":
                case "Instruction":
                case "LeaderBoard":
                    currentMapClip = AudioController.instance.menu;
                    volume = 0.1f;
                    break;
                default:
                    Debug.Log("No music assigned for: " + sceneName);
                    break;
            }

            // Nếu có nhạc phù hợp thì gọi PlayMapMusic
            if (currentMapClip != null)
            {
                AudioController.instance.PlayMapMusic(currentMapClip,volume);
            }
        }
        else
        {
            Debug.LogWarning("AudioController.instance is null!");
        }

    }
}