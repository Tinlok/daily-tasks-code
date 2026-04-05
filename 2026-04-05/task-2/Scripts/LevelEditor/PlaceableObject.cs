using UnityEngine;

namespace LevelEditor
{
    /// <summary>
    /// Base component for all placeable objects in the level editor.
    /// Attach this to prefabs to make them placeable in the editor.
    /// </summary>
    public class PlaceableObject : MonoBehaviour
    {
        [Header("Placeable Settings")]
        [Tooltip("The type of this placeable object")]
        [SerializeField] private PlaceableType _placeableType = PlaceableType.Platform;

        [Tooltip("Unique ID for this object instance (generated at runtime)")]
        [SerializeField] private string _instanceId;

        [Tooltip("Should this object snap to the grid when placed?")]
        [SerializeField] private bool _snapToGrid = true;

        [Tooltip("Grid cell size for snapping")]
        [SerializeField] private float _gridSize = 1f;

        /// <summary>
        /// The type of this placeable object
        /// </summary>
        public PlaceableType PlaceableType => _placeableType;

        /// <summary>
        /// Unique identifier for this object instance
        /// </summary>
        public string InstanceId
        {
            get => _instanceId;
            set => _instanceId = value;
        }

        /// <summary>
        /// Whether this object should snap to grid
        /// </summary>
        public bool SnapToGrid => _snapToGrid;

        /// <summary>
        /// The grid cell size
        /// </summary>
        public float GridSize => _gridSize;

        private void Awake()
        {
            // Generate unique ID if not set
            if (string.IsNullOrEmpty(_instanceId))
            {
                _instanceId = System.Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Snap the object's position to the grid
        /// </summary>
        public Vector3 GetSnappedPosition(Vector3 position)
        {
            if (!_snapToGrid) return position;

            float x = Mathf.Round(position.x / _gridSize) * _gridSize;
            float y = Mathf.Round(position.y / _gridSize) * _gridSize;
            float z = Mathf.Round(position.z / _gridSize) * _gridSize;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Called when the object is placed in the editor
        /// </summary>
        public virtual void OnPlaced()
        {
            // Override in derived classes for custom placement behavior
        }

        /// <summary>
        /// Called when the object is removed in the editor
        /// </summary>
        public virtual void OnRemoved()
        {
            // Override in derived classes for custom removal behavior
        }

        /// <summary>
        /// Get the data for serialization
        /// </summary>
        public virtual PlaceableData GetData()
        {
            return new PlaceableData
            {
                typeId = _placeableType.ToString(),
                instanceId = _instanceId,
                position = transform.position,
                rotation = transform.rotation.eulerAngles,
                scale = transform.localScale
            };
        }

        /// <summary>
        /// Apply data to this object
        /// </summary>
        public virtual void SetData(PlaceableData data)
        {
            _placeableType = (PlaceableType)System.Enum.Parse(typeof(PlaceableType), data.typeId);
            _instanceId = data.instanceId;
            transform.position = data.position;
            transform.rotation = Quaternion.Euler(data.rotation);
            transform.localScale = data.scale;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw gizmos in the editor for better visualization
        /// </summary>
        private void OnDrawGizmos()
        {
            // Draw grid snap preview
            if (_snapToGrid)
            {
                Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
                Vector3 snapped = GetSnappedPosition(transform.position);
                Gizmos.DrawWireCube(snapped, Vector3.one * _gridSize);
            }
        }
#endif
    }

    /// <summary>
    /// Serializable data for placeable objects
    /// </summary>
    [System.Serializable]
    public class PlaceableData
    {
        public string typeId;
        public string instanceId;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        /// <summary>
        /// Custom JSON data for derived class properties
        /// </summary>
        public string customData = "";
    }
}
