using UnityEngine;

namespace LevelEditor
{
    /// <summary>
    /// Represents a platform in the game level.
    /// Can be normal ground, ice (slippery), bouncy, moving, or breakable.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class Platform : MonoBehaviour
    {
        [Header("Platform Settings")]
        [SerializeField] private PlatformType platformType = PlatformType.Normal;

        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color normalColor = new(0.4f, 0.3f, 0.2f);
        [SerializeField] private Color iceColor = new(0.6f, 0.8f, 1f);
        [SerializeField] private Color bouncyColor = new(1f, 0.4f, 0.8f);
        [SerializeField] private Color movingColor = new(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color breakableColor = new(0.8f, 0.5f, 0.3f);

        [Header("Moving Platform Settings")]
        [SerializeField] private Vector2 moveDirection = Vector2.right;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveDistance = 3f;

        [Header("Breakable Platform Settings")]
        [SerializeField] private float breakDelay = 0.5f;
        [SerializeField] private int maxTouches = 1;

        private Vector2 startPosition;
        private int currentTouches;
        private BoxCollider2D collider;

        public PlatformType Type => platformType;
        public float BounceForce => platformType == PlatformType.Bouncy ? 15f : 0f;
        public float Friction => platformType == PlatformType.Ice ? 0.02f : 0.6f;

        private void Awake()
        {
            collider = GetComponent<BoxCollider2D>();
            startPosition = transform.position;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            UpdateAppearance();
        }

        private void Update()
        {
            if (platformType == PlatformType.Moving)
            {
                UpdateMovement();
            }
        }

        private void UpdateMovement()
        {
            float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
            transform.position = startPosition + moveDirection * offset;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (platformType == PlatformType.Breakable)
            {
                currentTouches++;
                if (currentTouches >= maxTouches)
                {
                    StartCoroutine(BreakAfterDelay());
                }
            }
        }

        private System.Collections.IEnumerator BreakAfterDelay()
        {
            yield return new WaitForSeconds(breakDelay);
            Destroy(gameObject);
        }

        /// <summary>
        /// Updates the platform's visual appearance based on its type.
        /// </summary>
        public void UpdateAppearance()
        {
            if (spriteRenderer == null) return;

            Color targetColor = platformType switch
            {
                PlatformType.Ice => iceColor,
                PlatformType.Bouncy => bouncyColor,
                PlatformType.Moving => movingColor,
                PlatformType.Breakable => breakableColor,
                _ => normalColor
            };

            spriteRenderer.color = targetColor;
        }

        /// <summary>
        /// Sets the platform type and updates appearance.
        /// </summary>
        public void SetType(PlatformType newType)
        {
            platformType = newType;
            UpdateAppearance();
        }

        /// <summary>
        /// Gets the platform data for serialization.
        /// </summary>
        public PlatformData GetData()
        {
            return new PlatformData
            {
                position = transform.position,
                scale = transform.localScale,
                type = platformType
            };
        }

        /// <summary>
        /// Loads data from a PlatformData object.
        /// </summary>
        public void LoadData(PlatformData data)
        {
            transform.position = data.position;
            transform.localScale = data.scale;
            platformType = data.type;
            startPosition = data.position;
            UpdateAppearance();
        }
    }
}
