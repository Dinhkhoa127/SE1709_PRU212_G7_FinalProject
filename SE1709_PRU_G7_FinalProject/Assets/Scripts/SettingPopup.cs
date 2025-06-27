using UnityEngine;

public class SettingPopup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject settingsPopup; // Kéo panel popup vào đây

    public void ShowPopup()
    {
        settingsPopup.SetActive(true);
    }

    public void HidePopup()
    {
        settingsPopup.SetActive(false);
    }
}
