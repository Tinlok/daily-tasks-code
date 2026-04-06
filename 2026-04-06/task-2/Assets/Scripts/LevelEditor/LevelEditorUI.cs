using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
    /// <summary>
    /// UI controller for the level editor.
    /// Provides buttons for tool selection, object type selection, and level operations.
    /// </summary>
    public class LevelEditorUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelEditor levelEditor;

        [Header("Tool Buttons")]
        [SerializeField] private Button selectButton;
        [SerializeField] private Button platformButton;
        [SerializeField] private Button trapButton;
        [SerializeField] private Button collectibleButton;
        [SerializeField] private Button eraseButton;

        [Header("Platform Type Buttons")]
        [SerializeField] private Button normalPlatformButton;
        [SerializeField] private Button icePlatformButton;
        [SerializeField] private Button bouncyPlatformButton;
        [SerializeField] private Button movingPlatformButton;
        [SerializeField] private Button breakablePlatformButton;

        [Header("Trap Type Buttons")]
        [SerializeField] private Button spikeTrapButton;
        [SerializeField] private Button sawbladeTrapButton;
        [SerializeField] private Button laserTrapButton;
        [SerializeField] private Button fireTrapButton;
        [SerializeField] private Button crusherTrapButton;

        [Header("Collectible Type Buttons")]
        [SerializeField] private Button coinButton;
        [SerializeField] private Button gemButton;
        [SerializeField] private Button heartButton;
        [SerializeField] private Button starButton;
        [SerializeField] private Button keyButton;

        [Header("Level Action Buttons")]
        [SerializeField] private Button newLevelButton;
        [SerializeField] private Button saveLevelButton;
        [SerializeField] private Button loadLevelButton;
        [SerializeField] private Button clearLevelButton;
        [SerializeField] private Button playLevelButton;

        [Header("Info Panel")]
        [SerializeField] private Text infoText;
        [SerializeField] private Text objectCountText;

        [Header("Colors")]
        [SerializeField] private Color selectedColor = Color.green;
        [SerializeField] private Color normalColor = Color.white;

        private Button currentSelectedButton;

        private void Start()
        {
            if (levelEditor == null)
            {
                levelEditor = FindObjectOfType<LevelEditor>();
            }

            SetupButtons();
            UpdateInfo("Welcome to Level Editor! Select a tool to begin.");
            SelectButton(selectButton);
        }

        private void OnEnable()
        {
            if (levelEditor != null)
            {
                levelEditor.OnToolChanged += HandleToolChanged;
                levelEditor.OnObjectSelected += HandleObjectSelected;
                levelEditor.OnLevelModified += HandleLevelModified;
            }
        }

        private void OnDisable()
        {
            if (levelEditor != null)
            {
                levelEditor.OnToolChanged -= HandleToolChanged;
                levelEditor.OnObjectSelected -= HandleObjectSelected;
                levelEditor.OnLevelModified -= HandleLevelModified;
            }
        }

        private void SetupButtons()
        {
            // Tool buttons
            selectButton?.onClick.AddListener(() => SelectTool(EditorTool.Select, selectButton));
            platformButton?.onClick.AddListener(() => SelectTool(EditorTool.Platform, platformButton));
            trapButton?.onClick.AddListener(() => SelectTool(EditorTool.Trap, trapButton));
            collectibleButton?.onClick.AddListener(() => SelectTool(EditorTool.Collectible, collectibleButton));
            eraseButton?.onClick.AddListener(() => SelectTool(EditorTool.Erase, eraseButton));

            // Platform type buttons
            normalPlatformButton?.onClick.AddListener(() => SelectPlatformType(PlatformType.Normal, normalPlatformButton));
            icePlatformButton?.onClick.AddListener(() => SelectPlatformType(PlatformType.Ice, icePlatformButton));
            bouncyPlatformButton?.onClick.AddListener(() => SelectPlatformType(PlatformType.Bouncy, bouncyPlatformButton));
            movingPlatformButton?.onClick.AddListener(() => SelectPlatformType(PlatformType.Moving, movingPlatformButton));
            breakablePlatformButton?.onClick.AddListener(() => SelectPlatformType(PlatformType.Breakable, breakablePlatformButton));

            // Trap type buttons
            spikeTrapButton?.onClick.AddListener(() => SelectTrapType(TrapType.Spike, spikeTrapButton));
            sawbladeTrapButton?.onClick.AddListener(() => SelectTrapType(TrapType.Sawblade, sawbladeTrapButton));
            laserTrapButton?.onClick.AddListener(() => SelectTrapType(TrapType.Laser, laserTrapButton));
            fireTrapButton?.onClick.AddListener(() => SelectTrapType(TrapType.Fire, fireTrapButton));
            crusherTrapButton?.onClick.AddListener(() => SelectTrapType(TrapType.Crusher, crusherTrapButton));

            // Collectible type buttons
            coinButton?.onClick.AddListener(() => SelectCollectibleType(CollectibleType.Coin, coinButton));
            gemButton?.onClick.AddListener(() => SelectCollectibleType(CollectibleType.Gem, gemButton));
            heartButton?.onClick.AddListener(() => SelectCollectibleType(CollectibleType.Heart, heartButton));
            starButton?.onClick.AddListener(() => SelectCollectibleType(CollectibleType.Star, starButton));
            keyButton?.onClick.AddListener(() => SelectCollectibleType(CollectibleType.Key, keyButton));

            // Level action buttons
            newLevelButton?.onClick.AddListener(NewLevel);
            saveLevelButton?.onClick.AddListener(SaveLevel);
            loadLevelButton?.onClick.AddListener(LoadLevel);
            clearLevelButton?.onClick.AddListener(ClearLevel);
            playLevelButton?.onClick.AddListener(PlayLevel);
        }

        private void SelectTool(EditorTool tool, Button button)
        {
            levelEditor?.SetTool(tool);
            SelectButton(button);
            UpdateInfo($"Tool: {tool}");
        }

        private void SelectPlatformType(PlatformType type, Button button)
        {
            levelEditor?.SetPlatformType(type);
            SelectButton(button);
            UpdateInfo($"Platform: {type}");
        }

        private void SelectTrapType(TrapType type, Button button)
        {
            levelEditor?.SetTrapType(type);
            SelectButton(button);
            UpdateInfo($"Trap: {type}");
        }

        private void SelectCollectibleType(CollectibleType type, Button button)
        {
            levelEditor?.SetCollectibleType(type);
            SelectButton(button);
            UpdateInfo($"Collectible: {type}");
        }

        private void SelectButton(Button button)
        {
            if (currentSelectedButton != null)
            {
                var colors = currentSelectedButton.colors;
                colors.normalColor = normalColor;
                currentSelectedButton.colors = colors;
            }

            currentSelectedButton = button;

            if (button != null)
            {
                var colors = button.colors;
                colors.normalColor = selectedColor;
                button.colors = colors;
            }
        }

        private void NewLevel()
        {
            levelEditor?.ClearLevel();
            UpdateInfo("New level created.");
        }

        private void SaveLevel()
        {
            if (levelEditor == null) return;

            string json = levelEditor.ExportLevel();
            string path = UnityEngine.Application.dataPath + "/Levels/level_" + System.DateTime.Now.Ticks + ".json";

            System.IO.Directory.CreateDirectory(UnityEngine.Application.dataPath + "/Levels");
            System.IO.File.WriteAllText(path, json);

            UpdateInfo($"Level saved to: {path}");
            Debug.Log($"Level saved: {path}");
        }

        private void LoadLevel()
        {
            // In a full implementation, this would open a file browser
            // For now, we'll log the action
            UpdateInfo("Load level: Open file browser to select a level file.");
            Debug.Log("Load level triggered - implement file browser for full functionality.");
        }

        private void ClearLevel()
        {
            levelEditor?.ClearLevel();
            UpdateInfo("Level cleared.");
        }

        private void PlayLevel()
        {
            UpdateInfo("Playing level... (implement scene loading for full functionality)");
            Debug.Log("Play level triggered - implement scene loading for full functionality.");
        }

        private void HandleToolChanged(EditorTool tool)
        {
            UpdateInfo($"Tool: {tool}");

            Button toolButton = tool switch
            {
                EditorTool.Select => selectButton,
                EditorTool.Platform => platformButton,
                EditorTool.Trap => trapButton,
                EditorTool.Collectible => collectibleButton,
                EditorTool.Erase => eraseButton,
                _ => null
            };

            SelectButton(toolButton);
        }

        private void HandleObjectSelected(GameObject obj)
        {
            if (obj != null)
            {
                UpdateInfo($"Selected: {obj.name}");
            }
            else
            {
                UpdateInfo("Selection cleared.");
            }
        }

        private void HandleLevelModified()
        {
            UpdateObjectCount();
        }

        private void UpdateInfo(string message)
        {
            if (infoText != null)
            {
                infoText.text = message;
            }
        }

        private void UpdateObjectCount()
        {
            if (objectCountText != null && levelEditor != null)
            {
                objectCountText.text = $"Platforms: {levelEditor.Platforms.Count} | " +
                                      $"Traps: {levelEditor.Traps.Count} | " +
                                      $"Collectibles: {levelEditor.Collectibles.Count}";
            }
        }
    }
}
