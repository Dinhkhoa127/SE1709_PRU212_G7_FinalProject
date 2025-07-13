using UnityEngine;
using System.Collections;

public class HeroKnight : MonoBehaviour {

    [SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_jumpForce = 7.5f;
    [SerializeField] float      m_rollForce = 6.0f;
    [SerializeField] bool       m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_HeroKnight   m_groundSensor;
    private Sensor_HeroKnight   m_wallSensorR1;
    private Sensor_HeroKnight   m_wallSensorR2;
    private Sensor_HeroKnight   m_wallSensorL1;
    private Sensor_HeroKnight   m_wallSensorL2;
    private bool                m_isWallSliding = false;
    private bool                m_grounded = false;
    private bool                m_rolling = false;
    private int                 m_facingDirection = 1;
    private int                 m_currentAttack = 0;
    private float               m_timeSinceAttack = 0.0f;
    private float               m_delayToIdle = 0.0f;
    private float               m_rollDuration = 8.0f / 14.0f;
    private float               m_rollCurrentTime;
    private bool                m_jumping = false; 
    [SerializeField] GameObject spikePrefab;
    private GameObject swordCollider1;
    private GameObject swordCollider2;
    private GameObject swordCollider3;





    // Use this for initialization
    void Start ()
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
    }

    // Update is called once per frame
    void Update ()
    {
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        // Increase timer that checks roll duration
        if(m_rolling)
            m_rollCurrentTime += Time.deltaTime;

        // Disable rolling if timer extends duration
        if(m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
            m_jumping = false;
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");

        // Swap direction of sprite depending on walk direction
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

        // Move
        if (!m_rolling )
            m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);

        // -- Handle Animations --
        //Wall Slide
        m_isWallSliding = (m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);

        //Death
        if (Input.GetKeyDown("e") && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }
            
        //Hurt
        else if (Input.GetKeyDown("q") && !m_rolling)
            m_animator.SetTrigger("Hurt");

        //Attack
        else if(Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;
        }

        // Block
        else if (Input.GetMouseButtonDown(1) && !m_rolling  && !m_jumping)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }

        else if (Input.GetMouseButtonUp(1))
            m_animator.SetBool("IdleBlock", false);

        // Roll
        else if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
        }
            

        //Jump
        else if (Input.GetKeyDown("space") && m_grounded && !m_rolling)
        {
            m_jumping = true;
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
                if(m_delayToIdle < 0)
                    m_animator.SetInteger("AnimState", 0);
        }

        // Skill R hóa berserk increase dame + spike từ ground.
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
            // Set correct arrow spawn position
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }
}
