using UnityEngine;

/// <summary>
/// 玩家输入管理器
///
/// 提供统一的输入接口，支持键盘和游戏手柄
/// 可通过事件系统与PlayerController2D解耦
///
/// 输入映射:
/// - 移动: WASD / 方向键 / 左摇杆
/// - 跳跃: Space / A 按钮 / 南按钮
/// - 冲刺: Left Shift / B 按钮 / 东按钮
/// - 下蹲: Ctrl / S / 左摇杆按下
/// - 攀爬: W/S / 方向键上下 / 右摇杆上下
/// </summary>
public class PlayerInputManager : MonoBehaviour
{
    #region Settings

    [Header("输入设置")]
    [Tooltip("输入死区（摇杆漂移处理）")]
    [Range(0f, 0.5f)]
    [SerializeField] private float deadZone = 0.1f;

    [Tooltip("输入灵敏度")]
    [Range(0.1f, 2f)]
    [SerializeField] private float inputSensitivity = 1f;

    [Tooltip("是否启用缓冲输入")]
    [SerializeField] private bool enableInputBuffer = true;

    [Tooltip("输入缓冲时间（秒）")]
    [SerializeField] private float bufferTime = 0.1f;

    [Tooltip("是否使用新输入系统")]
    [SerializeField] private bool useNewInputSystem = false;

    #endregion

    #region Events

    /// <summary>移动输入事件（-1到1）</summary>
    public event System.Action<float> OnMove;

    /// <summary>垂直输入事件（-1到1）</summary>
    public event System.Action<float> OnVerticalMove;

    /// <summary>跳跃按下事件</summary>
    public event System.Action OnJumpPressed;

    /// <summary>跳跃释放事件</summary>
    public event System.Action OnJumpReleased;

    /// <summary>跳跃持续事件</summary>
    public event System.Action<bool> OnJumpHeld;

    /// <summary>冲刺按下事件</summary>
    public event System.Action OnDashPressed;

    /// <summary>下蹲状态改变事件</summary>
    public event System.Action<bool> OnCrouchChanged;

    /// <summary>交互按下事件</summary>
    public event System.Action OnInteractPressed;

    /// <summary>暂停按下事件</summary>
    public event System.Action OnPausePressed;

    #endregion

    #region Private Variables

    private float _horizontalInput;
    private float _verticalInput;
    private bool _jumpHeld;
    private bool _crouchHeld;
    private Vector2 _rawInput;

    // 输入缓冲
    private float _jumpBufferTimer;
    private bool _jumpBuffered;

    #endregion

    #region Public Properties

    /// <summary>当前水平输入（-1到1）</summary>
    public float HorizontalInput => _horizontalInput;

    /// <summary>当前垂直输入（-1到1）</summary>
    public float VerticalInput => _verticalInput;

    /// <summary>原始输入向量</summary>
    public Vector2 RawInput => _rawInput;

    /// <summary>是否正在跳跃</summary>
    public bool IsJumping => _jumpHeld;

    /// <summary>是否正在下蹲</summary>
    public bool IsCrouching => _crouchHeld;

    #endregion

    #region Unity Lifecycle

    private void Update()
    {
        GatherInput();
        ProcessInput();
        HandleInputBuffer();
    }

    #endregion

    #region Input Gathering

