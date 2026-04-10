using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 玩家音效控制器
///
/// 管理角色移动、跳跃、冲刺等动作的音效播放
/// 支持音效随机化和3D空间音效
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerAudioController : MonoBehaviour
{
    #region Settings

    [Header("音频源设置")]
    [Tooltip("是否使用3D空间音效")]
    [SerializeField] private bool use3DSound = false;

    [Tooltip("音效音量")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    [Header("脚步音效")]
    [Tooltip("脚步音效列表（随机选择）")]
    [SerializeField] private List<AudioClip> footstepSounds = new();

    [Tooltip("脚步音效间隔（秒）")]
    [SerializeField] private float footstepInterval = 0.4f;

    [Tooltip("奔跑时脚步间隔")]
    [SerializeField] private float runFootstepInterval = 0.25f;

    [Header("跳跃音效")]
    [Tooltip("跳跃音效")]
    [SerializeField] private AudioClip jumpSound;

    [Tooltip("落地音效")]
    [SerializeField] private AudioClip landSound;

    [Tooltip("二段跳音效")]
    [SerializeField] private AudioClip doubleJumpSound;

    [Header("冲刺音效")]
    [Tooltip("冲刺音效")]
    [SerializeField] private AudioClip dashSound;

    [Tooltip("冲刺冷却完成音效")]
    [SerializeField] private AudioClip dashReadySound;

    [Header("墙壁交互音效")]
    [Tooltip("墙壁滑行音效")]
    [SerializeField] private AudioClip wallSlideSound;

    [Tooltip("蹬墙跳音效")]
    [SerializeField] private AudioClip wallJumpSound;

    [Header("攀爬音效")]
    [Tooltip("攀爬音效循环")]
    [SerializeField] private AudioClip climbSound;

    [Tooltip("攀爬音效间隔")]
    [SerializeField] private float climbSoundInterval = 0.3f;

    [Header("受伤音效")]
    [Tooltip("受伤音效列表")]
    [SerializeField] private List<AudioClip> hurtSounds = new();

    [Tooltip("死亡音效")]
    [SerializeField] private AudioClip deathSound;

    #endregion

    #region Private Variables

    private AudioSource _audioSource;
    private AudioSource _loopAudioSource; // 用于循环播放（如墙壁滑行）
    private PlayerController2D _player;
    private float _footstepTimer;
    private float _climbSoundTimer;
    private bool _wasGrounded;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        // 创建循环音效源
        _loopAudioSource = gameObject.AddComponent<AudioSource>();
        _loopAudioSource.playOnAwake = false;
        _loopAudioSource.loop = true;

        // 配置音频源
        ConfigureAudioSources();
    }

    private void Start()
    {
        _player = GetComponent<PlayerController2D>();
        if (_player == null)
        {
            Debug.LogWarning("PlayerAudioController: PlayerController2D not found on same GameObject!");
        }
    }

    private void Update()
    {
        if (_player == null) return;

        HandleFootstepSounds();
        HandleGroundedSounds();
        HandleWallSlideSound();
        HandleClimbSound();
    }

    #endregion

    #region Configuration

    /// <summary>
    /// 配置音频源设置
    /// </summary>
    private void ConfigureAudioSources()
    {
        _audioSource.playOnAwake = false;

        if (use3DSound)
        {
            _audioSource.spatialBlend = 1f;
            _loopAudioSource.spatialBlend = 1f;
        }
        else
        {
            _audioSource.spatialBlend = 0f;
            _loopAudioSource.spatialBlend = 0f;
        }
    }

    #endregion

    #region Sound Playback

    /// <summary>
    /// 播放音效（带音量控制）
    /// </summary>
    private void PlaySound(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;

        _audioSource.PlayOneShot(clip, masterVolume * volumeMultiplier);
    }

    /// <summary>
    /// 从列表中随机播放音效
    /// </summary>
    private void PlayRandomSound(List<AudioClip> clips, float volumeMultiplier = 1f)
    {
        if (clips == null || clips.Count == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Count)];
        PlaySound(clip, volumeMultiplier);
    }

    #endregion

    #region Footstep Sounds

    /// <summary>
    /// 处理脚步音效
    /// </summary>
    private void HandleFootstepSounds()
    {
        if (!_player.IsGrounded) return;

        // 检查是否在移动
        bool isMoving = Mathf.Abs(_player.Velocity.x) > 0.1f;

        if (!isMoving)
        {
            _footstepTimer = 0;
            return;
        }

        // 根据速度选择间隔
        float interval = Mathf.Abs(_player.Velocity.x) > 6f ? runFootstepInterval : footstepInterval;

        _footstepTimer += Time.deltaTime;

        if (_footstepTimer >= interval)
        {
            _footstepTimer = 0;
            PlayRandomSound(footstepSounds);
        }
    }

    #endregion

    #region Grounded Sounds

    /// <summary>
    /// 处理落地音效
    /// </summary>
    private void HandleGroundedSounds()
    {
        bool isGrounded = _player.IsGrounded;

        // 检测落地瞬间
        if (isGrounded && !_wasGrounded)
        {
            PlaySound(landSound);
        }

        _wasGrounded = isGrounded;
    }

    #endregion

    #region Jump Sounds

    /// <summary>
    /// 播放跳跃音效
    /// </summary>
    public void PlayJumpSound()
    {
        PlaySound(jumpSound);
    }

    /// <summary>
    /// 播放二段跳音效
    /// </summary>
    public void PlayDoubleJumpSound()
    {
        PlaySound(doubleJumpSound);
    }

    #endregion

    #region Dash Sounds

    /// <summary>
    /// 播放冲刺音效
    /// </summary>
    public void PlayDashSound()
    {
        PlaySound(dashSound);
    }

    /// <summary>
    /// 播放冲刺就绪音效
    /// </summary>
    public void PlayDashReadySound()
    {
        PlaySound(dashReadySound);
    }

    #endregion

    #region Wall Sounds

    /// <summary>
    /// 处理墙壁滑行音效
    /// </summary>
    private void HandleWallSlideSound()
    {
        if (wallSlideSound == null) return;

        if (_player.IsWallSliding && !_loopAudioSource.isPlaying)
        {
            _loopAudioSource.clip = wallSlideSound;
            _loopAudioSource.volume = masterVolume * 0.5f;
            _loopAudioSource.Play();
        }
        else if (!_player.IsWallSliding && _loopAudioSource.isPlaying)
        {
            _loopAudioSource.Stop();
        }
    }

    /// <summary>
    /// 播放蹬墙跳音效
    /// </summary>
    public void PlayWallJumpSound()
    {
        PlaySound(wallJumpSound);
    }

    #endregion

    #region Climb Sounds

    /// <summary>
    /// 处理攀爬音效
    /// </summary>
    private void HandleClimbSound()
    {
        if (!_player.IsClimbing || climbSound == null) return;

        // 只在主动攀爬时播放
        bool activelyClimbing = Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;

        if (activelyClimbing)
        {
            _climbSoundTimer += Time.deltaTime;

            if (_climbSoundTimer >= climbSoundInterval)
            {
                _climbSoundTimer = 0;
                PlaySound(climbSound, 0.7f);
            }
        }
        else
        {
            _climbSoundTimer = 0;
        }
    }

    #endregion

    #region Hurt and Death Sounds

    /// <summary>
    /// 播放受伤音效
    /// </summary>
    public void PlayHurtSound()
    {
        PlayRandomSound(hurtSounds);
    }

    /// <summary>
    /// 播放死亡音效
    /// </summary>
    public void PlayDeathSound()
    {
        PlaySound(deathSound);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 设置主音量
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        _audioSource.volume = masterVolume;
        _loopAudioSource.volume = masterVolume;
    }

    /// <summary>
    /// 暂停所有音效
    /// </summary>
    public void PauseAllSounds()
    {
        _audioSource.Pause();
        _loopAudioSource.Pause();
    }

    /// <summary>
    /// 恢复所有音效
    /// </summary>
    public void ResumeAllSounds()
    {
        _audioSource.UnPause();
        _loopAudioSource.UnPause();
    }

    /// <summary>
    /// 停止所有音效
    /// </summary>
    public void StopAllSounds()
    {
        _audioSource.Stop();
        _loopAudioSource.Stop();
    }

    #endregion
}
