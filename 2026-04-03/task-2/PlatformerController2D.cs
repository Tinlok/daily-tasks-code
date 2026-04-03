using UnityEngine;

/// <summary>
/// 2D Platformer Character Controller
/// 支持移动、跳跃、二段跳、下蹲、墙壁滑行、蹬墙跳
/// </summary>
public class PlatformerController2D : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 8f;
    [SerializeField] private float airControlFactor = 0.7f;

    [Header("跳跃设置")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private int maxJumpCount = 2;

    [Header("下蹲设置")]
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private Vector2 crouchSize = new Vector2(1f, 0.7f);
    [SerializeField] private Vector2 standSize = new Vector2(1f, 1.8f);

    [Header("墙壁交互")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallJumpForceX = 8f;
    [SerializeField] private float wallJumpForceY = 10f;
    [SerializeField] private float wallJumpCooldown = 0.2f;

    [Header("地面检测")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, -0.9f, 0);

    [Header("墙壁检测")]
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.6f);
    [SerializeField] private Vector3 wallCheckOffset = new Vector3(0.5f, 0.2f, 0);

    // 组件引用
    private Rigidbody2D _rb;
    private BoxCollider2D _boxCollider;
    private SpriteRenderer _spriteRenderer;

    // 状态变量
    private Vector2 _velocity;
    private bool _isGrounded;
    private bool _isCrouching;
    private bool _isWallSliding;
    private int _wallDirection;
    private int _jumpCount;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _wallJumpCooldownTimer;
    private float _defaultGravityScale;

    // 输入
    private float _inputX;
    private bool _jumpPressed;
    private bool _jumpReleased;
    private bool _crouchHeld;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultGravityScale = _rb.gravityScale;
    }

    private void Update()
    {
        GatherInput();
        CheckGround();
        CheckWall();
        HandleTimers();
        HandleJump();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleGravity();
    }

    private void GatherInput()
    {
        _inputX = Input.GetAxisRaw("Horizontal");
        _jumpPressed = Input.GetButtonDown("Jump");
        _jumpReleased = Input.GetButtonUp("Jump");
        _crouchHeld = Input.GetButton("Crouch") || Input.GetKey(KeyCode.S);
    }

    private void CheckGround()
    {
        Vector2 checkPos = (Vector2)transform.position + groundCheckOffset;
        _isGrounded = Physics2D.OverlapBox(checkPos, groundCheckSize, 0f, groundLayer);

        if (_isGrounded)
        {
            _coyoteTimer = coyoteTime;
            _jumpCount = 0;
            _isCrouching = false;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }
    }

    private void CheckWall()
    {
        _isWallSliding = false;
        _wallDirection = 0;

        if (_isGrounded || _rb.velocity.y >= 0) return;

        // 检测右侧墙壁
        Vector2 rightCheck = (Vector2)transform.position + wallCheckOffset;
        if (Physics2D.OverlapBox(rightCheck, wallCheckSize, 0f, groundLayer))
        {
            _wallDirection = 1;
        }

        // 检测左侧墙壁
        Vector2 leftCheck = (Vector2)transform.position - wallCheckOffset;
        if (Physics2D.OverlapBox(leftCheck, wallCheckSize, 0f, groundLayer))
        {
            _wallDirection = -1;
        }

        _isWallSliding = _wallDirection != 0 && _inputX == _wallDirection;
    }

    private void HandleTimers()
    {
        if (_jumpBufferTimer > 0)
            _jumpBufferTimer -= Time.deltaTime;

        if (_wallJumpCooldownTimer > 0)
            _wallJumpCooldownTimer -= Time.deltaTime;

        if (_jumpPressed)
            _jumpBufferTimer = jumpBufferTime;
    }

    private void HandleJump()
    {
        // 墙壁跳跃
        if (_isWallSliding && _jumpPressed && _wallJumpCooldownTimer <= 0)
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
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * 0.5f);
        }
    }

    private void PerformJump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
        _jumpCount++;
        _coyoteTimer = 0;
        _jumpBufferTimer = 0;
    }

    private void PerformWallJump()
    {
        float forceX = -_wallDirection * wallJumpForceX;
        _rb.velocity = new Vector2(forceX, wallJumpForceY);
        _wallJumpCooldownTimer = wallJumpCooldown;
        _jumpCount = 0;

        // 翻转面朝方向
        transform.localScale = new Vector3(-_wallDirection, 1, 1);
    }

    private void HandleMovement()
    {
        // 墙壁跳跃冷却期间限制输入
        if (_wallJumpCooldownTimer > 0)
        {
            return;
        }

        // 下蹲
        if (_crouchHeld && _isGrounded)
        {
            _isCrouching = true;
            _boxCollider.size = crouchSize;
            _boxCollider.offset = new Vector2(0, -0.2f);
        }
        else if (!_crouchHeld)
        {
            _isCrouching = false;
            _boxCollider.size = standSize;
            _boxCollider.offset = Vector2.zero;
        }

        // 计算目标速度
        float targetSpeed = _isCrouching ? crouchSpeed : moveSpeed;
        float controlFactor = _isGrounded ? 1f : airControlFactor;
        float targetVelocityX = _inputX * targetSpeed;

        // 平滑加减速
        float accelRate = Mathf.Abs(targetVelocityX) > 0.01f ? acceleration : deceleration;
        float speedDiff = targetVelocityX - _rb.velocity.x;
        _rb.velocity = new Vector2(
            Mathf.MoveTowards(_rb.velocity.x, targetVelocityX, accelRate * controlFactor * Time.fixedDeltaTime),
            _rb.velocity.y
        );

        // 翻转朝向
        if (Mathf.Abs(_inputX) > 0.01f && !_isWallSliding)
        {
            _spriteRenderer.flipX = _inputX < 0;
        }
    }

    private void HandleGravity()
    {
        // 墙壁滑行：降低下落速度
        if (_isWallSliding)
        {
            _rb.gravityScale = 0;
            _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -wallSlideSpeed));
        }
        else
        {
            _rb.gravityScale = _defaultGravityScale;
        }
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
    }
}
