using UnityEngine;
using UnityEngine.UI;

namespace DailyTasks.UI
{
    /// <summary>
    /// 主菜单界面 - 包含开始游戏、设置、退出按钮
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Optional Components")]
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private GameObject versionText;

        [Header("Animation")]
        [SerializeField] private float buttonAppearDelay = 0.1f;
        [SerializeField] private float buttonAnimationDuration = 0.3f;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeButtons();
            AnimateButtonsIn();
        }

        private void OnEnable()
        {
            // 每次面板启用时重新初始化按钮
            InitializeButtons();
        }

        #endregion

        #region Private Methods

        private void InitializeButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(OnStartClicked);
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

        private void AnimateButtonsIn()
        {
            // 简单的按钮淡入动画
            Button[] buttons = { startButton, settingsButton, quitButton };

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
                    buttons[i].transform.localScale = Vector3.one * 0.8f;

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
                float easedT = EaseOutBack(t);

                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                transform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, easedT);

                yield return null;
            }

            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;
        }

        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        #endregion

        #region Button Callbacks

        private void OnStartClicked()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.StartGame();
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
            if (UIManager.Instance != null)
            {
                UIManager.Instance.QuitGame();
            }
        }

        #endregion
    }
}
