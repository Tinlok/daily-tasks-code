using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor
{
    /// <summary>
    /// Core level editor class that manages the creation, placement, and management
    /// of level objects (platforms, traps, collectibles).
    /// </summary>
    public class LevelEditor : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private int gridWidth = 20;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private bool showGrid = true;
        [SerializeField] private Color gridColor = new(0.5f, 0.5f, 0.5f, 0.5f);

        [Header("Prefabs")]
        [SerializeField] private GameObject platformPrefab;
        [SerializeField] private GameObject trapPrefab;
        [SerializeField] private GameObject collectiblePrefab;

        [Header("Editor State")]
        [SerializeField] private EditorTool currentTool = EditorTool.Select;
        [SerializeField] private PlatformType currentPlatformType = PlatformType.Normal;
        [SerializeField] private TrapType currentTrapType = TrapType.Spike;
        [SerializeField] private CollectibleType currentCollectibleType = CollectibleType.Coin;

        [Header("Level Data")]
        [SerializeField] private LevelData currentLevel;

        // Runtime collections
        private readonly List<Platform> platforms = new();
        private readonly List<Trap> traps = new();
        private readonly List<Collectible> collectibles = new();
        private GameObject selectedObject;

        // Events
        public event Action<EditorTool> OnToolChanged;
        public event Action<GameObject> OnObjectSelected;
        public event Action OnLevelModified;

        public EditorTool CurrentTool => currentTool;
        public IReadOnlyList<Platform> Platforms => platforms;
        public IReadOnlyList<Trap> Traps => traps;
        public IReadOnlyList<Collectible> Collectibles => collectibles;
        public GameObject SelectedObject => selectedObject;

        private void Awake()
        {
            if (currentLevel == null)
            {
                currentLevel = new LevelData
                {
                    cellSize = cellSize,
                    gridWidth = gridWidth,
                    gridHeight = gridHeight
                };
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void OnDrawGizmos()
        {
            if (!showGrid) return;

            Gizmos.color = gridColor;
            Vector3 origin = transform.position;

            // Draw vertical lines
            for (int x = 0; x <= gridWidth; x++)
            {
                Vector3 start = origin + new Vector3(x * cellSize, 0, 0);
                Vector3 end = origin + new Vector3(x * cellSize, gridHeight * cellSize, 0);
                Gizmos.DrawLine(start, end);
            }

            // Draw horizontal lines
            for (int y = 0; y <= gridHeight; y++)
            {
                Vector3 start = origin + new Vector3(0, y * cellSize, 0);
                Vector3 end = origin + new Vector3(gridWidth * cellSize, y * cellSize, 0);
                Gizmos.DrawLine(start, end);
            }
        }

        private void HandleInput()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 gridPos = SnapToGrid(mousePos);

            if (Input.GetMouseButtonDown(0))
            {
                HandleLeftClick(gridPos);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                HandleRightClick(gridPos);
            }
            else if (Input.GetKeyDown(KeyCode.Delete))
            {
                DeleteSelected();
            }
        }

        private void HandleLeftClick(Vector2 gridPos)
        {
            switch (currentTool)
            {
                case EditorTool.Platform:
                    PlacePlatform(gridPos);
                    break;
                case EditorTool.Trap:
                    PlaceTrap(gridPos);
                    break;
                case EditorTool.Collectible:
                    PlaceCollectible(gridPos);
                    break;
                case EditorTool.Select:
                    SelectObjectAt(gridPos);
                    break;
                case EditorTool.Erase:
                    EraseObjectAt(gridPos);
                    break;
            }
        }

        private void HandleRightClick(Vector2 gridPos)
        {
            EraseObjectAt(gridPos);
        }

        private Vector2 SnapToGrid(Vector2 position)
        {
            return new Vector2(
                Mathf.Round(position.x / cellSize) * cellSize,
                Mathf.Round(position.y / cellSize) * cellSize
            );
        }

        private void PlacePlatform(Vector2 gridPos)
        {
            // Check if platform already exists at this position
            if (FindPlatformAt(gridPos) != null) return;

            GameObject obj = Instantiate(platformPrefab, gridPos, Quaternion.identity, transform);
            Platform platform = obj.GetComponent<Platform>();
            if (platform != null)
            {
                platform.SetType(currentPlatformType);
                platforms.Add(platform);
                OnLevelModified?.Invoke();
            }
        }

        private void PlaceTrap(Vector2 gridPos)
        {
            if (FindTrapAt(gridPos) != null) return;

            GameObject obj = Instantiate(trapPrefab, gridPos, Quaternion.identity, transform);
            Trap trap = obj.GetComponent<Trap>();
            if (trap != null)
            {
                trap.SetType(currentTrapType);
                traps.Add(trap);
                OnLevelModified?.Invoke();
            }
        }

        private void PlaceCollectible(Vector2 gridPos)
        {
            if (FindCollectibleAt(gridPos) != null) return;

            GameObject obj = Instantiate(collectiblePrefab, gridPos, Quaternion.identity, transform);
            Collectible collectible = obj.GetComponent<Collectible>();
            if (collectible != null)
            {
                collectible.SetType(currentCollectibleType);
                collectibles.Add(collectible);
                OnLevelModified?.Invoke();
            }
        }

        private Platform FindPlatformAt(Vector2 position)
        {
            foreach (var platform in platforms)
            {
                if (Vector2.Distance(platform.transform.position, position) < cellSize * 0.5f)
                {
                    return platform;
                }
            }
            return null;
        }

        private Trap FindTrapAt(Vector2 position)
        {
            foreach (var trap in traps)
            {
                if (Vector2.Distance(trap.transform.position, position) < cellSize * 0.5f)
                {
                    return trap;
                }
            }
            return null;
        }

        private Collectible FindCollectibleAt(Vector2 position)
        {
            foreach (var collectible in collectibles)
            {
                if (Vector2.Distance(collectible.transform.position, position) < cellSize * 0.5f)
                {
                    return collectible;
                }
            }
            return null;
        }

        private void SelectObjectAt(Vector2 position)
        {
            // Check in reverse order (top-most first)
            for (int i = collectibles.Count - 1; i >= 0; i--)
            {
                if (Vector2.Distance(collectibles[i].transform.position, position) < cellSize * 0.5f)
                {
                    SelectObject(collectibles[i].gameObject);
                    return;
                }
            }

            for (int i = traps.Count - 1; i >= 0; i--)
            {
                if (Vector2.Distance(traps[i].transform.position, position) < cellSize * 0.5f)
                {
                    SelectObject(traps[i].gameObject);
                    return;
                }
            }

            for (int i = platforms.Count - 1; i >= 0; i--)
            {
                if (Vector2.Distance(platforms[i].transform.position, position) < cellSize * 0.5f)
                {
                    SelectObject(platforms[i].gameObject);
                    return;
                }
            }

            DeselectObject();
        }

        private void SelectObject(GameObject obj)
        {
            selectedObject = obj;
            OnObjectSelected?.Invoke(obj);
        }

        private void DeselectObject()
        {
            selectedObject = null;
            OnObjectSelected?.Invoke(null);
        }

        private void EraseObjectAt(Vector2 position)
        {
            Platform platform = FindPlatformAt(position);
            if (platform != null)
            {
                platforms.Remove(platform);
                DestroyImmediate(platform.gameObject);
                OnLevelModified?.Invoke();
                return;
            }

            Trap trap = FindTrapAt(position);
            if (trap != null)
            {
                traps.Remove(trap);
                DestroyImmediate(trap.gameObject);
                OnLevelModified?.Invoke();
                return;
            }

            Collectible collectible = FindCollectibleAt(position);
            if (collectible != null)
            {
                collectibles.Remove(collectible);
                DestroyImmediate(collectible.gameObject);
                OnLevelModified?.Invoke();
            }
        }

        private void DeleteSelected()
        {
            if (selectedObject == null) return;

            Platform platform = selectedObject.GetComponent<Platform>();
            if (platform != null)
            {
                platforms.Remove(platform);
            }

            Trap trap = selectedObject.GetComponent<Trap>();
            if (trap != null)
            {
                traps.Remove(trap);
            }

            Collectible collectible = selectedObject.GetComponent<Collectible>();
            if (collectible != null)
            {
                collectibles.Remove(collectible);
            }

            DestroyImmediate(selectedObject);
            selectedObject = null;
            OnLevelModified?.Invoke();
        }

        /// <summary>
        /// Sets the current editor tool.
        /// </summary>
        public void SetTool(EditorTool tool)
        {
            currentTool = tool;
            OnToolChanged?.Invoke(tool);
        }

        /// <summary>
        /// Sets the platform type for placement.
        /// </summary>
        public void SetPlatformType(PlatformType type)
        {
            currentPlatformType = type;
            SetTool(EditorTool.Platform);
        }

        /// <summary>
        /// Sets the trap type for placement.
        /// </summary>
        public void SetTrapType(TrapType type)
        {
            currentTrapType = type;
            SetTool(EditorTool.Trap);
        }

        /// <summary>
        /// Sets the collectible type for placement.
        /// </summary>
        public void SetCollectibleType(CollectibleType type)
        {
            currentCollectibleType = type;
            SetTool(EditorTool.Collectible);
        }

        /// <summary>
        /// Clears all objects from the level.
        /// </summary>
        public void ClearLevel()
        {
            foreach (var platform in platforms)
            {
                if (platform != null) DestroyImmediate(platform.gameObject);
            }
            platforms.Clear();

            foreach (var trap in traps)
            {
                if (trap != null) DestroyImmediate(trap.gameObject);
            }
            traps.Clear();

            foreach (var collectible in collectibles)
            {
                if (collectible != null) DestroyImmediate(collectible.gameObject);
            }
            collectibles.Clear();

            selectedObject = null;
            OnLevelModified?.Invoke();
        }

        /// <summary>
        /// Exports the current level to JSON.
        /// </summary>
        public string ExportLevel()
        {
            currentLevel.platforms.Clear();
            currentLevel.traps.Clear();
            currentLevel.collectibles.Clear();

            foreach (var platform in platforms)
            {
                currentLevel.platforms.Add(platform.GetData());
            }

            foreach (var trap in traps)
            {
                currentLevel.traps.Add(trap.GetData());
            }

            foreach (var collectible in collectibles)
            {
                currentLevel.collectibles.Add(collectible.GetData());
            }

            return currentLevel.ToJson();
        }

        /// <summary>
        /// Imports a level from JSON.
        /// </summary>
        public void ImportLevel(string json)
        {
            ClearLevel();
            currentLevel = LevelData.FromJson(json);

            LoadLevel(currentLevel);
        }

        /// <summary>
        /// Loads a level from LevelData.
        /// </summary>
        public void LoadLevel(LevelData levelData)
        {
            ClearLevel();
            currentLevel = levelData;

            foreach (var platformData in levelData.platforms)
            {
                GameObject obj = Instantiate(platformPrefab, transform);
                Platform platform = obj.GetComponent<Platform>();
                platform?.LoadData(platformData);
                platforms.Add(platform);
            }

            foreach (var trapData in levelData.traps)
            {
                GameObject obj = Instantiate(trapPrefab, transform);
                Trap trap = obj.GetComponent<Trap>();
                trap?.LoadData(trapData);
                traps.Add(trap);
            }

            foreach (var collectibleData in levelData.collectibles)
            {
                GameObject obj = Instantiate(collectiblePrefab, transform);
                Collectible collectible = obj.GetComponent<Collectible>();
                collectible?.LoadData(collectibleData);
                collectibles.Add(collectible);
            }

            OnLevelModified?.Invoke();
        }
    }

    /// <summary>
    /// Available tools in the level editor.
    /// </summary>
    public enum EditorTool
    {
        Select,
        Platform,
        Trap,
        Collectible,
        Erase
    }
}
