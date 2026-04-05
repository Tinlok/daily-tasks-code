using UnityEngine;

namespace LevelEditor
{
    /// <summary>
    /// Component for platform objects with configurable properties
    /// </summary>
    public class Platform : PlaceableObject
    {
        [Header("Platform Settings")]
        [Tooltip("Is this a one-way platform? (player can jump through from below)")]
        [SerializeField] private bool _isOneWay = false;

        [Tooltip("Is this a moving platform?")]
        [SerializeField] private bool _isMoving = false;

        [Tooltip("Movement speed for moving platforms")]
        [SerializeField] private float _moveSpeed = 2f;

        [Tooltip("Waypoints for moving platform (local positions)")]
        [SerializeField] private Vector3[] _waypoints = new Vector3[0];

        [Tooltip("Should the platform loop through waypoints?")]
        [SerializeField] private bool _loopWaypoints = true;

        [Tooltip("Wait time at each waypoint (seconds)")]
        [SerializeField] private float _waitTime = 0.5f;

        /// <summary>Is this a one-way platform?</summary>
        public bool IsOneWay => _isOneWay;

        /// <summary>Is this a moving platform?</summary>
        public bool IsMoving => _isMoving;

        /// <summary>Movement speed</summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary>Waypoints for movement</summary>
        public Vector3[] Waypoints => _waypoints;

        /// <summary>Should waypoints loop?</summary>
        public bool LoopWaypoints => _loopWaypoints;

        /// <summary>Wait time at waypoints</summary>
        public float WaitTime => _waitTime;

        private int _currentWaypointIndex;
        private float _waitTimer;
        private Vector3 _startPosition;

        private void Start()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            if (_isMoving && _waypoints != null && _waypoints.Length > 0)
            {
                MovePlatform();
            }
        }

        private void MovePlatform()
        {
            Vector3 targetWorldPosition = _startPosition + _waypoints[_currentWaypointIndex];
            float distance = Vector3.Distance(transform.position, targetWorldPosition);

            if (distance < 0.05f)
            {
                // Reached waypoint
                if (_waitTimer < _waitTime)
                {
                    _waitTimer += Time.deltaTime;
                    return;
                }

                _waitTimer = 0;
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;

                if (!_loopWaypoints && _currentWaypointIndex == 0)
                {
                    // Reverse direction instead of looping
                    System.Array.Reverse(_waypoints);
                    _currentWaypointIndex = 1;
                }
            }
            else
            {
                // Move towards target
                Vector3 direction = (targetWorldPosition - transform.position).normalized;
                transform.position += direction * _moveSpeed * Time.deltaTime;
            }
        }

        public override PlaceableData GetData()
        {
            var data = base.GetData();
            data.customData = JsonUtility.ToJson(new PlatformExtraData
            {
                isOneWay = _isOneWay,
                isMoving = _isMoving,
                moveSpeed = _moveSpeed,
                waypoints = _waypoints,
                loopWaypoints = _loopWaypoints,
                waitTime = _waitTime
            });
            return data;
        }

        public override void SetData(PlaceableData data)
        {
            base.SetData(data);
            if (!string.IsNullOrEmpty(data.customData))
            {
                var extra = JsonUtility.FromJson<PlatformExtraData>(data.customData);
                _isOneWay = extra.isOneWay;
                _isMoving = extra.isMoving;
                _moveSpeed = extra.moveSpeed;
                _waypoints = extra.waypoints;
                _loopWaypoints = extra.loopWaypoints;
                _waitTime = extra.waitTime;
            }
        }

        /// <summary>
        /// Draw waypoints in editor
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_isMoving && _waypoints != null && _waypoints.Length > 0)
            {
                Gizmos.color = Color.cyan;
                Vector3 basePos = Application.isPlaying ? _startPosition : transform.position;

                for (int i = 0; i < _waypoints.Length; i++)
                {
                    Vector3 wp = basePos + _waypoints[i];
                    Gizmos.DrawWireSphere(wp, 0.2f);

                    if (i < _waypoints.Length - 1)
                    {
                        Vector3 nextWp = basePos + _waypoints[i + 1];
                        Gizmos.DrawLine(wp, nextWp);
                    }
                    else if (_loopWaypoints)
                    {
                        Vector3 firstWp = basePos + _waypoints[0];
                        Gizmos.DrawLine(wp, firstWp);
                    }
                }
            }
        }

        [System.Serializable]
        private class PlatformExtraData
        {
            public bool isOneWay;
            public bool isMoving;
            public float moveSpeed;
            public Vector3[] waypoints;
            public bool loopWaypoints;
            public float waitTime;
        }
    }

    /// <summary>
    /// Extended data class for platform serialization
    /// </summary>
    [System.Serializable]
    public class PlatformData : PlaceableData
    {
        public bool isOneWay;
        public bool isMoving;
        public float moveSpeed;
        public Vector3[] waypoints;
    }
}
