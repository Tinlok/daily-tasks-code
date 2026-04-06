using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public string name = "New Level";
    public List<LevelObjectData> objects = new List<LevelObjectData>();
    public Vector2 worldSize = new Vector2(100, 100);
    public string description = "";
    public string difficulty = "Easy";
    public string author = "";
    public System.DateTime createdDate = System.DateTime.Now;
    public System.DateTime lastModified = System.DateTime.Now;
    
    // Level metadata
    public string sceneName = "";
    public string thumbnailPath = "";
    public int playCount = 0;
    public int completionRate = 0;
    
    // Level statistics
    public int platformCount = 0;
    public int trapCount = 0;
    public int collectibleCount = 0;
    public int playerStartCount = 0;
    public int goalCount = 0;
    
    // Validation flags
    public bool isValid = true;
    public string validationMessage = "";
    
    public void UpdateStatistics()
    {
        platformCount = 0;
        trapCount = 0;
        collectibleCount = 0;
        playerStartCount = 0;
        goalCount = 0;
        
        foreach (var obj in objects)
        {
            switch (obj.type)
            {
                case EditMode.Platform:
                    platformCount++;
                    break;
                case EditMode.Trap:
                    trapCount++;
                    break;
                case EditMode.Collectible:
                    collectibleCount++;
                    break;
                case EditMode.PlayerStart:
                    playerStartCount++;
                    break;
                case EditMode.Goal:
                    goalCount++;
                    break;
            }
        }
        
        ValidateLevel();
    }
    
    public void ValidateLevel()
    {
        isValid = true;
        validationMessage = "";
        
        // Check for at least one player start
        if (playerStartCount == 0)
        {
            isValid = false;
            validationMessage += "Missing player start position. ";
        }
        
        // Check for at least one goal
        if (goalCount == 0)
        {
            isValid = false;
            validationMessage += "Missing goal position. ";
        }
        
        // Check for reasonable object count
        if (objects.Count == 0)
        {
            isValid = false;
            validationMessage += "Level is empty. ";
        }
        
        // Check for too many objects (performance warning)
        if (objects.Count > 500)
        {
            validationMessage += "Warning: High object count may affect performance. ";
        }
        
        // Check if player is trapped
        if (IsPlayerTrapped())
        {
            isValid = false;
            validationMessage += "Warning: Player may be trapped. ";
        }
        
        lastModified = System.DateTime.Now;
    }
    
    private bool IsPlayerTrapped()
    {
        // Check if there are platforms or traps completely surrounding the player start
        // This is a simplified check - you might want to implement more sophisticated pathfinding
        foreach (var obj in objects)
        {
            if (obj.type == EditMode.Platform)
            {
                // Check if platform is surrounding player start
                // Implementation would involve distance calculations and angles
                // For now, return false (not trapped)
            }
        }
        
        return false;
    }
    
    public LevelData Clone()
    {
        LevelData clone = new LevelData();
        clone.name = name + " (Copy)";
        clone.worldSize = worldSize;
        clone.description = description;
        clone.difficulty = difficulty;
        clone.author = author;
        clone.createdDate = createdDate;
        clone.lastModified = System.DateTime.Now;
        clone.sceneName = sceneName;
        clone.thumbnailPath = thumbnailPath;
        clone.playCount = playCount;
        clone.completionRate = completionRate;
        clone.platformCount = platformCount;
        clone.trapCount = trapCount;
        clone.collectibleCount = collectibleCount;
        clone.playerStartCount = playerStartCount;
        clone.goalCount = goalCount;
        clone.isValid = isValid;
        clone.validationMessage = validationMessage;
        
        // Deep copy objects list
        foreach (var obj in objects)
        {
            clone.objects.Add(obj.Clone());
        }
        
        return clone;
    }
    
    public void AddObject(LevelObjectData objData)
    {
        objects.Add(objData);
        UpdateStatistics();
    }
    
    public void RemoveObject(int objectId)
    {
        objects.RemoveAll(obj => obj.id == objectId);
        UpdateStatistics();
    }
    
    public LevelObjectData GetObject(int objectId)
    {
        foreach (var obj in objects)
        {
            if (obj.id == objectId)
            {
                return obj;
            }
        }
        return null;
    }
    
    public List<LevelObjectData> GetObjectsByType(EditMode type)
    {
        List<LevelObjectData> result = new List<LevelObjectData>();
        foreach (var obj in objects)
        {
            if (obj.type == type)
            {
                result.Add(obj);
            }
        }
        return result;
    }
    
    public Vector3 GetPlayerStartPosition()
    {
        foreach (var obj in objects)
        {
            if (obj.type == EditMode.PlayerStart)
            {
                return obj.position;
            }
        }
        return Vector3.zero; // Default position
    }
    
    public Vector3 GetGoalPosition()
    {
        foreach (var obj in objects)
        {
            if (obj.type == EditMode.Goal)
            {
                return obj.position;
            }
        }
        return Vector3.zero; // Default position
    }
}

