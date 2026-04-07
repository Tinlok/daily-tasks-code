using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DailyTasks.UI
{
    /// <summary>
    /// UI管理器 - 负责UI面板的切换和管理
    /// 单例模式，确保全局只有一个UI管理器
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
                        GameObject go = new GameObject("UIManager");
                        _instance = go.AddComponent<UIManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Serialized Fields

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject hudPanel;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        #endregion

        #region Private Fields

        private CanvasGroup _currentCanvasGroup;
        private Coroutine _currentFadeCoroutine;

        #endregion

        #region Properties

        public bool IsPaused { get; private set; }
        public GameObject CurrentPanel { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 确保单例
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化
            InitializePanels();
        }

        private void Start()
        {
            // 默认显示主菜单
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                ShowMainMenu();
            }
        }

        private void Update()
        {
            // ESC键切换暂停菜单
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsPaused)
                {
                    ResumeGame();
                }
                else if (SceneManager.GetActiveScene().name != "MainMenu")
                {
                    PauseGame();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 显示主菜单
        /// </summary>
        public void ShowMainMenu()
        {
            ShowPanel(mainMenuPanel);
            Time.timeScale = 1f; // 确保时间正常流逝
        }

        /// <summary>
        /// 显示暂停菜单
        /// </summary>
        public void ShowPauseMenu()
        {
            ShowPanel(pauseMenuPanel);
        }

        /// <summary>
        /// 显示设置菜单
        /// </summary>
        public void ShowSettingsMenu()
        {
            ShowPanel(settingsPanel);
        }

        /// <summary>
        /// 返回上一个面板
        /// </summary>
        public void GoBack()
        {
            if (CurrentPanel == settingsPanel)
            {
                if (IsPaused)
                {
                    ShowPauseMenu();
                }
                else
                {
                    ShowMainMenu();
                }
            }
            else if (CurrentPanel == pauseMenuPanel)
            {
                ResumeGame();
            }
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            ShowPauseMenu();
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        public void ResumeGame()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            HidePanel(pauseMenuPanel);
            ShowPanel(hudPanel);
        }

        /// <summary>
        /// 重新开始当前关卡
        /// </summary>
        public void RestartLevel()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// 开始游戏（加载第一个游戏场景）
        /// </summary>
        public void StartGame()
        {
            Time.timeScale = 1f;
            // 假设游戏场景名为 "GameLevel"
            SceneManager.LoadScene("GameLevel");
        }

        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        /// <summary>
        /// 显示指定面板（带淡入动画）
        /// </summary>
        public void ShowPanel(GameObject panel)
        {
            if (panel == null) return;

            // 隐藏当前面板
            if (CurrentPanel != null && CurrentPanel != panel)
            {
                HidePanelImmediate(CurrentPanel);
            }

            CurrentPanel = panel;
            panel.SetActive(true);

            // 淡入动画
            _currentCanvasGroup = panel.GetComponent<CanvasGroup>();
            if (_currentCanvasGroup == null)
            {
                _currentCanvasGroup = panel.AddComponent<CanvasGroup>();
            }

            if (_currentFadeCoroutine != null)
            {
                StopCoroutine(_currentFadeCoroutine);
            }

            _currentFadeCoroutine = StartCoroutine(FadeIn(_currentCanvasGroup));
        }

        /// <summary>
        /// 隐藏指定面板（带淡出动画）
        /// </summary>
        public void HidePanel(GameObject panel)
        {
            if (panel == null) return;

            _currentCanvasGroup = panel.GetComponent<CanvasGroup>();
            if (_currentCanvasGroup == null)
            {
                _currentCanvasGroup = panel.AddComponent<CanvasGroup>();
            }

            if (_currentFadeCoroutine != null)
            {
                StopCoroutine(_currentFadeCoroutine);
            }

            _currentFadeCoroutine = StartCoroutine(FadeOut(panel, _currentCanvasGroup));
        }

        #endregion

        #region Private Methods

        private void InitializePanels()
        {
            // 为所有面板添加CanvasGroup（如果没有）
            InitializePanel(mainMenuPanel);
            InitializePanel(pauseMenuPanel);
            InitializePanel(settingsPanel);
            InitializePanel(hudPanel);

            // 初始隐藏所有面板
            HideAllPanels();
        }

        private void InitializePanel(GameObject panel)
        {
            if (panel == null) return;

            if (panel.GetComponent<CanvasGroup>() == null)
            {
                panel.AddComponent<CanvasGroup>();
            }
        }

        private void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);

            CurrentPanel = null;
        }

        private void HidePanelImmediate(GameObject panel)
        {
            if (panel == null) return;

            var canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            panel.SetActive(false);
        }

        private IEnumerator FadeIn(CanvasGroup canvasGroup)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, fadeCurve.Evaluate(t));
                yield return null;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        private IEnumerator FadeOut(GameObject panel, CanvasGroup canvasGroup)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, fadeCurve.Evaluate(t));
                yield return null;
            }

            canvasGroup.alpha = 0f;
            panel.SetActive(false);

            if (CurrentPanel == panel)
            {
                CurrentPanel = null;
            }
        }

        #endregion
    }
}
