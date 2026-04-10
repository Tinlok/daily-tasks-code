using UnityEngine;

/// <summary>
/// 可攀爬表面标记组件
///
/// 将此组件添加到梯子、藤蔓、绳索等GameObject上
/// 使其可以被PlayerController2D识别为可攀爬表面
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ClimbableSurface : MonoBehaviour
{
    #region Settings

    [Header("攀爬设置")]
    [Tooltip("攀爬速度倍率（1=正常速度）")]
    [Range(0.1f, 3f)]
    [SerializeField] private float climbSpeedMultiplier = 1f;

    [Tooltip("是否可以水平攀爬")]
    [SerializeField] private bool allowHorizontalClimb = false;

    [Tooltip("攀爬时体力消耗倍率")]
    [Range(0f, 2f)]
    [SerializeField] private float staminaDrainMultiplier = 1f;

    [Header("可视化")]
    [Tooltip("调试模式下显示攀爬区域")]
    [SerializeField] private bool showDebugGizmos = true;

    [Tooltip("调试颜色")]
    [SerializeField] private Color debugColor = new Color(0f, 1f, 0.5f, 0.3f);

    #endregion

    #region Private Variables

    private Collider2D _collider;

    #endregion

    #region Public Properties

    /// <summary>攀爬速度倍率</summary>
    public float ClimbSpeedMultiplier => climbSpeedMultiplier;

    /// <summary>是否可以水平攀爬</summary>
    public bool AllowHorizontalClimb => allowHorizontalClimb;

    /// <summary>体力消耗倍率</summary>
    public float StaminaDrainMultiplier => staminaDrainMultiplier;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();

        // 确保碰撞体是触发器
        if (!_collider.isTrigger)
        {
            Debug.LogWarning($"ClimbableSurface on {gameObject.name}: Collider2D should be set as Trigger!");
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = debugColor;

        // 根据碰撞体类型绘制
        switch (col)
        {
            case BoxCollider2D box:
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.offset, box.size);
                break;

            case CircleCollider2D circle:
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
                break;

            case CapsuleCollider2D capsule:
                Gizmos.DrawWireSphere(transform.position + (Vector3)capsule.offset + Vector3.up * (capsule.size / 2 - capsule.radius), capsule.radius);
                Gizmos.DrawWireSphere(transform.position + (Vector3)capsule.offset - Vector3.up * (capsule.size / 2 - capsule.radius), capsule.radius);
                break;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 设置攀爬速度倍率
    /// </summary>
    public void SetClimbSpeedMultiplier(float multiplier)
    {
        climbSpeedMultiplier = Mathf.Max(0.1f, multiplier);
    }

    /// <summary>
    /// 设置体力消耗倍率
    /// </summary>
    public void SetStaminaDrainMultiplier(float multiplier)
    {
        staminaDrainMultiplier = Mathf.Max(0f, multiplier);
    }

    #endregion
}
