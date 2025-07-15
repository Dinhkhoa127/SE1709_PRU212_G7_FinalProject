using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    public Toggle soundToggle;
    public Toggle musicToggle;

    void Start()
    {
        Debug.Log("SoundToggle: " + soundToggle.name);
        Debug.Log("MusicToggle: " + musicToggle.name);
        // Lấy trạng thái từ AudioController để đồng bộ toggle
        soundToggle.isOn = AudioController.instance.isSoundOn;
        musicToggle.isOn = AudioController.instance.isMusicOn;

        // Khi thay đổi toggle, gọi hàm trong AudioController
        soundToggle.onValueChanged.AddListener(AudioController.instance.SetSound);
        musicToggle.onValueChanged.AddListener(AudioController.instance.SetMusic);
    }
}
