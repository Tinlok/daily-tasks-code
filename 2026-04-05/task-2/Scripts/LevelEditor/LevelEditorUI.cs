using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
    /// <summary>
    /// Simple UI for the level editor.
    /// Attach this to a Canvas GameObject in your editor scene.
    /// </summary>
    public class LevelEditorUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the LevelEditor component")]
        [SerializeField] private LevelEditor _levelEditor;

        [Header("UI Elements")]
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _platformButton;
        [SerializeField] private Button _trapButton;
        [SerializeField] private Button _collectibleButton;
        [SerializeField] private Button _eraserButton;

        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _clearButton;

        [SerializeField] private Text _statsText;
        [SerializeField] private Text _toolText;
        [SerializeField] private Text _instructionsText;

        [Header("Platform Type Buttons")]
        [SerializeField] private Button _platformTypeButton;
        [SerializeField] private Button _movingPlatformButton;
        [SerializeField] private Button _oneWayPlatformButton;

        [Header("Trap Type Buttons")]
        [SerializeField] private Button _spikeTrapButton;
        [SerializeField] private Button _sawTrapButton;

        [Header("Collectible Type Buttons")]
        [SerializeField] private Button _coinButton;
        [SerializeField] private Button _gemButton;
        [SerializeField] private Button _heartButton;
        [SerializeField] private Button _keyButton;

        private void Start()
        {
            if (_levelEditor == null)
            {
                _levelEditor = FindObjectOfType<LevelEditor>();
            }

            SetupButtons();
            UpdateUI();
        }

        private void Update()
        {
            if (_levelEditor != null)
            {
                UpdateToolText();
                UpdateStats();
            }
        }

        private void SetupButtons()
        {
            // Tool buttons
            if (_selectButton != null) _selectButton.onClick.AddListener(() => _levelEditor?.SetTool(EditorTool.Select));
            if (_platformButton != null) _platformButton.onClick.AddListener(() => _levelEditor?.SetTool(EditorTool.Platform));
            if (_trapButton != null) _trapButton.onClick.AddListener(() => _levelEditor?.SetTool(EditorTool.Trap));
            if (_collectibleButton != null) _collectibleButton.onClick.AddListener(() => _levelEditor?.SetTool(EditorTool.Collectible));
            if (_eraserButton != null) _eraserButton.onClick.AddListener(() => _levelEditor?.SetTool(EditorTool.Eraser));

            // Action buttons
            if (_saveButton != null) _saveButton.onClick.AddListener(() => _levelEditor?.SaveLevel());
            if (_loadButton != null) _loadButton.onClick.AddListener(() => _levelEditor?.LoadLevel());
            if (_clearButton != null) _clearButton.onClick.AddListener(() => _levelEditor?.ClearAllObjects());

            // Platform type buttons
            if (_platformTypeButton != null)
                _platformTypeButton.onClick.AddListener(() => _levelEditor?.SetPlaceableType(PlaceableType.Platform));
            if (_movingPlatformButton != null)
                _movingPlatformButton.onClick.AddListener(() => _levelEditor?.SetPlaceableType(PlaceableType.MovingPlatform));
            if (_oneWayPlatformButton != null)
                _oneWayPlatformButton.onClick.AddListener(() => _levelEditor?.SetPlaceableType(PlaceableType.OneWayPlatform));

            // Trap type buttons
            if (_spikeTrapButton != null)
                _spikeTrapButton.onClick.AddListener(() => _levelEditor?.SetPlaceableType(PlaceableType.SpikeTrap));
            if (_sawTrapButton != null)
                _sawTrapButton.onClick.AddListener(() => _levelEditor?.SetPlaceableType(PlaceableType.SawTrap));

            // Collectible type buttons
            if (_coinButton != null)
                _coinButton.onClick.AddListener(() => _levelEditor?.SetPlaceableType(PlaceableType.Coin));
            if (_gemButton != null)
                _gemButton.onClick.AddListener(() => _levelEditor?.SetPlaceableType(PlaceableType.Gem));
            if (_heartButton != null)
                _heartButton.onClick.AddListener(() => _levelEditor?.SetPlaceableType(PlaceableType.Heart));
            if (_keyButton != null)
                _keyButton.onClick.AddListener(() => _levelEditor?.SetPlaceableType(PlaceableType.Key));
        }

        private void UpdateToolText()
        {
            if (_toolText != null)
            {
                _toolText.text = $"Tool: {_levelEditor.CurrentTool}\nType: {_levelEditor.SelectedPlaceableType}";
            }
        }

        private void UpdateStats()
        {
            if (_statsText != null)
            {
                _statsText.text = _levelEditor.GetStatistics();
            }
        }

        private void UpdateUI()
        {
            if (_instructionsText != null)
            {
                _instructionsText.text = GetInstructionsText();
            }
        }

        private string GetInstructionsText()
        {
            return @"Level Editor Controls:
1-5: Select Tool
Tab: Cycle Placeable Type
Click: Place/Select
Delete: Remove Selected
Ctrl+S: Save
Ctrl+L: Load
Shift+C: Clear All";
        }
    }
}
