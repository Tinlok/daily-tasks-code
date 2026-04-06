using UnityEngine;

namespace LevelEditor
{
    /// <summary>
    /// Represents a collectible item in the game level.
    /// Provides score, health, or other benefits when collected.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        [Header("Collectible Settings")]
        [SerializeField] private CollectibleType collectibleType = CollectibleType.Coin;
        [SerializeField] private int value = 1;
        [SerializeField] private bool respawnOnDeath = false;

        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float bobAmount = 0.2f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float rotationSpeed = 90f;

        [Header("Particle Effects")]
        [SerializeField] private GameObject collectParticle;
        [SerializeField] private AudioClip collectSound;

        private Vector2 startPosition;
        private Collider2D collectibleCollider;

        public CollectibleType Type => collectibleType;
        public int Value => value;

        private void Awake()
        {
            collectibleCollider = GetComponent<Collider2D>();
            startPosition = transform.position;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            collectibleCollider.isTrigger = true;
        }

        private void Update()
        {
            Animate();
        }

        private void Animate()
        {
            // Bobbing animation
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = startPosition + Vector2.up * bobOffset;

            // Rotation animation
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Collect(other.GetComponent<PlayerCollector>());
            }
        }

        private void Collect(PlayerCollector player)
        {
            if (player == null) return;

            // Apply effect based on type
            switch (collectibleType)
            {
                case CollectibleType.Coin:
                case CollectibleType.Gem:
                    player.AddScore(value);
                    break;
                case CollectibleType.Heart:
                    player.Heal(value);
                    break;
                case CollectibleType.Star:
                    player.ActivatePowerUp();
                    break;
                case CollectibleType.Key:
                    player.AddKey();
                    break;
            }

            // Play effects
            PlayCollectEffects();

            // Destroy or respawn
            if (respawnOnDeath)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void PlayCollectEffects()
        {
            if (collectParticle != null)
            {
                Instantiate(collectParticle, transform.position, Quaternion.identity);
            }

            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }
        }

        /// <summary>
        /// Respawns the collectible (for respawnOnDeath type).
        /// </summary>
        public void Respawn()
        {
            if (respawnOnDeath)
            {
                gameObject.SetActive(true);
                startPosition = transform.position;
            }
        }

        /// <summary>
        /// Sets the collectible type.
        /// </summary>
        public void SetType(CollectibleType newType)
        {
            collectibleType = newType;
            value = GetValueForType(newType);
        }

        private int GetValueForType(CollectibleType type)
        {
            return type switch
            {
                CollectibleType.Coin => 1,
                CollectibleType.Gem => 10,
                CollectibleType.Heart => 1,
                CollectibleType.Star => 1,
                CollectibleType.Key => 1,
                _ => 1
            };
        }

        /// <summary>
        /// Gets the collectible data for serialization.
        /// </summary>
        public CollectibleData GetData()
        {
            return new CollectibleData
            {
                position = transform.position,
                type = collectibleType,
                value = value,
                respawnOnDeath = respawnOnDeath
            };
        }

        /// <summary>
        /// Loads data from a CollectibleData object.
        /// </summary>
        public void LoadData(CollectibleData data)
        {
            transform.position = data.position;
            startPosition = data.position;
            collectibleType = data.type;
            value = data.value;
            respawnOnDeath = data.respawnOnDeath;
        }
    }

    /// <summary>
    /// Component for the player that handles collecting items.
    /// </summary>
    public class PlayerCollector : MonoBehaviour
    {
        [SerializeField] private int score;
        [SerializeField] private int health;
        [SerializeField] private int keys;

        public int Score => score;
        public int Health => health;
        public int Keys => keys;

        public void AddScore(int amount)
        {
            score += amount;
            Debug.Log($"Score: {score}");
        }

        public void Heal(int amount)
        {
            health += amount;
            Debug.Log($"Health: {health}");
        }

        public void ActivatePowerUp()
        {
            Debug.Log("Power-up activated!");
        }

        public void AddKey()
        {
            keys++;
            Debug.Log($"Keys: {keys}");
        }

        public void TakeDamage(float amount)
        {
            health -= Mathf.CeilToInt(amount);
            Debug.Log($"Took {amount} damage! Health: {health}");
        }
    }
}
