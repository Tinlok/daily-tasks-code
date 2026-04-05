using UnityEngine;

namespace LevelEditor
{
    /// <summary>
    /// Component for collectible objects (coins, gems, health, etc.)
    /// </summary>
    public class Collectible : PlaceableObject
    {
        [Header("Collectible Settings")]
        [Tooltip("Type of collectible")]
        [SerializeField] private CollectibleType _collectibleType = CollectibleType.Coin;

        [Tooltip("Score value when collected")]
        [SerializeField] private int _scoreValue = 10;

        [Tooltip("Health value when collected (for health pickups)")]
        [SerializeField] private int _healthValue = 1;

        [Tooltip("Should this collectible respawn after being collected?")]
        [SerializeField] private bool _respawn = false;

        [Tooltip("Respawn delay in seconds")]
        [SerializeField] private float _respawnDelay = 5f;

        [Header("Visual Settings")]
        [Tooltip("Should the collectible float/bob?")]
        [SerializeField] private bool _shouldFloat = true;

        [Tooltip("Float/bob height")]
        [SerializeField] private float _floatHeight = 0.2f;

        [Tooltip("Float/bob speed")]
        [SerializeField] private float _floatSpeed = 2f;

        [Tooltip("Should the collectible rotate?")]
        [SerializeField] private bool _shouldRotate = true;

        [Tooltip("Rotation speed")]
        [SerializeField] private float _rotationSpeed = 90f;

        /// <summary>Type of collectible</summary>
        public CollectibleType CollectibleType => _collectibleType;

        /// <summary>Score value</summary>
        public int ScoreValue => _scoreValue;

        /// <summary>Health value</summary>
        public int HealthValue => _healthValue;

        /// <summary>Should this collectible respawn?</summary>
        public bool Respawn => _respawn;

        private bool _collected = false;
        private Vector3 _startPosition;
        private float _floatTime;
        private Collider2D _collider;
        private SpriteRenderer _renderer;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            if (_collected) return;

            // Float animation
            if (_shouldFloat)
            {
                _floatTime += Time.deltaTime * _floatSpeed;
                float yOffset = Mathf.Sin(_floatTime) * _floatHeight;
                transform.position = _startPosition + Vector3.up * yOffset;
            }

            // Rotation animation
            if (_shouldRotate)
            {
                transform.Rotate(Vector3.forward, _rotationSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_collected) return;

            if (collision.CompareTag("Player"))
            {
                Collect(collision.gameObject);
            }
        }

        /// <summary>
        /// Collect this item
        /// </summary>
        public void Collect(GameObject player)
        {
            if (_collected) return;

            _collected = true;

            // Apply effect based on type
            switch (_collectibleType)
            {
                case CollectibleType.Coin:
                case CollectibleType.Gem:
                case CollectibleType.Star:
                    AddScore(player);
                    break;
                case CollectibleType.Heart:
                    RestoreHealth(player);
                    break;
                case CollectibleType.Key:
                    AddKey(player);
                    break;
            }

            // Hide or respawn
            if (_respawn)
            {
                StartCoroutine(RespawnCoroutine());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void AddScore(GameObject player)
        {
            var scoreCollector = player.GetComponent<IScoreCollector>();
            scoreCollector?.AddScore(_scoreValue);
        }

        private void RestoreHealth(GameObject player)
        {
            var health = player.GetComponent<IPlayerHealth>();
            health?.TakeDamage(-_healthValue); // Negative damage = heal
        }

        private void AddKey(GameObject player)
        {
            var keyCollector = player.GetComponent<IKeyCollector>();
            keyCollector?.AddKey(_collectibleType);
        }

        private System.Collections.IEnumerator RespawnCoroutine()
        {
            // Disable collision and rendering
            if (_collider) _collider.enabled = false;
            if (_renderer) _renderer.enabled = false;

            yield return new WaitForSeconds(_respawnDelay);

            // Reset and re-enable
            _collected = false;
            transform.position = _startPosition;
            if (_collider) _collider.enabled = true;
            if (_renderer) _renderer.enabled = true;
        }

        public override PlaceableData GetData()
        {
            var data = base.GetData();
            data.customData = JsonUtility.ToJson(new CollectibleExtraData
            {
                collectibleType = (int)_collectibleType,
                scoreValue = _scoreValue,
                healthValue = _healthValue,
                respawn = _respawn,
                respawnDelay = _respawnDelay
            });
            return data;
        }

        public override void SetData(PlaceableData data)
        {
            base.SetData(data);
            if (!string.IsNullOrEmpty(data.customData))
            {
                var extra = JsonUtility.FromJson<CollectibleExtraData>(data.customData);
                _collectibleType = (CollectibleType)extra.collectibleType;
                _scoreValue = extra.scoreValue;
                _healthValue = extra.healthValue;
                _respawn = extra.respawn;
                _respawnDelay = extra.respawnDelay;
            }
        }

        [System.Serializable]
        private class CollectibleExtraData
        {
            public int collectibleType;
            public int scoreValue;
            public int healthValue;
            public bool respawn;
            public float respawnDelay;
        }
    }

    /// <summary>
    /// Types of collectibles
    /// </summary>
    public enum CollectibleType
    {
        Coin,
        Gem,
        Star,
        Heart,
        Key
    }

    /// <summary>
    /// Interface for score collection
    /// </summary>
    public interface IScoreCollector
    {
        void AddScore(int amount);
    }

    /// <summary>
    /// Interface for key collection
    /// </summary>
    public interface IKeyCollector
    {
        void AddKey(CollectibleType keyType);
    }

    /// <summary>
    /// Extended data class for collectible serialization
    /// </summary>
    [System.Serializable]
    public class CollectibleData : PlaceableData
    {
        public int collectibleType;
        public int scoreValue;
        public int healthValue;
    }
}
