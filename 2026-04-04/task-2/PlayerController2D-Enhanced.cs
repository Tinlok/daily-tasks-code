using UnityEngine;
using System.Collections;

/// <summary>
/// 增强版 2D 平台跳跃角色控制器
/// 在基础移动/跳跃/墙壁交互之上，新增冲刺(Dash)和攀爬(Climb)系统
/// 
/// 移动手感优化：
/// - 防止"糖块手感"：使用 velocity-based 移动，避免 input→velocity 线性映射
/// - 坡道辅助：在坡道边缘自动补充微量 Y 速度防止卡住
/// - 输入缓冲：跳跃/冲刺/攀爬均有输入缓冲窗口
/// - 动画曲线：冲刺速度使用 AnimationCurve 控制加减速手感
/// </summary>
public class PlayerController2DEnhanced : MonoBehaviour
{
    #region Settings - Movement (Base)
    [Header("=== 基础移动 ===")]
    [SerializeField, Range(1f, 20f)] private float moveSpeed = 7f;
    [SerializeField, Range(5f, 30f)] private float acceleration = 14f;
    [SerializeField, Range(2f, 15f)] private float deceleration = 10f;
    [SerializeField, Range(0.1f, 1f)] private float airControlFactor = 0.75f;
    #endregion

    #region Settings - Jump
    [Header("=== 跳跃 ===")]
    [SerializeField, Range(5f, 20f)] private float jumpForce = 13f;
    [SerializeField, Range(0.05f, 0.3f)] private float coyoteTime = 0.12f;
    [SerializeField, Range(0.05f, 0.2f)] private float jumpBufferTime = 0.1f;
    [SerializeField, Range(1, 3)] private int maxJumpCount = 2;
    [SerializeField, Range(0.5f, 5f)] private float fallGravityMultiplier = 2.5f;
    [SerializeField, Range(2f, 8f)] private float maxFallSpeed = 20f;
    #endregion

