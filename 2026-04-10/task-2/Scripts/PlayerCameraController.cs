using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家跟随相机控制器
///
/// 特性:
/// - 平滑跟随玩家
/// - 可配置的跟随区域限制
/// - 死区设置（玩家在中心区域时相机不移动）
/// - 视界边缘预测（根据移动方向提前移动相机）
/// - 冲刺时的特殊效果
/// - 屏幕震动
/// </summary>
[ExecuteInEditMode]
public class PlayerCameraController : MonoBehaviour
{
    #region Settings

    [Header("目标设置")]
    [Tooltip("要跟随的目标")]
    [SerializeField] private Transform target;

    [Tooltip("跟随偏移")]
    [SerializeField] private Vector3 followOffset = new(0, 0, -10);

    [Header("平滑设置")]
    [Tooltip("位置平滑速度（较小值更平滑）")]
    [Range(0.1f, 10f)]
    [SerializeField] private float positionSmoothSpeed = 3f;

    [Tooltip("使用阻尼平滑（更自然的跟随效果）")]
    [SerializeField] private bool useDamping = true;

    [Tooltip("阻尼系数（0-1，越小跟随越慢）")]
    [Range(0.01f, 1f)]
    [SerializeField] private float dampingFactor = 0.15f;

    [Header("死区设置")]
    [Tooltip("启用死区（玩家在中心区域时相机不移动）")]
    [SerializeField] private bool useDeadZone = true;

    [Tooltip("死区大小")]
    [SerializeField] private Vector2 deadZoneSize = new(2f, 1.5f);

    [Header("视界预测")]
    [Tooltip("启用视界边缘预测")]
    [SerializeField] private bool useLookAhead = true;

    [Tooltip("预测距离")]
    [SerializeField] private float lookAheadDistance = 2f;

    [Tooltip("预测平滑速度")]
    [SerializeField] private float lookAheadSmoothSpeed = 5f;

    [Header("边界限制")]
    [Tooltip("限制相机在指定区域内")]
    [SerializeField] private bool useBounds = false;

    [Tooltip("边界左下角")]
    [SerializeField] private Vector2 boundsMin = new(-50, -20);

    [Tooltip("边界右上角")]
    [SerializeField] private Vector2 boundsMax = new(50, 20);

    [Header("缩放设置")]
    [Tooltip("允许动态缩放")]
    [SerializeField] private bool allowZoom = true;

    [Tooltip("基础缩放")]
    [SerializeField] private float baseZoom = 5f;

    [Tooltip("缩放平滑速度")]
    [SerializeField] private float zoomSmoothSpeed = 3f;

    [Header("冲刺效果")]
    [Tooltip("冲刺时的视场角拉伸")]
    [SerializeField] private float dashFOVIncrease = 1f;

    [Tooltip("冲刺FOV变化速度")]
    [SerializeField] private float dashFOVSpeed = 5f;

    [Header("屏幕震动")]
    [Tooltip("震动强度")]
    [SerializeField] private float shakeIntensity = 0.5f;

    [Tooltip("震动持续时间")]
    [SerializeField] private float shakeDuration = 0.3f;

    [Tooltip("震动衰减速度")]
    [SerializeField] private float shakeDecay = 5f;

    #endregion

    #region Private Variables

    private Camera _camera;
    private Vector3 _velocity;
    private Vector3 _targetPosition;
    private Vector2 _lookAheadOffset;
    private Vector2 _currentLookAhead;
    private float _currentZoom;
    private float _targetZoom;
    private Vector3 _shakeOffset;

    // 用于视界预测
    private Vector2 _previousTargetPosition;
    private Vector2 _targetVelocity;

    #endregion

    #region Public Properties

    /// <summary>当前目标</summary>
    public Transform Target
    {
        get => target;
        set => target = value;
    }

    /// <summary>相机是否正在震动</summary>
    public bool IsShaking { get; private set; }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            _camera = gameObject.AddComponent<Camera>();
        }

        _currentZoom = baseZoom;
        _targetZoom = baseZoom;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        UpdateTargetVelocity();
        CalculateTargetPosition();
        ApplyDeadZone();
        ApplyLookAhead();
        ApplyBounds();
        ApplyZoom();
        ApplyShake();

        if (useDamping)
        {
            ApplyDampingMovement();
        }
        else
        {
            ApplySmoothMovement();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 绘制死区
        if (useDeadZone)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Vector3 center = transform.position + (Vector3)followOffset;
            Gizmos.DrawWireCube(center, deadZoneSize);
        }

        // 绘制边界
        if (useBounds)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Vector3 center = new Vector3(
                (boundsMin.x + boundsMax.x) / 2,
                (boundsMin.y + boundsMax.y) / 2,
                0
            );
            Vector3 size = new Vector3(
                boundsMax.x - boundsMin.x,
                boundsMax.y - boundsMin.y,
                0
            );
            Gizmos.DrawWireCube(center, size);
        }
    }
