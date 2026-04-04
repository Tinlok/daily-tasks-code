using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 增强版2D玩家控制器 - 包含冲刺和攀爬机制
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(GroundCheck))]
public class PlayerController2D_Enhanced : MonoBehaviour
{
    #region 组件引用
    private Rigidbody2D rb;
    private Animator animator;
    private GroundCheck groundCheck;
    private PlayerState playerState;
    private PlayerStats playerStats;
    private PlayerInput playerInput;
    #endregion

    #region 移动参数
    [Header("基础移动")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 25f;
    [SerializeField] private float groundFriction = 0.9f;
    [SerializeField] private float airFriction = 0.98f;
    [SerializeField] private float maxFallSpeed = 15f;
    [SerializeField] private float gravityScale = 1f;

    [Header("跳跃参数")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private int maxJumps = 1;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float jumpHoldMultiplier = 1.2f;
    
    [Header("冲刺参数")]
    [SerializeField] private float sprintSpeedMultiplier = 1.6f;
    [SerializeField] private float sprintAcceleration = 35f;
    [SerializeField] private float sprintDeceleration = 40f;
    [SerializeField] private float sprintStaminaCost = 20f;
    [SerializeField] private float sprintStaminaRegen = 10f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenDelay = 1f;
    
    [Header("攀爬参数")]
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float climbStaminaCost = 15f;
    [SerializeField] private float climbDistance = 0.5f;
    [SerializeField] private LayerMask climbableLayers;
    [SerializeField] private float climbRaycastDistance = 0.2f;
    #endregion

    #region 移动状态
    private Vector2 moveInput;
    private Vector2 velocity;
    private bool isFacingRight = true;
    private float currentStamina;
    private float staminaRegenTimer;
    private bool isSprinting;
    private bool isClimbing;
    
    // 跳跃相关
    private bool jumpRequested = false;
    private float jumpRequestTimer = 0f;
    private int jumpCount = 0;
    private float coyoteTimer = 0f;
    private bool isJumpHold = false;
    
    // 移动手感相关
    private float targetSpeed = 0f;
    private float currentSpeed = 0f;
    private float velocityX = 0f;
    private float velocityY = 0f;
    #endregion

    #region 事件
    public System.Action onJump;
    public System.Action onLand;
    public System.Action onStartSprint;
    public System.Action onEndSprint;
    public System.Action onStartClimb;
    public System.Action onEndClimb;
    public System.Action onStaminaChanged;
    #endregion

    #region Unity生命周期
    private void Awake()
    {
        // 获取组件
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        groundCheck = GetComponent<GroundCheck>();
        
        // 初始化状态
        playerState = new PlayerState();
        playerStats = GetComponent<PlayerStats>();
        playerInput = GetComponent<PlayerInput>();
        
        // 初始化耐力
        currentStamina = maxStamina;
        
        // 设置刚体属性
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = gravityScale;
    }

    private void Update()
    {
        // 处理输入
        HandleInput();
        
        // 更新计时器
        UpdateTimers();
        
        // 检测攀爬
        CheckClimb();
        
        // 更新动画
        UpdateAnimation();
        
        // 更新状态
        UpdatePlayerState();
    }

    private void FixedUpdate()
    {
        // 处理移动
        HandleMovement();
        
        // 处理跳跃
        HandleJump();
        
        // 处理冲刺
        HandleSprint();
        
        // 处理攀爬
        HandleClimb();
        
        // 应用物理
        ApplyPhysics();
    }
    #endregion

    #region 输入处理
    private void HandleInput()
    {
        // 获取移动输入
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        // 跳跃输入
        if (Input.GetButtonDown("Jump"))
        {
            RequestJump();
        }
        
        if (Input.GetButton("Jump"))
        {
            isJumpHold = true;
        }
        else
        {
            isJumpHold = false;
        }
        
        // 冲刺输入
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            TryStartSprint();
        }
        else
        {
            TryEndSprint();
        }
        
        // 攀爬输入（在攀爬状态下）
        if (isClimbing)
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
        
        // 朝向处理
        if (moveInput.x > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && isFacingRight)
        {
            Flip();
        }
    }
    #endregion

    #region 移动处理
    private void HandleMovement()
    {
        // 计算目标速度
        float baseSpeed = isSprinting ? moveSpeed * sprintSpeedMultiplier : moveSpeed;
        targetSpeed = moveInput.x * baseSpeed;
        
        // 计算加速度和减速度
        float currentAcceleration = isSprinting ? sprintAcceleration : acceleration;
        float currentDeceleration = isSprinting ? sprintDeceleration : deceleration;
        
        // 应用加速度和减速度
        if (Mathf.Abs(targetSpeed) > 0.1f)
        {
            // 加速
            velocityX = Mathf.Lerp(velocityX, targetSpeed, currentAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            // 减速
            velocityX = Mathf.Lerp(velocityX, 0, currentDeceleration * Time.fixedDeltaTime);
        }
        
        // 应用摩擦力
        if (groundCheck.IsGrounded)
        {
            velocityX *= groundFriction;
        }
        else
        {
            velocityX *= airFriction;
        }
        
        // 更新速度
        currentSpeed = Mathf.Abs(velocityX);
    }
    #endregion

    #region 跳跃处理
    private void RequestJump()
    {
        jumpRequested = true;
        jumpRequestTimer = jumpBufferTime;
    }

    private void HandleJump()
    {
        // 处理跳跃缓冲
        if (jumpRequested)
        {
            jumpRequestTimer -= Time.fixedDeltaTime;
            
            if (jumpRequestTimer <= 0f)
            {
                jumpRequested = false;
            }
        }
        
        // 处理 Coyote Time
        if (!groundCheck.IsGrounded)
        {
            coyoteTimer -= Time.fixedDeltaTime;
        }
        else
        {
            coyoteTimer = coyoteTime;
            jumpCount = 0;
        }
        
        // 执行跳跃
        if (jumpRequested && CanJump())
        {
            ExecuteJump();
            jumpRequested = false;
        }
        
        // 跳跃高度控制
        if (isJumpHold && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpHoldMultiplier);
        }
    }

    private bool CanJump()
    {
        // 在地面上或在 Coyote Time 内
        bool canJumpGrounded = groundCheck.IsGrounded || coyoteTimer > 0f;
        
        // 空中跳跃
        bool canJumpAirborne = jumpCount < maxJumps;
        
        return canJumpGrounded || canJumpAirborne;
    }

    private void ExecuteJump()
    {
        float jumpForceActual = jumpForce;
        
        // 跳跃高度控制
        if (!isJumpHold)
        {
            jumpForceActual *= jumpCutMultiplier;
        }
        
        // 应用跳跃力
        rb.velocity = new Vector2(rb.velocity.x, jumpForceActual);
        
        // 更新跳跃计数
        jumpCount++;
        
        // 触发事件
        onJump?.Invoke();
        
        // 更新动画
        animator.SetTrigger("Jump");
    }
    #endregion

    #region 冲刺处理
    private void TryStartSprint()
    {
        if (CanSprint() && !isClimbing)
        {
            isSprinting = true;
            onStartSprint?.Invoke();
        }
    }

    private void TryEndSprint()
    {
        if (isSprinting)
        {
            isSprinting = false;
            onEndSprint?.Invoke();
        }
    }

    private void HandleSprint()
    {
        if (isSprinting)
        {
            // 消耗耐力
            currentStamina -= sprintStaminaCost * Time.fixedDeltaTime;
            
            // 检查耐力是否耗尽
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                TryEndSprint();
            }
        }
        else
        {
            // 恢复耐力
            if (staminaRegenTimer <= 0f)
            {
                currentStamina = Mathf.Min(currentStamina + sprintStaminaRegen * Time.fixedDeltaTime, maxStamina);
            }
        }
        
        // 更新耐力恢复计时器
        if (!isSprinting && currentStamina < maxStamina)
        {
            staminaRegenTimer -= Time.fixedDeltaTime;
        }
        
        // 触发耐力变化事件
        onStaminaChanged?.Invoke();
    }

