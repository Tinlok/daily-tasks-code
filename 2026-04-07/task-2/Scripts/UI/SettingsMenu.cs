using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace DailyTasks.UI
{
    /// <summary>
    /// 设置界面 - 包含音量控制、画质设置、控制设置
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Audio Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Text masterVolumeValueText;
        [SerializeField] private Text musicVolumeValueText;
        [SerializeField] private Text sfxVolumeValueText;

        [Header("Graphics Settings")]
        [SerializeField] private Dropdown qualityDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Dropdown resolutionDropdown;
        [SerializeField] private Toggle vSyncToggle;

        [Header("Controls Settings")]
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private Text mouseSensitivityValueText;
        [SerializeField] private Dropdown controlSchemeDropdown;

        [Header("Navigation")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetButton;

        [Header("Audio Mixer (Optional)")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";

        #endregion

        #region Private Fields

        private Resolution[] _availableResolutions;
        private SettingsData _currentSettings;
        private SettingsData _appliedSettings;

        #endregion

        #region Nested Classes

        [System.Serializable]
        private class SettingsData
        {
            public float masterVolume = 1f;
            public float musicVolume = 1f;
            public float sfxVolume = 1f;
            public int qualityLevel = 2;
            public bool fullscreen = true;
            public int resolutionIndex = 0;
            public bool vSync = true;
            public float mouseSensitivity = 1f;
            public int controlScheme = 0;
        }

        #endregion

        #region Constants

        private const string MASTER_VOLUME_KEY = "MasterVolume";
        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string SFX_VOLUME_KEY = "SFXVolume";
        private const string QUALITY_LEVEL_KEY = "QualityLevel";
        private const string FULLSCREEN_KEY = "Fullscreen";
        private const string RESOLUTION_INDEX_KEY = "ResolutionIndex";
        private const string VSYNC_KEY = "VSync";
        private const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";
        private const string CONTROL_SCHEME_KEY = "ControlScheme";

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _availableResolutions = Screen.resolutions;
            InitializeUI();
            LoadSettings();
            ApplySettings(_appliedSettings);
        }

        private void OnEnable()
        {
            InitializeButtons();
            LoadSettings();
            UpdateUIValues();
        }

        #endregion

        #region Private Methods - Initialization

        private void InitializeUI()
        {
            // 初始化画质下拉菜单
            if (qualityDropdown != null)
            {
                qualityDropdown.options.Clear();
                string[] qualityLevels = QualitySettings.names;
                for (int i = 0; i < qualityLevels.Length; i++)
                {
                    qualityDropdown.options.Add(new Dropdown.OptionData(qualityLevels[i]));
                }
            }

            // 初始化分辨率下拉菜单
            if (resolutionDropdown != null)
            {
                resolutionDropdown.options.Clear();
                _availableResolutions = Screen.resolutions;

                for (int i = 0; i < _availableResolutions.Length; i++)
                {
                    string option = $"{_availableResolutions[i].width} x {_availableResolutions[i].height} @{_availableResolutions[i].refreshRate}Hz";
                    resolutionDropdown.options.Add(new Dropdown.OptionData(option));
                }
            }

            // 初始化控制方案下拉菜单
            if (controlSchemeDropdown != null)
            {
                controlSchemeDropdown.options.Clear();
                controlSchemeDropdown.options.Add(new Dropdown.OptionData("Keyboard & Mouse"));
                controlSchemeDropdown.options.Add(new Dropdown.OptionData("Gamepad"));
                controlSchemeDropdown.options.Add(new Dropdown.OptionData("Touch"));
            }

            InitializeSliders();
        }

        private void InitializeSliders()
        {
            // 音量滑块
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.minValue = 0f;
                masterVolumeSlider.maxValue = 1f;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.minValue = 0f;
                musicVolumeSlider.maxValue = 1f;
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.minValue = 0f;
                sfxVolumeSlider.maxValue = 1f;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            // 鼠标灵敏度滑块
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.minValue = 0.1f;
                mouseSensitivitySlider.maxValue = 3f;
                mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            }

            // 其他控件
            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }

            if (vSyncToggle != null)
            {
                vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
            }

            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }

            if (resolutionDropdown != null)
            {
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }

            if (controlSchemeDropdown != null)
            {
                controlSchemeDropdown.onValueChanged.AddListener(OnControlSchemeChanged);
            }
        }

        private void InitializeButtons()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (applyButton != null)
            {
                applyButton.onClick.RemoveAllListeners();
                applyButton.onClick.AddListener(OnApplyClicked);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
                resetButton.onClick.AddListener(OnResetClicked);
            }
        }

        #endregion

        #region Private Methods - Settings Management

        private void LoadSettings()
        {
            _currentSettings = new SettingsData
            {
                masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f),
                musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f),
                sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f),
                qualityLevel = PlayerPrefs.GetInt(QUALITY_LEVEL_KEY, QualitySettings.GetQualityLevel()),
                fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1,
                resolutionIndex = PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY, _availableResolutions.Length - 1),
                vSync = PlayerPrefs.GetInt(VSYNC_KEY, 1) == 1,
                mouseSensitivity = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, 1f),
                controlScheme = PlayerPrefs.GetInt(CONTROL_SCHEME_KEY, 0)
            };

            _appliedSettings = CloneSettings(_currentSettings);
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, _currentSettings.masterVolume);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, _currentSettings.musicVolume);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, _currentSettings.sfxVolume);
            PlayerPrefs.SetInt(QUALITY_LEVEL_KEY, _currentSettings.qualityLevel);
            PlayerPrefs.SetInt(FULLSCREEN_KEY, _currentSettings.fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(RESOLUTION_INDEX_KEY, _currentSettings.resolutionIndex);
            PlayerPrefs.SetInt(VSYNC_KEY, _currentSettings.vSync ? 1 : 0);
            PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, _currentSettings.mouseSensitivity);
            PlayerPrefs.SetInt(CONTROL_SCHEME_KEY, _currentSettings.controlScheme);
            PlayerPrefs.Save();

            _appliedSettings = CloneSettings(_currentSettings);
        }

        private void ApplySettings(SettingsData settings)
        {
            // 应用音量设置
            SetVolume(settings.masterVolume, settings.musicVolume, settings.sfxVolume);

            // 应用画质设置
            QualitySettings.SetQualityLevel(settings.qualityLevel);

            // 应用全屏设置
            Screen.fullScreen = settings.fullscreen;

            // 应用分辨率
            if (settings.resolutionIndex >= 0 && settings.resolutionIndex < _availableResolutions.Length)
            {
                Resolution resolution = _availableResolutions[settings.resolutionIndex];
                Screen.SetResolution(resolution.width, resolution.height, settings.fullscreen);
            }

            // 应用垂直同步
            QualitySettings.vSyncCount = settings.vSync ? 1 : 0;
        }

        private void SetVolume(float master, float music, float sfx)
        {
            if (audioMixer != null)
            {
                // 转换线性值到分贝
                float masterDB = Mathf.Log10(Mathf.Max(master, 0.0001f)) * 20f;
                float musicDB = Mathf.Log10(Mathf.Max(music, 0.0001f)) * 20f;
                float sfxDB = Mathf.Log10(Mathf.Max(sfx, 0.0001f)) * 20f;

                audioMixer.SetFloat(masterVolumeParam, masterDB);
                audioMixer.SetFloat(musicVolumeParam, musicDB);
                audioMixer.SetFloat(sfxVolumeParam, sfxDB);
            }
            else
            {
                // 如果没有AudioMixer，使用AudioListener
                AudioListener.volume = master;
            }
        }

        private void UpdateUIValues()
        {
            // 音量
            if (masterVolumeSlider != null) masterVolumeSlider.value = _currentSettings.masterVolume;
            if (musicVolumeSlider != null) musicVolumeSlider.value = _currentSettings.musicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = _currentSettings.sfxVolume;

            UpdateVolumeText();

            // 画质
            if (qualityDropdown != null) qualityDropdown.value = _currentSettings.qualityLevel;
            if (fullscreenToggle != null) fullscreenToggle.isOn = _currentSettings.fullscreen;
            if (resolutionDropdown != null) resolutionDropdown.value = _currentSettings.resolutionIndex;
            if (vSyncToggle != null) vSyncToggle.isOn = _currentSettings.vSync;

            // 控制
            if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = _currentSettings.mouseSensitivity;
            if (controlSchemeDropdown != null) controlSchemeDropdown.value = _currentSettings.controlScheme;

            UpdateMouseSensitivityText();
        }

        private void UpdateVolumeText()
        {
            if (masterVolumeValueText != null)
            {
                masterVolumeValueText.text = Mathf.RoundToInt(_currentSettings.masterVolume * 100) + "%";
            }

            if (musicVolumeValueText != null)
            {
                musicVolumeValueText.text = Mathf.RoundToInt(_currentSettings.musicVolume * 100) + "%";
            }

            if (sfxVolumeValueText != null)
            {
                sfxVolumeValueText.text = Mathf.RoundToInt(_currentSettings.sfxVolume * 100) + "%";
            }
        }

        private void UpdateMouseSensitivityText()
        {
            if (mouseSensitivityValueText != null)
            {
                mouseSensitivityValueText.text = _currentSettings.mouseSensitivity.ToString("F1");
            }
        }

        private SettingsData CloneSettings(SettingsData original)
        {
            return new SettingsData
            {
                masterVolume = original.masterVolume,
                musicVolume = original.musicVolume,
                sfxVolume = original.sfxVolume,
                qualityLevel = original.qualityLevel,
                fullscreen = original.fullscreen,
                resolutionIndex = original.resolutionIndex,
                vSync = original.vSync,
                mouseSensitivity = original.mouseSensitivity,
                controlScheme = original.controlScheme
            };
        }

        private bool HasUnsavedChanges()
        {
            return _currentSettings.masterVolume != _appliedSettings.masterVolume ||
                   _currentSettings.musicVolume != _appliedSettings.musicVolume ||
                   _currentSettings.sfxVolume != _appliedSettings.sfxVolume ||
                   _currentSettings.qualityLevel != _appliedSettings.qualityLevel ||
                   _currentSettings.fullscreen != _appliedSettings.fullscreen ||
                   _currentSettings.resolutionIndex != _appliedSettings.resolutionIndex ||
                   _currentSettings.vSync != _appliedSettings.vSync ||
                   _currentSettings.mouseSensitivity != _appliedSettings.mouseSensitivity ||
                   _currentSettings.controlScheme != _appliedSettings.controlScheme;
        }

        #endregion

        #region Event Handlers

        private void OnMasterVolumeChanged(float value)
        {
            _currentSettings.masterVolume = value;
            UpdateVolumeText();
            SetVolume(_currentSettings.masterVolume, _currentSettings.musicVolume, _currentSettings.sfxVolume);
        }

        private void OnMusicVolumeChanged(float value)
        {
            _currentSettings.musicVolume = value;
            UpdateVolumeText();
            SetVolume(_currentSettings.masterVolume, _currentSettings.musicVolume, _currentSettings.sfxVolume);
        }

        private void OnSFXVolumeChanged(float value)
        {
            _currentSettings.sfxVolume = value;
            UpdateVolumeText();
            SetVolume(_currentSettings.masterVolume, _currentSettings.musicVolume, _currentSettings.sfxVolume);
        }

        private void OnQualityChanged(int index)
        {
            _currentSettings.qualityLevel = index;
        }

        private void OnFullscreenChanged(bool value)
        {
            _currentSettings.fullscreen = value;
        }

        private void OnResolutionChanged(int index)
        {
            _currentSettings.resolutionIndex = index;
        }

        private void OnVSyncChanged(bool value)
        {
            _currentSettings.vSync = value;
        }

        private void OnMouseSensitivityChanged(float value)
        {
            _currentSettings.mouseSensitivity = value;
            UpdateMouseSensitivityText();
        }

        private void OnControlSchemeChanged(int index)
        {
            _currentSettings.controlScheme = index;
        }

        #endregion

        #region Button Callbacks

        private void OnBackClicked()
        {
            // 如果有未保存的更改，先应用
            if (HasUnsavedChanges())
            {
                SaveSettings();
                ApplySettings(_currentSettings);
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.GoBack();
            }
        }

        private void OnApplyClicked()
        {
            SaveSettings();
            ApplySettings(_currentSettings);
        }

        private void OnResetClicked()
        {
            // 重置为默认值
            _currentSettings = new SettingsData();
            UpdateUIValues();
            ApplySettings(_currentSettings);
            SaveSettings();
        }

        #endregion
    }
}
