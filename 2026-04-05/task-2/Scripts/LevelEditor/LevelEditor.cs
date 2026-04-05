using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelEditor
{
    /// <summary>
    /// Main level editor component for placing and managing level objects.
    /// Attach this to a GameObject in your editor scene.
    /// </summary>
    public class LevelEditor : MonoBehaviour
    {
        [Header("Editor Settings")]
        [Tooltip("Grid cell size for snapping")]
        [SerializeField] private float _gridSize = 1f;

        [Tooltip("Show grid in scene view")]
        [SerializeField] private bool _showGrid = true;

        [Tooltip("Grid color")]
        [SerializeField] private Color _gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        [Tooltip("Grid size in cells")]
        [SerializeField] private int _gridWidth = 40;

        [Tooltip("Grid size in cells")]
        [SerializeField] private int _gridHeight = 20;

        [Header("Prefabs")]
        [Tooltip("Platform prefab (with Platform component)")]
        [SerializeField] private GameObject _platformPrefab;

        [Tooltip("One-way platform prefab")]
        [SerializeField] private GameObject _oneWayPlatformPrefab;

        [Tooltip("Moving platform prefab")]
        [SerializeField] private GameObject _movingPlatformPrefab;

        [Tooltip("Spike trap prefab")]
        [SerializeField] private GameObject _spikeTrapPrefab;

        [Tooltip("Saw trap prefab")]
        [SerializeField] private GameObject _sawTrapPrefab;

        [Tooltip("Coin prefab")]
        [SerializeField] private GameObject _coinPrefab;

        [Tooltip("Gem prefab")]
        [SerializeField] private GameObject _gemPrefab;

        [Tooltip("Heart prefab")]
        [SerializeField] private GameObject _heartPrefab;

        [Tooltip("Key prefab")]
        [SerializeField] private GameObject _keyPrefab;

        [Header("Preview Settings")]
        [Tooltip("Color for placement preview")]
        [SerializeField] private Color _previewColor = new Color(0, 1, 0, 0.5f);

        [Tooltip("Color for invalid placement")]
        [SerializeField] private Color _invalidColor = new Color(1, 0, 0, 0.5f);

        [Header("Level Data")]
        [Tooltip("Current level data (auto-saved)")]
        [SerializeField] private LevelData _currentLevel;

        // Editor state
        private EditorTool _currentTool = EditorTool.Select;
        private PlaceableType _selectedPlaceableType = PlaceableType.Platform;
        private GameObject _previewObject;
        private GameObject _selectedObject;
        private List<PlaceableObject> _placedObjects = new List<PlaceableObject>();
        private Camera _editorCamera;

        // Public properties
        public EditorTool CurrentTool => _currentTool;
        public PlaceableType SelectedPlaceableType => _selectedPlaceableType;
        public List<PlaceableObject> PlacedObjects => _placedObjects;
        public LevelData CurrentLevel => _currentLevel;

        private void Awake()
        {
            _editorCamera = Camera.main;
            if (_editorCamera == null)
            {
                Debug.LogWarning("LevelEditor: No main camera found. Please add a camera to the scene.");
            }

            InitializeLevelData();
        }

        private void Start()
        {
            // Find existing placeable objects in scene
            PlaceableObject[] existing = FindObjectsOfType<PlaceableObject>();
            foreach (var obj in existing)
            {
                if (!_placedObjects.Contains(obj))
                {
                    _placedObjects.Add(obj);
                }
            }
        }

        private void Update()
        {
            HandleInput();
            UpdatePreview();
        }

        private void OnDrawGizmos()
        {
            if (_showGrid)
            {
                DrawGrid();
            }
        }

        #region Input Handling

        private void HandleInput()
        {
            // Tool selection with number keys
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetTool(EditorTool.Select);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetTool(EditorTool.Platform);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetTool(EditorTool.Trap);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SetTool(EditorTool.Collectible);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SetTool(EditorTool.Eraser);

            // Cycle through placeable types with Tab
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CyclePlaceableType();
            }

            // Mouse click for placement/erasure
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }

            // Delete key to remove selected object
            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
            {
                DeleteSelectedObject();
            }

            // Save/Load shortcuts
            if (Input.GetKeyDown(KeyCode.S) && (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)))
            {
                SaveLevel();
            }
            if (Input.GetKeyDown(KeyCode.L) && (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)))
            {
                LoadLevel();
            }

            // Clear all objects
            if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftShift))
            {
                ClearAllObjects();
            }
        }

        private void HandleMouseClick()
        {
            Vector2 mousePosition = GetMouseWorldPosition();

            switch (_currentTool)
            {
                case EditorTool.Platform:
                case EditorTool.Trap:
                case EditorTool.Collectible:
                    PlaceObject(mousePosition);
                    break;

                case EditorTool.Select:
                    SelectObjectAt(mousePosition);
                    break;

                case EditorTool.Eraser:
                    EraseObjectAt(mousePosition);
                    break;
            }
        }

        #endregion

        #region Object Placement

        /// <summary>
        /// Place an object at the given position
        /// </summary>
        public void PlaceObject(Vector2 position)
        {
            GameObject prefab = GetPrefabForType(_selectedPlaceableType);
            if (prefab == null)
            {
                Debug.LogWarning($"LevelEditor: No prefab found for type {_selectedPlaceableType}");
                return;
            }

            // Snap to grid
            Vector3 snappedPosition = SnapToGrid(position);

            // Check if position is valid (not overlapping)
            if (IsPositionOccupied(snappedPosition))
            {
                Debug.LogWarning("LevelEditor: Position already occupied");
                return;
            }

            // Instantiate object
            GameObject newObj = Instantiate(prefab, snappedPosition, Quaternion.identity);
            newObj.name = $"{_selectedPlaceableType}_{_placedObjects.Count}";

            PlaceableObject placeable = newObj.GetComponent<PlaceableObject>();
            if (placeable != null)
            {
                placeable.InstanceId = Guid.NewGuid().ToString();
                placeable.OnPlaced();
                _placedObjects.Add(placeable);
            }
            else
            {
                Debug.LogWarning($"LevelEditor: Placed object has no PlaceableObject component");
            }

            // Play placement sound effect (optional)
            // AudioManager.Instance.PlaySound("place");
        }

        /// <summary>
        /// Erase object at the given position
        /// </summary>
        public void EraseObjectAt(Vector2 position)
        {
            PlaceableObject obj = GetObjectAt(position);
            if (obj != null)
            {
                RemoveObject(obj);
            }
        }

        /// <summary>
        /// Remove a placeable object
        /// </summary>
        public void RemoveObject(PlaceableObject obj)
        {
            if (obj == null) return;

            obj.OnRemoved();
            _placedObjects.Remove(obj);

            if (_selectedObject == obj.gameObject)
            {
                _selectedObject = null;
            }

            Destroy(obj.gameObject);
        }

        /// <summary>
        /// Select object at the given position
        /// </summary>
        public void SelectObjectAt(Vector2 position)
        {
            PlaceableObject obj = GetObjectAt(position);

            if (_selectedObject != null)
            {
                // Deselect previous
                SetObjectSelected(_selectedObject, false);
            }

            if (obj != null)
            {
                _selectedObject = obj.gameObject;
                SetObjectSelected(_selectedObject, true);
            }
            else
            {
                _selectedObject = null;
            }
        }

        /// <summary>
        /// Delete the currently selected object
        /// </summary>
        public void DeleteSelectedObject()
        {
            if (_selectedObject != null)
            {
                PlaceableObject placeable = _selectedObject.GetComponent<PlaceableObject>();
                if (placeable != null)
                {
                    RemoveObject(placeable);
                }
            }
        }

        /// <summary>
        /// Clear all placed objects
        /// </summary>
        public void ClearAllObjects()
        {
            // Create a copy to avoid modification during iteration
            var objects = new List<PlaceableObject>(_placedObjects);
            foreach (var obj in objects)
            {
                RemoveObject(obj);
            }
            _placedObjects.Clear();
        }

        #endregion

        #region Helper Methods

        private GameObject GetPrefabForType(PlaceableType type)
        {
            return type switch
            {
                PlaceableType.Platform => _platformPrefab,
                PlaceableType.MovingPlatform => _movingPlatformPrefab,
                PlaceableType.OneWayPlatform => _oneWayPlatformPrefab,
                PlaceableType.SpikeTrap => _spikeTrapPrefab,
                PlaceableType.SawTrap => _sawTrapPrefab,
                PlaceableType.Coin => _coinPrefab,
                PlaceableType.Gem => _gemPrefab,
                PlaceableType.Heart => _heartPrefab,
                PlaceableType.Key => _keyPrefab,
                _ => null
            };
        }

        private PlaceableObject GetObjectAt(Vector2 position)
        {
            float checkRadius = _gridSize * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, checkRadius);

            foreach (var hit in hits)
            {
                PlaceableObject obj = hit.GetComponent<PlaceableObject>();
                if (obj != null && _placedObjects.Contains(obj))
                {
                    return obj;
                }
            }

            return null;
        }

        private bool IsPositionOccupied(Vector3 position)
        {
            float checkRadius = _gridSize * 0.4f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, checkRadius);

            foreach (var hit in hits)
            {
                PlaceableObject obj = hit.GetComponent<PlaceableObject>();
                if (obj != null && _placedObjects.Contains(obj))
                {
                    return true;
                }
            }

            return false;
        }

        private Vector3 SnapToGrid(Vector2 position)
        {
            float x = Mathf.Round(position.x / _gridSize) * _gridSize;
            float y = Mathf.Round(position.y / _gridSize) * _gridSize;
            return new Vector3(x, y, 0);
        }

        private Vector2 GetMouseWorldPosition()
        {
            if (_editorCamera == null) return Vector2.zero;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -_editorCamera.transform.position.z;
            return _editorCamera.ScreenToWorldPoint(mousePos);
        }

        private void SetObjectSelected(GameObject obj, bool selected)
        {
            // Visual feedback for selection
            if (obj.TryGetComponent<SpriteRenderer>(out var renderer))
            {
                renderer.color = selected ? Color.yellow : Color.white;
            }
        }

        private void CyclePlaceableType()
        {
            Array types = Enum.GetValues(typeof(PlaceableType));
            int currentIndex = Array.IndexOf(types, _selectedPlaceableType);
            int nextIndex = (currentIndex + 1) % types.Length;
            _selectedPlaceableType = (PlaceableType)types.GetValue(nextIndex);
        }

        #endregion

        #region Preview

        private void UpdatePreview()
        {
            // Destroy old preview
            if (_previewObject != null)
            {
                Destroy(_previewObject);
            }

            // Only show preview for placement tools
            if (_currentTool == EditorTool.Select || _currentTool == EditorTool.Eraser)
            {
                return;
            }

            GameObject prefab = GetPrefabForType(_selectedPlaceableType);
            if (prefab == null) return;

            Vector2 mousePos = GetMouseWorldPosition();
            Vector3 snappedPos = SnapToGrid(mousePos);

            // Create preview
            _previewObject = Instantiate(prefab, snappedPos, Quaternion.identity);

            // Remove components that shouldn't be active in preview
            if (_previewObject.GetComponent<Collider2D>() != null)
            {
                Destroy(_previewObject.GetComponent<Collider2D>());
            }

            // Set preview color
            if (_previewObject.TryGetComponent<SpriteRenderer>(out var renderer))
            {
                renderer.color = IsPositionOccupied(snappedPos) ? _invalidColor : _previewColor;
            }

            // Destroy preview after frame (it will be recreated next frame)
            Destroy(_previewObject, 0f);
        }

        #endregion

        #region Grid Rendering

        private void DrawGrid()
        {
            Vector3 center = Vector3.zero;
            float halfWidth = (_gridWidth * _gridSize) * 0.5f;
            float halfHeight = (_gridHeight * _gridSize) * 0.5f;

            // Vertical lines
            for (int x = 0; x <= _gridWidth; x++)
            {
                float xPos = center.x - halfWidth + (x * _gridSize);
                Vector3 start = new Vector3(xPos, center.y - halfHeight, 0);
                Vector3 end = new Vector3(xPos, center.y + halfHeight, 0);
                Gizmos.DrawLine(start, end);
            }

            // Horizontal lines
            for (int y = 0; y <= _gridHeight; y++)
            {
                float yPos = center.y - halfHeight + (y * _gridSize);
                Vector3 start = new Vector3(center.x - halfWidth, yPos, 0);
                Vector3 end = new Vector3(center.x + halfWidth, yPos, 0);
                Gizmos.DrawLine(start, end);
            }
        }

        #endregion

        #region Save/Load

        private void InitializeLevelData()
        {
            if (_currentLevel == null)
            {
                _currentLevel = new LevelData();
                _currentLevel.GenerateId();
            }
        }

        /// <summary>
        /// Save current level to JSON
        /// </summary>
        public void SaveLevel()
        {
            if (_currentLevel == null)
            {
                InitializeLevelData();
            }

            // Update level data from placed objects
            _currentLevel.platforms.Clear();
            _currentLevel.traps.Clear();
            _currentLevel.collectibles.Clear();

            foreach (var obj in _placedObjects)
            {
                PlaceableData data = obj.GetData();

                switch (obj.PlaceableType)
                {
                    case PlaceableType.Platform:
                    case PlaceableType.MovingPlatform:
                    case PlaceableType.OneWayPlatform:
                        _currentLevel.platforms.Add(data);
                        break;
                    case PlaceableType.SpikeTrap:
                    case PlaceableType.SawTrap:
                        _currentLevel.traps.Add(data);
                        break;
                    case PlaceableType.Coin:
                    case PlaceableType.Gem:
                    case PlaceableType.Heart:
                    case PlaceableType.Key:
                        _currentLevel.collectibles.Add(data);
                        break;
                }
            }

            // Serialize to JSON
            string json = JsonUtility.ToJson(_currentLevel, true);
            string fileName = $"{_currentLevel.levelName}_{_currentLevel.levelId}.json";
            string path = Path.Combine(Application.persistentDataPath, "Levels", fileName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Write to file
            File.WriteAllText(path, json);

            Debug.Log($"Level saved to: {path}");
        }

        /// <summary>
        /// Load level from JSON
        /// </summary>
        public void LoadLevel(string path = null)
        {
            if (path == null)
            {
                // Find most recent level file
                string levelsDir = Path.Combine(Application.persistentDataPath, "Levels");
                if (!Directory.Exists(levelsDir))
                {
                    Debug.LogWarning("LevelEditor: No saved levels found");
                    return;
                }

                string[] files = Directory.GetFiles(levelsDir, "*.json");
                if (files.Length == 0)
                {
                    Debug.LogWarning("LevelEditor: No saved levels found");
                    return;
                }

                path = files[0]; // Load first found file
            }

            string json = File.ReadAllText(path);
            _currentLevel = JsonUtility.FromJson<LevelData>(json);

            // Clear existing objects
            ClearAllObjects();

            // Instantiate objects from data
            LoadObjectsFromData(_currentLevel.platforms);
            LoadObjectsFromData(_currentLevel.traps);
            LoadObjectsFromData(_currentLevel.collectibles);

            Debug.Log($"Level loaded from: {path}");
        }

        private void LoadObjectsFromData(List<PlaceableData> dataList)
        {
            foreach (var data in dataList)
            {
                PlaceableType type = (PlaceableType)Enum.Parse(typeof(PlaceableType), data.typeId);
                GameObject prefab = GetPrefabForType(type);

                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, data.position, Quaternion.Euler(data.rotation));
                    obj.transform.localScale = data.scale;

                    PlaceableObject placeable = obj.GetComponent<PlaceableObject>();
                    if (placeable != null)
                    {
                        placeable.SetData(data);
                        _placedObjects.Add(placeable);
                    }
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Set the current editor tool
        /// </summary>
        public void SetTool(EditorTool tool)
        {
            _currentTool = tool;
            Debug.Log($"Editor tool: {tool}");
        }

        /// <summary>
        /// Set the selected placeable type
        /// </summary>
        public void SetPlaceableType(PlaceableType type)
        {
            _selectedPlaceableType = type;

            // Auto-switch to appropriate tool
            if (type == PlaceableType.Platform || type == PlaceableType.MovingPlatform || type == PlaceableType.OneWayPlatform)
            {
                SetTool(EditorTool.Platform);
            }
            else if (type == PlaceableType.SpikeTrap || type == PlaceableType.SawTrap)
            {
                SetTool(EditorTool.Trap);
            }
            else
            {
                SetTool(EditorTool.Collectible);
            }
        }

        /// <summary>
        /// Get statistics about placed objects
        /// </summary>
        public string GetStatistics()
        {
            int platforms = 0, traps = 0, collectibles = 0;

            foreach (var obj in _placedObjects)
            {
                switch (obj.PlaceableType)
                {
                    case PlaceableType.Platform:
                    case PlaceableType.MovingPlatform:
                    case PlaceableType.OneWayPlatform:
                        platforms++;
                        break;
                    case PlaceableType.SpikeTrap:
                    case PlaceableType.SawTrap:
                        traps++;
                        break;
                    default:
                        collectibles++;
                        break;
                }
            }

            return $"Platforms: {platforms} | Traps: {traps} | Collectibles: {collectibles}";
        }

        #endregion
    }
}
