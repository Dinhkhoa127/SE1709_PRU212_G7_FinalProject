using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Đảm bảo có ở đầu file nếu dùng List
using TMPro;
using Assets.Scripts; // Ở đầu file
using UnityEngine.SceneManagement; // Đặt ở đầu file nếu chưa có

public class PlayerKnight : MonoBehaviour
{
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;
    [SerializeField] public int maxHealth = 5; // Có thể chỉnh trong Play Mode
    [SerializeField] int health = 5;
    
    [Header("Debug - Play Mode Editable")]
    [SerializeField] private int debugMaxHealth = 20; // Có thể chỉnh trong Play Mode
    
    // Base stats để save/load (không bao gồm equipment bonus)
    private int baseMaxHealth;
    private int baseAttackDamage;
    private int baseMaxArmorShield;
    private int baseMaxMagicShield;
    private int baseMaxMana;

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
    public List<ItemData> inventory = new List<ItemData>();
    
    // Equipment System
    [Header("Equipment")]
    public Dictionary<EquipmentType, ItemInfo> equippedItems = new Dictionary<EquipmentType, ItemInfo>();
    
    [Header("Equipment Settings")]
    [Tooltip("Khi trang bị tăng maxHealth, có tự động tăng health hiện tại không?")]
    public bool autoIncreaseCurrentHealth = false;
    
    [Header("Debug Settings")]
    [Tooltip("Hiện debug logs?")]
    public bool enableDebugLogs = true;
    
    // Calculated stats (base + equipment bonuses)
    private int totalAttackDamage;
    private int totalArmor;
    private int totalMagicResist;
    private int totalMaxHealth;
    private int totalMaxMana;
    
    // Debug tracking
    private float lastRecalculateTime = 0f;
    private float lastDebugCommandTime = 0f;
    
    // Equipment UI update flag
    private bool shouldUpdateEquipmentUIOnInventoryOpen = false;
    
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
        
        // Initialize base stats for save/load
        baseMaxHealth = maxHealth;
        baseAttackDamage = attackDamage;
        baseMaxArmorShield = maxArmorShield;
        baseMaxMagicShield = maxMagicShield;
        baseMaxMana = maxMana;
        
