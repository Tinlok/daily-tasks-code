using System;
using System.Collections;
using UnityEngine;

namespace LevelEditor
{
    /// <summary>
    /// Represents a trap hazard in the game level.
    /// Damages the player when touched.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Trap : MonoBehaviour
    {
        [Header("Trap Settings")]
        [SerializeField] private TrapType trapType = TrapType.Spike;
        [SerializeField] private float damage = 1f;
        [SerializeField] private bool isActivated = true;

        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color activeColor = Color.red;
        [SerializeField] private Color inactiveColor = Color.gray;

        [Header("Animation Settings")]
        [SerializeField] private float animationSpeed = 2f;

        [Header("Laser Settings")]
        [SerializeField] private float laserLength = 5f;
        [SerializeField] private LineRenderer laserLine;

        [Header("Crusher Settings")]
        [SerializeField] private float crusherSpeed = 3f;
        [SerializeField] private float crusherHeight = 2f;

        private Vector2 startPosition;
        private Collider2D trapCollider;

        public TrapType Type => trapType;
        public float Damage => damage;
        public bool IsActivated
        {
            get => isActivated;
            set => isActivated = value;
        }

        private void Awake()
        {
            trapCollider = GetComponent<Collider2D>();
            startPosition = transform.position;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (laserLine == null)
            {
                laserLine = GetComponent<LineRenderer>();
            }

            trapCollider.isTrigger = true;
            UpdateAppearance();
        }

        private void Update()
        {
            if (!isActivated) return;

            switch (trapType)
            {
                case TrapType.Sawblade:
                    AnimateSawblade();
                    break;
                case TrapType.Laser:
                    UpdateLaser();
                    break;
                case TrapType.Crusher:
                    AnimateCrusher();
                    break;
            }
        }

        private void AnimateSawblade()
        {
            transform.Rotate(Vector3.forward, 360 * animationSpeed * Time.deltaTime);
        }

        private void UpdateLaser()
        {
            if (laserLine != null)
            {
                laserLine.enabled = true;
                laserLine.SetPosition(0, transform.position);
                laserLine.SetPosition(1, transform.position + Vector3.right * laserLength);
            }
        }

        private void AnimateCrusher()
        {
            float offset = Mathf.Abs(Mathf.Sin(Time.time * crusherSpeed)) * crusherHeight;
            transform.position = startPosition + Vector2.down * offset;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActivated) return;

            if (other.CompareTag("Player"))
            {
                var health = other.GetComponent<IHealth>();
                health?.TakeDamage(damage);
            }
        }

        /// <summary>
        /// Updates the trap's visual appearance based on activation state.
        /// </summary>
        public void UpdateAppearance()
        {
            if (spriteRenderer == null) return;

            spriteRenderer.color = isActivated ? activeColor : inactiveColor;
        }

        /// <summary>
        /// Toggles the trap's active state.
        /// </summary>
        public void Toggle()
        {
            isActivated = !isActivated;
            UpdateAppearance();
        }

        /// <summary>
        /// Sets the trap type.
        /// </summary>
        public void SetType(TrapType newType)
        {
            trapType = newType;
        }

        /// <summary>
        /// Gets the trap data for serialization.
        /// </summary>
        public TrapData GetData()
        {
            return new TrapData
            {
                position = transform.position,
                type = trapType,
                damage = damage,
                isActivated = isActivated
            };
        }

        /// <summary>
        /// Loads data from a TrapData object.
        /// </summary>
        public void LoadData(TrapData data)
        {
            transform.position = data.position;
            startPosition = data.position;
            trapType = data.type;
            damage = data.damage;
            isActivated = data.isActivated;
            UpdateAppearance();
        }
    }

    /// <summary>
    /// Interface for entities that can take damage.
    /// </summary>
    public interface IHealth
    {
        void TakeDamage(float amount);
    }
}
