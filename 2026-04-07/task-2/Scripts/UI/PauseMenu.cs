using UnityEngine;
using UnityEngine.UI;

namespace DailyTasks.UI
{
    /// <summary>
    /// 暂停菜单界面 - 包含继续、重新开始、设置、退出按钮
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Optional Components")]
        [SerializeField] private Text pauseTitleText;
        [SerializeField] private string pauseTitle = "PAUSED";

        [Header("Animation")]
        [SerializeField] private float buttonAppearDelay = 0.08f;
        [SerializeField] private float buttonAnimationDuration = 0.25f;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeButtons();
            InitializeTitle();
        }

        private void OnEnable()
        {
            // 每次面板启用时重新初始化按钮并播放动画
            InitializeButtons();
            AnimatePanelIn();
        }

        private void OnDisable()
        {
            // 面板禁用时可以执行清理操作
        }

        #endregion

        #region Private Methods

        private void InitializeButtons()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void InitializeTitle()
        {
            if (pauseTitleText != null)
            {
                pauseTitleText.text = pauseTitle;
            }
        }

        private void AnimatePanelIn()
        {
            // 整体面板淡入
            CanvasGroup panelCanvasGroup = GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            StartCoroutine(AnimatePanelInCoroutine(panelCanvasGroup));
            AnimateButtonsIn();
        }

        private System.Collections.IEnumerator AnimatePanelInCoroutine(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0f;

            float elapsed = 0f;
            float duration = 0.2f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private void AnimateButtonsIn()
        {
            Button[] buttons = { resumeButton, restartButton, settingsButton, quitButton };

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    CanvasGroup canvasGroup = buttons[i].GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = buttons[i].gameObject.AddComponent<CanvasGroup>();
                    }

                    // 初始状态
                    canvasGroup.alpha = 0f;
                    RectTransform rectTransform = buttons[i].transform as RectTransform;
                    if (rectTransform != null)
                    {
                        Vector3 originalPos = rectTransform.anchoredPosition;
                        rectTransform.anchoredPosition = originalPos + Vector2.up * 20f;
                    }

                    // 启动动画协程
                    StartCoroutine(AnimateButtonIn(canvasGroup, buttons[i].transform, i * buttonAppearDelay));
                }
            }
        }

        private System.Collections.IEnumerator AnimateButtonIn(CanvasGroup canvasGroup, Transform transform, float delay)
        {
            yield return new WaitForSeconds(delay);

            float elapsed = 0f;

            while (elapsed < buttonAnimationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / buttonAnimationDuration;

                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

                RectTransform rectTransform = transform as RectTransform;
                if (rectTransform != null)
                {
                    Vector3 currentPos = rectTransform.anchoredPosition;
                    rectTransform.anchoredPosition = Vector3.Lerp(
                        currentPos,
                        rectTransform.anchoredPosition - Vector2.up * 20f * (1f - t),
                        t
                    );
                }

                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        #endregion

        #region Button Callbacks

        private void OnResumeClicked()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ResumeGame();
            }
        }

        private void OnRestartClicked()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RestartLevel();
            }
        }

        private void OnSettingsClicked()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowSettingsMenu();
            }
        }

        private void OnQuitClicked()
        {
            // 从暂停菜单退出通常返回主菜单
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMainMenu();
            }
        }

        #endregion
    }
}
