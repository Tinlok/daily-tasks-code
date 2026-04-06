using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor
{
    /// <summary>
    /// Serializable data structure for storing level information.
    /// Can be saved to JSON for persistence.
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public string levelName = "New Level";
        public Vector2 gridOrigin = Vector2.zero;
        public float cellSize = 1f;
        public int gridWidth = 20;
        public int gridHeight = 10;

        public List<PlatformData> platforms = new();
        public List<TrapData> traps = new();
        public List<CollectibleData> collectibles = new();

        /// <summary>
        /// Converts the LevelData to JSON format for saving.
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        /// <summary>
        /// Creates LevelData from JSON string.
        /// </summary>
        public static LevelData FromJson(string json)
        {
            return JsonUtility.FromJson<LevelData>(json);
        }

        /// <summary>
        /// Creates a deep copy of this level data.
        /// </summary>
        public LevelData Clone()
        {
            return FromJson(ToJson());
        }

        /// <summary>
        /// Clears all level objects.
        /// </summary>
        public void Clear()
        {
            platforms.Clear();
            traps.Clear();
            collectibles.Clear();
        }
    }

    /// <summary>
    /// Serializable data for a platform in the level.
    /// </summary>
    [Serializable]
    public class PlatformData
    {
        public Vector2 position;
        public Vector2 scale = Vector2.one;
        public PlatformType type = PlatformType.Normal;
        public string spritePath = "";
    }

    /// <summary>
    /// Serializable data for a trap in the level.
    /// </summary>
    [Serializable]
    public class TrapData
    {
        public Vector2 position;
        public TrapType type = TrapType.Spike;
        public float damage = 1f;
        public bool isActivated = true;
    }

    /// <summary>
    /// Serializable data for a collectible in the level.
    /// </summary>
    [Serializable]
    public class CollectibleData
    {
        public Vector2 position;
        public CollectibleType type = CollectibleType.Coin;
        public int value = 1;
        public bool respawnOnDeath = false;
    }

    /// <summary>
    /// Types of platforms available in the level editor.
    /// </summary>
    public enum PlatformType
    {
        Normal,
        Ice,
        Bouncy,
        Moving,
        Breakable
    }

    /// <summary>
    /// Types of traps available in the level editor.
    /// </summary>
    public enum TrapType
    {
        Spike,
        Sawblade,
        Laser,
        Fire,
        Crusher
    }

    /// <summary>
    /// Types of collectibles available in the level editor.
    /// </summary>
    public enum CollectibleType
    {
        Coin,
        Gem,
        Heart,
        Star,
        Key
    }
}
