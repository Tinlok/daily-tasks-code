# Unity 2D Level Editor Prototype

A simple but extensible level editor system for Unity 2D platformer games. This prototype provides a complete framework for placing platforms, traps, and collectibles with a grid-based placement system.

## Features

- **Grid-based placement system** with snap-to-grid functionality
- **Three main categories of placeable objects:**
  - **Platforms**: Standard, one-way, and moving platforms
  - **Traps**: Static and moving spike/saw traps with configurable damage
  - **Collectibles**: Coins, gems, hearts, and keys with collection logic
- **Save/Load system** with JSON serialization
- **In-editor preview** showing where objects will be placed
- **Keyboard shortcuts** for efficient editing
- **Extensible architecture** for adding new object types

## Project Structure

```
Scripts/LevelEditor/
├── EditorTool.cs           # Enum for editor tools (Select, Platform, Trap, Collectible, Eraser)
├── PlaceableType.cs        # Enum for placeable object types
├── PlaceableObject.cs      # Base class for all placeable objects
├── Platform.cs             # Platform component with movement support
├── Trap.cs                 # Trap component with damage and movement
├── Collectible.cs          # Collectible component with pickup logic
├── LevelData.cs            # Level data for save/load
├── LevelEditor.cs          # Main editor controller
└── LevelEditorUI.cs        # Simple UI for the editor
```

## Setup Instructions

### 1. Create the Scene

1. Create a new 2D scene in Unity
2. Add a **Main Camera** (Orthographic)
3. Create an empty GameObject named "LevelEditor"
4. Attach the `LevelEditor` script to it

### 2. Create Prefabs

For each object type, create a prefab with the following structure:

#### Platform Prefab
- **Sprite**: A platform sprite (e.g., a rectangle)
- **Components**:
  - `BoxCollider2D` (isTrigger = false)
  - `Platform` script
  - `PlaceableObject` script (auto-added by Platform)
- **Layer**: Ground

#### One-Way Platform Prefab
- Same as Platform, but with:
  - `Platform` script: Is One Way = true
  - `BoxCollider2D`: Used by Effector = true
  - `PlatformEffector2D`: Use One Way = true

#### Moving Platform Prefab
- Same as Platform, but with:
  - `Platform` script: Is Moving = true, configure Waypoints

#### Trap Prefabs (Spike/Saw)
- **Sprite**: Spike or saw blade sprite
- **Components**:
  - `BoxCollider2D` or `CircleCollider2D` (isTrigger = true)
  - `Trap` script
- **Settings**: Configure damage, lethal, movement options

#### Collectible Prefabs
- **Sprite**: Coin, gem, heart, or key sprite
- **Components**:
  - `CircleCollider2D` or `BoxCollider2D` (isTrigger = true)
  - `Collectible` script
  - `Collectible` script: Set Collectible Type appropriately

### 3. Configure the Level Editor

In the Inspector for the LevelEditor GameObject:

1. **Grid Settings**:
   - Grid Size: 1 (or your preferred tile size)
   - Show Grid: true
   - Grid Width/Height: Adjust for your level size

2. **Assign Prefabs**:
   - Drag your created prefabs to the corresponding slots

3. **Player Setup**:
   - Create a Player GameObject with tag "Player"
   - Add a collider and rigidbody
   - (Optional) Implement `IPlayerHealth`, `IScoreCollector`, `IKeyCollector` interfaces

### 4. Create UI (Optional)

1. Create a Canvas
2. Add buttons for tools and placeable types
3. Attach the `LevelEditorUI` script
4. Wire up button references in the Inspector

## Controls

### Keyboard Shortcuts

| Key | Action |
|-----|--------|
| 1 | Select Tool |
| 2 | Platform Tool |
| 3 | Trap Tool |
| 4 | Collectible Tool |
| 5 | Eraser Tool |
| Tab | Cycle placeable type |
| Click | Place/Select object |
| Delete/Backspace | Remove selected |
| Ctrl+S | Save level |
| Ctrl+L | Load level |
| Shift+C | Clear all objects |

### Mouse Controls

- **Left Click**: Place object (when using placement tools)
- **Left Click**: Select object (when using Select tool)
- **Left Click**: Remove object (when using Eraser tool)

## Placeable Object Types

### Platforms

| Type | Description |
|------|-------------|
| Platform | Standard solid platform |
| MovingPlatform | Platform that moves between waypoints |
| OneWayPlatform | Platform player can jump through from below |

### Traps

| Type | Description |
|------|-------------|
| SpikeTrap | Static spike trap |
| SawTrap | Moving circular blade trap |

### Collectibles

| Type | Description |
|------|-------------|
| Coin | Basic score pickup (10 points) |
| Gem | High-value pickup |
| Heart | Restores health |
| Key | Collectible key (for doors) |

## Configuration

### Platform Properties

- **Is One Way**: Player can jump up through
- **Is Moving**: Enable movement
- **Move Speed**: Movement speed
- **Waypoints**: Local positions for movement path
- **Loop Waypoints**: Should movement loop?
- **Wait Time**: Pause at each waypoint

### Trap Properties

- **Damage**: Damage dealt on contact
- **Is Lethal**: Instant kill
- **Is Active**: Initial state
- **Can Be Toggled**: Can be activated/deactivated
- **Is Moving**: Enable movement
- **Move Speed**: Movement speed
- **Move Range**: Movement distance
- **Move Axis**: Direction of movement

### Collectible Properties

- **Score Value**: Points awarded
- **Health Value**: Health restored
- **Respawn**: Should item respawn?
- **Respawn Delay**: Time before respawn
- **Should Float**: Enable bobbing animation
- **Should Rotate**: Enable spinning animation

## Save/Load System

Levels are saved as JSON files in `Application.persistentDataPath/Levels/`.

Each level file contains:
- Level metadata (name, ID, version)
- Camera bounds
- Gravity setting
- All placed objects with positions and properties

### Level Data Format

```json
{
  "levelName": "My Level",
  "levelId": "unique-id",
  "platforms": [...],
  "traps": [...],
  "collectibles": [...],
  "playerSpawnPosition": {"x": 0, "y": 0}
}
```

## Extending the Editor

### Adding New Object Types

1. Add the type to `PlaceableType` enum
2. Create a new component inheriting from `PlaceableObject`
3. Add the prefab to the LevelEditor's prefab list
4. Update `GetPrefabForType()` in LevelEditor.cs

### Adding Custom Properties

1. Add fields to your component class
2. Override `GetData()` to serialize your properties
3. Override `SetData()` to deserialize your properties

## Requirements

- Unity 2022.3 or later
- Unity 2D package

## License

MIT License - Feel free to use and modify for your projects.
