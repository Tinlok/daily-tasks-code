using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace DailyTasks.UI
{
    /// <summary>
    /// 按钮动画组件 - 为UI按钮提供悬停和点击动画效果
    /// </summary>
    public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Serialized Fields

        [Header("Transform Animation")]
        [SerializeField] private bool animateScale = true;
        [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f);
        [SerializeField] private Vector3 pressedScale = new Vector3(0.95f, 0.95f, 1f);
        [SerializeField] private float scaleDuration = 0.15f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Color Animation")]
        [SerializeField] private bool animateColor = true;
        [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 1f, 1f);
        [SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.8f, 1f);
        [SerializeField] private float colorDuration = 0.1f;

        [Header("Sound")]
        [SerializeField] private bool playSound = false;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private float soundVolume = 1f;

        #endregion

        #region Private Fields

        private Button _button;
        private Image _image;
        private Text _text;
        private Color _originalColor;
        private Vector3 _originalScale;
        private Coroutine _scaleCoroutine;
        private Coroutine _colorCoroutine;
        private AudioSource _audioSource;

        #endregion

        #region Properties

        public bool IsInteractable
        {
            get => _button != null && _button.interactable;
            set
            {
                if (_button != null)
                {
                    _button.interactable = value;
                }
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
            _text = GetComponentInChildren<Text>();
            _originalScale = transform.localScale;

            if (_image != null)
            {
                _originalColor = _image.color;
            }
            else if (_text != null)
            {
                _originalColor = _text.color;
            }

            // 创建AudioSource（如果需要播放音效）
            if (playSound && (hoverSound != null || clickSound != null))
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.volume = soundVolume;
            }
        }

        private void OnDisable()
        {
            // 重置状态
            transform.localScale = _originalScale;

            if (_image != null)
            {
                _image.color = _originalColor;
            }

            if (_text != null)
            {
                _text.color = _originalColor;
            }
        }

        #endregion

        #region Pointer Event Handlers

        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!IsInteractable) return;

            if (animateScale)
            {
                AnimateScale(hoverScale);
            }

            if (animateColor)
            {
                AnimateColor(hoverColor);
            }

            if (playSound && hoverSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(hoverSound, soundVolume);
            }
        }

        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!IsInteractable) return;

            if (animateScale)
            {
                AnimateScale(_originalScale);
            }

            if (animateColor)
            {
                AnimateColor(_originalColor);
            }
        }

        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!IsInteractable) return;

            if (animateScale)
            {
                AnimateScale(pressedScale);
            }

            if (animateColor)
            {
                AnimateColor(pressedColor);
            }
        }

        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!IsInteractable) return;

            if (animateScale)
            {
                AnimateScale(hoverScale);
            }

            if (animateColor)
            {
                AnimateColor(hoverColor);
            }

            if (playSound && clickSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(clickSound, soundVolume);
            }
        }

        #endregion

        #region Private Methods - Animation

        private void AnimateScale(Vector3 targetScale)
        {
            if (_scaleCoroutine != null)
            {
                StopCoroutine(_scaleCoroutine);
            }

            _scaleCoroutine = StartCoroutine(ScaleAnimation(targetScale));
        }

        private IEnumerator ScaleAnimation(Vector3 targetScale)
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            while (elapsed < scaleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / scaleDuration;
                transform.localScale = Vector3.Lerp(startScale, targetScale, scaleCurve.Evaluate(t));
                yield return null;
            }

            transform.localScale = targetScale;
        }

        private void AnimateColor(Color targetColor)
        {
            if (_colorCoroutine != null)
            {
                StopCoroutine(_colorCoroutine);
            }

            _colorCoroutine = StartCoroutine(ColorAnimation(targetColor));
        }

        private IEnumerator ColorAnimation(Color targetColor)
        {
            float elapsed = 0f;
            Color startColor = _image != null ? _image.color : (_text != null ? _text.color : Color.white);

            while (elapsed < colorDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / colorDuration;
                Color newColor = Color.Lerp(startColor, targetColor, t);

                if (_image != null)
                {
                    _image.color = newColor;
                }

                if (_text != null)
                {
                    _text.color = newColor;
                }

                yield return null;
            }

            if (_image != null)
            {
                _image.color = targetColor;
            }

            if (_text != null)
            {
                _text.color = targetColor;
            }
        }

        #endregion
    }
}
