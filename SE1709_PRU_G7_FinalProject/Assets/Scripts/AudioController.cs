using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;

    public AudioSource sfxSource;      // Dùng cho hiệu ứng
    public AudioSource musicSource;    // Dùng cho nhạc nền



    public bool isSoundOn = true;
    public bool isMusicOn = true;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeAudio();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeAudio();
    }

    void InitializeAudio()
    {
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;

        if (sfxSource != null)
            sfxSource.mute = !isSoundOn;

        if (musicSource != null)
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                musicSource.mute = !isMusicOn;
                if (!musicSource.isPlaying && isMusicOn)
                    musicSource.Play();
            }
            else
            {
                musicSource.Stop();
            }
        }
    }

    public void SetSound(bool on)
    {
        isSoundOn = on;
        if (sfxSource != null)
            sfxSource.mute = !on;
        PlayerPrefs.SetInt("SoundOn", on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMusic(bool on)
    {
        isMusicOn = on;
        if (musicSource != null)
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                musicSource.mute = !on;
                if (on && !musicSource.isPlaying)
                    musicSource.Play();
                else if (!on && musicSource.isPlaying)
                    musicSource.Stop();
            }
            else
            {
                musicSource.Stop();
            }
        }
        PlayerPrefs.SetInt("MusicOn", on ? 1 : 0);
        PlayerPrefs.Save();
    }
}