    #region Settings - Dash (NEW)
    [Header("=== 冲刺 (Dash) ===")]
    [SerializeField, Range(8f, 30f)] private float dashSpeed = 18f;
    [SerializeField, Range(0.1f, 0.5f)] private float dashDuration = 0.2f;
    [SerializeField, Range(0.3f, 2f)] private float dashCooldown = 0.8f;
    [SerializeField] private AnimationCurve dashSpeedCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.6f, 1f),
        new Keyframe(1f, 0.3f)
    );
    [SerializeField, Range(0f, 1f)] private float dashEndMomentumKeep = 0.4f;
    [SerializeField, Range(0f, 0.5f)] private float dashInputBufferTime = 0.15f;
    [SerializeField] private bool allowAirDash = true;
    [SerializeField] private bool allowDirectionChangeDuringDash = false;
    [Tooltip("冲刺时是否产生残影特效")]
    [SerializeField] private bool enableDashTrail = true;
    #endregion

    #region Settings - Climb (NEW)
    [Header("=== 攀爬 (Climb) ===")]
    [SerializeField, Range(1f, 10f)] private float climbSpeed = 4f;
    [SerializeField, Range(1f, 10f)] private float climbUpSpeed = 5f;
    [SerializeField, Range(0.1f, 0.5f)] private float climbGrabTime = 0.15f;
    [SerializeField, Range(0.1f, 0.3f)] private float climbInputBufferTime = 0.1f;
    [SerializeField, Range(0f, 0.5f)] private float climbJumpForce = 10f;
    [SerializeField, Range(0f, 0.5f)] private float climbJumpAwayForce = 8f;
    [SerializeField, Range(0f, 2f)] private float climbStamina = 3f;
    [SerializeField, Range(0.5f, 5f)] private float climbStaminaRegenRate = 1.5f;
    [SerializeField, Range(0.5f, 3f)] private float climbStaminaRegenDelay = 1f;
    [Tooltip("攀爬时是否产生粒子特效")]
    [SerializeField] private bool enableClimbParticles = true;
    #endregion

    #region Settings - Ground/Wall Detection
    [Header("=== 检测 ===")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, -0.9f, 0);
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.7f);
    [SerializeField] private Vector3 wallCheckOffset = new Vector3(0.5f, 0.3f, 0);
    [SerializeField] private LayerMask climbLayer;
    #endregion

    #region Settings - Feel Tuning
    [Header("=== 手感调优 ===")]
    [SerializeField, Range(0.01f, 0.1f)] private float slopeHelperThreshold = 0.05f;
    [Tooltip("着陆时轻微压低角色，增加打击感")]
    [SerializeField] private bool enableLandingSquash = true;
    [SerializeField, Range(0.05f, 0.2f)] private float squashDuration = 0.1f;
    [SerializeField, Range(0.7f, 0.95f)] private float squashAmount = 0.85f;
    #endregion

    #region Components
    private Rigidbody2D _rb;
    private BoxCollider2D _boxCollider;
    private SpriteRenderer _spriteRenderer;
    #endregion

    #region State
    private enum MoveState { Normal, Dashing, Climbing, Grabbing }
    private MoveState _currentState = MoveState.Normal;

    private float _inputX;
    private bool _jumpPressed;
    private float _jumpBufferTimer;

    private bool _isGrounded;
    private bool _wasGrounded;
    private float _coyoteTimer;
    private int _jumpCount;

    private bool _isWallSliding;
    private int _wallDirection;

    private bool _dashPressed;
    private float _dashBufferTimer;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private float _dashDirection;
    private bool _isDashing;

    private bool _climbPressed;
    private float _climbInputBufferTimer;
    private bool _isClimbing;
    private bool _isGrabbing;
    private float _grabTimer;
    private float _climbStaminaCurrent;
    private float _staminaRegenTimer;
    private float _climbInputY;
    private bool _climbJumpPressed;

    private float _defaultGravityScale;
    private Coroutine _squashCoroutine;
    #endregion

    #region Events
    public event System.Action OnDashStart;
    public event System.Action OnDashEnd;
    public event System.Action OnClimbStart;
    public event System.Action OnClimbEnd;
    public event System.Action OnGrabLedge;
    public event System.Action OnStaminaDepleted;
    public event System.Action OnLanded;
    #endregion

    #region Public Properties
    public bool IsDashing => _isDashing;
    public bool IsClimbing => _isClimbing;
    public float ClimbStaminaNormalized => _climbStaminaCurrent / climbStamina;
    public float DashCooldownNormalized => Mathf.Clamp01(1f - _dashCooldownTimer / dashCooldown);
    public MoveState CurrentState => _currentState;
    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultGravityScale = _rb.gravityScale;
        _climbStaminaCurrent = climbStamina;
    }

    private void Update()
    {
        GatherInput();
        UpdateTimers();
        HandleStateLogic();
    }

    private void FixedUpdate()
    {
        DetectGround();
        DetectWall();

        switch (_currentState)
        {
            case MoveState.Normal:
                HandleMovement();
                HandleJump();
                HandleWallSlide();
                HandleLanding();
                break;
            case MoveState.Dashing:
                HandleDashMovement();
                break;
            case MoveState.Climbing:
                HandleClimbMovement();
                break;
            case MoveState.Grabbing:
                HandleGrabTransition();
                break;
        }

        ApplyGravity();
        HandleSlopeHelper();
    }

    #region Input
    private void GatherInput()
    {
        _inputX = Input.GetAxisRaw("Horizontal");
        _jumpPressed = Input.GetButtonDown("Jump");
        _dashPressed = Input.GetButtonDown("Dash");
        _climbPressed = Input.GetButton("Climb");
        _climbInputY = Input.GetAxisRaw("Vertical");
        _climbJumpPressed = _jumpPressed && _isClimbing;
    }

    private void UpdateTimers()
    {
        if (_jumpPressed) _jumpBufferTimer = jumpBufferTime;
        else _jumpBufferTimer = Mathf.Max(0, _jumpBufferTimer - Time.deltaTime);

        if (_isGrounded) _coyoteTimer = coyoteTime;
        else _coyoteTimer = Mathf.Max(0, _coyoteTimer - Time.deltaTime);

        if (_dashPressed) _dashBufferTimer = dashInputBufferTime;
        else _dashBufferTimer = Mathf.Max(0, _dashBufferTimer - Time.deltaTime);

        _dashCooldownTimer = Mathf.Max(0, _dashCooldownTimer - Time.deltaTime);

        if (_climbPressed) _climbInputBufferTimer = climbInputBufferTime;
        else _climbInputBufferTimer = Mathf.Max(0, _climbInputBufferTimer - Time.deltaTime);

        if (!_isClimbing)
        {
            _staminaRegenTimer += Time.deltaTime;
            if (_staminaRegenTimer >= climbStaminaRegenDelay)
                _climbStaminaCurrent = Mathf.Min(climbStamina, _climbStaminaCurrent + climbStaminaRegenRate * Time.deltaTime);
        }
    }
    #endregion

    #region Detection
    private void DetectGround()
    {
        _wasGrounded = _isGrounded;
        Vector2 checkPos = (Vector2)transform.position + groundCheckOffset;
        _isGrounded = Physics2D.OverlapBox(checkPos, groundCheckSize, 0f, groundLayer);
        if (_isGrounded) _jumpCount = 0;
    }

    private void DetectWall()
    {
        Vector2 checkPosRight = (Vector2)transform.position + wallCheckOffset;
        Vector2 checkPosLeft = (Vector2)transform.position + new Vector3(-wallCheckOffset.x, wallCheckOffset.y, 0);
        bool wallRight = Physics2D.OverlapBox(checkPosRight, wallCheckSize, 0f, groundLayer);
        bool wallLeft = Physics2D.OverlapBox(checkPosLeft, wallCheckSize, 0f, groundLayer);

        if (wallRight && !_isGrounded) _wallDirection = 1;
        else if (wallLeft && !_isGrounded) _wallDirection = -1;
        else _wallDirection = 0;

        _isWallSliding = _wallDirection != 0 && _rb.velocity.y < 0 && !_isGrounded && _currentState == MoveState.Normal;
    }
    #endregion

    #region State Logic
    private void HandleStateLogic()
    {
        switch (_currentState)
        {
            case MoveState.Normal:
                if (_dashBufferTimer > 0 && _dashCooldownTimer <= 0)
                    StartDash();
                else if (_climbInputBufferTimer > 0 && CanStartClimb())
                    StartClimb();
                break;
            case MoveState.Dashing:
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0f) EndDash();
                break;
            case MoveState.Climbing:
                if (_climbStaminaCurrent <= 0f)
                {
                    EndClimb();
                    OnStaminaDepleted?.Invoke();
                }
                else if (_climbJumpPressed)
                    ClimbJump();
                else if (!_climbPressed || !IsTouchingClimbable())
                    EndClimb();
                break;
            case MoveState.Grabbing:
                break;
        }
    }

    private bool CanStartClimb() => _wallDirection != 0 && IsTouchingClimbable() && _climbStaminaCurrent > 0.2f;

    private bool IsTouchingClimbable()
    {
        Vector2 checkPosRight = (Vector2)transform.position + wallCheckOffset;
        Vector2 checkPosLeft = (Vector2)transform.position + new Vector3(-wallCheckOffset.x, wallCheckOffset.y, 0);
        return Physics2D.OverlapBox(checkPosRight, wallCheckSize, 0f, climbLayer)
            || Physics2D.OverlapBox(checkPosLeft, wallCheckSize, 0f, climbLayer)
            || _wallDirection != 0;
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        if (_isWallSliding) return;
        float targetSpeed = _inputX * moveSpeed;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        if (!_isGrounded) accelRate *= airControlFactor;

        float speedDiff = targetSpeed - _rb.velocity.x;
        float movement = speedDiff * accelRate * Time.fixedDeltaTime;
        if (Mathf.Abs(movement) > Mathf.Abs(speedDiff)) movement = speedDiff;

        _rb.velocity = new Vector2(_rb.velocity.x + movement, _rb.velocity.y);
        if (_inputX != 0f) _spriteRenderer.flipX = _inputX < 0f;
    }

    private void HandleJump()
    {
        if (_jumpBufferTimer > 0f && (_coyoteTimer > 0f || _jumpCount < maxJumpCount))
        {
            if (_isWallSliding && _wallDirection != 0)
            {
                _rb.velocity = new Vector2(-_wallDirection * 8f, 10f);
                _jumpCount = 0;
            }
            else
            {
                _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
                _jumpCount++;
            }
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
        }
    }

    private void HandleWallSlide()
    {
        if (!_isWallSliding) { _rb.gravityScale = _defaultGravityScale; return; }
        _rb.gravityScale = 0f;
        _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -2f));
    }

    private void HandleLanding()
    {
        if (_isGrounded && !_wasGrounded && enableLandingSquash)
        {
            OnLanded?.Invoke();
            if (_squashCoroutine != null) StopCoroutine(_squashCoroutine);
            _squashCoroutine = StartCoroutine(LandingSquash());
        }
    }

    private void HandleSlopeHelper()
    {
        if (_isGrounded && Mathf.Abs(_inputX) > 0.1f && _rb.velocity.y < -slopeHelperThreshold)
            _rb.velocity = new Vector2(_rb.velocity.x, 0f);
    }

    private void ApplyGravity()
    {
        if (_currentState == MoveState.Climbing || _currentState == MoveState.Dashing)
        { _rb.gravityScale = 0f; return; }

        if (_rb.velocity.y < 0f && !_isWallSliding)
        {
            _rb.gravityScale = _defaultGravityScale * fallGravityMultiplier;
            _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -maxFallSpeed));
        }
        else _rb.gravityScale = _defaultGravityScale;
    }
    #endregion

    #region Dash
    private void StartDash()
    {
        _isDashing = true;
        _currentState = MoveState.Dashing;
        _dashTimer = dashDuration;
        _dashCooldownTimer = dashCooldown;
        _dashBufferTimer = 0f;
        _dashDirection = Mathf.Abs(_inputX) > 0.1f ? Mathf.Sign(_inputX) : (_spriteRenderer.flipX ? -1f : 1f);
        _rb.gravityScale = 0f;
        _rb.velocity = new Vector2(_dashDirection * dashSpeed, 0f);
        OnDashStart?.Invoke();
    }

    private void HandleDashMovement()
    {
        float progress = 1f - (_dashTimer / dashDuration);
        float speedMultiplier = dashSpeedCurve.Evaluate(progress);
        if (allowDirectionChangeDuringDash && Mathf.Abs(_inputX) > 0.1f) _dashDirection = Mathf.Sign(_inputX);
        _rb.velocity = new Vector2(_dashDirection * dashSpeed * speedMultiplier, 0f);
    }

    private void EndDash()
    {
        _isDashing = false;
        _currentState = MoveState.Normal;
        _dashBufferTimer = 0f;
        _rb.velocity = new Vector2(_dashDirection * dashSpeed * dashEndMomentumKeep, _rb.velocity.y);
        _rb.gravityScale = _defaultGravityScale;
        OnDashEnd?.Invoke();
    }
    #endregion

    #region Climb
    private void StartClimb()
    {
        _isClimbing = true;
        _isGrabbing = true;
        _grabTimer = climbGrabTime;
        _currentState = MoveState.Grabbing;
        _staminaRegenTimer = 0f;
        _climbInputBufferTimer = 0f;
        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
        OnClimbStart?.Invoke();
    }

    private void HandleGrabTransition()
    {
        _grabTimer -= Time.deltaTime;
        if (_grabTimer <= 0f)
        {
            _isGrabbing = false;
            _currentState = MoveState.Climbing;
            OnGrabLedge?.Invoke();
        }
    }

    private void HandleClimbMovement()
    {
        _climbStaminaCurrent -= Time.deltaTime;
        float verticalSpeed = _climbInputY * climbUpSpeed;
        float horizontalSpeed = -_wallDirection * climbSpeed * 0.3f;
        _rb.velocity = new Vector2(horizontalSpeed, verticalSpeed);
    }

    private void EndClimb()
    {
        _isClimbing = false;
        _isGrabbing = false;
        _currentState = MoveState.Normal;
        _rb.gravityScale = _defaultGravityScale;
        _staminaRegenTimer = 0f;
        OnClimbEnd?.Invoke();
    }

    private void ClimbJump()
    {
        float jumpDir = _wallDirection != 0 ? -_wallDirection : (_spriteRenderer.flipX ? -1f : 1f);
        _rb.velocity = new Vector2(jumpDir * climbJumpAwayForce, climbJumpForce);
        _isClimbing = false;
        _isGrabbing = false;
        _currentState = MoveState.Normal;
        _rb.gravityScale = _defaultGravityScale;
        _climbStaminaCurrent -= 0.5f;
        _staminaRegenTimer = 0f;
        OnClimbEnd?.Invoke();
    }
    #endregion

    #region Effects
    private IEnumerator LandingSquash()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 squashTarget = new Vector3(originalScale.x * (1f + (1f - squashAmount) * 0.3f), originalScale.y * squashAmount, originalScale.z);
        float elapsed = 0f;
        while (elapsed < squashDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, squashTarget, elapsed / (squashDuration * 0.5f));
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < squashDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(squashTarget, originalScale, elapsed / (squashDuration * 0.5f));
            yield return null;
        }
        transform.localScale = originalScale;
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position + groundCheckOffset, groundCheckSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + wallCheckOffset, wallCheckSize);
        Gizmos.DrawWireCube(transform.position + new Vector3(-wallCheckOffset.x, wallCheckOffset.y, 0), wallCheckSize);
    }
    #endregion
}