    /// <summary>
    /// 收集原始输入数据
    /// </summary>
    private void GatherInput()
    {
        // 水平输入
        float rawHorizontal = Input.GetAxisRaw("Horizontal");

        // 垂直输入
        float rawVertical = Input.GetAxisRaw("Vertical");

        // 应用死区
        _horizontalInput = ApplyDeadZone(rawHorizontal);
        _verticalInput = ApplyDeadZone(rawVertical);

        // 应用灵敏度
        _horizontalInput *= inputSensitivity;
        _verticalInput *= inputSensitivity;

        // 钳制到-1到1范围
        _horizontalInput = Mathf.Clamp(_horizontalInput, -1f, 1f);
        _verticalInput = Mathf.Clamp(_verticalInput, -1f, 1f);

        _rawInput = new Vector2(_horizontalInput, _verticalInput);

        // 按钮输入
        bool jumpPressed = Input.GetButtonDown("Jump");
        bool jumpReleased = Input.GetButtonUp("Jump");
        _jumpHeld = Input.GetButton("Jump");

        bool dashPressed = Input.GetButtonDown("Dash");

        bool crouchPressed = Input.GetButton("Crouch") || Input.GetKey(KeyCode.S);
        bool crouchReleased = !crouchPressed;

        bool interactPressed = Input.GetButtonDown("Interact") || Input.GetKeyDown(KeyCode.E);
        bool pausePressed = Input.GetButtonDown("Pause") || Input.GetKeyDown(KeyCode.Escape);

        // 更新下蹲状态
        if (crouchPressed != _crouchHeld)
        {
            _crouchHeld = crouchPressed;
            OnCrouchChanged?.Invoke(_crouchHeld);
        }

        // 触发事件
        if (jumpPressed)
        {
            if (enableInputBuffer)
            {
                _jumpBuffered = true;
                _jumpBufferTimer = bufferTime;
            }
            OnJumpPressed?.Invoke();
        }

        if (jumpReleased)
        {
            OnJumpReleased?.Invoke();
        }

        OnJumpHeld?.Invoke(_jumpHeld);

        if (dashPressed)
        {
            OnDashPressed?.Invoke();
        }

        if (interactPressed)
        {
            OnInteractPressed?.Invoke();
        }

        if (pausePressed)
        {
            OnPausePressed?.Invoke();
        }
    }

    /// <summary>
    /// 应用死区处理
    /// </summary>
    private float ApplyDeadZone(float value)
    {
        if (Mathf.Abs(value) < deadZone)
        {
            return 0f;
        }

        // 重新缩放输入，使死区后的输入平滑过渡
        return (value - (Mathf.Sign(value) * deadZone)) / (1f - deadZone);
    }

    #endregion

    #region Input Processing

    /// <summary>
    /// 处理并发送输入事件
    /// </summary>
    private void ProcessInput()
    {
        // 移动输入
        if (Mathf.Abs(_horizontalInput) > 0.01f)
        {
            OnMove?.Invoke(_horizontalInput);
        }

        // 垂直输入
        if (Mathf.Abs(_verticalInput) > 0.01f)
        {
            OnVerticalMove?.Invoke(_verticalInput);
        }
    }

    /// <summary>
    /// 处理输入缓冲
    /// </summary>
    private void HandleInputBuffer()
    {
        if (!enableInputBuffer) return;

        if (_jumpBuffered)
        {
            _jumpBufferTimer -= Time.deltaTime;

            if (_jumpBufferTimer <= 0)
            {
                _jumpBuffered = false;
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 检查是否有跳跃缓冲输入
    /// </summary>
    public bool HasJumpBuffer()
    {
        return _jumpBuffered;
    }

    /// <summary>
    /// 消耗跳跃缓冲输入
    /// </summary>
    public void ConsumeJumpBuffer()
    {
        _jumpBuffered = false;
        _jumpBufferTimer = 0;
    }

    /// <summary>
    /// 禁用输入（用于过场动画等）
    /// </summary>
    public void DisableInput()
    {
        enabled = false;
        _horizontalInput = 0;
        _verticalInput = 0;
        _rawInput = Vector2.zero;
    }

    /// <summary>
    /// 启用输入
    /// </summary>
    public void EnableInput()
    {
        enabled = true;
    }

    /// <summary>
    /// 重置所有输入状态
    /// </summary>
    public void ResetInput()
    {
        _horizontalInput = 0;
        _verticalInput = 0;
        _rawInput = Vector2.zero;
        _jumpHeld = false;
        _crouchHeld = false;
        _jumpBuffered = false;
        _jumpBufferTimer = 0;
    }

    #endregion
}
