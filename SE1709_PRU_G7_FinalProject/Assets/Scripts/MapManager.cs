using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MapManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI timerText; // Kéo TextMeshProUGUI vào đây
    private float elapsedTime = 0f;
    private bool isRunning = true;
    public TextMeshProUGUI percentText; // Kéo TextMeshProUGUI này vào để hiển thị phần trăm hoàn thành
    private int totalEnemies;
    private int currentEnemies;

    void Start()
    {
        // Đếm tổng số quái khi bắt đầu map
        totalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        currentEnemies = totalEnemies;
        UpdatePercentUI();
    }

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            if (timerText != null)
                timerText.text = FormatTime(elapsedTime);
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    string FormatTime(float time)
    {
        int hours = Mathf.FloorToInt(time / 3600F);
        int minutes = Mathf.FloorToInt((time % 3600F) / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    // Gọi hàm này mỗi khi một quái bị tiêu diệt
    public void OnEnemyKilled()
    {
        currentEnemies--;
        UpdatePercentUI();
    }

    void UpdatePercentUI()
    {
        float percent = 0;
        if (totalEnemies > 0)
            percent = ((float)(totalEnemies - currentEnemies) / totalEnemies) * 100f;

        if (percentText != null)
            percentText.text = "Completed: " + percent.ToString("F1") + "%";
    }
}
