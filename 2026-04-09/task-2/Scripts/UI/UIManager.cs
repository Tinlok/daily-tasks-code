using UnityEngine;
using System;
using System.Collections.Generic;

namespace GameUI
{
    /// <summary>
    /// Manages all UI panels and their transitions.
    /// Handles panel showing/hiding with optional animations.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Singleton

        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("UIManager");
                        _instance = go.AddComponent<UIManager>();
                        DontDestroyOnLoad(go);
                    }
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
        }

        #endregion

        #region Events

        /// <summary>
        /// Fired when a panel is shown.
        /// </summary>
        public event Action<UIPanelType> OnPanelShown;

        /// <summary>
        /// Fired when a panel is hidden.
        /// </summary>
        public event Action<UIPanelType> OnPanelHidden;

        /// <summary>
        /// Fired when the pause state changes.
        /// </summary>
        public event Action<bool> OnPauseStateChanged;

        #endregion

        #region Panel References

        [Header("Panel References")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _pauseMenuPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _hudPanel;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _victoryPanel;

        private readonly Dictionary<UIPanelType, GameObject> _panels = new();

        #endregion

        #region State

        private UIPanelType _currentPanel = UIPanelType.None;
        private readonly Stack<UIPanelType> _panelHistory = new();
        private bool _isPaused;

        /// <summary>
        /// Currently visible panel type.
        /// </summary>
        public UIPanelType CurrentPanel => _currentPanel;

        /// <summary>
        /// Is the game currently paused?
        /// </summary>
        public bool IsPaused => _isPaused;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializePanels();
            HideAllPanels();
        }

        private void Update()
        {
            HandlePauseInput();
        }

        #endregion

        #region Initialization

        private void InitializePanels()
        {
            _panels.Clear();

            if (_mainMenuPanel != null) _panels[UIPanelType.MainMenu] = _mainMenuPanel;
            if (_pauseMenuPanel != null) _panels[UIPanelType.PauseMenu] = _pauseMenuPanel;
            if (_settingsPanel != null) _panels[UIPanelType.Settings] = _settingsPanel;
            if (_hudPanel != null) _panels[UIPanelType.HUD] = _hudPanel;
            if (_gameOverPanel != null) _panels[UIPanelType.GameOver] = _gameOverPanel;
            if (_victoryPanel != null) _panels[UIPanelType.Victory] = _victoryPanel;
        }

        public void RegisterPanel(UIPanelType type, GameObject panel)
        {
            if (panel != null)
            {
                _panels[type] = panel;
            }
        }

        #endregion

        #region Panel Management

        /// <summary>
        /// Shows a panel of the specified type.
        /// </summary>
        public void ShowPanel(UIPanelType panelType, bool addToHistory = true)
        {
            if (_currentPanel == panelType) return;

            // Hide current panel
            if (_currentPanel != UIPanelType.None)
            {
                HidePanelImmediate(_currentPanel);
            }

            // Add to history if requested
            if (addToHistory && _currentPanel != UIPanelType.None)
            {
                _panelHistory.Push(_currentPanel);
            }

            // Show new panel
            ShowPanelImmediate(panelType);
            _currentPanel = panelType;
            OnPanelShown?.Invoke(panelType);

            // Handle pause state
            UpdatePauseState();
        }

        /// <summary>
        /// Hides the current panel and returns to the previous one.
        /// </summary>
        public void GoBack()
        {
            if (_panelHistory.Count > 0)
            {
                var previousPanel = _panelHistory.Pop();
                HidePanelImmediate(_currentPanel);
                ShowPanelImmediate(previousPanel);
                _currentPanel = previousPanel;
                OnPanelShown?.Invoke(previousPanel);
                UpdatePauseState();
            }
            else
            {
                HideAllPanels();
            }
        }

        /// <summary>
        /// Hides all panels and clears history.
        /// </summary>
        public void HideAllPanels()
        {
            foreach (var panel in _panels.Values)
            {
                if (panel != null) panel.SetActive(false);
            }
            _currentPanel = UIPanelType.None;
            _panelHistory.Clear();
            _isPaused = false;
            OnPauseStateChanged?.Invoke(false);
            Time.timeScale = 1f;
        }

        private void ShowPanelImmediate(UIPanelType panelType)
        {
            if (_panels.TryGetValue(panelType, out var panel))
            {
                panel.SetActive(true);
            }
        }

        private void HidePanelImmediate(UIPanelType panelType)
        {
            if (_panels.TryGetValue(panelType, out var panel))
            {
                panel.SetActive(false);
            }
            OnPanelHidden?.Invoke(panelType);
        }

        /// <summary>
        /// Checks if a specific panel is currently visible.
        /// </summary>
        public bool IsPanelVisible(UIPanelType panelType)
        {
            if (_panels.TryGetValue(panelType, out var panel))
            {
                return panel != null && panel.activeSelf;
            }
            return false;
        }

        #endregion

        #region Pause Management

        private void HandlePauseInput()
        {
            // Toggle pause with Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_currentPanel == UIPanelType.PauseMenu || _currentPanel == UIPanelType.Settings)
                {
                    if (_panelHistory.Count > 0 && _panelHistory.Peek() == UIPanelType.PauseMenu)
                    {
                        GoBack(); // Return from settings to pause menu
                    }
                    else if (_currentPanel == UIPanelType.PauseMenu)
                    {
                        ResumeGame();
                    }
                }
                else if (_currentPanel == UIPanelType.HUD || _currentPanel == UIPanelType.None)
                {
                    PauseGame();
                }
            }
        }

        /// <summary>
        /// Pauses the game and shows the pause menu.
        /// </summary>
        public void PauseGame()
        {
            ShowPanel(UIPanelType.PauseMenu, addToHistory: false);
        }

        /// <summary>
        /// Resumes the game and hides pause menu.
        /// </summary>
        public void ResumeGame()
        {
            HideAllPanels();
            ShowPanelImmediate(UIPanelType.HUD);
            _currentPanel = UIPanelType.HUD;
        }

        private void UpdatePauseState()
        {
            bool shouldBePaused = _currentPanel == UIPanelType.PauseMenu ||
                                  _currentPanel == UIPanelType.Settings ||
                                  _currentPanel == UIPanelType.MainMenu;

            if (shouldBePaused != _isPaused)
            {
                _isPaused = shouldBePaused;
                Time.timeScale = _isPaused ? 0f : 1f;
                OnPauseStateChanged?.Invoke(_isPaused);
            }
        }

        #endregion

        #region Game Flow

        /// <summary>
        /// Shows the main menu.
        /// </summary>
        public void ShowMainMenu()
        {
            HideAllPanels();
            ShowPanel(UIPanelType.MainMenu, addToHistory: false);
        }

        /// <summary>
        /// Starts a new game.
        /// </summary>
        public void StartGame()
        {
            HideAllPanels();
            ShowPanelImmediate(UIPanelType.HUD);
            _currentPanel = UIPanelType.HUD;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Shows the game over screen.
        /// </summary>
        public void ShowGameOver()
        {
            ShowPanel(UIPanelType.GameOver, addToHistory: false);
        }

        /// <summary>
        /// Shows the victory screen.
        /// </summary>
        public void ShowVictory()
        {
            ShowPanel(UIPanelType.Victory, addToHistory: false);
        }

        /// <summary>
        /// Opens settings from the current panel.
        /// </summary>
        public void OpenSettings()
        {
            ShowPanel(UIPanelType.Settings, addToHistory: true);
        }

        /// <summary>
        /// Quits the application.
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }

    /// <summary>
    /// Types of UI panels in the game.
    /// </summary>
    [Serializable]
    public enum UIPanelType
    {
        None,
        MainMenu,
        PauseMenu,
        Settings,
        HUD,
        GameOver,
        Victory
    }
}
