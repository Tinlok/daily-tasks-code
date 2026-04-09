using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private Slider brightnessSlider;

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle muteToggle;

    [Header("Controls Settings")]
    [SerializeField] private Dropdown controlSchemeDropdown;
    [SerializeField] private Button rebindButton;
    [SerializeField] private Text controlSchemeText;

    [Header("Language Settings")]
    [SerializeField] private Dropdown languageDropdown;

    private Resolution[] resolutions;

    private void Start()
    {
        InitializeDropdowns();
        LoadSettings();
        SetupEventListeners();
    }

    private void InitializeDropdowns()
    {
        // 初始化分辨率下拉菜单
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> resolutionOptions = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string resolutionOption = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "Hz";
            resolutionOptions.Add(resolutionOption);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;

        // 初始化质量下拉菜单
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string> { "Very Low", "Low", "Medium", "High", "Very High", "Ultra" });

        // 初始化控制方案下拉菜单
        controlSchemeDropdown.ClearOptions();
        controlSchemeDropdown.AddOptions(new List<string> { "Keyboard & Mouse", "Gamepad", "Custom" });

        // 初始化语言下拉菜单
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new List<string> { "中文", "English", "日本語", "한국어" });
    }

    private void SetupEventListeners()
    {
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);

        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        muteToggle.onValueChanged.AddListener(OnMuteChanged);

        controlSchemeDropdown.onValueChanged.AddListener(OnControlSchemeChanged);
        rebindButton.onClick.AddListener(OnRebindControls);
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void LoadSettings()
    {
        // 加载视频设置
        if (PlayerPrefs.HasKey("Resolution"))
        {
            resolutionDropdown.value = PlayerPrefs.GetInt("Resolution");
        }

        if (PlayerPrefs.HasKey("Quality"))
        {
            qualityDropdown.value = PlayerPrefs.GetInt("Quality");
        }

        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen") == 1;
        }

        if (PlayerPrefs.HasKey("VSync"))
        {
            vsyncToggle.isOn = PlayerPrefs.GetInt("VSync") == 1;
        }

        if (PlayerPrefs.HasKey("Brightness"))
        {
            brightnessSlider.value = PlayerPrefs.GetFloat("Brightness");
        }

        // 加载音频设置
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        }
        else
        {
            masterVolumeSlider.value = 0.8f;
        }

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        }
        else
        {
            musicVolumeSlider.value = 0.7f;
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        }
        else
        {
            sfxVolumeSlider.value = 0.8f;
        }

        if (PlayerPrefs.HasKey("Mute"))
        {
            muteToggle.isOn = PlayerPrefs.GetInt("Mute") == 1;
        }

        // 加载控制设置
        if (PlayerPrefs.HasKey("ControlScheme"))
        {
            controlSchemeDropdown.value = PlayerPrefs.GetInt("ControlScheme");
        }

        // 加载语言设置
        if (PlayerPrefs.HasKey("Language"))
        {
            languageDropdown.value = PlayerPrefs.GetInt("Language");
        }
    }

    private void SaveSettings()
    {
        // 保存视频设置
        PlayerPrefs.SetInt("Resolution", resolutionDropdown.value);
        PlayerPrefs.SetInt("Quality", qualityDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("VSync", vsyncToggle.isOn ? 1 : 0);
        PlayerPrefs.SetFloat("Brightness", brightnessSlider.value);

        // 保存音频设置
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetInt("Mute", muteToggle.isOn ? 1 : 0);

        // 保存控制设置
        PlayerPrefs.SetInt("ControlScheme", controlSchemeDropdown.value);

        // 保存语言设置
        PlayerPrefs.SetInt("Language", languageDropdown.value);

        PlayerPrefs.Save();
    }

    private void ApplySettings()
    {
        // 应用分辨率设置
        Resolution selectedResolution = resolutions[resolutionDropdown.value];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, fullscreenToggle.isOn);

        // 应用质量设置
        QualitySettings.SetQualityLevel(qualityDropdown.value);

        // 应用垂直同步
        QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;

        // 应用亮度
        RenderSettings.ambientLight = new Color(brightnessSlider.value, brightnessSlider.value, brightnessSlider.value);

        // 应用音频设置
        AudioListener.volume = masterVolumeSlider.value;

        // 应用语言设置
        ApplyLanguage(languageDropdown.value);
    }

    private void OnResolutionChanged(int index)
    {
        Resolution selectedResolution = resolutions[index];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, fullscreenToggle.isOn);
        SaveSettings();
    }

    private void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index);
        SaveSettings();
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        SaveSettings();
    }

    private void OnVSyncChanged(bool enableVSync)
    {
        QualitySettings.vSyncCount = enableVSync ? 1 : 0;
        SaveSettings();
    }

    private void OnBrightnessChanged(float brightness)
    {
        RenderSettings.ambientLight = new Color(brightness, brightness, brightness);
        SaveSettings();
    }

    private void OnMasterVolumeChanged(float volume)
    {
        AudioListener.volume = volume;
        SaveSettings();
    }

    private void OnMusicVolumeChanged(float volume)
    {
        // 实际项目中这里应该控制音乐播放器
        Debug.Log($"Music volume changed to: {volume}");
        SaveSettings();
    }

    private void OnSFXVolumeChanged(float volume)
    {
        // 实际项目中这里应该控制音效播放器
        Debug.Log($"SFX volume changed to: {volume}");
        SaveSettings();
    }

    private void OnMuteChanged(bool isMuted)
    {
        AudioListener.pause = isMuted;
        SaveSettings();
    }

    private void OnControlSchemeChanged(int scheme)
    {
        controlSchemeText.text = GetControlSchemeDescription(scheme);
        SaveSettings();
    }

    private void OnRebindControls()
    {
        // 实际项目中这里应该打开控制绑定界面
        Debug.Log("Control rebinding interface would open here");
        SaveSettings();
    }

    private void OnLanguageChanged(int languageIndex)
    {
        ApplyLanguage(languageIndex);
        SaveSettings();
    }

    private string GetControlSchemeDescription(int scheme)
    {
        switch (scheme)
        {
            case 0: return "Standard keyboard/mouse controls";
            case 1: return "Gamepad optimized controls";
            case 2: return "Custom control bindings";
            default: return "Default controls";
        }
    }

    private void ApplyLanguage(int languageIndex)
    {
        // 实际项目中这里应该加载对应的语言资源
        string language = "English";
        switch (languageIndex)
        {
            case 0: language = "中文"; break;
            case 1: language = "English"; break;
            case 2: language = "日本語"; break;
            case 3: language = "한국어"; break;
        }
        Debug.Log($"Language changed to: {language}");
    }

    // 返回按钮事件
    public void OnBackPressed()
    {
        SaveSettings();
        ApplySettings();
        
        // 返回主菜单或暂停菜单
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            // 返回到主菜单
            FindObjectOfType<MainMenuController>()?.BackToMainMenu();
        }
        else
        {
            // 返回到暂停菜单
            FindObjectOfType<PauseMenuController>()?.BackToPauseMenu();
        }
    }
}