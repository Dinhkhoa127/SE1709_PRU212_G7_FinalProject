using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Đảm bảo có ở đầu file nếu dùng List
using TMPro;
using Assets.Scripts; // Ở đầu file

public class PlayerKnight : MonoBehaviour
{
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] int health = 5;

    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] GameObject spikePrefab;

    [SerializeField] private int maxArmorShield = 2;
    [SerializeField] private int maxMagicShield = 2;
    [SerializeField] private int maxMana = 100;

    private int manaCost = 10;
    public GameObject skillProjectilePrefab;
    public Transform castPoint;
    private int currentMana;
    private int currentArmorShield;
    private int currentMagicShield;
    private float manaRegenTimer = 0f;
    public float manaRegenInterval = 1f; // Mỗi 1 giây
    public int manaRegenAmount = 2; // Hồi 2 mana

    //Thanh Stamina của BlockEffect
    [SerializeField] private float maxBlockStamina = 100f;
    [SerializeField] private float blockStamina = 100f;
    [SerializeField] private float blockStaminaDecreasePerHit = 34f;
    [SerializeField] private float blockCooldownTime = 10f;
    private bool isBlockOnCooldown = false;
    private float blockCooldownTimer = 0f;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private Sensor_HeroKnight m_wallSensorR1;
    private Sensor_HeroKnight m_wallSensorR2;
    private Sensor_HeroKnight m_wallSensorL1;
    private Sensor_HeroKnight m_wallSensorL2;
    private bool m_isWallSliding = false;
    private bool m_grounded = false;
    private bool m_rolling = false;
    private bool m_jumping = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;
    private float inputX = 0f;
    private bool onMovingGround = false;
    private MovingGround currentGround;
    private bool isDead = false;

    private bool isBlocking = false;
    private GameObject swordCollider1;
    private GameObject swordCollider2;
    private GameObject swordCollider3;

    public List<string> learnedSkills = new List<string>(); // Nếu chưa có thì thêm

    public TMP_Text autosaveText; // Thêm vào class PlayerKnight

    public int gold = 0;
    public string currentStage = "Stage1"; // hoặc tên scene mặc định đầu tiên
    public List<string> inventory = new List<string>();
    void Start()
    {
        swordCollider1 = transform.Find("SwordCollider1").gameObject;
        swordCollider2 = transform.Find("SwordCollider2").gameObject;
        swordCollider3 = transform.Find("SwordCollider3").gameObject;
        swordCollider1.SetActive(false);
        swordCollider2.SetActive(false);
        swordCollider3.SetActive(false);
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
        currentArmorShield = maxArmorShield;
        currentMagicShield = maxMagicShield;
        currentMana = maxMana;
    }

    void Update()
    {
        if (isDead) return;
        HandleTimers();
        HandleGroundCheck();
        HandleMove();
        HandleWallSlide();
        HandleAttack();
        HandleBlock();
        HandleRoll();
        HandleJump();
        HandleRunIdle();
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(1); // Nhấn H để hồi 1 máu
        }
        HandleSkillBerserk(); // Nhấn R để sử dụng kỹ năng Berserk
        if (Input.GetKeyDown(KeyCode.Q))
        {
            HandleSkillCast(manaCost); // Nhấn Q để chưởng chiêu
        }
        manaRegenTimer += Time.deltaTime;
        if (manaRegenTimer >= manaRegenInterval)
        {
            RegenerateMana(manaRegenAmount);
            manaRegenTimer = 0f;
        }
        if (isBlockOnCooldown) //Cooldown block
        {
            blockCooldownTimer -= Time.deltaTime;
            if (blockCooldownTimer <= 0)
            {
                isBlockOnCooldown = false;
                blockStamina = maxBlockStamina;
            }
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGame();
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            PrintSaveData();
        }
    }

    public int GetHealth() { return health; }
    public int GetMaxHealth() { return maxHealth; }
    void HandleTimers()
    {
        m_timeSinceAttack += Time.deltaTime;
        if (m_rolling)
            m_rollCurrentTime += Time.deltaTime;
        if (m_rollCurrentTime > m_rollDuration)
            m_rolling = false;
    }

    void HandleGroundCheck()
    {
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
            m_jumping = false;
        }
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }
    }

    void HandleMove()
    {
        inputX = Input.GetAxis("Horizontal");
        if (inputX > 0)
        {
            m_facingDirection = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (inputX < 0)
        {
            m_facingDirection = -1;
            transform.localScale = new Vector3(-1, 1, 1);
        }
        if (!m_rolling)
            m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);
        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);
    }

    void HandleWallSlide()
    {
        m_isWallSliding = (m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);
    }

    //void HandleDeath()
    //{
    //    if (Input.GetKeyDown("e") && !m_rolling)
    //    {
    //        m_animator.SetBool("noBlood", m_noBlood);
    //        m_animator.SetTrigger("Death");
    //    }
    //}

    //void HandleHurt()
    //{
    //    if (Input.GetKeyDown("q") && !m_rolling)
    //        m_animator.SetTrigger("Hurt");
    //}

    void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;
            if (m_currentAttack > 3)
                m_currentAttack = 1;
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;
            m_animator.SetTrigger("Attack" + m_currentAttack);
            m_timeSinceAttack = 0.0f;
        }
    }

    void HandleBlock()
    {
        if (isBlockOnCooldown) return; // Không cho block khi cooldown

        if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
            isBlocking = true; // Bắt đầu block
        }
        else if (Input.GetMouseButtonUp(1))
        {
            m_animator.SetBool("IdleBlock", false);
            isBlocking = false; // Ngừng block
        }
    }

    void HandleRoll()
    {
        if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding && m_grounded)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown("space") && m_grounded && !m_rolling)
        {
            m_jumping = true;
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }
    }

    void HandleRunIdle()
    {
        if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        else
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isBlocking && !isBlockOnCooldown)
        {
            blockStamina -= blockStaminaDecreasePerHit;
            if (blockStamina <= 0)
            {
                blockStamina = 0;
                isBlockOnCooldown = true;
                blockCooldownTimer = blockCooldownTime;
                isBlocking = false;
                m_animator.SetBool("IdleBlock", false);
                // Có thể thêm hiệu ứng block bị vỡ ở đây
            }
            Debug.Log("Block hit! Stamina: " + blockStamina);
            return;
        }
        else
        {
            health -= amount;
            Debug.Log("Mất máu: " + amount + " | Máu còn lại: " + health);
        }
        if (health <= 0)
        {
            Die();
        }
        else
        {
            m_animator.SetTrigger("Hurt");
        }
    }
    public int GetMaxMana() { return maxMana; }
    public int GetCurrentMana() { return currentMana; }
    public int GetCurrentArmorShield() => currentArmorShield;
    public int GetCurrentMagicShield() => currentMagicShield;
    public float MaxBlockStamina { get { return maxBlockStamina; } }
    public float BlockStamina { get { return blockStamina; } }
    public bool IsBlockOnCooldown { get { return isBlockOnCooldown; } }
    public void TakeMagicDamage(int amount)
    {
        if (isBlocking && !isBlockOnCooldown)
        {
            blockStamina -= blockStaminaDecreasePerHit;
            if (blockStamina <= 0)
            {
                blockStamina = 0;
                isBlockOnCooldown = true;
                blockCooldownTimer = blockCooldownTime;
                isBlocking = false;
                m_animator.SetBool("IdleBlock", false);
                // Có thể thêm hiệu ứng block bị vỡ ở đây
            }
            Debug.Log("Block hit! Stamina: " + blockStamina);
            return;
        }

        int reducedDamage = Mathf.Max(0, amount - currentMagicShield);
        health -= reducedDamage;
        Debug.Log($"Magic shield reduced damage: {amount} -> {reducedDamage}. Health left: {health}");

        if (health <= 0) Die();
        else m_animator.SetTrigger("Hurt");
    }


    public void TakePhysicalDamage(int amount)
    {
        if (isBlocking && !isBlockOnCooldown)
        {
            blockStamina -= blockStaminaDecreasePerHit;
            if (blockStamina <= 0)
            {
                blockStamina = 0;
                isBlockOnCooldown = true;
                blockCooldownTimer = blockCooldownTime;
                isBlocking = false;
                m_animator.SetBool("IdleBlock", false);
                // Có thể thêm hiệu ứng block bị vỡ ở đây
            }
            Debug.Log("Block hit! Stamina: " + blockStamina);
            return;
        }

        int damageAfterArmor = Mathf.Max(0, amount - currentArmorShield);

        health -= damageAfterArmor;
        Debug.Log($"Armor reduced damage: {amount} -> {damageAfterArmor}, remaining health: {health}");
        Debug.Log($"{currentMagicShield} {currentMagicShield}");
        if (health <= 0)
        {
            Die();
        }
        else
        {
            m_animator.SetTrigger("Hurt");
        }
    }
    public void HandleSkillCast(int manaCost)
    {
        if (currentMana >= manaCost)
        {
            currentMana -= manaCost;

            GameObject projectile = Instantiate(skillProjectilePrefab, castPoint.position, Quaternion.identity);

            // Xác định hướng
            Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            projectile.GetComponent<SkillProjectile>().SetDirection(direction);

            // Lật hình nếu nhân vật quay trái
            Vector3 scale = projectile.transform.localScale;
            scale.x = transform.localScale.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            projectile.transform.localScale = scale;

            // Option: animation chưởng
            // m_animator.SetTrigger("Cast");
        }
        else
        {
            Debug.Log("Không đủ mana để chưởng!");
        }
    }
    public void RegenerateMana(int amount)
    {
        currentMana += amount;
        if (currentMana > maxMana)
        {
            currentMana = maxMana;
            return;
        }

        Debug.Log($" Đã hồi {amount} mana. Mana hiện tại: {currentMana}/{maxMana}");
    }

    void Die()
    {
        if (isDead) return;
        // Play death animation or effect here if needed
        isDead = true;
        m_animator.SetBool("noBlood", m_noBlood);
        m_animator.SetTrigger("Death");
    }

    public void DestroyPlayerSelf()
    {
        Destroy(gameObject);
    }

    public void DealDamageToEnemy()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
        }
    }
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    // Animation Events
    // Called in slide animation.
    void AE_SlideDust()
    {
        Vector3 spawnPosition;
        if (m_facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;
        if (m_slideDust != null)
        {
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }

    public void Heal(int amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
    }

    void EnableSwordCollider1()
    {
        Debug.Log(">> EnableSwordCollider CALLED");
        swordCollider1.SetActive(true);
    }

    void EnableSwordCollider2()
    {
        Debug.Log(">> EnableSwordCollider2 CALLED");
        swordCollider2.SetActive(true);
    }

    void EnableSwordCollider3()
    {
        Debug.Log(">> EnableSwordCollider3 CALLED");
        swordCollider3.SetActive(true);
    }

    void DisableSwordCollider1()
    {
        Debug.Log(">> DisableSwordCollider CALLED");
        swordCollider1.SetActive(false);
    }

    void DisableSwordCollider2()
    {
        Debug.Log(">> DisableSwordCollider2 CALLED");
        swordCollider2.SetActive(false);
    }

    void DisableSwordCollider3()
    {
        Debug.Log(">> DisableSwordCollider3 CALLED");
        swordCollider3.SetActive(false);
    }

    void HandleSkillBerserk()
    {
        if (Input.GetKeyDown("r") && !m_rolling && !m_jumping)
        {
            StartCoroutine(SpikeRoutine());
        }
    }
    IEnumerator SpikeRoutine()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Đổi màu sprite thành màu đỏ
        sr.color = new Color(215f / 255f, 92f / 255f, 92f / 255f);

        m_animator.SetTrigger("Attack3");

        // Đợi 0.2 giây để animation bắt đầu
        yield return new WaitForSeconds(0.1f);

        // Dừng lại thêm một chút trước khi bắt đầu flicker
        yield return new WaitForSeconds(0.3f); // Dừng 0.5s trước khi flicker

        // Tạo spike không gắn vào player
        Vector3 spawnOffset = new Vector3(m_facingDirection * 3.5f, 1.5f, 0);
        Vector3 spawnPosition = transform.position + spawnOffset;
        GameObject spike = Instantiate(spikePrefab, spawnPosition, Quaternion.identity);
        Animator sg = spike.GetComponent<Animator>();
        sg.SetBool("isSpike", true);

        // Bắt đầu hiệu ứng nhấp nháy cho sprite và giữ spike hoạt động trong cùng thời gian
        float flickerDuration = 1f; // Thời gian cho spike hoạt động (đồng thời với nhấp nháy)
        float flickerInterval = 0.1f; // Khoảng thời gian giữa mỗi lần đổi màu
        float timeElapsed = 0f;

        // Nhấp nháy màu liên tục trong thời gian spike hoạt động
        while (timeElapsed < flickerDuration)
        {
            sr.color = (sr.color == new Color(215f / 255f, 92f / 255f, 92f / 255f)) ? Color.white : new Color(215f / 255f, 92f / 255f, 92f / 255f);
            timeElapsed += flickerInterval;
            yield return new WaitForSeconds(flickerInterval);
        }

        // Đảm bảo màu sprite trở về trắng mặc định sau khi kết thúc
        sr.color = Color.white;

        // Xử lý spike sau khi hoạt động 1.5 giây
        sg.SetBool("isSpike", false);
        Destroy(spike); // Xóa spike
    }

    // Hàm LateUpdate để cập nhật vị trí của player khi đứng trên MovingGround
    void LateUpdate()
    {
        if (onMovingGround && currentGround != null)
        {
            transform.position += currentGround.DeltaMovement;
        }
    }

    // Hàm OnCollisionEnter2D và OnCollisionExit2D để phát hiện khi player đứng trên MovingGround
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingGround"))
        {
            onMovingGround = true;
            currentGround = collision.gameObject.GetComponent<MovingGround>();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingGround"))
        {
            onMovingGround = false;
            currentGround = null;
        }
    }
    /// <summary>
    /// Lưu toàn bộ chỉ số hiện tại của Player.
    /// </summary>
    public void SaveGame()
    {
        PlayerData data = new PlayerData();
        data.maxHealth = maxHealth;
        data.health = health;
        data.maxArmorShield = maxArmorShield;
        data.currentArmorShield = currentArmorShield;
        data.maxMagicShield = maxMagicShield;
        data.currentMagicShield = currentMagicShield;
        data.maxMana = maxMana;
        data.currentMana = currentMana;
        data.attackDamage = attackDamage;
        data.maxBlockStamina = maxBlockStamina;
        data.blockStamina = blockStamina;
        data.learnedSkills = learnedSkills;
        data.gold = gold;
        data.currentStage = currentStage;
        data.inventory = inventory;

        SaveManager.Save(data);
        Debug.Log("Game Saved!");
        if (autosaveText != null)
        {
            autosaveText.text = "Đã tự động lưu!";
            CancelInvoke(nameof(HideAutosaveText)); // Đảm bảo không bị chồng lệnh
            Invoke(nameof(HideAutosaveText), 2f); // Ẩn sau 2 giây
        }
    }

    /// <summary>
    /// Tải lại toàn bộ chỉ số đã lưu cho Player.
    /// </summary>
    public void LoadGame()
    {
        PlayerData data = SaveManager.Load();
        if (data != null)
        {
            maxHealth = data.maxHealth;
            health = data.health;
            maxArmorShield = data.maxArmorShield;
            currentArmorShield = data.currentArmorShield;
            maxMagicShield = data.maxMagicShield;
            currentMagicShield = data.currentMagicShield;
            maxMana = data.maxMana;
            currentMana = data.currentMana;
            attackDamage = data.attackDamage;
            maxBlockStamina = data.maxBlockStamina;
            blockStamina = data.blockStamina;
            learnedSkills = data.learnedSkills ?? new List<string>();
            gold = data.gold;
            currentStage = data.currentStage;
            inventory = data.inventory ?? new List<string>();

            Debug.Log("Game Loaded!");
        }
        else
        {
            Debug.Log("No save data found!");
        }
    }

    void HideAutosaveText()
    {
        if (autosaveText != null)
            autosaveText.text = "";
    }

    public void PrintSaveData()
    {
        string json = PlayerPrefs.GetString("playerData", "Chưa có save");
        Debug.Log("Nội dung save hiện tại: " + json);
    }
}