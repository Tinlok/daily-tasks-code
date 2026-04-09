using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameUI
{
    /// <summary>
    /// Pause menu UI controller.
    /// Handles pause menu interactions and game state management.
    /// Attach this to the pause menu panel GameObject.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        #region References

        [Header("Main Menu Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _saveGameButton;
        [SerializeField] private Button _loadGameButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _quitButton;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _pauseTitleText;
        [SerializeField] private TextMeshProUGUI _saveStatusText;
        [SerializeField] private GameObject _saveIndicator;

        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject _confirmationDialog;
        [SerializeField] private TextMeshProUGUI _confirmationText;
        [SerializeField] private Button _confirmYesButton;
        [SerializeField] private Button _confirmNoButton;

        [Header("Settings")]
        [SerializeField] private string _pauseTitle = "PAUSED";
        [SerializeField] private float _saveDisplayDuration = 2f;

        #endregion

        #region State

        private System.Action _pendingAction;
        private float _saveStatusTimer;
        private bool _isWaitingForConfirmation;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeUI();
            SetupButtons();
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (_resumeButton != null) _resumeButton.onClick.RemoveAllListeners();
            if (_settingsButton != null) _settingsButton.onClick.RemoveAllListeners();
            if (_saveGameButton != null) _saveGameButton.onClick.RemoveAllListeners();
            if (_loadGameButton != null) _loadGameButton.onClick.RemoveAllListeners();
            if (_restartButton != null) _restartButton.onClick.RemoveAllListeners();
            if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveAllListeners();
            if (_quitButton != null) _quitButton.onClick.RemoveAllListeners();
            if (_confirmYesButton != null) _confirmYesButton.onClick.RemoveAllListeners();
            if (_confirmNoButton != null) _confirmNoButton.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            UpdateSaveStatus();
        }

        #endregion

        #region Initialization

        private void InitializeUI()
        {
            // Set pause title
            if (_pauseTitleText != null)
            {
                _pauseTitleText.text = _pauseTitle;
            }

            // Hide save status initially
            if (_saveStatusText != null)
            {
                _saveStatusText.gameObject.SetActive(false);
            }

            if (_saveIndicator != null)
            {
                _saveIndicator.SetActive(false);
            }

            // Hide confirmation dialog
            if (_confirmationDialog != null)
            {
                _confirmationDialog.SetActive(false);
            }
        }

        private void SetupButtons()
        {
            if (_resumeButton != null)
            {
                _resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_saveGameButton != null)
            {
                _saveGameButton.onClick.AddListener(OnSaveGameClicked);
            }

            if (_loadGameButton != null)
            {
                _loadGameButton.onClick.AddListener(OnLoadGameClicked);
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
            }

            // Confirmation dialog buttons
            if (_confirmYesButton != null)
            {
                _confirmYesButton.onClick.AddListener(OnConfirmationYes);
            }

            if (_confirmNoButton != null)
            {
                _confirmNoButton.onClick.AddListener(OnConfirmationNo);
            }
        }

        #endregion

        #region Button Handlers

        private void OnResumeClicked()
        {
            ResumeGame();
        }

        private void OnSettingsClicked()
        {
            OpenSettings();
        }

        private void OnSaveGameClicked()
        {
            SaveGame();
        }

        private void OnLoadGameClicked()
        {
            LoadGame();
        }

        private void OnRestartClicked()
        {
            ShowConfirmation("Restart from checkpoint?", () =>
            {
                RestartGame();
            });
        }

        private void OnMainMenuClicked()
        {
            ShowConfirmation("Return to main menu? Any unsaved progress will be lost.", () =>
            {
                ReturnToMainMenu();
            });
        }

        private void OnQuitClicked()
        {
            ShowConfirmation("Quit to desktop? Any unsaved progress will be lost.", () =>
            {
                QuitGame();
            });
        }

        #endregion

        #region Game Actions

        /// <summary>
        /// Resumes the game and hides pause menu.
        /// </summary>
        public void ResumeGame()
        {
            UIManager.Instance?.ResumeGame();
        }

        /// <summary>
        /// Opens the settings menu.
        /// </summary>
        public void OpenSettings()
        {
            UIManager.Instance?.OpenSettings();
        }

        /// <summary>
        /// Saves the current game state.
        /// </summary>
        public void SaveGame()
        {
            // TODO: Implement actual save logic
            ShowSaveStatus("Game Saved!", true);
        }

        /// <summary>
        /// Loads the most recent save.
        /// </summary>
        public void LoadGame()
        {
            // TODO: Implement actual load logic
            ShowSaveStatus("Game Loaded!", true);
        }

        /// <summary>
        /// Restarts the current level.
        /// </summary>
        public void RestartGame()
        {
            // TODO: Implement restart logic
            HideConfirmation();
            UIManager.Instance?.ResumeGame();
        }

        /// <summary>
        /// Returns to the main menu.
        /// </summary>
        public void ReturnToMainMenu()
        {
            HideConfirmation();
            UIManager.Instance?.ShowMainMenu();
        }

        /// <summary>
        /// Quits the application.
        /// </summary>
        public void QuitGame()
        {
            HideConfirmation();
            UIManager.Instance?.QuitGame();
        }

        #endregion

        #region Confirmation Dialog

        private void ShowConfirmation(string message, System.Action confirmAction)
        {
            if (_confirmationDialog != null && _confirmationText != null)
            {
                _confirmationText.text = message;
                _confirmationDialog.SetActive(true);
                _pendingAction = confirmAction;
                _isWaitingForConfirmation = true;
            }
            else
            {
                // No confirmation dialog, execute directly
                confirmAction?.Invoke();
            }
        }

        private void HideConfirmation()
        {
            if (_confirmationDialog != null)
            {
                _confirmationDialog.SetActive(false);
            }
            _pendingAction = null;
            _isWaitingForConfirmation = false;
        }

        private void OnConfirmationYes()
        {
            _pendingAction?.Invoke();
            HideConfirmation();
        }

        private void OnConfirmationNo()
        {
            HideConfirmation();
        }

        #endregion

        #region Save Status Display

        private void ShowSaveStatus(string message, bool success)
        {
            if (_saveStatusText != null)
            {
                _saveStatusText.text = message;
                _saveStatusText.color = success ? Color.green : Color.red;
                _saveStatusText.gameObject.SetActive(true);
                _saveStatusTimer = _saveDisplayDuration;
            }

            if (_saveIndicator != null)
            {
                _saveIndicator.SetActive(true);
            }
        }

        private void UpdateSaveStatus()
        {
            if (_saveStatusTimer > 0)
            {
                _saveStatusTimer -= Time.unscaledDeltaTime;

                if (_saveStatusTimer <= 0)
                {
                    if (_saveStatusText != null)
                    {
                        _saveStatusText.gameObject.SetActive(false);
                    }

                    if (_saveIndicator != null)
                    {
                        _saveIndicator.SetActive(false);
                    }
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Called when the pause menu is shown.
        /// </summary>
        public void OnPauseMenuShown()
        {
            // Reset state
            _isWaitingForConfirmation = false;
            _pendingAction = null;
            HideConfirmation();

            // Select resume button for gamepad navigation
            if (_resumeButton != null)
            {
                _resumeButton.Select();
            }
        }

        /// <summary>
        /// Called when the pause menu is hidden.
        /// </summary>
        public void OnPauseMenuHidden()
        {
            // Clean up any open dialogs
            HideConfirmation();
        }

        #endregion
    }
}
