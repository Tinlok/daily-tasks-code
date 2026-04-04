using UnityEngine;

/// <summary>
/// 可攀爬表面 - 标记哪些表面可以被玩家攀爬
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ClimbableSurface : MonoBehaviour
{
    [Header("攀爬设置")]
    [SerializeField] private float climbForce = 10f;
    [SerializeField] private bool isOneWay = false;
    [SerializeField] private AudioClip climbSound;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        // 确保碰撞体设置为触发器
        Collider2D collider = GetComponent<Collider2D>();
        if (!collider.isTrigger)
        {
            collider.isTrigger = true;
        }
        
        // 添加音频源
        audioSource = gameObject.AddComponent<AudioSource>();
        if (climbSound != null)
        {
            audioSource.clip = climbSound;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否有玩家控制器
        PlayerController2D_Enhanced player = other.GetComponent<PlayerController2D_Enhanced>();
        if (player != null)
        {
            // 检查玩家是否可以攀爬
            if (player.CanClimb())
            {
                // 可以攀爬
                Debug.Log("Player can climb on this surface");
            }
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerController2D_Enhanced player = other.GetComponent<PlayerController2D_Enhanced>();
        if (player != null)
        {
            // 播放攀爬声音
            if (climbSound != null && audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}