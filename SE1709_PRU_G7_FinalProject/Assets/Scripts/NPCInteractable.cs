using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    public enum NPCType { Statue, Shop, Item }
    public NPCType npcType = NPCType.Statue;
    public GameObject shopPanel;
    [Tooltip("FPrompt là GameObject chứa TextMeshPro, sẽ hiện trên đầu NPC khi lại gần.")]
    public GameObject fPrompt;
    private bool playerInRange = false;
    private PlayerKnight playerKnight;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (fPrompt != null) fPrompt.SetActive(true);
            // Lưu lại tham chiếu PlayerKnight để dùng khi bấm F
            playerKnight = other.GetComponent<PlayerKnight>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (fPrompt != null) fPrompt.SetActive(false);
            playerKnight = null;
            // Tự động đóng shop khi rời khỏi NPC
            if (shopPanel != null && shopPanel.activeSelf)
            {
                shopPanel.SetActive(false);
                Time.timeScale = 1f;
            }   
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            switch (npcType)
            {
                case NPCType.Statue:
                    if (playerKnight != null)
                    {
                        playerKnight.Heal(playerKnight.GetMaxHealth());
                        playerKnight.RestoreFullMana();
                        playerKnight.RestoreFullStamina();
                        Debug.Log("Player được tượng thần ban phước và hồi đầy máu!");
                        playerKnight.SaveGame(); // Tự động lưu sau khi hồi máu
                    }
                    if (fPrompt != null) fPrompt.SetActive(false);
                    break;
                case NPCType.Shop:


                    if (shopPanel != null)
                    {
                        bool isActive = shopPanel.activeSelf;
                        if (fPrompt != null) fPrompt.SetActive(false);
                        if (isActive)
                        {
                            // Nếu đang mở thì đóng
                            shopPanel.SetActive(false);
                            Time.timeScale = 1f;
                            if (fPrompt != null) fPrompt.SetActive(true); // hiện lại F để tương tác tiếp
                        }
                        else
                        {
                            // Nếu đang tắt thì mở
                            shopPanel.SetActive(true);
                            Time.timeScale = 0f;
                            if (fPrompt != null) fPrompt.SetActive(false); // ẩn F khi shop đang mở
                        }
                    }
                    break;
                case NPCType.Item:
                    Debug.Log("Mua vật phẩm từ NPC!");
                    // TODO: Gọi hàm nhận vật phẩm ở đây
                    if (shopPanel != null)
                    {
                        bool isActive = shopPanel.activeSelf;
                        if (fPrompt != null) fPrompt.SetActive(false);
                        if (isActive)
                        {
                            // Nếu đang mở thì đóng
                            shopPanel.SetActive(false);
                            Time.timeScale = 1f;
                            if (fPrompt != null) fPrompt.SetActive(true); // hiện lại F để tương tác tiếp
                        }
                        else
                        {
                            // Nếu đang tắt thì mở
                            shopPanel.SetActive(true);
                            Time.timeScale = 0f;
                            if (fPrompt != null) fPrompt.SetActive(false); // ẩn F khi shop đang mở
                        }
                    }
                    break;
            }
        }
    }
}
