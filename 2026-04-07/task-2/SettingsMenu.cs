using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置菜单控制器 - 处理游戏设置界面的显示和交互
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle muteToggle;
    
    [Header("Graphics Settings")]
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;
    
    [Header("Control Settings")]
    [SerializeField] private Dropdown controlSchemeDropdown;
    [SerializeField] private Button rebindButton;
    
    [Header("Animation")]
    [SerializeField] private CanvasGroup settingsCanvasGroup;
    [SerializeField] private float fadeInDuration = 0.3f;
    
    private void Awake()
    {
        // 初始化设置值
        InitializeSettings();
        
        // 绑定按钮事件
        rebindButton.onClick.AddListener(OnRebindControls);
        
        // 绑定设置变化事件
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        muteToggle.onValueChanged.AddListener(OnMuteChanged);
        
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        vsyncToggle.onValueChanged.AddListener(OnVsyncChanged);
        
        controlSchemeDropdown.onValueChanged.AddListener(OnControlSchemeChanged);
    }
    
    private void Start()
    {
        // 初始淡入效果
        StartCoroutine(FadeInSettings());
    }
    
    /// <summary>
    /// 初始化设置值
    /// </summary>
    private void InitializeSettings()
    {
        // 音量设置
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        muteToggle.isOn = PlayerPrefs.GetInt("Mute", 0) == 1;
        
        // 画质设置
        qualityDropdown.value = PlayerPrefs.GetInt("QualityQuality", 2); // 默认高质量
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
        
        // 控制设置
        controlSchemeDropdown.value = PlayerPrefs.GetInt("ControlScheme", 0);
    }
    
    /// <summary>
    /// 淡入设置菜单
    /// </summary>
    private System.Collections.IEnumerator FadeInSettings()
    {
        float elapsedTime = 0f;
        settingsCanvasGroup.alpha = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            settingsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }
        
        settingsCanvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// 主音量改变
    /// </summary>
    private void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value * (muteToggle.isOn ? 0f : 1f);
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 音乐音量改变
    /// </summary>
    private void OnMusicVolumeChanged(float value)
    {
        // 这里应该控制音乐音量
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 音效音量改变
    /// </summary>
    private void OnSfxVolumeChanged(float value)
    {
        // 这里应该控制音效音量
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 静音切换
    /// </summary>
    private void OnMuteChanged(bool isMuted)
    {
        AudioListener.volume = isMuted ? 0f : masterVolumeSlider.value;
        PlayerPrefs.SetInt("Mute", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 画质改变
    /// </summary>
    private void OnQualityChanged(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityQuality", qualityIndex);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 全屏切换
    /// </summary>
    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 垂直同步切换
    /// </summary>
    private void OnVsyncChanged(bool enableVsync)
    {
        QualitySettings.vSyncCount = enableVsync ? 1 : 0;
        PlayerPrefs.SetInt("VSync", enableVsync ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 控制方案改变
    /// </summary>
    private void OnControlSchemeChanged(int schemeIndex)
    {
        PlayerPrefs.SetInt("ControlScheme", schemeIndex);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 重新绑定控制
    /// </summary>
    private void OnRebindControls()
    {
        Debug.Log("重新绑定控制键");
        // 这里可以实现按键重新绑定逻辑
        // 例如：显示按键绑定界面，让玩家按下新的按键
    }
    
    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void OnBackToMenu()
    {
        Debug.Log("返回主菜单");
        UIManager.Instance.ShowMainMenu();
    }
    
    /// <summary>
    /// 应用设置（可选）
    /// </summary>
    public void OnApplySettings()
    {
        Debug.Log("应用设置");
        // 设置已经实时保存，这里可以添加一些确认逻辑
    }
}