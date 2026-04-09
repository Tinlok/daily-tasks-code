using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameUI
{
    /// <summary>
    /// Settings menu UI controller.
    /// Handles all settings adjustments and persistence.
    /// Attach this to the settings panel GameObject.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        #region References

        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _applyButton;

        [Header("Audio Settings")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI _masterVolumeValue;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI _musicVolumeValue;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI _sfxVolumeValue;
        [SerializeField] private Toggle _muteToggle;

        [Header("Graphics Settings")]
        [SerializeField] private TMP_Dropdown _qualityDropdown;
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private TMP_Dropdown _fullScreenModeDropdown;
        [SerializeField] private TMP_Dropdown _vSyncDropdown;
        [SerializeField] private Toggle _limitFpsToggle;
        [SerializeField] private TMP_Dropdown _targetFpsDropdown;

        [Header("Gameplay Settings")]
        [SerializeField] private Toggle _subtitlesToggle;
        [SerializeField] private Toggle _invertYToggle;
        [SerializeField] private Slider _mouseSensitivitySlider;
        [SerializeField] private TextMeshProUGUI _mouseSensitivityValue;
        [SerializeField] private Slider _controllerSensitivitySlider;
        [SerializeField] private TextMeshProUGUI _controllerSensitivityValue;

        [Header("Accessibility Settings")]
        [SerializeField] private Toggle _highContrastToggle;
        [SerializeField] private TMP_Dropdown _colorBlindModeDropdown;
        [SerializeField] private Toggle _screenReaderToggle;
        [SerializeField] private Toggle _largeTextToggle;

        [Header("Other Settings")]
        [SerializeField] private TMP_Dropdown _languageDropdown;
        [SerializeField] private Toggle _tutorialsToggle;
        [SerializeField] private Toggle _autoSaveToggle;

        [Header("Tabs")]
        [SerializeField] private Button _audioTabButton;
        [SerializeField] private Button _graphicsTabButton;
        [SerializeField] private Button _gameplayTabButton;
        [SerializeField] private Button _accessibilityTabButton;

        [Header("Tab Panels")]
        [SerializeField] private GameObject _audioPanel;
        [SerializeField] private GameObject _graphicsPanel;
        [SerializeField] private GameObject _gameplayPanel;
        [SerializeField] private GameObject _accessibilityPanel;

        #endregion

        #region State

        private SettingsData _currentSettings;
        private SettingsData _appliedSettings;
        private SettingsTab _currentTab = SettingsTab.Audio;
        private Resolution[] _availableResolutions;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _currentSettings = SettingsManager.Instance?.Settings ?? SettingsData.CreateDefault();
            _appliedSettings = CloneSettings(_currentSettings);
            _availableResolutions = Screen.resolutions;

            InitializeUI();
            SetupButtons();
            LoadSettingsToUI(_currentSettings);
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (_backButton != null) _backButton.onClick.RemoveAllListeners();
            if (_resetButton != null) _resetButton.onClick.RemoveAllListeners();
            if (_applyButton != null) _applyButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Initialization

        private void InitializeUI()
        {
            InitializeQualityDropdown();
            InitializeResolutionDropdown();
            InitializeFullScreenModeDropdown();
            InitializeVSyncDropdown();
            InitializeFpsDropdown();
            InitializeColorBlindDropdown();
            InitializeLanguageDropdown();
            ShowTab(_currentTab);
        }

        private void InitializeQualityDropdown()
        {
            if (_qualityDropdown != null)
            {
                _qualityDropdown.options.Clear();
                string[] qualityLevels = QualitySettings.names;
                for (int i = 0; i < qualityLevels.Length; i++)
                {
                    _qualityDropdown.options.Add(new TMP_Dropdown.OptionData(qualityLevels[i]));
                }
            }
        }

        private void InitializeResolutionDropdown()
        {
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.options.Clear();

                // Get unique resolutions
                var uniqueResolutions = new System.Collections.Generic.List<Resolution>();
                foreach (var res in _availableResolutions)
                {
                    if (!uniqueResolutions.Exists(r => r.width == res.width && r.height == res.height))
                    {
                        uniqueResolutions.Add(res);
                    }
                }

                foreach (var res in uniqueResolutions)
                {
                    _resolutionDropdown.options.Add(new TMP_Dropdown.OptionData($"{res.width} x {res.height}"));
                }
            }
        }

        private void InitializeFullScreenModeDropdown()
        {
            if (_fullScreenModeDropdown != null)
            {
                _fullScreenModeDropdown.options.Clear();
                _fullScreenModeDropdown.options.Add(new TMP_Dropdown.OptionData("Exclusive Fullscreen"));
                _fullScreenModeDropdown.options.Add(new TMP_Dropdown.OptionData("Fullscreen Window"));
                _fullScreenModeDropdown.options.Add(new TMP_Dropdown.OptionData("Windowed"));
            }
        }

        private void InitializeVSyncDropdown()
        {
            if (_vSyncDropdown != null)
            {
                _vSyncDropdown.options.Clear();
                _vSyncDropdown.options.Add(new TMP_Dropdown.OptionData("Off"));
                _vSyncDropdown.options.Add(new TMP_Dropdown.OptionData("On"));
            }
        }

        private void InitializeFpsDropdown()
        {
            if (_targetFpsDropdown != null)
            {
                _targetFpsDropdown.options.Clear();
                _targetFpsDropdown.options.Add(new TMP_Dropdown.OptionData("30"));
                _targetFpsDropdown.options.Add(new TMP_Dropdown.OptionData("60"));
                _targetFpsDropdown.options.Add(new TMP_Dropdown.OptionData("120"));
                _targetFpsDropdown.options.Add(new TMP_Dropdown.OptionData("144"));
                _targetFpsDropdown.options.Add(new TMP_Dropdown.OptionData("Unlimited"));
            }
        }

        private void InitializeColorBlindDropdown()
        {
            if (_colorBlindModeDropdown != null)
            {
                _colorBlindModeDropdown.options.Clear();
                _colorBlindModeDropdown.options.Add(new TMP_Dropdown.OptionData("None"));
                _colorBlindModeDropdown.options.Add(new TMP_Dropdown.OptionData("Protanopia (Red-weak)"));
                _colorBlindModeDropdown.options.Add(new TMP_Dropdown.OptionData("Deuteranopia (Green-weak)"));
                _colorBlindModeDropdown.options.Add(new TMP_Dropdown.OptionData("Tritanopia (Blue-weak)"));
                _colorBlindModeDropdown.options.Add(new TMP_Dropdown.OptionData("Monochromacy"));
            }
        }

        private void InitializeLanguageDropdown()
        {
            if (_languageDropdown != null)
            {
                _languageDropdown.options.Clear();
                _languageDropdown.options.Add(new TMP_Dropdown.OptionData("English"));
                _languageDropdown.options.Add(new TMP_Dropdown.OptionData("中文"));
                _languageDropdown.options.Add(new TMP_Dropdown.OptionData("日本語"));
                _languageDropdown.options.Add(new TMP_Dropdown.OptionData("Español"));
                _languageDropdown.options.Add(new TMP_Dropdown.OptionData("Français"));
            }
        }

        private void SetupButtons()
        {
            // Navigation buttons
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackPressed);
            }

            if (_resetButton != null)
            {
                _resetButton.onClick.AddListener(OnResetClicked);
            }

            if (_applyButton != null)
            {
                _applyButton.onClick.AddListener(OnApplyClicked);
            }

            // Tab buttons
            if (_audioTabButton != null)
            {
                _audioTabButton.onClick.AddListener(() => ShowTab(SettingsTab.Audio));
            }

            if (_graphicsTabButton != null)
            {
                _graphicsTabButton.onClick.AddListener(() => ShowTab(SettingsTab.Graphics));
            }

            if (_gameplayTabButton != null)
            {
                _gameplayTabButton.onClick.AddListener(() => ShowTab(SettingsTab.Gameplay));
            }

            if (_accessibilityTabButton != null)
            {
                _accessibilityTabButton.onClick.AddListener(() => ShowTab(SettingsTab.Accessibility));
            }

            // Audio sliders
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (_muteToggle != null)
            {
                _muteToggle.onValueChanged.AddListener(OnMuteToggled);
            }

            // Graphics controls
            if (_qualityDropdown != null)
            {
                _qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }

            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }

            if (_fullScreenModeDropdown != null)
            {
                _fullScreenModeDropdown.onValueChanged.AddListener(OnFullScreenModeChanged);
            }

            if (_vSyncDropdown != null)
            {
                _vSyncDropdown.onValueChanged.AddListener(OnVSyncChanged);
            }

            // Gameplay controls
            if (_mouseSensitivitySlider != null)
            {
                _mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            }

            if (_controllerSensitivitySlider != null)
            {
                _controllerSensitivitySlider.onValueChanged.AddListener(OnControllerSensitivityChanged);
            }
        }

        #endregion

        #region UI Loading

        private void LoadSettingsToUI(SettingsData settings)
        {
            // Audio
            if (_masterVolumeSlider != null) _masterVolumeSlider.value = settings.MasterVolume;
            if (_musicVolumeSlider != null) _musicVolumeSlider.value = settings.MusicVolume;
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.value = settings.SfxVolume;
            if (_muteToggle != null) _muteToggle.isOn = settings.IsMuted;
            UpdateVolumeLabels();

            // Graphics
            if (_qualityDropdown != null) _qualityDropdown.value = settings.QualityLevel;
            UpdateResolutionDropdown(settings.ResolutionWidth, settings.ResolutionHeight);
            UpdateFullScreenModeDropdown(settings.FullScreenMode);
            if (_vSyncDropdown != null) _vSyncDropdown.value = settings.VSyncCount;

            // Gameplay
            if (_subtitlesToggle != null) _subtitlesToggle.isOn = settings.ShowSubtitles;
            if (_invertYToggle != null) _invertYToggle.isOn = settings.InvertYAxis;
            if (_mouseSensitivitySlider != null) _mouseSensitivitySlider.value = settings.MouseSensitivity;
            if (_controllerSensitivitySlider != null) _controllerSensitivitySlider.value = settings.ControllerSensitivity;
            UpdateSensitivityLabels();

            // Accessibility
            if (_highContrastToggle != null) _highContrastToggle.isOn = settings.HighContrastMode;
            if (_colorBlindModeDropdown != null) _colorBlindModeDropdown.value = (int)settings.ColorBlindMode;
            if (_screenReaderToggle != null) _screenReaderToggle.isOn = settings.ScreenReaderEnabled;
            if (_largeTextToggle != null) _largeTextToggle.isOn = settings.LargeText;

            // Other
            if (_tutorialsToggle != null) _tutorialsToggle.isOn = settings.ShowTutorials;
            if (_autoSaveToggle != null) _autoSaveToggle.isOn = settings.AutoSaveEnabled;
        }

        private void UpdateResolutionDropdown(int width, int height)
        {
            if (_resolutionDropdown != null)
            {
                for (int i = 0; i < _resolutionDropdown.options.Count; i++)
                {
                    if (_resolutionDropdown.options[i].text == $"{width} x {height}")
                    {
                        _resolutionDropdown.value = i;
                        break;
                    }
                }
            }
        }

        private void UpdateFullScreenModeDropdown(FullScreenMode mode)
        {
            if (_fullScreenModeDropdown != null)
            {
                _fullScreenModeDropdown.value = mode switch
                {
                    FullScreenMode.ExclusiveFullScreen => 0,
                    FullScreenMode.FullScreenWindow => 1,
                    FullScreenMode.Windowed => 2,
                    _ => 1
                };
            }
        }

        private void UpdateVolumeLabels()
        {
            if (_masterVolumeValue != null)
                _masterVolumeValue.text = $"{Mathf.RoundToInt(_masterVolumeSlider.value * 100)}%";
            if (_musicVolumeValue != null)
                _musicVolumeValue.text = $"{Mathf.RoundToInt(_musicVolumeSlider.value * 100)}%";
            if (_sfxVolumeValue != null)
                _sfxVolumeValue.text = $"{Mathf.RoundToInt(_sfxVolumeSlider.value * 100)}%";
        }

        private void UpdateSensitivityLabels()
        {
            if (_mouseSensitivityValue != null)
                _mouseSensitivityValue.text = _mouseSensitivitySlider.value.ToString("F1");
            if (_controllerSensitivityValue != null)
                _controllerSensitivityValue.text = _controllerSensitivitySlider.value.ToString("F1");
        }

        #endregion

        #region Settings Changed Handlers

        private void OnMasterVolumeChanged(float value)
        {
            _currentSettings.MasterVolume = value;
            UpdateVolumeLabels();
            SettingsManager.Instance?.UpdateVolumeSettings(_currentSettings);
        }

        private void OnMusicVolumeChanged(float value)
        {
            _currentSettings.MusicVolume = value;
            UpdateVolumeLabels();
            SettingsManager.Instance?.UpdateVolumeSettings(_currentSettings);
        }

        private void OnSfxVolumeChanged(float value)
        {
            _currentSettings.SfxVolume = value;
            UpdateVolumeLabels();
            SettingsManager.Instance?.UpdateVolumeSettings(_currentSettings);
        }

        private void OnMuteToggled(bool value)
        {
            _currentSettings.IsMuted = value;
            SettingsManager.Instance?.UpdateVolumeSettings(_currentSettings);
        }

        private void OnQualityChanged(int value)
        {
            _currentSettings.QualityLevel = value;
            QualitySettings.SetQualityLevel(value);
        }

        private void OnResolutionChanged(int index)
        {
            // Find unique resolution at index
            var uniqueResolutions = new System.Collections.Generic.List<Resolution>();
            foreach (var res in _availableResolutions)
            {
                if (!uniqueResolutions.Exists(r => r.width == res.width && r.height == res.height))
                {
                    uniqueResolutions.Add(res);
                }
            }

            if (index >= 0 && index < uniqueResolutions.Count)
            {
                var res = uniqueResolutions[index];
                _currentSettings.ResolutionWidth = res.width;
                _currentSettings.ResolutionHeight = res.height;
            }
        }

        private void OnFullScreenModeChanged(int index)
        {
            _currentSettings.FullScreenMode = index switch
            {
                0 => FullScreenMode.ExclusiveFullScreen,
                1 => FullScreenMode.FullScreenWindow,
                2 => FullScreenMode.Windowed,
                _ => FullScreenMode.FullScreenWindow
            };
        }

        private void OnVSyncChanged(int value)
        {
            _currentSettings.VSyncCount = value;
            QualitySettings.vSyncCount = value;
        }

        private void OnMouseSensitivityChanged(float value)
        {
            _currentSettings.MouseSensitivity = value;
            UpdateSensitivityLabels();
        }

        private void OnControllerSensitivityChanged(float value)
        {
            _currentSettings.ControllerSensitivity = value;
            UpdateSensitivityLabels();
        }

        #endregion

        #region Button Handlers

        private void OnBackPressed()
        {
            ApplySettings();
            UIManager.Instance?.GoBack();
        }

        private void OnResetClicked()
        {
            _currentSettings = SettingsData.CreateDefault();
            LoadSettingsToUI(_currentSettings);
        }

        private void OnApplyClicked()
        {
            ApplySettings();
        }

        #endregion

        #region Tab Management

        private void ShowTab(SettingsTab tab)
        {
            _currentTab = tab;

            // Hide all panels
            if (_audioPanel != null) _audioPanel.SetActive(false);
            if (_graphicsPanel != null) _graphicsPanel.SetActive(false);
            if (_gameplayPanel != null) _gameplayPanel.SetActive(false);
            if (_accessibilityPanel != null) _accessibilityPanel.SetActive(false);

            // Show selected panel
            switch (tab)
            {
                case SettingsTab.Audio:
                    if (_audioPanel != null) _audioPanel.SetActive(true);
                    break;
                case SettingsTab.Graphics:
                    if (_graphicsPanel != null) _graphicsPanel.SetActive(true);
                    break;
                case SettingsTab.Gameplay:
                    if (_gameplayPanel != null) _gameplayPanel.SetActive(true);
                    break;
                case SettingsTab.Accessibility:
                    if (_accessibilityPanel != null) _accessibilityPanel.SetActive(true);
                    break;
            }
        }

        #endregion

        #region Settings Application

        private void ApplySettings()
        {
            // Collect final settings from UI
            if (_subtitlesToggle != null) _currentSettings.ShowSubtitles = _subtitlesToggle.isOn;
            if (_invertYToggle != null) _currentSettings.InvertYAxis = _invertYToggle.isOn;
            if (_highContrastToggle != null) _currentSettings.HighContrastMode = _highContrastToggle.isOn;
            if (_colorBlindModeDropdown != null) _currentSettings.ColorBlindMode = (ColorBlindMode)_colorBlindModeDropdown.value;
            if (_screenReaderToggle != null) _currentSettings.ScreenReaderEnabled = _screenReaderToggle.isOn;
            if (_largeTextToggle != null) _currentSettings.LargeText = _largeTextToggle.isOn;
            if (_tutorialsToggle != null) _currentSettings.ShowTutorials = _tutorialsToggle.isOn;
            if (_autoSaveToggle != null) _currentSettings.AutoSaveEnabled = _autoSaveToggle.isOn;

            // Apply resolution and fullscreen
            Screen.SetResolution(
                _currentSettings.ResolutionWidth,
                _currentSettings.ResolutionHeight,
                _currentSettings.FullScreenMode
            );

            // Save settings
            SettingsManager.Instance?.SaveSettings(_currentSettings);
            _appliedSettings = CloneSettings(_currentSettings);
        }

        private SettingsData CloneSettings(SettingsData original)
        {
            return JsonUtility.FromJson<SettingsData>(JsonUtility.ToJson(original));
        }

        #endregion

        #region Public API

        /// <summary>
        /// Called when settings panel is shown.
        /// </summary>
        public void OnSettingsShown()
        {
            _currentSettings = SettingsManager.Instance?.Settings ?? SettingsData.CreateDefault();
            LoadSettingsToUI(_currentSettings);
        }

        #endregion
    }

    /// <summary>
    /// Settings tabs for organization.
    /// </summary>
    public enum SettingsTab
    {
        Audio,
        Graphics,
        Gameplay,
        Accessibility
    }
}
