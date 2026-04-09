# Unity Game UI System

A comprehensive UI system for Unity games featuring main menu, pause menu, and settings management with full persistence support.

## Features

- **Main Menu** - New game, continue, settings, credits, and quit functionality
- **Pause Menu** - Resume, settings, save/load, restart, main menu, and quit with confirmation dialogs
- **Settings System** - Complete settings management with:
  - Audio settings (master, music, SFX volume with mute)
  - Graphics settings (quality, resolution, fullscreen mode, V-Sync, frame rate)
  - Gameplay settings (subtitles, invert Y-axis, sensitivity)
  - Accessibility settings (high contrast, color blind modes, screen reader, large text)
- **Panel Management** - Centralized UI panel coordination with history navigation
- **Settings Persistence** - JSON-based settings save/load system
- **Singleton Pattern** - Easy access from anywhere in the codebase
- **Event System** - Decoupled communication between UI components

## Project Structure

```
Scripts/UI/
├── SettingsData.cs       # Serializable settings data structure
├── SettingsManager.cs    # Singleton settings persistence manager
├── UIManager.cs          # Central UI panel coordination
├── MainMenuUI.cs         # Main menu controller
├── PauseMenuUI.cs        # Pause menu controller
└── SettingsUI.cs         # Settings menu controller
```

## Setup Instructions

### 1. Create the Canvas

1. Create a new Canvas in your scene (GameObject > UI > Canvas)
2. Set Canvas Scaler to "Scale With Screen Size"
3. Set Reference Resolution to 1920x1080

### 2. Create UI Panels

Create the following panels as children of the Canvas:

#### Main Menu Panel
- Title Text (TextMeshPro)
- New Game Button
- Continue Button
- Settings Button
- Credits Button
- Quit Button
- Version Text
- Credits Panel (hidden by default)

#### Pause Menu Panel
- Pause Title Text
- Resume Button
- Settings Button
- Save Game Button
- Load Game Button
- Restart Button
- Main Menu Button
- Quit Button
- Save Status Text (hidden by default)
- Save Indicator (hidden by default)
- Confirmation Dialog (hidden by default)

#### Settings Panel
Create tab buttons and content panels:
- Audio Tab Button
- Graphics Tab Button
- Gameplay Tab Button
- Accessibility Tab Button
- Audio Panel with:
  - Master Volume Slider + Value Text
  - Music Volume Slider + Value Text
  - SFX Volume Slider + Value Text
  - Mute Toggle
- Graphics Panel with:
  - Quality Dropdown
  - Resolution Dropdown
  - Fullscreen Mode Dropdown
  - V-Sync Dropdown
- Gameplay Panel with:
  - Subtitles Toggle
  - Invert Y Toggle
  - Mouse Sensitivity Slider + Value Text
  - Controller Sensitivity Slider + Value Text
- Accessibility Panel with:
  - High Contrast Toggle
  - Color Blind Mode Dropdown
  - Screen Reader Toggle
  - Large Text Toggle
- Navigation Buttons:
  - Back Button
  - Reset Button
  - Apply Button

#### HUD Panel
- Create a panel for in-game HUD elements
- Health bar, score display, etc.

#### Game Over Panel
- Game Over Title
- Restart Button
- Main Menu Button

#### Victory Panel
- Victory Title
- Continue Button
- Main Menu Button

### 3. Attach Scripts

1. Create an empty GameObject named "UIManager"
2. Attach the `UIManager` script
3. Assign panel references in the Inspector

4. Attach `MainMenuUI` to the Main Menu panel
5. Assign UI element references

6. Attach `PauseMenuUI` to the Pause Menu panel
7. Assign UI element references

8. Attach `SettingsUI` to the Settings panel
9. Assign UI element references

### 4. Initial Scene Setup

```csharp
// In your scene initialization or game manager:
void Start()
{
    // Initialize settings
    var settingsManager = SettingsManager.Instance;

    // Show main menu
    UIManager.Instance.ShowMainMenu();

    // Or start directly in game:
    // UIManager.Instance.StartGame();
}
```

### 5. Usage Examples

#### Starting a New Game
```csharp
UIManager.Instance.StartGame();
```

#### Pausing the Game
```csharp
UIManager.Instance.PauseGame();
```

#### Opening Settings
```csharp
UIManager.Instance.OpenSettings();
```

#### Showing Game Over
```csharp
UIManager.Instance.ShowGameOver();
```

#### Accessing Settings
```csharp
var settings = SettingsManager.Instance.Settings;
float volume = settings.MasterVolume;
bool subtitles = settings.ShowSubtitles;
```

#### Updating Settings
```csharp
var settings = SettingsManager.Instance.Settings;
settings.MasterVolume = 0.5f;
SettingsManager.Instance.SaveSettings(settings);
```

## Controls

### Keyboard
- **Escape** - Toggle pause menu / Return from settings
- **Enter** - Confirm dialog

### Gamepad
- **Start Button** - Toggle pause menu
- **A Button** - Confirm
- **B Button** - Cancel/Back

## Panel Flow

```
Main Menu
    ├─→ Settings
    ├─→ Credits
    └─→ New Game → HUD

HUD (In-Game)
    ├─→ Pause Menu
    │   ├─→ Settings
    │   ├─→ Save/Load
    │   ├─→ Restart
    │   ├─→ Main Menu
    │   └─→ Resume Game
    ├─→ Game Over
    └─→ Victory
```

## Extending the System

### Adding New Panels

1. Add new enum value to `UIPanelType`:
```csharp
public enum UIPanelType
{
    // ... existing values
    Inventory,
    Dialog,
    // etc.
}
```

2. Register the panel in UIManager:
```csharp
[SerializeField] private GameObject _inventoryPanel;

private void InitializePanels()
{
    _panels[UIPanelType.Inventory] = _inventoryPanel;
}
```

### Adding New Settings

1. Add fields to `SettingsData.cs`:
```csharp
[Header("Custom Settings")]
public float MyCustomSetting = 1.0f;
```

2. Add UI controls in `SettingsUI.cs`
3. Update `LoadSettingsToUI()` and `ApplySettings()` methods

### Subscribing to Events

```csharp
void OnEnable()
{
    UIManager.Instance.OnPauseStateChanged += HandlePause;
    SettingsManager.Instance.OnSettingsChanged += HandleSettingsChange;
}

void OnDisable()
{
    UIManager.Instance.OnPauseStateChanged -= HandlePause;
    SettingsManager.Instance.OnSettingsChanged -= HandleSettingsChange;
}

void HandlePause(bool isPaused)
{
    // Handle pause state
}

void HandleSettingsChange(SettingsData settings)
{
    // Handle settings change
}
```

## Requirements

- Unity 2022.3 or later
- TextMeshPro (included with Unity)

## License

MIT License - Feel free to use and modify for your projects.
