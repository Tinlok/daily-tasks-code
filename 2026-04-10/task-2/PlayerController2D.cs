using UnityEngine;

/// <summary>
 Enhanced 2D Platformer Character Controller
 支持移动、跳跃、二段跳、下蹲、墙壁滑行、蹬墙跳、冲刺、攀爬
 包含优化的移动手感和物理反馈
</summary>
public class PlayerController2D : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float airControlFactor = 0.6f;
    [SerializeField] private float friction = 0.1f;
    
    [Header("冲刺设置")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.8f;
    [SerializeField] private int maxDashCount = 1;
    [SerializeField] private float dashRechargeTime = 2f;
    
    [Header("跳跃设置")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float jumpAddForce = 3f;
    
    [Header("下蹲设置")]
    [SerializeField] private float crouchSpeed = 4f;
    [SerializeField] private Vector2 crouchSize = new Vector2(1f, 0.7f);
    [SerializeField] private Vector2 standSize = new Vector2(1f, 1.8f);
    [SerializeField] private float crouchTransitionTime = 0.15f;
    
    [Header("墙壁交互")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpForceX = 10f;
    [SerializeField] private float wallJumpForceY = 12f;
    [SerializeField] private float wallJumpCooldown = 0.3f;
    [SerializeField] private float wallJumpBufferTime = 0.2f;
    
    [Header("攀爬设置")]
    [SerializeField] private float climbSpeed = 4f;
    [SerializeField] private float climbJumpForce = 8f;
    [SerializeField] private float climbCheckDistance = 0.3f;
    [SerializeField] private LayerMask climbableLayer;
    [SerializeField] private float grabDistance = 0.2f;
    
    [Header("地面检测")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.9f, 0.15f);
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, -0.9f, 0);
    
    [Header("墙壁检测")]
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.15f, 0.8f);
    [SerializeField] private Vector3 wallCheckOffset = new Vector3(0.6f, 0.2f, 0);
    
    [Header("高级设置")]
    [SerializeField] private bool enableAdvancedMovement = true;
    [SerializeField] private float stepSmoothness = 0.1f;
    [SerializeField] private float landingForce = 0.2f;
    
    // 组件引用
    private Rigidbody2D _rb;
    private BoxCollider2D _boxCollider;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    
    // 状态变量
    private Vector2 _velocity;
    private bool _isGrounded;
    private bool _isCrouching;
    private bool _isWallSliding;
    private bool _isDashing;
    private bool _isClimbing;
    private int _wallDirection;
    private int _jumpCount;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _wallJumpCooldownTimer;
    private float _wallJumpBufferTimer;
    private float _dashCooldownTimer;
    private float _dashTimer;
    private int _dashCount;
    private float _defaultGravityScale;
    
    // 输入
    private float _inputX;
    private float _inputY;
    private bool _jumpPressed;
    private bool _jumpReleased;
    private bool _crouchHeld;
    private bool _dashPressed;
    private bool _climbPressed;
    private bool _isGrabbing;
    
    // 用于平滑过渡
    private Vector2 _currentColliderSize;
    private Vector2 _currentColliderOffset;
    private float _currentGravityScale;
    
    // 用于高级移动手感
    private float _stepTimer;
    private float _landingTimer;
    private Vector2 _lastVelocity;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 尝试获取Animator组件
        _animator = GetComponent<Animator>();
        
        _defaultGravityScale = _rb.gravityScale;
        _currentColliderSize = standSize;
        _currentColliderOffset = Vector2.zero;
        
        // 初始化默认碰撞体大小
        _boxCollider.size = _currentColliderSize;
        _boxCollider.offset = _currentColliderOffset;
    }
    
    private void Update()
    {
        if (!_isDashing)
        {
            GatherInput();
            CheckGround();
            CheckWall();
            CheckClimbable();
            HandleTimers();
            HandleJump();
            HandleClimb();
            HandleDash();
        }
        else
        {
            HandleDashUpdate();
        }
        
        // 更新动画状态
        UpdateAnimationStates();
    }
    
    private void FixedUpdate()
    {
        if (!_isDashing)
        {
            HandleMovement();
            HandleGravity();
            HandleLandingEffects();
        }
        else
        {
            HandleDashFixedUpdate();
        }
    }
    
    private void GatherInput()
    {
        _inputX = Input.GetAxisRaw("Horizontal");
        _inputY = Input.GetAxisRaw("Vertical");
        _jumpPressed = Input.GetButtonDown("Jump");
        _jumpReleased = Input.GetButtonUp("Jump");
        _crouchHeld = Input.GetButton("Crouch") || Input.GetKey(KeyCode.S);
        _dashPressed = Input.GetButtonDown("Fire3"); // 默认是Shift键
        _climbPressed = Input.GetButtonDown("Fire2"); // 默认是Ctrl键
        
        // 获取当前朝向
        if (Mathf.Abs(_inputX) > 0.01f && !_isWallSliding)
        {
            _spriteRenderer.flipX = _inputX < 0;
        }
    }
    
    private void CheckGround()
    {
        Vector2 checkPos = (Vector2)transform.position + groundCheckOffset;
        bool wasGrounded = _isGrounded;
        _isGrounded = Physics2D.OverlapBox(checkPos, groundCheckSize, 0f, groundLayer);
        
        if (_isGrounded && !wasGrounded)
        {
            // 刚落地
            _landingTimer = landingForce;
            _isCrouching = false;
            ResetClimbState();
        }
        
        if (_isGrounded)
        {
            _coyoteTimer = coyoteTime;
            _jumpCount = 0;
            
            // 落地时恢复冲刺次数
            if (_dashCount < maxDashCount)
            {
                _dashCount++;
            }
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }
    }
    
    private void CheckWall()
    {
        if (_isGrounded || _isClimbing) return;
        
        _isWallSliding = false;
        _wallDirection = 0;
        
        // 检测右侧墙壁
        Vector2 rightCheck = (Vector2)transform.position + wallCheckOffset;
        RaycastHit2D rightHit = Physics2D.BoxCast(rightCheck, wallCheckSize, 0f, Vector2.zero, 0f, groundLayer);
        
        // 检测左侧墙壁
        Vector2 leftCheck = (Vector2)transform.position - wallCheckOffset;
        RaycastHit2D leftHit = Physics2D.BoxCast(leftCheck, wallCheckSize, 0f, Vector2.zero, 0f, groundLayer);
        
        if (rightHit.collider != null && _inputX > 0)
        {
            _wallDirection = 1;
            _isWallSliding = _rb.velocity.y < 0;
        }
        else if (leftHit.collider != null && _inputX < 0)
        {
            _wallDirection = -1;
            _isWallSliding = _rb.velocity.y < 0;
        }
        
        if (_isWallSliding)
        {
            _isCrouching = false;
        }
    }
    
    private void CheckClimbable()
    {
        if (_isGrounded || _isDashing) return;
        
        _isGrabbing = false;
        
        // 检查前方是否有可攀爬的墙壁
        Vector2 checkPos = (Vector2)transform.position + (Vector2.right * _spriteRenderer.flipX ? -climbCheckDistance : climbCheckDistance);
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.zero, grabDistance, climbableLayer);
        
        if (hit.collider != null && Mathf.Abs(_inputY) > 0.01f)
        {
            _isGrabbing = true;
            _isClimbing = true;
            _isWallSliding = false;
            
            // 切换到攀爬物理
            _rb.gravityScale = 0;
            _rb.velocity = new Vector2(0, _inputY * climbSpeed);
        }
        else
        {
            if (_isClimbing)
            {
                ResetClimbState();
            }
        }
    }
    
    private void ResetClimbState()
    {
        _isClimbing = false;
        _isGrabbing = false;
        _rb.gravityScale = _defaultGravityScale;
    }
    
    private void HandleTimers()
    {
        if (_jumpBufferTimer > 0)
            _jumpBufferTimer -= Time.deltaTime;
            
        if (_wallJumpCooldownTimer > 0)
            _wallJumpCooldownTimer -= Time.deltaTime;
            
        if (_wallJumpBufferTimer > 0)
            _wallJumpBufferTimer -= Time.deltaTime;
            
        if (_dashCooldownTimer > 0)
            _dashCooldownTimer -= Time.deltaTime;
            
        if (_dashTimer > 0)
            _dashTimer -= Time.deltaTime;
            
        if (_stepTimer > 0)
            _stepTimer -= Time.deltaTime;
            
        if (_landingTimer > 0)
            _landingTimer -= Time.deltaTime;
            
        if (_jumpPressed)
            _jumpBufferTimer = jumpBufferTime;
            
        if (_climbPressed && _isGrabbing)
            _wallJumpBufferTimer = wallJumpBufferTime;
    }
    
    private void HandleJump()
    {
        if (_isClimbing && _jumpPressed)
        {
            // 攀爬跳跃
            PerformClimbJump();
            return;
        }
        
        // 墙壁跳跃
        if (_isWallSliding && (_jumpPressed || _wallJumpBufferTimer > 0) && _wallJumpCooldownTimer <= 0)
        {
            PerformWallJump();
            return;
        }
        
        // 普通跳跃
        if (_jumpBufferTimer > 0 && _coyoteTimer > 0)
        {
            PerformJump();
            return;
        }
        
        // 空中跳跃（二段跳）
        if (_jumpPressed && !_isGrounded && _coyoteTimer <= 0 && _jumpCount < maxJumpCount)
        {
            PerformJump();
            return;
        }
        
        // 可变跳跃高度
        if (_jumpReleased && _rb.velocity.y > 0)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * jumpCutMultiplier);
        }
        
        // 空中蓄力跳跃
        if (_jumpPressed && !_isGrounded && _jumpCount >= maxJumpCount)
        {
            _rb.velocity += new Vector2(0, jumpAddForce * Time.deltaTime);
        }
    }
    
    private void PerformJump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
        _jumpCount++;
        _coyoteTimer = 0;
        _jumpBufferTimer = 0;
        _wallJumpBufferTimer = 0;
    }
    
    private void PerformWallJump()
    {
        float forceX = -_wallDirection * wallJumpForceX;
        _rb.velocity = new Vector2(forceX, wallJumpForceY);
        _wallJumpCooldownTimer = wallJumpCooldown;
        _jumpCount = 0;
        _jumpBufferTimer = 0;
        
        // 翻转面朝方向
        transform.localScale = new Vector3(-_wallDirection, 1, 1);
    }
    
    private void PerformClimbJump()
    {
        _rb.velocity = new Vector2(0, climbJumpForce);
        ResetClimbState();
        _jumpCount = 0;
    }
    
    private void HandleDash()
    {
        if (_dashPressed && _dashCount > 0 && _dashCooldownTimer <= 0 && !_isClimbing)
        {
            StartDash();
        }
    }
    
    private void StartDash()
    {
        _isDashing = true;
        _dashCount--;
        _dashTimer = dashDuration;
        _dashCooldownTimer = dashCooldown;
        
        // 设置冲刺速度
        if (Mathf.Abs(_inputX) > 0.01f || Mathf.Abs(_inputY) > 0.01f)
        {
            _rb.velocity = new Vector2(_inputX, _inputY).normalized * dashSpeed;
        }
        else
        {
            // 如果没有输入，朝面朝方向冲刺
            _rb.velocity = new Vector2(_spriteRenderer.flipX ? -dashSpeed : dashSpeed, 0);
        }
    }
    
    private void HandleDashUpdate()
    {
        _dashTimer -= Time.deltaTime;
        
        if (_dashTimer <= 0)
        {
            _isDashing = false;
        }
    }
    
    private void HandleDashFixedUpdate()
    {
        // 冲刺期间保持速度
        if (_dashTimer <= 0)
        {
            _isDashing = false;
        }
    }
    
    private void HandleClimb()
    {
        if (_isClimbing)
        {
            // 攀爬时改变碰撞体大小
            _currentColliderSize = new Vector2(0.8f, 1.5f);
            _currentColliderOffset = new Vector2(0, -0.2f);
        }
        else
        {
            // 恢复正常碰撞体大小
            _currentColliderSize = _crouchHeld && _isGrounded ? crouchSize : standSize;
            _currentColliderOffset = _crouchHeld && _isGrounded ? new Vector2(0, -0.2f) : Vector2.zero;
        }
        
        // 平滑过渡碰撞体大小
        _boxCollider.size = Vector2.Lerp(_boxCollider.size, _currentColliderSize, Time.deltaTime / crouchTransitionTime);
        _boxCollider.offset = Vector2.Lerp(_boxCollider.offset, _currentColliderOffset, Time.deltaTime / crouchTransitionTime);
    }
    
    private void HandleMovement()
    {
        // 墙壁跳跃冷却期间限制输入
        if (_wallJumpCooldownTimer > 0)
        {
            return;
        }
        
        // 计算目标速度
        float targetSpeed = _isCrouching ? crouchSpeed : moveSpeed;
        float controlFactor = _isGrounded ? 1f : airControlFactor;
        
        // 攀爬时特殊处理
        if (_isClimbing)
        {
            targetSpeed = climbSpeed;
            controlFactor = 1f;
        }
        
        float targetVelocityX = _inputX * targetSpeed;
        float targetVelocityY = _inputY * (targetSpeed * 0.8f); // 垂直移动稍慢
        
        // 平滑加减速
        float accelRate = Mathf.Abs(targetVelocityX) > 0.01f ? acceleration : deceleration;
        float speedDiff = targetVelocityX - _rb.velocity.x;
        
        _rb.velocity = new Vector2(
            Mathf.MoveTowards(_rb.velocity.x, targetVelocityX, accelRate * controlFactor * Time.fixedDeltaTime),
            targetVelocityY
        );
        
        // 地面摩擦力
        if (_isGrounded && Mathf.Abs(_inputX) < 0.01f)
        {
            _rb.velocity *= (1 - friction * Time.fixedDeltaTime);
        }
        
        // 记录速度用于步进效果
        _lastVelocity = _rb.velocity;
    }
    
    private void HandleGravity()
    {
        // 墙壁滑行：降低下落速度
        if (_isWallSliding && !_isClimbing)
        {
            _rb.gravityScale = 0;
            _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -wallSlideSpeed));
        }
        else if (!_isClimbing)
        {
            _rb.gravityScale = _defaultGravityScale;
        }
    }
    
    private void HandleLandingEffects()
    {
        if (_isGrounded && _landingTimer > 0)
        {
            // 落地反馈效果
            _stepTimer = stepSmoothness;
        }
    }
    
    private void UpdateAnimationStates()
    {
        if (_animator != null)
        {
            _animator.SetBool("IsGrounded", _isGrounded);
            _animator.SetBool("IsCrouching", _isCrouching);
            _animator.SetBool("IsWallSliding", _isWallSliding);
            _animator.SetBool("IsClimbing", _isClimbing);
            _animator.SetBool("IsDashing", _isDashing);
            
            // 设置移动速度动画参数
            float speed = Mathf.Abs(_rb.velocity.x);
            _animator.SetFloat("Speed", speed);
            
            // 设置跳跃动画参数
            if (_jumpCount > 0)
            {
                _animator.SetInteger("JumpCount", _jumpCount);
            }
        }
    }
    
    // 用于获取当前冲刺次数
    public int GetDashCount()
    {
        return _dashCount;
    }
    
    // 用于重置冲刺次数（例如在特定区域）
    public void ResetDashCount()
    {
        _dashCount = maxDashCount;
    }
    
    // 用于检测是否可以冲刺
    public bool CanDash()
    {
        return _dashCount > 0 && _dashCooldownTimer <= 0 && !_isClimbing;
    }
    
    // 调试可视化
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            transform.position + groundCheckOffset,
            groundCheckSize
        );
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(
            transform.position + wallCheckOffset,
            wallCheckSize
        );
        Gizmos.DrawWireCube(
            transform.position - wallCheckOffset,
            wallCheckSize
        );
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(
            transform.position + (Vector2.right * _spriteRenderer.flipX ? -climbCheckDistance : climbCheckDistance),
            new Vector2(grabDistance, 0.2f)
        );
    }
    
    // 私有辅助方法
    private bool IsMoving => Mathf.Abs(_rb.velocity.x) > 0.01f || Mathf.Abs(_rb.velocity.y) > 0.01f;
    
    // 用于触发动画事件
    public void OnLanding()
    {
        _landingTimer = landingForce;
        if (_animator != null)
        {
            _animator.SetTrigger("Landed");
        }
    }
    
    // 用于触发冲刺动画事件
    public void OnDashStart()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Dash");
        }
    }
}