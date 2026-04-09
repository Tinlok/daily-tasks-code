using UnityEngine;
using System;

namespace GameUI
{
    /// <summary>
    /// Serializable data structure for game settings.
    /// Supports persistence via JSON serialization.
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        #region Audio Settings

        [Header("Audio")]
        [Tooltip("Master volume level (0.0 to 1.0)")]
        [Range(0f, 1f)]
        public float MasterVolume = 0.8f;

        [Tooltip("Music volume level (0.0 to 1.0)")]
        [Range(0f, 1f)]
        public float MusicVolume = 0.7f;

        [Tooltip("Sound effects volume level (0.0 to 1.0)")]
        [Range(0f, 1f)]
        public float SfxVolume = 0.8f;

        [Tooltip("Is audio muted")]
        public bool IsMuted = false;

        #endregion

        #region Graphics Settings

        [Header("Graphics")]
        [Tooltip("Screen resolution quality level (0-5)")]
        [Range(0, 5)]
        public int QualityLevel = 3;

        [Tooltip("Vertical sync count")]
        public int VSyncCount = 1;

        [Tooltip("Target frame rate (0 = unlimited)")]
        public int TargetFrameRate = 60;

        [Tooltip("Fullscreen mode")]
        public FullScreenMode FullScreenMode = FullScreenMode.FullScreenWindow;

        [Tooltip("Screen resolution width")]
        public int ResolutionWidth = 1920;

        [Tooltip("Screen resolution height")]
        public int ResolutionHeight = 1080;

        #endregion

        #region Gameplay Settings

        [Header("Gameplay")]
        [Tooltip("Display subtitles")]
        public bool ShowSubtitles = true;

        [Tooltip("Invert Y axis for camera controls")]
        public bool InvertYAxis = false;

        [Tooltip("Mouse sensitivity")]
        [Range(0.1f, 2f)]
        public float MouseSensitivity = 1.0f;

        [Tooltip("Controller sensitivity")]
        [Range(0.1f, 2f)]
        public float ControllerSensitivity = 1.0f;

        #endregion

        #region Accessibility Settings

        [Header("Accessibility")]
        [Tooltip("High contrast mode")]
        public bool HighContrastMode = false;

        [Tooltip("Color blind mode")]
        public ColorBlindMode ColorBlindMode = ColorBlindMode.None;

        [Tooltip("Screen reader enabled")]
        public bool ScreenReaderEnabled = false;

        [Tooltip("Larger text size")]
        public bool LargeText = false;

        #endregion

        #region Other Settings

        [Header("Other")]
        [Tooltip("Language code")]
        public string Language = "en";

        [Tooltip("Show tutorial hints")]
        public bool ShowTutorials = true;

        [Tooltip("Auto-save enabled")]
        public bool AutoSaveEnabled = true;

        #endregion

        /// <summary>
        /// Creates a default settings instance.
        /// </summary>
        public static SettingsData CreateDefault() => new();

        /// <summary>
        /// Serializes settings to JSON.
        /// </summary>
        public string ToJson() => JsonUtility.ToJson(this, prettyPrint: true);

        /// <summary>
        /// Deserializes settings from JSON.
        /// </summary>
        public static SettingsData FromJson(string json) =>
            string.IsNullOrEmpty(json) ? CreateDefault() : JsonUtility.FromJson<SettingsData>(json);
    }

    /// <summary>
    /// Color blindness accessibility modes.
    /// </summary>
    [Serializable]
    public enum ColorBlindMode
    {
        None,
        Protanopia,     // Red-weak
        Deuteranopia,   // Green-weak
        Tritanopia,     // Blue-weak
        Monochromacy    // Total color blindness
    }
}