    private bool CanSprint()
    {
        return currentStamina > sprintStaminaCost && currentSpeed > 0.1f;
    }
    #endregion

    #region 攀爬处理
    private void CheckClimb()
    {
        // 检测是否可以攀爬
        if (moveInput.y != 0f && IsNearClimbableSurface())
        {
            if (!isClimbing && CanClimb())
            {
                StartClimb();
            }
        }
        else if (isClimbing)
        {
            EndClimb();
        }
    }

    private bool IsNearClimbableSurface()
    {
        // 检测角色周围的墙面
        Vector2 checkPosition = transform.position;
        Vector2 checkDirection = new Vector2(moveInput.x, 0f).normalized;
        
        RaycastHit2D hit = Physics2D.Raycast(
            checkPosition,
            checkDirection,
            climbRaycastDistance,
            climbableLayers
        );
        
        return hit.collider != null;
    }

    private bool CanClimb()
    {
        return currentStamina > climbStaminaCost && !groundCheck.IsGrounded;
    }

    private void StartClimb()
    {
        isClimbing = true;
        
        // 冻结垂直速度
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.gravityScale = 0f;
        
        // 触发事件
        onStartClimb?.Invoke();
    }

    private void EndClimb()
    {
        isClimbing = false;
        
        // 恢复重力
        rb.gravityScale = gravityScale;
        
        // 触发事件
        onEndClimb?.Invoke();
    }

