using UnityEngine;

/// <summary>
/// 地面检测 - 检测玩家是否在地面上
/// </summary>
public class GroundCheck : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float raycastDistance = 0.2f;
    
    [Header("调试")]
    [SerializeField] private bool showGizmos = true;
    
    private bool _isGrounded;
    
    /// <summary>
    /// 是否在地面上
    /// </summary>
    public bool IsGrounded => _isGrounded;
    
    private void Update()
    {
        CheckGround();
    }
    
    private void CheckGround()
    {
        // 使用圆形检测
        _isGrounded = Physics2D.OverlapCircle(
            groundCheckPoint.position,
            groundCheckRadius,
            groundLayers
        );
        
        // 备用射线检测
        if (!_isGrounded)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                groundCheckPoint.position,
                Vector2.down,
                raycastDistance,
                groundLayers
            );
            
            _isGrounded = hit.collider != null;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // 绘制地面检测范围
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
            
            // 绘制射线
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                groundCheckPoint.position,
                groundCheckPoint.position + Vector2.down * raycastDistance
            );
        }
    }
}