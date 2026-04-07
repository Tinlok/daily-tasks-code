using UnityEngine;
using System.Collections;

namespace DailyTasks.UI
{
    /// <summary>
    /// UI面板基类 - 提供通用的面板动画和生命周期管理
    /// </summary>
    public abstract class UIPanelBase : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Animation Settings")]
        [SerializeField] protected float fadeDuration = 0.3f;
        [SerializeField] protected AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] protected bool animateOnEnable = true;

        #endregion

        #region Protected Fields

        protected CanvasGroup _canvasGroup;
        protected Coroutine _currentAnimation;

        #endregion

        #region Properties

        public bool IsAnimating { get; private set; }
        public bool IsVisible { get; private set; }

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        protected virtual void OnEnable()
        {
            if (animateOnEnable)
            {
                Show();
            }
        }

        protected virtual void OnDisable()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
                _currentAnimation = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 显示面板（带淡入动画）
        /// </summary>
        public virtual void Show()
        {
            gameObject.SetActive(true);

            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _currentAnimation = StartCoroutine(FadeIn());
        }

        /// <summary>
        /// 隐藏面板（带淡出动画）
        /// </summary>
        public virtual void Hide()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _currentAnimation = StartCoroutine(FadeOut());
        }

        /// <summary>
        /// 立即显示面板，无动画
        /// </summary>
        public virtual void ShowImmediate()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
                _currentAnimation = null;
            }

            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            IsVisible = true;
        }

        /// <summary>
        /// 立即隐藏面板，无动画
        /// </summary>
        public virtual void HideImmediate()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
                _currentAnimation = null;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            IsVisible = false;
        }

        #endregion

        #region Protected Methods

        protected virtual void OnShowComplete()
        {
            IsVisible = true;
            OnPanelShown();
        }

        protected virtual void OnHideComplete()
        {
            IsVisible = false;
            gameObject.SetActive(false);
            OnPanelHidden();
        }

        /// <summary>
        /// 面板显示完成时的回调，子类可重写
        /// </summary>
        protected virtual void OnPanelShown() { }

        /// <summary>
        /// 面板隐藏完成时的回调，子类可重写
        /// </summary>
        protected virtual void OnPanelHidden() { }

        #endregion

        #region Private Methods - Coroutines

        private IEnumerator FadeIn()
        {
            IsAnimating = true;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            float startAlpha = _canvasGroup.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeDuration;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, fadeCurve.Evaluate(t));
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            IsAnimating = false;

            OnShowComplete();
        }

        private IEnumerator FadeOut()
        {
            IsAnimating = true;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            float startAlpha = _canvasGroup.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeDuration;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, fadeCurve.Evaluate(t));
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            IsAnimating = false;

            OnHideComplete();
        }

        #endregion
    }
}