#endif

    #endregion

    #region Movement

    /// <summary>
    /// 更新目标速度（用于视界预测）
    /// </summary>
    private void UpdateTargetVelocity()
    {
        if (target == null) return;

        Vector2 currentPos = target.position;
        _targetVelocity = (currentPos - _previousTargetPosition) / Time.deltaTime;
        _previousTargetPosition = currentPos;
    }

    /// <summary>
    /// 计算目标位置
    /// </summary>
    private void CalculateTargetPosition()
    {
        _targetPosition = target.position + followOffset;
    }

    /// <summary>
    /// 应用死区
    /// </summary>
    private void ApplyDeadZone()
    {
        if (!useDeadZone) return;

        Vector3 currentPos = transform.position;
        Vector3 diff = _targetPosition - currentPos;

        // 检查是否超出死区
        if (Mathf.Abs(diff.x) < deadZoneSize.x / 2 && Mathf.Abs(diff.y) < deadZoneSize.y / 2)
        {
            // 在死区内，保持当前位置
            _targetPosition = currentPos;
        }
        else
        {
            // 超出死区，移动到死区边缘
            float newX = Mathf.Abs(diff.x) > deadZoneSize.x / 2
                ? _targetPosition.x
                : currentPos.x;
            float newY = Mathf.Abs(diff.y) > deadZoneSize.y / 2
                ? _targetPosition.y
                : currentPos.y;
            _targetPosition = new Vector3(newX, newY, currentPos.z);
        }
    }

    /// <summary>
    /// 应用视界预测
    /// </summary>
    private void ApplyLookAhead()
    {
        if (!useLookAhead) return;

        // 根据目标速度计算预测偏移
        _lookAheadOffset = new Vector2(
            Mathf.Sign(_targetVelocity.x) * lookAheadDistance * Mathf.Clamp01(Mathf.Abs(_targetVelocity.x) / 10f),
            Mathf.Sign(_targetVelocity.y) * lookAheadDistance * 0.3f * Mathf.Clamp01(Mathf.Abs(_targetVelocity.y) / 10f)
        );

        // 平滑过渡
        _currentLookAhead = Vector2.Lerp(
            _currentLookAhead,
            _lookAheadOffset,
            lookAheadSmoothSpeed * Time.deltaTime
        );

        _targetPosition += (Vector3)_currentLookAhead;
    }

    /// <summary>
    /// 应用边界限制
    /// </summary>
    private void ApplyBounds()
    {
        if (!useBounds) return;

        // 获取相机在世界空间中的视口大小
        float height = _camera.orthographicSize * 2;
        float width = height * _camera.aspect;

        // 限制位置
        _targetPosition.x = Mathf.Clamp(
            _targetPosition.x,
            boundsMin.x + width / 2,
            boundsMax.x - width / 2
        );
        _targetPosition.y = Mathf.Clamp(
            _targetPosition.y,
            boundsMin.y + height / 2,
            boundsMax.y - height / 2
        );
    }

    /// <summary>
    /// 应用平滑移动
    /// </summary>
    private void ApplySmoothMovement()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            _targetPosition,
            ref _velocity,
            1f / positionSmoothSpeed
        );
    }

    /// <summary>
    /// 应用阻尼移动
    /// </summary>
    private void ApplyDampingMovement()
    {
        Vector3 diff = _targetPosition - transform.position;
        _velocity = diff * dampingFactor;
        transform.position += _velocity + _shakeOffset;
    }

    #endregion

    #region Zoom

    /// <summary>
    /// 应用缩放
    /// </summary>
    private void ApplyZoom()
    {
        if (!allowZoom) return;

        _camera.orthographicSize = Mathf.Lerp(
            _camera.orthographicSize,
            _targetZoom,
            zoomSmoothSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// 设置目标缩放
    /// </summary>
    public void SetZoom(float zoom)
    {
        _targetZoom = Mathf.Clamp(zoom, 1f, 20f);
    }

    /// <summary>
    /// 瞬时缩放效果（冲刺时使用）
    /// </summary>
    public void ApplyDashZoom(bool isDashing)
    {
        _targetZoom = isDashing
            ? baseZoom - dashFOVIncrease
            : baseZoom;
    }

    /// <summary>
    /// 重置缩放到基础值
    /// </summary>
    public void ResetZoom()
    {
        _targetZoom = baseZoom;
    }

    #endregion

    #region Screen Shake

    /// <summary>
    /// 应用震动
    /// </summary>
    private void ApplyShake()
    {
        if (!IsShaking) return;

        // 生成随机震动偏移
        _shakeOffset = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0
        ) * shakeIntensity;

        transform.position += _shakeOffset;
    }

    /// <summary>
    /// 触发屏幕震动
    /// </summary>
    public void Shake(float intensity = -1f, float duration = -1f)
    {
        shakeIntensity = intensity > 0 ? intensity : shakeIntensity;
        shakeDuration = duration > 0 ? duration : shakeDuration;
        StartCoroutine(ShakeCoroutine());
    }

    /// <summary>
    /// 震动协程
    /// </summary>
    private IEnumerator ShakeCoroutine()
    {
        IsShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float currentIntensity = Mathf.Lerp(shakeIntensity, 0, elapsed / shakeDuration);
            shakeIntensity = currentIntensity;
            yield return null;
        }

        IsShaking = false;
        shakeIntensity = 0.5f; // 重置为默认值
        _shakeOffset = Vector3.zero;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 立即移动相机到目标位置（无动画）
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;

        transform.position = target.position + followOffset;
        _velocity = Vector3.zero;
        _currentLookAhead = Vector2.zero;
    }

    /// <summary>
    /// 设置边界
    /// </summary>
    public void SetBounds(Vector2 min, Vector2 max)
    {
        boundsMin = min;
        boundsMax = max;
        useBounds = true;
    }

    /// <summary>
    /// 清除边界限制
    /// </summary>
    public void ClearBounds()
    {
        useBounds = false;
    }

    /// <summary>
    /// 暂停相机跟随
    /// </summary>
    public void Pause()
    {
        enabled = false;
    }

    /// <summary>
    /// 恢复相机跟随
    /// </summary>
    public void Resume()
    {
        enabled = true;
    }

    #endregion
}