        // Initialize equipment system
        InitializeEquipmentSystem();
        RecalculateStats();
    }
    
    // Tự động cập nhật khi thay đổi maxHealth trong Inspector (cả Play Mode)
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            // Đảm bảo values hợp lệ
            if (maxHealth < 1) maxHealth = 1;
            if (attackDamage < 1) attackDamage = 1;
            if (maxArmorShield < 0) maxArmorShield = 0;
            if (maxMagicShield < 0) maxMagicShield = 0;
            if (maxMana < 0) maxMana = 0;
            
            // Cập nhật base values khi thay đổi trong Inspector
            baseMaxHealth = maxHealth;
            baseAttackDamage = attackDamage;
            baseMaxArmorShield = maxArmorShield;
            baseMaxMagicShield = maxMagicShield;
            baseMaxMana = maxMana;
            
            // Recalculate khi thay đổi trong Play Mode
            if (equippedItems != null)
            {
                RecalculateStats();
                Debug.Log($"💡 Stats changed in Play Mode! MaxHP: {totalMaxHealth}, Attack: {totalAttackDamage}, Mana: {totalMaxMana}");
            }
        }
    }
    
    void InitializeEquipmentSystem()
    {
        // Initialize equipped items dictionary
        equippedItems = new Dictionary<EquipmentType, ItemInfo>();
        
        // Initialize all equipment slots as empty
        System.Array equipmentTypes = System.Enum.GetValues(typeof(EquipmentType));
        foreach (EquipmentType equipType in equipmentTypes)
        {
            equippedItems[equipType] = null;
        }
        
        // Initialize total stats to base values
        totalAttackDamage = baseAttackDamage;
        totalArmor = baseMaxArmorShield;
        totalMagicResist = baseMaxMagicShield;
        totalMaxHealth = baseMaxHealth;
        totalMaxMana = baseMaxMana;
    }
    
    public void EquipItem(ItemInfo item)
    {
        if (item == null || item.itemType != ItemType.Equipment) return;
        
        // Unequip current item if any
        if (equippedItems[item.equipmentType] != null)
        {
            UnequipItem(item.equipmentType);
        }
        
        // Equip new item
        equippedItems[item.equipmentType] = item;
        
        // DON'T remove from inventory - keep it there for UI display
        // Equipment items stay in inventory even when equipped
        
        // Recalculate stats
        RecalculateStats();
        
        // Update equipment UI
        UpdateEquipmentUI();
        
        // Auto-save after equipping item
        SaveGame();
    }
    
    public void UnequipItem(EquipmentType equipType)
    {
        if (equippedItems[equipType] == null) return;
        
        ItemInfo item = equippedItems[equipType];
        
        // Equipment items are already in inventory, no need to add back
        
        // Remove from equipped
        equippedItems[equipType] = null;
        
        // Recalculate stats
        RecalculateStats();
        
        // Update equipment UI
        UpdateEquipmentUI();
        
        // Auto-save after unequipping item
        SaveGame();
    }
    
    void RecalculateStats()
    {
        // Lưu old maxHealth để tính toán
        int oldMaxHealth = totalMaxHealth;
        
        // Reset to base stats
        totalAttackDamage = baseAttackDamage;
        totalArmor = baseMaxArmorShield;
        totalMagicResist = baseMaxMagicShield;
        totalMaxHealth = baseMaxHealth;
        totalMaxMana = baseMaxMana;
        
        // Add equipment bonuses
        foreach (var equippedItem in equippedItems.Values)
        {
            if (equippedItem != null)
            {
                totalAttackDamage += equippedItem.attackBonus;
                totalArmor += equippedItem.armorBonus;
                totalMagicResist += equippedItem.magicResistBonus;
                totalMaxHealth += equippedItem.healthBonus;
                totalMaxMana += equippedItem.manaBonus;
            }
        }
        
        // CẬP NHẬT maxHealth field trong Inspector để đồng bộ
        maxHealth = totalMaxHealth;
        
        // Update current shields to new calculated values
        currentArmorShield = totalArmor;
        currentMagicShield = totalMagicResist;
        
        // Handle health increase option
        if (autoIncreaseCurrentHealth && oldMaxHealth > 0 && totalMaxHealth > oldMaxHealth)
        {
            // Tăng current health theo tỷ lệ equipment bonus
            int healthIncrease = totalMaxHealth - oldMaxHealth;
            health += healthIncrease;
            Debug.Log($"Auto-increased current health by {healthIncrease} due to equipment bonus");
        }
        
        // Ensure current health/mana don't exceed new max values
        if (health > totalMaxHealth)
            health = totalMaxHealth;
        if (currentMana > totalMaxMana)
            currentMana = totalMaxMana;
        
        // Cập nhật Inspector values để đồng bộ với total values
        maxHealth = totalMaxHealth;
        attackDamage = totalAttackDamage;
        maxArmorShield = totalArmor;
        maxMagicShield = totalMagicResist;
        maxMana = totalMaxMana;
        
        // Force update all UI elements that depend on health/mana
        UpdateAllHealthUI();
        
        // Debug log khi có thay đổi
        if (enableDebugLogs)
        {
            Debug.Log($"Stats recalculated - MaxHP: {totalMaxHealth}, HP: {health}");
        }
    }
    
    void UpdateAllHealthUI()
    {
        // Update main health bar - đơn giản và trực tiếp
        var healthBarUI = FindObjectOfType<PlayerHealthBarUI>();
        if (healthBarUI != null)
        {
            healthBarUI.UpdateSliderValues();
        }
        
        // Update character stats UI in inventory
        var characterStatsUI = FindObjectOfType<CharacterStatsUI>();
        if (characterStatsUI != null)
        {
            characterStatsUI.UpdateCharacterStats();
        }
        
        Debug.Log($"Health UI updated - MaxHP: {totalMaxHealth}, HP: {health}");
    }
    
    void UpdateEquipmentUI()
    {
        // Update equipment slots UI
        var equipmentUI = FindObjectOfType<EquipmentSlotsUI>();
        if (equipmentUI != null)
        {
            equipmentUI.UpdateEquipmentDisplay();
        }
        
        // Also update inventory UI to refresh equipped items display
        var inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.UpdateUI();
        }
        
        // Update individual equipment slots directly as backup
        var equipmentSlots = FindObjectsOfType<EquipmentSlot>();
        foreach (var slot in equipmentSlots)
        {
            slot.RefreshPlayerReference();
            var equippedItem = GetEquippedItem(slot.allowedType);
            if (equippedItem != null)
            {
                slot.EquipItem(equippedItem);
            }
            else
            {
                slot.ClearSlot();
            }
        }
    }
    
    // Updated getters to return total stats (base + equipment bonuses)
    public int GetAttackDamage() { return totalAttackDamage; }
    public int GetBaseAttackDamage() { return baseAttackDamage; }
    public int GetCurrentArmorShield() => totalArmor;
    public int GetBaseArmorShield() => baseMaxArmorShield;
    public int GetCurrentMagicShield() => totalMagicResist;
    public int GetBaseMagicShield() => baseMaxMagicShield;
    
    // Health getters - phân biệt rõ ràng
    public int GetMaxHealth() { return totalMaxHealth; } // Tổng maxHealth (base + equipment)
    public int GetBaseMaxHealth() { return baseMaxHealth; } // MaxHealth gốc (không có equipment)
    public int GetCurrentHealth() { return health; } // Máu hiện tại
    public int GetHealth() { return health; } // Backward compatibility
    
    // Mana getters
    public int GetMaxMana() { return totalMaxMana; }
    public int GetBaseMana() { return baseMaxMana; }
    public int GetCurrentMana() { return currentMana; }
    
    // Stamina getters
    public float MaxBlockStamina { get { return maxBlockStamina; } }
    public float BlockStamina { get { return blockStamina; } }
    public bool IsBlockOnCooldown { get { return isBlockOnCooldown; } }
    
    public ItemInfo GetEquippedItem(EquipmentType equipType)
    {
        if (equippedItems.ContainsKey(equipType))
            return equippedItems[equipType];
        return null;
    }

    void Update()
    {
        if (InventoryManager.IsInventoryOpen) return; // Không xử lý input khi inventory mở
        if (isDead) return;
        HandleTimers();
        HandleGroundCheck();
        HandleMove();
        HandleWallSlide();
        HandleRoll();
        HandleJump();
        HandleRunIdle();
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(1); // Nhấn H để hồi 1 máu
        }
       // KHÓA ĐÒN ĐÁNH VÀ KỸ NĂNG Ở MAP ĐẶC BIỆT
        if (!IsAttackLockedScene())
        {
            HandleAttack();
            HandleBlock();
            HandleSkillBerserk();
            if (Input.GetKeyDown(KeyCode.Q))
            {
                HandleSkillCast(manaCost);
            }
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
        
        // Test sử dụng items với phím số
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseItem("Health Potion"); // Phím số 1 để test hồi máu
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseItem("Mana Potion"); // Phím số 2 để test hồi mana
        }
        
        // Test health stats with G key
        if (Input.GetKeyDown(KeyCode.G))
        {
            PrintHealthStats();
        }
        
        // Debug commands với cooldown
        if (Time.time - lastDebugCommandTime > 0.3f) // Cooldown 0.3 giây
        {
            // Debug: Increase maxHealth with + key
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus))
            {
                DebugIncreaseMaxHealth(5);
                lastDebugCommandTime = Time.time;
            }
            
            // Debug: Decrease maxHealth with - key
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.Underscore))
            {
                DebugDecreaseMaxHealth(5);
                lastDebugCommandTime = Time.time;
            }
            
            // Debug: Apply debugMaxHealth with U key
            if (Input.GetKeyDown(KeyCode.U))
            {
                ApplyDebugMaxHealth();
                lastDebugCommandTime = Time.time;
            }
        }
    }

    public void RestoreFullMana() { currentMana = totalMaxMana; }
    public void RestoreFullStamina() { blockStamina = maxBlockStamina; }
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
            AudioController.instance.PlayAttackSound();
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
            AudioController.instance.PlayBlockSound();
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
            AudioController.instance.PlayRollSound();
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
            AudioController.instance.PlayJumpSound();
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
    // Removed duplicate getters - using the updated ones above
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
        else
        {
            m_animator.SetTrigger("Hurt");
            AudioController.instance.PlayHurtSound();
        }
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
            AudioController.instance.PlayHurtSound();
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
            AudioController.instance.PlaySkillCastSound();
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
        if (currentMana > totalMaxMana)
        {
            currentMana = totalMaxMana;
            return;
        }
        Debug.Log($" Đã hồi {amount} mana. Mana hiện tại: {currentMana}/{totalMaxMana}");
    }

    void Die()
    {
        if (isDead) return;
        // Play death animation or effect here if needed
        isDead = true;
        m_animator.SetBool("noBlood", m_noBlood);
        m_animator.SetTrigger("Death");
        AudioController.instance.PlayDeathSound();

        
        // Thêm dòng này
        GameManager.Instance.OnPlayerDied();

    }

    public void DestroyPlayerSelf()
    {
        Destroy(gameObject);
    }

    public void DealDamageToEnemy()
    {
        //Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        //foreach (Collider2D enemy in hitEnemies)
        //{
        //    enemy.GetComponent<Enemy>().TakeDamage(totalAttackDamage);
        //    AudioController.instance.PlayEnemyTakeDame();
        //}
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            // Original code for Enemy type
            var enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.TakeDamage(totalAttackDamage);
                AudioController.instance.PlayEnemyTakeDame();
                continue;
            }

            // Additional code for RockEnemy and other IDamageable entities
            var damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(totalAttackDamage);
                AudioController.instance.PlayEnemyTakeDame();
            }
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
        AudioController.instance.PlayHealSound();
        health += amount;
        if (health > totalMaxHealth) health = totalMaxHealth;
        
        // Force update UI after healing
        UpdateAllHealthUI();
        
        Debug.Log($"Healed {amount} HP. Current: {health}/{totalMaxHealth}");
    }

    public void AddItem(string itemName, int amount)
    {
        var item = inventory.Find(i => i.itemName == itemName);
        if (item != null)
            item.quantity += amount;
        else
            inventory.Add(new ItemData(itemName, amount));
            
        Debug.Log($"Added {amount}x {itemName} to inventory");
    }

    public bool RemoveItem(string itemName, int amount)
    {
        var item = inventory.Find(i => i.itemName == itemName);
        if (item != null && item.quantity >= amount)
        {
            item.quantity -= amount;
            if (item.quantity == 0)
                inventory.Remove(item);
            return true;
        }
        return false;
    }

    public int GetItemQuantity(string itemName)
    {
        var item = inventory.Find(i => i.itemName == itemName);
        return item != null ? item.quantity : 0;
    }

    // Phương thức sử dụng item với hiệu ứng thật
    public bool UseItem(ItemInfo itemInfo)
    {
        if (GetItemQuantity(itemInfo.itemName) <= 0) return false;
        
        // Áp dụng hiệu ứng
        switch (itemInfo.itemType)
        {
            case ItemType.HealthPotion:
                if (health < totalMaxHealth)
                {
                    Heal(itemInfo.effectValue);
                    RemoveItem(itemInfo.itemName, 1);
                    Debug.Log($"Đã sử dụng {itemInfo.itemName}, hồi {itemInfo.effectValue} HP");
                    
                    // Cập nhật inventory UI ngay lập tức
                    UpdateInventoryUI();
                    
                    // Lưu game sau khi sử dụng item
                    SaveGame();
                    return true;
                }
                break;
                
            case ItemType.ManaPotion:
                if (currentMana < totalMaxMana)
                {
                    AudioController.instance.PlayHealSound();
                    RegenerateMana(itemInfo.effectValue);
                    RemoveItem(itemInfo.itemName, 1);
                    Debug.Log($"Đã sử dụng {itemInfo.itemName}, hồi {itemInfo.effectValue} MP");
                    
                    // Cập nhật inventory UI ngay lập tức
                    UpdateInventoryUI();
                    
                    // Lưu game sau khi sử dụng item
                    SaveGame();
                    return true;
                }
                break;
        }
        
        return false; // Không thể sử dụng (full health/mana hoặc không hợp lệ)
    }

    // Overload để sử dụng với tên item (tìm ItemInfo từ ItemManager global)
    public bool UseItem(string itemName)
    {
        // Tìm ItemInfo từ ItemManager global
        if (ItemManager.Instance != null)
        {
            var itemInfo = ItemManager.Instance.GetItemInfo(itemName);
            if (itemInfo != null)
            {
                return UseItem(itemInfo);
            }
        }
        
        Debug.Log($"Không tìm thấy thông tin cho item: {itemName}");
        return false;
    }

    // Cập nhật inventory UI khi có thay đổi
    void UpdateInventoryUI()
    {
        var inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.UpdateUI();
        }
        
        // Cập nhật QuickSlots UI
        var quickSlotsUI = FindObjectOfType<QuickSlotsUI>();
        if (quickSlotsUI != null)
        {
            quickSlotsUI.ForceUpdate();
        }
    }
    
    // Public method để force refresh inventory UI
    public void ForceUpdateInventoryUI()
    {
        UpdateInventoryUI();
    }
    
    // Public method để force refresh tất cả UI liên quan đến equipment
    public void ForceUpdateAllEquipmentUI()
    {
        // Update equipment UI
        UpdateEquipmentUI();
        
        // Update inventory UI
        UpdateInventoryUI();
        
        // Update character stats
        var characterStatsUI = FindObjectOfType<CharacterStatsUI>();
        if (characterStatsUI != null)
        {
            characterStatsUI.UpdateCharacterStats();
        }
        
        // Force update equipment slots specifically
        StartCoroutine(ForceUpdateEquipmentSlotsUI());
        
        // Start immediate update coroutine as backup
        StartCoroutine(ImmediateUIUpdate());
    }
    
    // Schedule equipment UI update for next inventory open
    public void ScheduleEquipmentUIUpdate()
    {
        shouldUpdateEquipmentUIOnInventoryOpen = true;
    }
    
    // Method called when inventory is opened to check if equipment UI needs update
    public void OnInventoryOpened()
    {
        // Always force update equipment UI when inventory is opened - no flag dependency
        StartCoroutine(ForceUpdateEquipmentSlotsUI());
        
        // Reset the flag for future use
        shouldUpdateEquipmentUIOnInventoryOpen = false;
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
        // Save base stats (not total stats)
        data.maxHealth = baseMaxHealth;
        data.health = health;
        data.maxArmorShield = baseMaxArmorShield;
        data.currentArmorShield = currentArmorShield;
        data.maxMagicShield = baseMaxMagicShield;
        data.currentMagicShield = currentMagicShield;
        data.maxMana = baseMaxMana;
        data.currentMana = currentMana;
        data.attackDamage = baseAttackDamage;
        data.maxBlockStamina = maxBlockStamina;
        data.blockStamina = blockStamina;
        data.learnedSkills = learnedSkills;
        data.gold = gold;
        data.currentStage = currentStage;
        data.inventory = inventory;
        
        // Save equipped items
        data.equippedItems = new List<EquippedItemData>();
        foreach (var equippedItem in equippedItems)
        {
            if (equippedItem.Value != null)
            {
                var equippedData = new EquippedItemData(equippedItem.Key, equippedItem.Value);
                data.equippedItems.Add(equippedData);
            }
        }

        SaveManager.Save(data);
        
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
            // Load base stats
            baseMaxHealth = data.maxHealth;
            health = data.health;
            baseMaxArmorShield = data.maxArmorShield;
            currentArmorShield = data.currentArmorShield;
            baseMaxMagicShield = data.maxMagicShield;
            currentMagicShield = data.currentMagicShield;
            baseMaxMana = data.maxMana;
            currentMana = data.currentMana;
            baseAttackDamage = data.attackDamage;
            maxBlockStamina = data.maxBlockStamina;
            blockStamina = data.blockStamina;
            learnedSkills = data.learnedSkills ?? new List<string>();
            currentStage = data.currentStage;
            
            // Smart inventory and gold loading
            int currentInventoryCount = inventory.Count;
            int currentGold = gold;
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            if (data.inventory != null && data.inventory.Count > 0)
            {
                if (currentScene == "MapRest" || currentInventoryCount == 0 || data.inventory.Count > currentInventoryCount)
                {
                    inventory = data.inventory;
                }
            }
            
            if (currentScene == "MapRest" || data.gold > currentGold || currentGold == 0)
            {
                gold = data.gold;
            }
            
            // Load equipped items
            LoadEquippedItems(data.equippedItems);
            
            // Recalculate stats after loading equipment
            RecalculateStats();
            
            // Schedule equipment UI update for when inventory is opened
            ScheduleEquipmentUIUpdate();
        }
    }
    
    public void LoadEquippedItems(List<EquippedItemData> savedEquipment)
    {
        // Clear current equipped items
        InitializeEquipmentSystem();
        
        if (savedEquipment == null || savedEquipment.Count == 0)
        {
            UpdateEquipmentUI();
            return;
        }
        
        foreach (var savedItem in savedEquipment)
        {
            ItemInfo itemToEquip = null;
            
            // Find original ItemInfo from ItemManager
            if (ItemManager.Instance != null)
            {
                itemToEquip = ItemManager.Instance.GetItemInfo(savedItem.itemName);
            }
            
            // Find from EquipmentShopItems if ItemManager failed
            if (itemToEquip == null)
            {
                var equipmentShopItems = FindObjectOfType<EquipmentShopManager>()?.equipmentShopItems;
                if (equipmentShopItems != null)
                {
                    foreach (var shopItem in equipmentShopItems.GetEquipmentItems())
                    {
                        if (shopItem.itemName == savedItem.itemName)
                        {
                            itemToEquip = shopItem;
                            break;
                        }
                    }
                }
            }
            
            // Search all ItemInfo assets in Resources
            if (itemToEquip == null)
            {
                var allItems = Resources.LoadAll<ItemInfo>("");
                foreach (var item in allItems)
                {
                    if (item.itemName == savedItem.itemName)
                    {
                        itemToEquip = item;
                        break;
                    }
                }
            }
            
            // Create new ItemInfo with saved data (no sprite)
            if (itemToEquip == null)
            {
                itemToEquip = ScriptableObject.CreateInstance<ItemInfo>();
                itemToEquip.itemName = savedItem.itemName;
                itemToEquip.itemType = ItemType.Equipment;
                itemToEquip.equipmentType = savedItem.equipmentType;
                itemToEquip.attackBonus = savedItem.attackBonus;
                itemToEquip.armorBonus = savedItem.armorBonus;
                itemToEquip.magicResistBonus = savedItem.magicResistBonus;
                itemToEquip.healthBonus = savedItem.healthBonus;
                itemToEquip.manaBonus = savedItem.manaBonus;
            }
            
            // Equip the item
            equippedItems[savedItem.equipmentType] = itemToEquip;
            
            // CRITICAL: Ensure equipped items are in inventory for UI display
            if (GetItemQuantity(savedItem.itemName) == 0)
            {
                AddItem(savedItem.itemName, 1);
            }
        }
        
        // Force update all equipment UI components
        StartCoroutine(ForceUpdateEquipmentSlotsUI());
    }

    void HideAutosaveText()
    {
        if (autosaveText != null)
            autosaveText.text = "";
    }
    
    // Immediate UI update - no delays
    System.Collections.IEnumerator ImmediateUIUpdate()
    {
        UpdateEquipmentUI();
        
        var inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.UpdateUI();
        }
        
        var characterStatsUI = FindObjectOfType<CharacterStatsUI>();
        if (characterStatsUI != null)
        {
            characterStatsUI.UpdateCharacterStats();
        }
        
        yield break; // End immediately
    }

    // Force update equipment slots UI specifically - immediate update
    System.Collections.IEnumerator ForceUpdateEquipmentSlotsUI()
    {
        // Find EquipmentSlotsUI immediately
        EquipmentSlotsUI equipmentSlotsUI = FindObjectOfType<EquipmentSlotsUI>();
        
        if (equipmentSlotsUI != null)
        {
            equipmentSlotsUI.UpdateEquipmentDisplay();
        }
        
        // Also direct update each equipment slot as backup
        var equipmentSlots = FindObjectsOfType<EquipmentSlot>();
        foreach (var slot in equipmentSlots)
        {
            slot.RefreshPlayerReference();
            var equippedItem = GetEquippedItem(slot.allowedType);
            if (equippedItem != null)
            {
                slot.EquipItem(equippedItem);
            }
            else
            {
                slot.ClearSlot();
            }
        }
        
        yield break; // End immediately
    }

    public void PrintSaveData()
    {
        string json = PlayerPrefs.GetString("playerData", "Chưa có save");
        Debug.Log("Nội dung save hiện tại: " + json);
    }
    
    public void PrintHealthStats()
    {
        Debug.Log("=== PLAYER STATS DEBUG ===");
        Debug.Log($"Base MaxHealth: {baseMaxHealth} -> Total: {totalMaxHealth}");
        Debug.Log($"Base Attack: {baseAttackDamage} -> Total: {totalAttackDamage}");
        Debug.Log($"Base Armor: {baseMaxArmorShield} -> Total: {totalArmor}");
        Debug.Log($"Base Magic Resist: {baseMaxMagicShield} -> Total: {totalMagicResist}");
        Debug.Log($"Base MaxMana: {baseMaxMana} -> Total: {totalMaxMana}");
        Debug.Log($"Current Health: {health}/{totalMaxHealth} ({(float)health / totalMaxHealth * 100:F1}%)");
        Debug.Log($"Current Mana: {currentMana}/{totalMaxMana} ({(float)currentMana / totalMaxMana * 100:F1}%)");
        Debug.Log("==============================");
    }
    
    [ContextMenu("Debug: Increase MaxHealth")]
    public void DebugIncreaseMaxHealth(int amount = 5)
    {
        baseMaxHealth += amount;
        maxHealth = baseMaxHealth; // Sync Inspector
        RecalculateStats();
        // Debug log đã được handled trong RecalculateStats()
    }
    
    [ContextMenu("Debug: Decrease MaxHealth")]
    public void DebugDecreaseMaxHealth(int amount = 5)
    {
        baseMaxHealth = Mathf.Max(1, baseMaxHealth - amount); // Không để baseMaxHealth < 1
        maxHealth = baseMaxHealth; // Sync Inspector
        RecalculateStats();
        // Debug log đã được handled trong RecalculateStats()
    }
    
    [ContextMenu("Debug: Set MaxHealth to 100")]
    public void DebugSetMaxHealth100()
    {
        baseMaxHealth = 100;
        maxHealth = baseMaxHealth; // Sync Inspector
        RecalculateStats();
        Debug.Log($"DEBUG: Set base maxHealth to 100. Total: {totalMaxHealth}");
    }
    
    [ContextMenu("Debug: Apply Debug MaxHealth")]
    public void ApplyDebugMaxHealth()
    {
        if (debugMaxHealth > 0)
        {
            baseMaxHealth = debugMaxHealth;
            maxHealth = baseMaxHealth; // Sync Inspector
            RecalculateStats();
            Debug.Log($"DEBUG: Applied debugMaxHealth {debugMaxHealth} to baseMaxHealth. Total: {totalMaxHealth}");
        }
    }

    private bool IsAttackLockedScene()
    {
        // Đổi "MapRest" thành đúng tên scene bạn muốn khóa
        return SceneManager.GetActiveScene().name == "MapRest";
    }

}