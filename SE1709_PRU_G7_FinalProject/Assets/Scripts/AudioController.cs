    using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;

    public AudioSource sfxSource;      // Dùng cho hiệu ứng
    public AudioSource musicSource;    // Dùng cho nhạc nền


    [Header("Sound Effects")]
    public AudioClip attackSFX;
    public AudioClip jumpSFX;
    public AudioClip hurtSFX;
    public AudioClip deathSFX;
    public AudioClip rollSFX;
    public AudioClip blockSFX;
    public AudioClip healManaSFX;
    public AudioClip skillCastSFX;
    public AudioClip enemyTakeDame;
    public AudioClip click;
    public AudioClip footSteep;
    public AudioClip menu;
    public AudioClip map1Music;
    public AudioClip map2Music;
    public AudioClip map3Music;
    public AudioClip restMapMusic;
    public AudioClip winMusic;
    private AudioClip lastMusicClip;
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
            musicSource.mute = !on;

            if (on && lastMusicClip != null)
            {
                PlayMapMusic(lastMusicClip, musicSource.volume); // Phát lại bài cũ
            }
            else if (!on)
            {
                musicSource.Stop();
            }
        }

        PlayerPrefs.SetInt("MusicOn", on ? 1 : 0);
        PlayerPrefs.Save();
    }


    public void PlayAttackSound()
    {
        PlaySFX(attackSFX, 0.9f); // Giảm âm thanh tấn công
    }
    public void PlayFootStep()
    {
        if (isSoundOn && sfxSource != null && footSteep != null)
        {
            sfxSource.PlayOneShot(footSteep, 0.7f); // âm lượng tùy chỉnh
        }
    }

    public void PlayEnemyTakeDame()
    {
        PlaySFX(enemyTakeDame, 0.5f);
    }

    public void PlayJumpSound()
    {
        PlaySFX(jumpSFX, 0.5f);
    }

    public void PlayHurtSound()
    {
        PlaySFX(hurtSFX, 0.4f);
    }

    public void PlayDeathSound()
    {
        PlaySFX(deathSFX, 0.4f);
    }

    public void PlayRollSound()
    {
        PlaySFX(rollSFX, 0.4f);
    }

    public void PlayBlockSound()
    {
        PlaySFX(blockSFX, 0.4f);
    }

    public void PlayHealSound()
    {
        PlaySFX(healManaSFX, 0.4f);
    }

    public void PlaySkillCastSound()
    {
        PlaySFX(skillCastSFX, 0.4f);
    }

    private void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (isSoundOn && sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayMapMusic(AudioClip music, float volume = 1f)
    {
        if (musicSource == null || music == null)
        {
            musicSource?.Stop();
            return;
        }

        lastMusicClip = music; // lưu lại bài nhạc

        if (!isMusicOn)
        {
            musicSource.Stop();
            return;
        }

        if (musicSource.clip != music)
        {
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.volume = volume;
            musicSource.Play();
        }
        else if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void PlayRestMapMusic()
    {
        PlayMapMusic(restMapMusic, 0.1f);
    }
    public void PlayMenuMusic()
    {
        PlayMapMusic(menu, 0.1f);
    }
    public void PlayMap1Music()
    {
        PlayMapMusic(map1Music, 1f);
    }
    public void PlayMap2Music()
    {
        PlayMapMusic(map2Music, 1f);
    }
    public void PlayMap3Music()
    {
        PlayMapMusic(map3Music, 1f);
    }
    public void PlayClickSound()
    {
        PlaySFX(click, 0.3f);
    }

    public void PlayWinMusic()
    {
        PlaySFX(winMusic, 0.8f);
    }
}