    private void HandleClimb()
    {
        if (isClimbing)
        {
            // 消耗耐力
            currentStamina -= climbStaminaCost * Time.fixedDeltaTime;
            
            // 检查耐力是否耗尽
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                EndClimb();
                return;
            }
            
            // 应用攀爬速度
            Vector2 climbVelocity = new Vector2(0f, moveInput.y * climbSpeed);
            rb.velocity = climbVelocity;
            
            // 水平移动
            if (moveInput.x != 0f)
            {
                float climbMoveSpeed = moveSpeed * 0.5f; // 攀爬时水平移动减慢
                rb.velocity = new Vector2(moveInput.x * climbMoveSpeed, climbVelocity.y);
            }
        }
    }
    #endregion

    #region 物理处理
    private void ApplyPhysics()
    {
        // 更新速度
        velocity = new Vector2(velocityX, rb.velocity.y);
        
        // 应用重力
        if (!isClimbing)
        {
            velocity.y -= gravityScale * Time.fixedDeltaTime * 9.81f;
        }
        
        // 限制下落速度
        velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
        
        // 应用速度
        rb.velocity = velocity;
        
        // 更新状态
        UpdatePlayerState();
    }
    #endregion

    #region 状态更新
    private void UpdateTimers()
    {
        // 更新耐力恢复计时器
        if (staminaRegenTimer > 0f)
        {
            staminaRegenTimer -= Time.fixedDeltaTime;
        }
    }

    private void UpdatePlayerState()
    {
        // 更新玩家状态
        playerState.isGrounded = groundCheck.IsGrounded;
        playerState.isMoving = currentSpeed > 0.1f;
        playerState.isSprinting = isSprinting;
        playerState.isClimbing = isClimbing;
        playerState.isJumping = !groundCheck.IsGrounded && rb.velocity.y > 0f;
        playerState.isFalling = !groundCheck.IsGrounded && rb.velocity.y < 0f;
        playerState.facingDirection = isFacingRight ? 1 : -1;
        playerState.speed = currentSpeed;
        playerState.stamina = currentStamina;
        playerState.maxStamina = maxStamina;
    }

    private void UpdateAnimation()
    {
        // 设置动画参数
        animator.SetBool("IsGrounded", groundCheck.IsGrounded);
        animator.SetBool("IsMoving", currentSpeed > 0.1f);
        animator.SetBool("IsSprinting", isSprinting);
        animator.SetBool("IsClimbing", isClimbing);
        animator.SetFloat("MoveSpeed", currentSpeed);
        animator.SetFloat("VerticalVelocity", rb.velocity.y);
        animator.SetInteger("FacingDirection", isFacingRight ? 1 : -1);
    }
    #endregion

    #region 工具方法
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        
        // 触发翻转事件
        onLand?.Invoke();
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 获取当前玩家状态
    /// </summary>
    public PlayerState GetPlayerState()
    {
        return playerState;
    }

    /// <summary>
    /// 获取当前耐力值
    /// </summary>
    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    /// <summary>
    /// 恢复耐力
    /// </summary>
    public void RestoreStamina(float amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        onStaminaChanged?.Invoke();
    }

    /// <summary>
    /// 消耗耐力
    /// </summary>
    public void ConsumeStamina(float amount)
    {
        currentStamina = Mathf.Max(currentStamina - amount, 0f);
        onStaminaChanged?.Invoke();
    }

    /// <summary>
    /// 设置耐力值
    /// </summary>
    public void SetStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0f, maxStamina);
        onStaminaChanged?.Invoke();
    }

    /// <summary>
    /// 是否可以冲刺
    /// </summary>
    public bool CanSprint()
    {
        return this.CanSprint();
    }

    /// <summary>
    /// 是否可以攀爬
    /// </summary>
    public bool CanClimb()
    {
        return this.CanClimb();
    }

    /// <summary>
    /// 是否可以跳跃
    /// </summary>
    public bool CanJump()
    {
        return this.CanJump();
    }
    #endregion
}

/// <summary>
/// 玩家状态类
/// </summary>
[System.Serializable]
public class PlayerState
{
    public bool isGrounded;
    public bool isMoving;
    public bool isSprinting;
    public bool isClimbing;
    public bool isJumping;
    public bool isFalling;
    public int facingDirection;
    public float speed;
    public float stamina;
    public float maxStamina;
}

/// <summary>
/// 玩家输入类
/// </summary>
public class PlayerInput : MonoBehaviour
{
    private Vector2 moveInput;
    
    public Vector2 MoveInput => moveInput;
    
    private void Update()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
}

/// <summary>
/// 玩家状态类
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("生命值")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("耐力值")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina;
    
    [Header("经验值")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int expToNextLevel = 100;
    
    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }
    
    public float Health => currentHealth;
    public float MaxHealth => maxHealth;
    public float Stamina => currentStamina;
    public float MaxStamina => maxStamina;
    public int Level => currentLevel;
    public int Exp => currentExp;
    public int ExpToNextLevel => expToNextLevel;
}