[System.Serializable]
public class LevelObjectData
{
    public int id;
    public EditMode type;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale = Vector3.one;
    public LayerName layer = LayerName.Default;
    
    // Custom properties for different object types
    public ObjectProperties properties = new ObjectProperties();
    
    public LevelObjectData Clone()
    {
        LevelObjectData clone = new LevelObjectData();
        clone.id = id;
        clone.type = type;
        clone.position = position;
        clone.rotation = rotation;
        clone.scale = scale;
        clone.layer = layer;
        clone.properties = properties.Clone();
        return clone;
    }
}

[System.Serializable]
public class ObjectProperties
{
    // Platform properties
    public float moveSpeed = 2f;
    public float moveDistance = 5f;
    public PlatformType platformType = PlatformType.Static;
    
    // Trap properties
    public TrapType trapType = TrapType.Spike;
    public float damage = 10f;
    public float activationDelay = 1f;
    public float cooldown = 3f;
    
    // Collectible properties
    public CollectibleType collectibleType = CollectibleType.Coin;
    public int value = 1;
    public float rotationSpeed = 100f;
    public float floatSpeed = 2f;
    
    // Player start properties
    public string playerName = "Player";
    public Vector2 playerStartVelocity = Vector2.zero;
    
    // Goal properties
    public string goalName = "Goal";
    public bool goalCompleted = false;
    
    // Generic properties
    public bool isActive = true;
    public bool isVisible = true;
    public string objectName = "";
    public string objectTag = "";
    public string objectLayer = "Default";
    
    public ObjectProperties Clone()
    {
        ObjectProperties clone = new ObjectProperties();
        clone.moveSpeed = moveSpeed;
        clone.moveDistance = moveDistance;
        clone.platformType = platformType;
        clone.trapType = trapType;
        clone.damage = damage;
        clone.activationDelay = activationDelay;
        clone.cooldown = cooldown;
        clone.collectibleType = collectibleType;
        clone.value = value;
        clone.rotationSpeed = rotationSpeed;
        clone.floatSpeed = floatSpeed;
        clone.playerName = playerName;
        clone.playerStartVelocity = playerStartVelocity;
        clone.goalName = goalName;
        clone.goalCompleted = goalCompleted;
        clone.isActive = isActive;
        clone.isVisible = isVisible;
        clone.objectName = objectName;
        clone.objectTag = objectTag;
        clone.objectLayer = objectLayer;
        return clone;
    }
}

// Extended enum for additional object types
public enum EditMode
{
    Platform,
    Trap,
    Collectible,
    Delete,
    PlayerStart,
    Goal,
    Decorative,
    Trigger,
    Enemy,
    PowerUp
}

public enum PlatformType
{
    Static,
    Moving,
    FollowPlayer,
    Rotating,
    Falling
}

public enum TrapType
{
    Spike,
    Laser,
    FallingRock,
    Sawblade,
    Poison,
    Fire
}

public enum CollectibleType
{
    Coin,
    Gem,
    PowerUp,
    Key,
    Health,
    Weapon
}

public enum LayerName
{
    Default = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2,
    Water = 4,
    UI = 5,
    Target = 8,
    Obstacle = 9,
    Collectible = 10,
    Player = 11,
    Enemy = 12,
    Trap = 13,
    Decorative = 14,
    Trigger = 15,
    Goal = 16
}