# Unity Level Editor Prototype

A simple 2D level editor system for Unity that allows placing platforms, traps, and collectibles through a grid-based interface.

## Features

- **Grid-based placement system** - Snap objects to a customizable grid
- **Multiple object types**:
  - Platforms (Normal, Ice, Bouncy, Moving, Breakable)
  - Traps (Spike, Sawblade, Laser, Fire, Crusher)
  - Collectibles (Coin, Gem, Heart, Star, Key)
- **Save/Load levels** - Export level data to JSON
- **UI Interface** - Complete editor UI for tool selection
- **Runtime preview** - See trap animations and collectible effects

## Project Structure

```
Assets/Scripts/LevelEditor/
├── LevelData.cs       # Serializable data structures
├── Platform.cs        # Platform behavior and types
├── Trap.cs           # Trap behavior and types
├── Collectible.cs    # Collectible behavior and types
├── LevelEditor.cs    # Core editor logic
└── LevelEditorUI.cs  # UI controller
```

## Setup Instructions

### 1. Create Prefabs

Create three prefabs with the following components:

**Platform Prefab:**
- Add `Platform.cs` component
- Add `BoxCollider2D`
- Add `SpriteRenderer`

**Trap Prefab:**
- Add `Trap.cs` component
- Add `BoxCollider2D` (set Is Trigger to true)
- Add `SpriteRenderer`

**Collectible Prefab:**
- Add `Collectible.cs` component
- Add `BoxCollider2D` (set Is Trigger to true)
- Add `SpriteRenderer`

### 2. Create Level Editor GameObject

1. Create an empty GameObject named "LevelEditor"
2. Add the `LevelEditor.cs` component
3. Assign the prefabs to the respective fields
4. Configure grid settings (cell size, dimensions)

### 3. Create UI

1. Create a Canvas
2. Add buttons for each tool and object type
3. Create an empty GameObject and add `LevelEditorUI.cs`
4. Assign all button references

### 4. Tag Setup

- Create a "Player" tag for player collision detection

## Usage

### Editor Mode

1. Select a tool from the UI:
   - **Select** - Click to select objects, Delete to remove
   - **Platform** - Click to place platforms
   - **Trap** - Click to place traps
   - **Collectible** - Click to place collectibles
   - **Erase** - Click to remove any object

2. Choose the specific type of object to place

3. Left-click to place, right-click to erase

4. Use Save/Load buttons to persist levels

### Keyboard Shortcuts

- `Delete` - Remove selected object
- Right-click - Quick erase

## Platform Types

| Type | Behavior |
|------|----------|
| Normal | Standard platform with default friction |
| Ice | Low friction, slippery surface |
| Bouncy | Bounces player on contact |
| Moving | Moves back and forth along defined path |
| Breakable | Breaks after being touched X times |

## Trap Types

| Type | Behavior |
|------|----------|
| Spike | Static damage on contact |
| Sawblade | Rotating blade, continuous damage |
| Laser | Horizontal beam damage |
| Fire | Area damage zone |
| Crusher | Vertical crushing motion |

## Collectible Types

| Type | Effect |
|------|--------|
| Coin | +1 Score |
| Gem | +10 Score |
| Heart | +1 Health |
| Star | Activate power-up |
| Key | Add key to inventory |

## Save Format

Levels are saved as JSON with the following structure:

```json
{
  "levelName": "New Level",
  "gridOrigin": {"x": 0, "y": 0},
  "cellSize": 1,
  "gridWidth": 20,
  "gridHeight": 10,
  "platforms": [...],
  "traps": [...],
  "collectibles": [...]
}
```

## Code Standards

- C# 9.0+ features
- Null-coalescing and null-conditional operators
- Expression-bodied members where appropriate
- Pattern matching with switch expressions
- XML documentation comments

## License

MIT License - Free to use for personal and commercial projects.
