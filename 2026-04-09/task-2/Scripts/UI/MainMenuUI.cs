using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameUI
{
    /// <summary>
    /// Main menu UI controller.
    /// Handles navigation and interactions in the main menu.
    /// Attach this to the main menu panel GameObject.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        #region References

        [Header("UI References")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _quitButton;

        [SerializeField] private TextMeshProUGUI _gameTitleText;
        [SerializeField] private TextMeshProUGUI _versionText;
        [SerializeField] private TextMeshProUGUI _continueButtonText;

        [Header("Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _creditsPanel;

        [Header("Settings")]
        [SerializeField] private string _gameTitle = "Game Title";
        [SerializeField] private Color _titleColor = Color.white;

        #endregion

        #region State

        private bool _hasSaveData;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeUI();
            SetupButtons();
            CheckSaveData();
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (_newGameButton != null) _newGameButton.onClick.RemoveAllListeners();
            if (_continueButton != null) _continueButton.onClick.RemoveAllListeners();
            if (_settingsButton != null) _settingsButton.onClick.RemoveAllListeners();
            if (_creditsButton != null) _creditsButton.onClick.RemoveAllListeners();
            if (_quitButton != null) _quitButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Initialization

        private void InitializeUI()
        {
            // Set game title
            if (_gameTitleText != null)
            {
                _gameTitleText.text = _gameTitle;
                _gameTitleText.color = _titleColor;
            }

            // Set version
            if (_versionText != null)
            {
                _versionText.text = $"v{Application.version}";
            }

            // Hide credits panel initially
            if (_creditsPanel != null)
            {
                _creditsPanel.SetActive(false);
            }
        }

        private void SetupButtons()
        {
            if (_newGameButton != null)
            {
                _newGameButton.onClick.AddListener(OnNewGameClicked);
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_creditsButton != null)
            {
                _creditsButton.onClick.AddListener(OnCreditsClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void CheckSaveData()
        {
            // Check for existing save data
            _hasSaveData = SaveSystem.Exists();
            UpdateContinueButton();
        }

        private void UpdateContinueButton()
        {
            if (_continueButton != null)
            {
                _continueButton.gameObject.SetActive(_hasSaveData);
            }

            if (_continueButtonText != null && _hasSaveData)
            {
                // Optionally show save slot info
                var saveInfo = SaveSystem.GetSaveInfo();
                _continueButtonText.text = saveInfo != null ? $"Continue: {saveInfo.SlotName}" : "Continue";
            }
        }

        #endregion

        #region Button Handlers

        private void OnNewGameClicked()
        {
            // Optionally show confirmation dialog
            if (_hasSaveData)
            {
                ShowNewGameConfirmation();
            }
            else
            {
                StartNewGame();
            }
        }

        private void OnContinueClicked()
        {
            if (_hasSaveData)
            {
                LoadGame();
            }
        }

        private void OnSettingsClicked()
        {
            UIManager.Instance?.OpenSettings();
        }

        private void OnCreditsClicked()
        {
            ShowCredits();
        }

        private void OnQuitClicked()
        {
            UIManager.Instance?.QuitGame();
        }

        #endregion

        #region Game Flow

        private void ShowNewGameConfirmation()
        {
            // TODO: Show confirmation dialog
            // For now, just start new game
            StartNewGame();
        }

        private void StartNewGame()
        {
            UIManager.Instance?.StartGame();
        }

        private void LoadGame()
        {
            // TODO: Implement save loading
            UIManager.Instance?.StartGame();
        }

        private void ShowCredits()
        {
            if (_creditsPanel != null)
            {
                _creditsPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hides the credits panel and returns to main menu.
        /// </summary>
        public void HideCredits()
        {
            if (_creditsPanel != null)
            {
                _creditsPanel.SetActive(false);
            }
        }

        #endregion

        #region Animation Support

        /// <summary>
        /// Plays menu entry animation.
        /// Override in derived class or call from animation event.
        /// </summary>
        public void PlayEntryAnimation()
        {
            // TODO: Add animation support
        }

        /// <summary>
        /// Plays menu exit animation.
        /// Override in derived class or call from animation event.
        /// </summary>
        public void PlayExitAnimation()
        {
            // TODO: Add animation support
        }

        #endregion
    }

    /// <summary>
    /// Simple save system interface.
    /// Implement this with your actual save system.
    /// </summary>
    public static class SaveSystem
    {
        public static bool Exists()
        {
            return PlayerPrefs.HasKey("HasSaveData");
        }

        public static SaveInfo GetSaveInfo()
        {
            if (Exists())
            {
                return new SaveInfo
                {
                    SlotName = PlayerPrefs.GetString("SaveSlotName", "Slot 1"),
                    PlayTime = PlayerPrefs.GetFloat("SavePlayTime", 0f),
                    Timestamp = PlayerPrefs.GetString("SaveTimestamp", string.Empty)
                };
            }
            return null;
        }

        public static void ClearSave()
        {
            PlayerPrefs.DeleteKey("HasSaveData");
            PlayerPrefs.DeleteKey("SaveSlotName");
            PlayerPrefs.DeleteKey("SavePlayTime");
            PlayerPrefs.DeleteKey("SaveTimestamp");
            PlayerPrefs.Save();
        }
    }

    [Serializable]
    public class SaveInfo
    {
        public string SlotName;
        public float PlayTime;
        public string Timestamp;
    }
}
