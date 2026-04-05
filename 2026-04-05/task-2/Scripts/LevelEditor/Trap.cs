using UnityEngine;

namespace LevelEditor
{
    /// <summary>
    /// Component for trap objects that damage the player
    /// </summary>
    public class Trap : PlaceableObject
    {
        [Header("Trap Settings")]
        [Tooltip("Damage dealt to player on contact")]
        [SerializeField] private int _damage = 1;

        [Tooltip("Does this trap destroy the player instantly?")]
        [SerializeField] private bool _isLethal = false;

        [Tooltip("Is this trap activated? (can be toggled)")]
        [SerializeField] private bool _isActive = true;

        [Tooltip("Can this trap be toggled by the player?")]
        [SerializeField] private bool _canBeToggled = false;

        [Header("Moving Trap Settings")]
        [Tooltip("Does this trap move?")]
        [SerializeField] private bool _isMoving = false;

        [Tooltip("Movement speed")]
        [SerializeField] private float _moveSpeed = 3f;

        [Tooltip("Movement range")]
        [SerializeField] private float _moveRange = 2f;

        [Tooltip("Movement axis")]
        [SerializeField] private Vector3 _moveAxis = Vector3.right;

        /// <summary>Damage dealt by this trap</summary>
        public int Damage => _damage;

        /// <summary>Is this trap lethal?</summary>
        public bool IsLethal => _isLethal;

        /// <summary>Is this trap currently active?</summary>
        public bool IsActive => _isActive;

        /// <summary>Can this trap be toggled?</summary>
        public bool CanBeToggled => _canBeToggled;

        private Vector3 _startPosition;
        private float _moveTime;

        private void Start()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            if (_isMoving && _isActive)
            {
                MoveTrap();
            }
        }

        private void MoveTrap()
        {
            _moveTime += Time.deltaTime * _moveSpeed;
            float offset = Mathf.PingPong(_moveTime, _moveRange * 2) - _moveRange;
            transform.position = _startPosition + _moveAxis.normalized * offset;
        }

        /// <summary>
        /// Toggle the trap on/off
        /// </summary>
        public void Toggle()
        {
            if (_canBeToggled)
            {
                _isActive = !_isActive;
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Set the active state
        /// </summary>
        public void SetActive(bool active)
        {
            if (_canBeToggled)
            {
                _isActive = active;
                UpdateVisualState();
            }
        }

        private void UpdateVisualState()
        {
            // Change visual based on active state
            if (TryGetComponent<SpriteRenderer>(out var renderer))
            {
                renderer.color = _isActive ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_isActive) return;

            // Check if player collided with trap
            if (collision.CompareTag("Player"))
            {
                if (_isLethal)
                {
                    // Kill player instantly
                    var playerHealth = collision.GetComponent<IPlayerHealth>();
                    playerHealth?.Kill();
                }
                else
                {
                    // Deal damage
                    var playerHealth = collision.GetComponent<IPlayerHealth>();
                    playerHealth?.TakeDamage(_damage);
                }
            }
        }

        public override PlaceableData GetData()
        {
            var data = base.GetData();
            data.customData = JsonUtility.ToJson(new TrapExtraData
            {
                damage = _damage,
                isLethal = _isLethal,
                isActive = _isActive,
                canBeToggled = _canBeToggled,
                isMoving = _isMoving,
                moveSpeed = _moveSpeed,
                moveRange = _moveRange,
                moveAxis = _moveAxis
            });
            return data;
        }

        public override void SetData(PlaceableData data)
        {
            base.SetData(data);
            if (!string.IsNullOrEmpty(data.customData))
            {
                var extra = JsonUtility.FromJson<TrapExtraData>(data.customData);
                _damage = extra.damage;
                _isLethal = extra.isLethal;
                _isActive = extra.isActive;
                _canBeToggled = extra.canBeToggled;
                _isMoving = extra.isMoving;
                _moveSpeed = extra.moveSpeed;
                _moveRange = extra.moveRange;
                _moveAxis = extra.moveAxis;
                UpdateVisualState();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_isMoving)
            {
                Gizmos.color = Color.red;
                Vector3 basePos = Application.isPlaying ? _startPosition : transform.position;

                Vector3 start = basePos - _moveAxis.normalized * _moveRange;
                Vector3 end = basePos + _moveAxis.normalized * _moveRange;

                Gizmos.DrawLine(start, end);
                Gizmos.DrawWireSphere(start, 0.1f);
                Gizmos.DrawWireSphere(end, 0.1f);
            }
        }

        [System.Serializable]
        private class TrapExtraData
        {
            public int damage;
            public bool isLethal;
            public bool isActive;
            public bool canBeToggled;
            public bool isMoving;
            public float moveSpeed;
            public float moveRange;
            public Vector3 moveAxis;
        }
    }

    /// <summary>
    /// Interface for player health component
    /// </summary>
    public interface IPlayerHealth
    {
        void TakeDamage(int amount);
        void Kill();
    }

    /// <summary>
    /// Extended data class for trap serialization
    /// </summary>
    [System.Serializable]
    public class TrapData : PlaceableData
    {
        public int damage;
        public bool isLethal;
        public bool isActive;
    }
}
