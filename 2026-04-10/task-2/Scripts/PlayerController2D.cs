using UnityEngine;
using System.Collections;

/// <summary>
/// 增强2D平台角色控制器
///
/// 功能特性:
/// - 冲刺: 短距离快速移动，有冷却时间
/// - 攀爬: 墙面攀爬、平台边缘攀爬
/// - 流畅移动: 可配置的加速度/减速度曲线
/// - 跳跃优化: 可变跳跃高度、土狼时间、跳跃缓冲
/// - 墙壁交互: 滑行、蹬墙跳
/// - 下蹲: 可调整碰撞体大小
///
/// 输入映射:
/// - Horizontal: A/D 或 左/右箭头
/// - Jump: Space
/// - Crouch: S 或 下箭头
/// - Dash: Left Shift
/// - Climb Up: W 或 上箭头
/// - Climb Down: S 或 下箭头（空中时）
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController2D : MonoBehaviour
{
    #region Inspector Settings - 移动

    [Header("基础移动")]
    [Tooltip("最大水平移动速度")]
    [SerializeField] private float walkSpeed = 5f;

    [Tooltip("奔跑速度（按住移动键时）")]
    [SerializeField] private float runSpeed = 8f;

    [Tooltip("静止到最大速度的加速度")]
    [SerializeField] private float groundAcceleration = 15f;

    [Tooltip("最大速度到静止的减速度")]
    [SerializeField] private float groundDeceleration = 20f;

    [Tooltip("空中控制系数（0-1，越小控制力越弱）")]
    [Range(0f, 1f)]
    [SerializeField] private float airControlFactor = 0.6f;

    [Tooltip("空中加速度")]
    [SerializeField] private float airAcceleration = 10f;

    [Tooltip("空中减速度")]
    [SerializeField] private float airDeceleration = 5f;

    [Tooltip("摩擦力（停止输入时的减速）")]
    [SerializeField] private float friction = 10f;

    #endregion

    #region Inspector Settings - 冲刺

    [Header("冲刺设置")]
    [Tooltip("冲刺速度")]
    [SerializeField] private float dashSpeed = 20f;

    [Tooltip("冲刺持续时间")]
    [SerializeField] private float dashDuration = 0.15f;

    [Tooltip("冲刺冷却时间")]
    [SerializeField] private float dashCooldown = 0.5f;

    [Tooltip("空中可冲刺次数")]
    [SerializeField] private int airDashCount = 1;

    [Tooltip("冲刺后暂时禁用输入的时间")]
    [SerializeField] private float dashEndLag = 0.1f;

    [Tooltip("冲刺时是否忽略重力")]
    [SerializeField] private bool dashIgnoreGravity = true;

    #endregion

    #region Inspector Settings - 攀爬

    [Header("攀爬设置")]
    [Tooltip("攀爬速度")]
    [SerializeField] private float climbSpeed = 4f;

    [Tooltip("可以攀爬的图层")]
    [SerializeField] private LayerMask climbableLayer;

    [Tooltip("攀爬时重力缩放")]
    [Range(0f, 1f)]
    [SerializeField] private float climbGravityScale = 0.1f;

    [Tooltip("体力（攀爬持续时间，秒）")]
    [SerializeField] private float climbStamina = 10f;

    [Tooltip("体力恢复速度")]
    [SerializeField] private float staminaRegenRate = 2f;

    [Tooltip("体力耗尽后的冷却时间")]
    [SerializeField] private float staminaExhaustedCooldown = 2f;

    #endregion

    #region Inspector Settings - 跳跃

    [Header("跳跃设置")]
    [Tooltip("跳跃力度")]
    [SerializeField] private float jumpForce = 14f;

    [Tooltip("可变跳跃高度（按住跳更高）")]
    [SerializeField] private float jumpHoldForce = 3f;

    [Tooltip("跳跃上升持续时间")]
    [SerializeField] private float jumpHoldDuration = 0.2f;

    [Tooltip("土狼时间（离地后仍可跳跃的时间）")]
    [SerializeField] private float coyoteTime = 0.15f;

    [Tooltip("跳跃缓冲（提前按键的容错时间）")]
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Tooltip("最大跳跃次数（包含地面跳跃）")]
    [SerializeField] private int maxJumpCount = 2;

    [Tooltip("二段跳力度")]
    [SerializeField] private float doubleJumpForce = 12f;

    [Tooltip("下蹲跳时的初始下落速度")]
    [SerializeField] private float jumpDownwardSpeed = 3f;

    #endregion

    #region Inspector Settings - 墙壁交互

    [Header("墙壁交互")]
    [Tooltip("墙壁滑行速度")]
    [SerializeField] private float wallSlideSpeed = 2f;

    [Tooltip("蹬墙跳水平力度")]
    [SerializeField] private float wallJumpHorizontalForce = 10f;

    [Tooltip("蹬墙跳垂直力度")]
    [SerializeField] private float wallJumpVerticalForce = 12f;

    [Tooltip("蹬墙跳后输入禁用时间")]
    [SerializeField] private float wallJumpInputLockTime = 0.2f;

    [Tooltip("可以攀爬的墙壁图层")]
    [SerializeField] private LayerMask wallLayer;

    #endregion

    #region Inspector Settings - 下蹲

    [Header("下蹲设置")]
    [Tooltip("下蹲移动速度")]
    [SerializeField] private float crouchSpeed = 2.5f;

    [Tooltip("下蹲时碰撞体大小")]
    [SerializeField] private Vector2 crouchColliderSize = new(0.8f, 1f);

    [Tooltip("下蹲时碰撞体偏移")]
    [SerializeField] private Vector2 crouchColliderOffset = new(0f, -0.4f);

    [Tooltip("站立时碰撞体大小")]
    [SerializeField] private Vector2 standColliderSize = new(0.8f, 1.8f);

    [Tooltip("站立时碰撞体偏移")]
    [SerializeField] private Vector2 standColliderOffset = new(0f, 0f);

    #endregion

    #region Inspector Settings - 检测

    [Header("检测设置")]
    [Tooltip("地面检测图层")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("地面检测框大小")]
    [SerializeField] private Vector2 groundCheckSize = new(0.6f, 0.1f);

    [Tooltip("地面检测框偏移")]
    [SerializeField] private Vector3 groundCheckOffset = new(0f, -0.9f, 0f);

    [Tooltip("墙壁检测框大小")]
    [SerializeField] private Vector2 wallCheckSize = new(0.1f, 0.8f);

    [Tooltip("墙壁检测框偏移")]
    [SerializeField] private Vector3 wallCheckOffset = new(0.5f, 0.2f, 0f);

    [Tooltip("边缘检测点偏移")]
    [SerializeField] private Vector3 ledgeCheckOffset = new(0.4f, -0.5f, 0f);

    [Tooltip("边缘检测半径")]
    [SerializeField] private float ledgeCheckRadius = 0.2f;

    #endregion

    #region Inspector Settings - 视觉效果

    [Header("视觉效果")]
    [Tooltip("冲刺时残影生成间隔")]
    [SerializeField] private float afterimageInterval = 0.02f;

    [Tooltip("残影持续时间")]
    [SerializeField] private float afterimageLifetime = 0.3f;

    [Tooltip("残影颜色")]
    [SerializeField] private Color afterimageColor = new(1f, 1f, 1f, 0.5f);

    #endregion

    #region Private Variables - 组件

    private Rigidbody2D _rb;
    private BoxCollider2D _boxCollider;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    #endregion

    #region Private Variables - 状态

    // 移动状态
    private Vector2 _velocity;
    private float _currentSpeed;
    private bool _isGrounded;
    private bool _wasGroundedLastFrame;

    // 冲刺状态
    private bool _isDashing;
    private bool _canDash = true;
    private int _currentAirDashCount;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private Vector2 _dashDirection;

    // 攀爬状态
    private bool _isClimbing;
    private bool _canClimb;
    private float _currentStamina;
    private bool _isStaminaExhausted;
    private float _staminaExhaustedTimer;
    private bool _isOnLedge;

    // 跳跃状态
    private int _jumpCount;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private bool _isJumping;
    private float _jumpHoldTimer;
    private bool _jumpHeld;

    // 墙壁状态
    private bool _isWallSliding;
    private int _wallDirection;
    private bool _isWallClimbing;
    private float _wallJumpLockTimer;

    // 下蹲状态
    private bool _isCrouching;
    private bool _wantsToCrouch;

    // 输入状态
    private float _inputX;
    private float _inputY;
    private bool _jumpPressed;
    private bool _jumpReleased;
    private bool _dashPressed;
    private bool _crouchHeld;

    // 物理默认值
    private float _defaultGravityScale;

    // 冲刺残影
    private GameObject _afterimageContainer;
    private Coroutine _afterimageCoroutine;

    #endregion

    #region Public Properties

    /// <summary>是否在地面上</summary>
    public bool IsGrounded => _isGrounded;

    /// <summary>是否正在冲刺</summary>
    public bool IsDashing => _isDashing;

    /// <summary>是否正在攀爬</summary>
    public bool IsClimbing => _isClimbing || _isWallClimbing;

    /// <summary>是否正在墙壁滑行</summary>
    public bool IsWallSliding => _isWallSliding;

    /// <summary>当前体力百分比</summary>
    public float StaminaPercent => _currentStamina / climbStamina;

    /// <summary>当前速度</summary>
    public Vector2 Velocity => _rb.velocity;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // 获取组件引用
        _rb = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        // 保存默认物理参数
        _defaultGravityScale = _rb.gravityScale;
        _currentStamina = climbStamina;

        // 创建残影容器
        _afterimageContainer = new GameObject("Afterimages");
        _afterimageContainer.transform.SetParent(transform.parent);
    }

    private void Update()
    {
        GatherInput();
        UpdateTimers();
        CheckCollisions();
        HandleJump();
        HandleDash();
        HandleClimb();
        HandleCrouch();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleGravity();
    }

    private void OnDestroy()
    {
        if (_afterimageContainer != null)
        {
            Destroy(_afterimageContainer);
        }
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// 收集玩家输入
    /// </summary>
    private void GatherInput()
    {
        _inputX = Input.GetAxisRaw("Horizontal");
        _inputY = Input.GetAxisRaw("Vertical");
        _jumpPressed = Input.GetButtonDown("Jump");
        _jumpReleased = Input.GetButtonUp("Jump");
        _jumpHeld = Input.GetButton("Jump");
        _dashPressed = Input.GetButtonDown("Dash");
        _wantsToCrouch = Input.GetButton("Crouch") || Input.GetKey(KeyCode.S);

        // 跳跃缓冲
        if (_jumpPressed)
        {
            _jumpBufferTimer = jumpBufferTime;
        }
    }

    #endregion

    #region Collision Detection

    /// <summary>
    /// 检测地面、墙壁和可攀爬表面
    /// </summary>
    private void CheckCollisions()
    {
        _wasGroundedLastFrame = _isGrounded;

        // 地面检测
        Vector2 groundCheckPos = (Vector2)transform.position + (Vector2)groundCheckOffset;
        _isGrounded = Physics2D.OverlapBox(groundCheckPos, groundCheckSize, 0f, groundLayer);

        // 更新土狼时间
        if (_isGrounded)
        {
            _coyoteTimer = coyoteTime;
            _jumpCount = 0;
            _currentAirDashCount = 0;
            _isJumping = false;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }

        // 墙壁检测
        CheckWalls();

        // 边缘检测
        CheckLedges();

        // 可攀爬表面检测
        CheckClimbableSurfaces();
    }

    /// <summary>
    /// 检测两侧墙壁
    /// </summary>
    private void CheckWalls()
    {
        _wallDirection = 0;
        _isWallSliding = false;
        _isWallClimbing = false;

        // 检测右侧
        Vector2 rightCheck = (Vector2)transform.position + wallCheckOffset;
        bool rightWall = Physics2D.OverlapBox(rightCheck, wallCheckSize, 0f, wallLayer);

        // 检测左侧
        Vector2 leftCheck = (Vector2)transform.position - wallCheckOffset;
        bool leftWall = Physics2D.OverlapBox(leftCheck, wallCheckSize, 0f, wallLayer);

        if (rightWall) _wallDirection = 1;
        else if (leftWall) _wallDirection = -1;

        // 墙壁滑行判定
        if (_wallDirection != 0 && !_isGrounded && _rb.velocity.y < 0)
        {
            // 按住朝向墙壁方向时滑行
            if (_inputX == _wallDirection || _inputX == 0)
            {
                _isWallSliding = true;
            }
        }
    }

    /// <summary>
    /// 检测平台边缘（用于攀爬）
    /// </summary>
    private void CheckLedges()
    {
        _isOnLedge = false;

        if (!_isWallClimbing && _wallDirection != 0)
        {
            // 检测墙壁上方的边缘
            Vector2 ledgeCheckPos = (Vector2)transform.position +
                (Vector3)(ledgeCheckOffset * _wallDirection) + Vector3.up * 0.5f;

            Collider2D[] hits = Physics2D.OverlapCircleAll(ledgeCheckPos, ledgeCheckRadius, groundLayer);
            _isOnLedge = hits.Length == 0; // 上方没有障碍物说明有边缘
        }
    }

    /// <summary>
    /// 检测可攀爬表面（梯子、藤蔓等）
    /// </summary>
    private void CheckClimbableSurfaces()
    {
        _canClimb = false;

        // 检测当前位置是否有可攀爬物
        Collider2D climbable = Physics2D.OverlapCircle(
            transform.position,
            0.5f,
            climbableLayer
        );

        if (climbable != null)
        {
            _canClimb = true;
        }
    }

    #endregion

    #region Timer Management

    /// <summary>
    /// 更新所有计时器
    /// </summary>
    private void UpdateTimers()
    {
        // 跳跃缓冲
        if (_jumpBufferTimer > 0)
        {
            _jumpBufferTimer -= Time.deltaTime;
        }

        // 冲刺冷却
        if (_dashCooldownTimer > 0)
        {
            _dashCooldownTimer -= Time.deltaTime;
            if (_dashCooldownTimer <= 0)
            {
                _canDash = true;
            }
        }

        // 墙壁跳跃输入锁定
        if (_wallJumpLockTimer > 0)
        {
            _wallJumpLockTimer -= Time.deltaTime;
        }

        // 体力耗尽冷却
        if (_isStaminaExhausted)
        {
            _staminaExhaustedTimer -= Time.deltaTime;
            if (_staminaExhaustedTimer <= 0)
            {
                _isStaminaExhausted = false;
            }
        }
        // 体力恢复
        else if (!_isClimbing && !_isWallClimbing && _currentStamina < climbStamina)
        {
            _currentStamina = Mathf.Min(climbStamina, _currentStamina + staminaRegenRate * Time.deltaTime);
        }

        // 跳跃按住计时
        if (_jumpHeld && _isJumping && _rb.velocity.y > 0)
        {
            _jumpHoldTimer += Time.deltaTime;
        }
    }

    #endregion

    #region Movement

    /// <summary>
    /// 处理水平移动
    /// </summary>
    private void HandleMovement()
    {
        // 冲刺期间锁定移动
        if (_isDashing) return;

        // 墙壁跳跃输入锁定
        if (_wallJumpLockTimer > 0) return;

        float targetSpeed = CalculateTargetSpeed();
        float acceleration = CalculateAcceleration();
        float deceleration = CalculateDeceleration();

        // 计算速度差
        float speedDiff = targetSpeed - _rb.velocity.x;
        float accelRate = Mathf.Abs(speedDiff) > 0.01f ? acceleration : deceleration;

        // 应用加速度
        float newVelocityX = Mathf.MoveTowards(
            _rb.velocity.x,
            targetSpeed,
            accelRate * Time.fixedDeltaTime
        );

        // 攀爬时限制水平移动
        if (_isClimbing || _isWallClimbing)
        {
            newVelocityX *= 0.5f;
        }

        _rb.velocity = new Vector2(newVelocityX, _rb.velocity.y);

        // 翻转精灵
        UpdateFacingDirection();
    }

    /// <summary>
    /// 计算目标移动速度
    /// </summary>
    private float CalculateTargetSpeed()
    {
        if (_isCrouching)
        {
            return _inputX * crouchSpeed;
        }

        float maxSpeed = Mathf.Abs(_inputX) > 0.5f ? runSpeed : walkSpeed;
        return _inputX * maxSpeed;
    }

    /// <summary>
    /// 计算当前加速度
    /// </summary>
    private float CalculateAcceleration()
    {
        bool hasInput = Mathf.Abs(_inputX) > 0.01f;

        if (!hasInput) return 0f;

        if (_isGrounded)
        {
            return groundAcceleration;
        }
        else
        {
            return airAcceleration * airControlFactor;
        }
    }

    /// <summary>
    /// 计算当前减速度
    /// </summary>
    private float CalculateDeceleration()
    {
        if (_isGrounded)
        {
            return groundDeceleration;
        }
        else
        {
            return airDeceleration * airControlFactor;
        }
    }

    /// <summary>
    /// 更新角色朝向
    /// </summary>
    private void UpdateFacingDirection()
    {
        if (Mathf.Abs(_inputX) > 0.01f && !_isWallSliding)
        {
            _spriteRenderer.flipX = _inputX < 0;
        }
    }

    #endregion

    #region Gravity

    /// <summary>
    /// 处理重力和垂直移动
    /// </summary>
    private void HandleGravity()
    {
        // 攀爬时使用自定义重力
        if (_isClimbing || _isWallClimbing)
        {
            HandleClimbingGravity();
            return;
        }

        // 墙壁滑行
        if (_isWallSliding)
        {
            HandleWallSlideGravity();
            return;
        }

        // 可变跳跃高度
        if (_jumpHeld && _isJumping && _rb.velocity.y > 0 && _jumpHoldTimer < jumpHoldDuration)
        {
            _rb.gravityScale = _defaultGravityScale * 0.5f;
        }
        else
        {
            _rb.gravityScale = _defaultGravityScale;
        }

        // 跳跃下落加速
        if (!_isJumping && _rb.velocity.y < 0)
        {
            _rb.gravityScale = _defaultGravityScale * 1.5f;
        }
    }

    /// <summary>
    /// 攀爬时的重力处理
    /// </summary>
    private void HandleClimbingGravity()
    {
        _rb.gravityScale = climbGravityScale;

        // 垂直攀爬移动
        float climbVelocityY = _inputY * climbSpeed;
        _rb.velocity = new Vector2(_rb.velocity.x * 0.7f, climbVelocityY);

        // 消耗体力
        if (Mathf.Abs(_inputY) > 0.01f || _isWallClimbing)
        {
            _currentStamina -= Time.fixedDeltaTime;
            if (_currentStamina <= 0)
            {
                _currentStamina = 0;
                _isStaminaExhausted = true;
                _staminaExhaustedTimer = staminaExhaustedCooldown;
                ExitClimb();
            }
        }
    }

    /// <summary>
    /// 墙壁滑行时的重力处理
    /// </summary>
    private void HandleWallSlideGravity()
    {
        _rb.gravityScale = 0;
        float limitedFallSpeed = Mathf.Max(_rb.velocity.y, -wallSlideSpeed);
        _rb.velocity = new Vector2(_rb.velocity.x, limitedFallSpeed);
    }

    #endregion

    #region Jump

    /// <summary>
    /// 处理跳跃输入
    /// </summary>
    private void HandleJump()
    {
        // 跳跃释放处理（可变高度）
        if (_jumpReleased && _rb.velocity.y > 0)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * 0.5f);
            _isJumping = false;
        }

        // 墙壁跳跃优先
        if ((_isWallSliding || _isWallClimbing) && _jumpPressed && _wallJumpLockTimer <= 0)
        {
            PerformWallJump();
            return;
        }

        // 普通跳跃
        if (_jumpBufferTimer > 0 && _coyoteTimer > 0 && !_isCrouching)
        {
            PerformJump();
            return;
        }

        // 空中跳跃（二段跳/多段跳）
        if (_jumpPressed && !_isGrounded && _coyoteTimer <= 0 && _jumpCount < maxJumpCount - 1)
        {
            PerformDoubleJump();
            return;
        }
    }

    /// <summary>
    /// 执行普通跳跃
    /// </summary>
    private void PerformJump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
        _jumpCount = 1;
        _coyoteTimer = 0;
        _jumpBufferTimer = 0;
        _isJumping = true;
        _jumpHoldTimer = 0;

        ExitClimb();
    }

    /// <summary>
    /// 执行二段跳
    /// </summary>
    private void PerformDoubleJump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, doubleJumpForce);
        _jumpCount++;
        _jumpBufferTimer = 0;
        _isJumping = true;
        _jumpHoldTimer = 0;

        // 二段跳特效或音效可以在这里触发
    }

    /// <summary>
    /// 执行墙壁跳跃
    /// </summary>
    private void PerformWallJump()
    {
        Vector2 jumpVelocity = new Vector2(
            -_wallDirection * wallJumpHorizontalForce,
            wallJumpVerticalForce
        );

        _rb.velocity = jumpVelocity;
        _wallJumpLockTimer = wallJumpInputLockTime;
        _jumpCount = 0;
        _jumpBufferTimer = 0;
        _isJumping = true;
        _jumpHoldTimer = 0;

        ExitClimb();

        // 翻转朝向
        _spriteRenderer.flipX = -_wallDirection < 0;
    }

    #endregion

    #region Dash

    /// <summary>
    /// 处理冲刺输入
    /// </summary>
    private void HandleDash()
    {
        if (_dashPressed && _canDash && CanDash())
        {
            StartCoroutine(PerformDash());
        }
    }

    /// <summary>
    /// 检查是否可以冲刺
    /// </summary>
    private bool CanDash()
    {
        // 地面时总是可以冲刺
        if (_isGrounded) return true;

        // 空中冲刺次数限制
        return _currentAirDashCount < airDashCount;
    }

    /// <summary>
    /// 执行冲刺
    /// </summary>
    private IEnumerator PerformDash()
    {
        _isDashing = true;
        _canDash = false;
        _dashCooldownTimer = dashCooldown;

        // 计算冲刺方向
        _dashDirection = new Vector2(_inputX, _inputY);

        // 如果没有输入，使用当前朝向
        if (_dashDirection == Vector2.zero)
        {
            _dashDirection = new Vector2(_spriteRenderer.flipX ? -1 : 1, 0);
        }
        else
        {
            _dashDirection = _dashDirection.normalized;
        }

        // 空中冲刺计数
        if (!_isGrounded)
        {
            _currentAirDashCount++;
        }

        // 开始生成残影
        if (_afterimageCoroutine != null)
        {
            StopCoroutine(_afterimageCoroutine);
        }
        _afterimageCoroutine = StartCoroutine(SpawnAfterimages());

        // 冲刺期间
        float dashTimer = dashDuration;
        while (dashTimer > 0)
        {
            // 应用冲刺速度
            _rb.velocity = _dashDirection * dashSpeed;

            // 冲刺时忽略重力
            if (dashIgnoreGravity)
            {
                _rb.gravityScale = 0;
            }

            dashTimer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 冲刺结束延迟
        if (dashEndLag > 0)
        {
            _rb.velocity = _dashDirection * (dashSpeed * 0.3f);
            yield return new WaitForSeconds(dashEndLag);
        }

        // 恢复正常状态
        _isDashing = false;
        _rb.gravityScale = _defaultGravityScale;
    }

    /// <summary>
    /// 生成冲刺残影
    /// </summary>
    private IEnumerator SpawnAfterimages()
    {
        while (_isDashing)
        {
            CreateAfterimage();
            yield return new WaitForSeconds(afterimageInterval);
        }
    }

    /// <summary>
    /// 创建单个残影
    /// </summary>
    private void CreateAfterimage()
    {
        GameObject afterimage = new GameObject("DashAfterimage");
        afterimage.transform.SetParent(_afterimageContainer.transform);
        afterimage.transform.position = transform.position;
        afterimage.transform.localScale = transform.localScale;

        // 复制精灵渲染器
        SpriteRenderer sr = afterimage.AddComponent<SpriteRenderer>();
        sr.sprite = _spriteRenderer.sprite;
        sr.color = afterimageColor;
        sr.material = _spriteRenderer.material;
        sr.sortingOrder = _spriteRenderer.sortingOrder - 1;

        // 淡出并销毁
        StartCoroutine(FadeOutAfterimage(afterimage));
    }

    /// <summary>
    /// 淡出并销毁残影
    /// </summary>
    private IEnumerator FadeOutAfterimage(GameObject afterimage)
    {
        SpriteRenderer sr = afterimage.GetComponent<SpriteRenderer>();
        float timer = afterimageLifetime;

        while (timer > 0)
        {
            float alpha = Mathf.Lerp(0, afterimageColor.a, timer / afterimageLifetime);
            sr.color = new Color(afterimageColor.r, afterimageColor.g, afterimageColor.b, alpha);
            timer -= Time.deltaTime;
            yield return null;
        }

        Destroy(afterimage);
    }

    #endregion

    #region Climb

    /// <summary>
    /// 处理攀爬输入
    /// </summary>
    private void HandleClimb()
    {
        // 体力耗尽时无法攀爬
        if (_isStaminaExhausted) return;

        bool wantsToClimb = _canClimb && (_inputY != 0 || (_wallDirection != 0 && _inputY >= 0));

        // 进入攀爬状态
        if (wantsToClimb && !_isClimbing && !_isGrounded)
        {
            EnterClimb();
        }

        // 墙壁攀爬
        if (_wallDirection != 0 && !_isGrounded && !_isClimbing)
        {
            if (_inputY > 0 || _isOnLedge)
            {
                EnterWallClimb();
            }
        }

        // 退出攀爬状态
        if (_isClimbing && !_canClimb)
        {
            ExitClimb();
        }

        if (_isWallClimbing && _wallDirection == 0)
        {
            ExitWallClimb();
        }
    }

    /// <summary>
    /// 进入梯子/藤蔓攀爬状态
    /// </summary>
    private void EnterClimb()
    {
        _isClimbing = true;
        _rb.velocity = new Vector2(_rb.velocity.x * 0.5f, 0);
    }

    /// <summary>
    /// 退出攀爬状态
    /// </summary>
    private void ExitClimb()
    {
        _isClimbing = false;
        _rb.gravityScale = _defaultGravityScale;
    }

    /// <summary>
    /// 进入墙壁攀爬状态
    /// </summary>
    private void EnterWallClimb()
    {
        _isWallClimbing = true;
        _isWallSliding = false;
        _rb.velocity = Vector2.zero;
    }

    /// <summary>
    /// 退出墙壁攀爬状态
    /// </summary>
    private void ExitWallClimb()
    {
        _isWallClimbing = false;
        _rb.gravityScale = _defaultGravityScale;
    }

    #endregion

    #region Crouch

    /// <summary>
    /// 处理下蹲
    /// </summary>
    private void HandleCrouch()
    {
        // 地面且按住下蹲键时下蹲
        if (_wantsToCrouch && _isGrounded)
        {
            EnterCrouch();
        }
        // 松开下蹲键且头顶无障碍物时站起
        else if (!_wantsToCrouch && _isCrouching)
        {
            // 检查头顶是否有空间
            Vector2 ceilingCheck = (Vector2)transform.position + Vector2.up * 1f;
            bool canStand = !Physics2D.OverlapBox(ceilingCheck, new Vector2(0.6f, 0.1f), 0f, groundLayer);

            if (canStand)
            {
                ExitCrouch();
            }
        }
    }

    /// <summary>
    /// 进入下蹲状态
    /// </summary>
    private void EnterCrouch()
    {
        if (_isCrouching) return;

        _isCrouching = true;
        _boxCollider.size = crouchColliderSize;
        _boxCollider.offset = crouchColliderOffset;
    }

    /// <summary>
    /// 退出下蹲状态
    /// </summary>
    private void ExitCrouch()
    {
        if (!_isCrouching) return;

        _isCrouching = false;
        _boxCollider.size = standColliderSize;
        _boxCollider.offset = standColliderOffset;
    }

    #endregion

    #region Animation

    /// <summary>
    /// 更新动画参数
    /// </summary>
    private void UpdateAnimations()
    {
        if (_animator == null) return;

        _animator.SetFloat("VelocityX", Mathf.Abs(_rb.velocity.x));
        _animator.SetFloat("VelocityY", _rb.velocity.y);
        _animator.SetBool("IsGrounded", _isGrounded);
        _animator.SetBool("IsCrouching", _isCrouching);
        _animator.SetBool("IsDashing", _isDashing);
        _animator.SetBool("IsClimbing", _isClimbing);
        _animator.SetBool("IsWallSliding", _isWallSliding);
        _animator.SetFloat("StaminaPercent", StaminaPercent);
    }

    #endregion

    #region Gizmos

    /// <summary>
    /// 绘制调试信息
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 地面检测 - 绿色
        Gizmos.color = _isGrounded ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(
            transform.position + groundCheckOffset,
            groundCheckSize
        );

        // 墙壁检测 - 蓝色
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(
            transform.position + wallCheckOffset,
            wallCheckSize
        );
        Gizmos.DrawWireCube(
            transform.position - wallCheckOffset,
            wallCheckSize
        );

        // 边缘检测 - 青色
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position + (ledgeCheckOffset * (_wallDirection != 0 ? _wallDirection : 1)) + Vector3.up * 0.5f,
            ledgeCheckRadius
        );

        // 攀爬检测 - 紫色
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 外部调用：施加力
    /// </summary>
    public void AddForce(Vector2 force)
    {
        _rb.velocity += force;
    }

    /// <summary>
    /// 外部调用：设置位置
    /// </summary>
    public void SetPosition(Vector2 position)
    {
        transform.position = position;
        _rb.velocity = Vector2.zero;
    }

    /// <summary>
    /// 外部调用：重置冲刺
    /// </summary>
    public void ResetDash()
    {
        _canDash = true;
        _currentAirDashCount = 0;
        _dashCooldownTimer = 0;
    }

    /// <summary>
    /// 外部调用：恢复体力
    /// </summary>
    public void RestoreStamina(float amount)
    {
        _currentStamina = Mathf.Min(climbStamina, _currentStamina + amount);
        _isStaminaExhausted = false;
    }

    #endregion
}
