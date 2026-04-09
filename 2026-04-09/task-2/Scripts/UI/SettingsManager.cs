using UnityEngine;
using System.IO;

namespace GameUI
{
    /// <summary>
    /// Singleton manager for game settings.
    /// Handles loading, saving, and applying settings.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        #region Singleton

        private static SettingsManager _instance;
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("SettingsManager");
                    _instance = go.AddComponent<SettingsManager>();
                    DontDestroyOnLoad(go);
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        #endregion

        #region Settings Data

        private SettingsData _settings;
        private const string SettingsFileName = "settings.json";

        /// <summary>
        /// Current game settings.
        /// </summary>
        public SettingsData Settings => _settings;

        #endregion

        #region Events

        /// <summary>
        /// Fired when settings are loaded or changed.
        /// </summary>
        public event System.Action<SettingsData> OnSettingsChanged;

        #endregion

        #region Initialization

        private void Initialize()
        {
            LoadSettings();
            ApplySettings(_settings);
        }

        #endregion

        #region Load/Save

        /// <summary>
        /// Loads settings from persistent storage.
        /// Creates default settings if none exist.
        /// </summary>
        public void LoadSettings()
        {
            string path = GetSettingsPath();

            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    _settings = SettingsData.FromJson(json);
                    Debug.Log("Settings loaded successfully.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load settings: {e.Message}");
                    _settings = SettingsData.CreateDefault();
                }
            }
            else
            {
                _settings = SettingsData.CreateDefault();
                SaveSettings(_settings);
                Debug.Log("Created default settings.");
            }

            OnSettingsChanged?.Invoke(_settings);
        }

        /// <summary>
        /// Saves settings to persistent storage.
        /// </summary>
        public void SaveSettings(SettingsData settings)
        {
            if (settings == null) return;

            _settings = settings;
            string path = GetSettingsPath();

            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = settings.ToJson();
                File.WriteAllText(path, json);
                Debug.Log("Settings saved successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save settings: {e.Message}");
            }

            OnSettingsChanged?.Invoke(_settings);
        }

        private string GetSettingsPath()
        {
            return Path.Combine(Application.persistentDataPath, SettingsFileName);
        }

        #endregion

        #region Apply Settings

        /// <summary>
        /// Applies all settings to the game.
        /// </summary>
        public void ApplySettings(SettingsData settings)
        {
            if (settings == null) return;

            ApplyAudioSettings(settings);
            ApplyGraphicsSettings(settings);
            ApplyGameplaySettings(settings);
            ApplyAccessibilitySettings(settings);
        }

        /// <summary>
        /// Applies audio-related settings.
        /// </summary>
        public void UpdateVolumeSettings(SettingsData settings)
        {
            ApplyAudioSettings(settings);
        }

        private void ApplyAudioSettings(SettingsData settings)
        {
            // TODO: Integrate with your audio system
            // Example with AudioListener:
            float volume = settings.IsMuted ? 0f : settings.MasterVolume;
            AudioListener.volume = volume;

            Debug.Log($"Audio settings applied - Master: {settings.MasterVolume}, Music: {settings.MusicVolume}, SFX: {settings.SfxVolume}, Muted: {settings.IsMuted}");
        }

        private void ApplyGraphicsSettings(SettingsData settings)
        {
            QualitySettings.SetQualityLevel(settings.QualityLevel);
            QualitySettings.vSyncCount = settings.VSyncCount;
            Application.targetFrameRate = settings.TargetFrameRate;

            Screen.SetResolution(
                settings.ResolutionWidth,
                settings.ResolutionHeight,
                settings.FullScreenMode
            );

            Debug.Log($"Graphics settings applied - Quality: {settings.QualityLevel}, Resolution: {settings.ResolutionWidth}x{settings.ResolutionHeight}");
        }

        private void ApplyGameplaySettings(SettingsData settings)
        {
            // Apply gameplay settings
            // These would typically be accessed by other systems through the SettingsManager

            Debug.Log($"Gameplay settings applied - Subtitles: {settings.ShowSubtitles}, Invert Y: {settings.InvertYAxis}");
        }

        private void ApplyAccessibilitySettings(SettingsData settings)
        {
            // Apply accessibility settings
            // These would typically be accessed by UI systems

            Debug.Log($"Accessibility settings applied - High Contrast: {settings.HighContrastMode}, Color Blind Mode: {settings.ColorBlindMode}");
        }

        #endregion

        #region Reset

        /// <summary>
        /// Resets all settings to default values.
        /// </summary>
        public void ResetToDefaults()
        {
            _settings = SettingsData.CreateDefault();
            SaveSettings(_settings);
            ApplySettings(_settings);
            Debug.Log("Settings reset to defaults.");
        }

        #endregion

        #region Utility

        /// <summary>
        /// Gets a specific setting value by key.
        /// Useful for external systems to access settings.
        /// </summary>
        public T GetSetting<T>(System.Func<SettingsData, T> selector)
        {
            return selector?.Invoke(_settings) ?? default;
        }

        #endregion
    }
}
