using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script quản lý chuyển đến Scene EndGame khi boss cụ thể bị đánh bại
/// 
/// Cách sử dụng:
/// 1. Gắn script này vào một GameObject trong scene
/// 2. Kéo boss cụ thể vào field "Target Boss" HOẶC nhập tên boss vào "Target Boss Name"
/// 3. Script sẽ chỉ trigger EndGame khi boss được chỉ định chết
/// 
/// Ví dụ: Nếu có 2 boss trong scene nhưng chỉ muốn "FinalBoss" trigger EndGame,
/// thì chỉ cần kéo GameObject "FinalBoss" vào field "Target Boss"
/// </summary>
public class EndGameManager : MonoBehaviour
{
    [Header("Boss Target Settings")]
    [SerializeField] private BossController targetBoss; // Boss cụ thể sẽ trigger EndGame
    [SerializeField] private string targetBossName = ""; // Hoặc dùng tên boss (backup option)
    
    [Header("EndGame Settings")]
    [SerializeField] private float delayBeforeEndGame = 3f; // Thời gian chờ trước khi chuyển scene
    [SerializeField] private string endGameSceneName = "EndGame"; // Tên scene EndGame
    [SerializeField] private bool onlyWorkInMap3 = true; // Chỉ hoạt động trong Map3
    
    [Header("Optional UI")]
    [SerializeField] private GameObject victoryUI; // UI hiển thị chiến thắng (optional)
    [SerializeField] private AudioClip victorySound; // Âm thanh chiến thắng (optional)
    
    private bool hasTriggeredEndGame = false; // Đảm bảo chỉ trigger 1 lần
    private AudioSource audioSource;
    private bool isTargetBossDefeated = false;

    private void Start()
    {
        // Lấy AudioSource component hoặc thêm mới nếu chưa có
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && victorySound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Kiểm tra xem có đang ở scene đúng không
        if (onlyWorkInMap3)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene != "Map3")
            {
                Debug.Log($"EndGameManager: Hiện tại đang ở scene {currentScene}, chỉ hoạt động trong Map3");
                gameObject.SetActive(false);
                return;
            }
        }
        
        // Tự động tìm target boss nếu chưa có và có tên boss
        if (targetBoss == null && !string.IsNullOrEmpty(targetBossName))
        {
            GameObject bossObject = GameObject.Find(targetBossName);
            if (bossObject != null)
            {
                targetBoss = bossObject.GetComponent<BossController>();
            }
        }
        
        if (targetBoss != null)
        {
            // Tự động lấy tên boss nếu targetBossName trống
            if (string.IsNullOrEmpty(targetBossName))
            {
                targetBossName = targetBoss.name;
            }
            Debug.Log($"EndGameManager: Đã khởi tạo, đang theo dõi boss '{targetBoss.name}'...");
        }
        else
        {
            Debug.LogWarning("EndGameManager: Chưa chỉ định target boss! Hãy kéo boss vào field 'Target Boss' hoặc nhập tên trong 'Target Boss Name'");
        }
    }

    private void Update()
    {
        // Kiểm tra target boss cụ thể thay vì dùng static property
        CheckTargetBossStatus();
        
        // Kiểm tra nếu target boss đã chết và chưa trigger EndGame
        if (isTargetBossDefeated && !hasTriggeredEndGame)
        {
            TriggerEndGame();
        }
    }
    
    private void CheckTargetBossStatus()
    {
        if (targetBoss == null)
        {
            // Thử tìm lại boss bằng tên nếu có
            if (!string.IsNullOrEmpty(targetBossName))
            {
                GameObject bossObject = GameObject.Find(targetBossName);
                if (bossObject != null)
                {
                    targetBoss = bossObject.GetComponent<BossController>();
                }
            }
            return;
        }
        
        // Kiểm tra nếu target boss đã chết:
        // 1. Object bị destroy HOẶC
        // 2. Collider2D bị disable (như trong method Die() của BossController)
        if (targetBoss == null || 
            !targetBoss.gameObject.activeInHierarchy || 
            (targetBoss.GetComponent<Collider2D>() != null && !targetBoss.GetComponent<Collider2D>().enabled))
        {
            if (!isTargetBossDefeated)
            {
                isTargetBossDefeated = true;
                string bossName = !string.IsNullOrEmpty(targetBossName) ? targetBossName : "Target Boss";
                Debug.Log($"Target boss '{bossName}' đã bị đánh bại!");
            }
        }
    }

    private void TriggerEndGame()
    {
        hasTriggeredEndGame = true;
        string bossName = !string.IsNullOrEmpty(targetBossName) ? targetBossName : "Target Boss";
        Debug.Log($"Boss '{bossName}' đã bị đánh bại! Chuyển đến màn hình EndGame...");
        
        // Phát âm thanh chiến thắng nếu có
        if (victorySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(victorySound);
        }
        
        // Hiển thị UI chiến thắng nếu có
        if (victoryUI != null)
        {
            victoryUI.SetActive(true);
        }
        
        // Bắt đầu coroutine chuyển scene
        StartCoroutine(LoadEndGameScene());
    }

    private IEnumerator LoadEndGameScene()
    {
        // Chờ một chút để animation chết của boss chạy xong
        yield return new WaitForSeconds(delayBeforeEndGame);
        
        // Đảm bảo rằng time scale về bình thường (trường hợp có pause game)
        Time.timeScale = 1f;
        
        // Chuyển đến Scene EndGame
        SceneManager.LoadScene(endGameSceneName);
    }

    // Method này có thể được gọi từ UI button hoặc script khác
    public void LoadEndGameImmediately()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(endGameSceneName);
    }
    
    // Method để restart game nếu cần
    public void RestartGame()
    {
        Time.timeScale = 1f;
        // Reset trạng thái boss
        // BossController.IsBossDefeated = false; // Cần thêm setter trong BossController nếu muốn reset
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
} 