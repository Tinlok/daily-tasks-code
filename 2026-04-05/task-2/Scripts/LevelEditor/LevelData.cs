using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelEditor
{
    /// <summary>
    /// Serializable data for a complete level
    /// </summary>
    [Serializable]
    public class LevelData
    {
        [Header("Level Info")]
        public string levelName = "New Level";
        public string levelId;
        public string description = "";
        public int version = 1;

        [Header("Level Settings")]
        public Vector2 cameraBoundsMin = new Vector2(-20, -10);
        public Vector2 cameraBoundsMax = new Vector2(20, 10);
        public float gravity = -9.81f;
        public string backgroundMusicId = "";
        public string backgroundSpriteId = "";

        [Header("Level Content")]
        public List<PlaceableData> platforms = new List<PlaceableData>();
        public List<PlaceableData> traps = new List<PlaceableData>();
        public List<PlaceableData> collectibles = new List<PlaceableData>();

        [Header("Player Spawn")]
        public Vector2 playerSpawnPosition = Vector2.zero;

        /// <summary>
        /// Generate a unique level ID
        /// </summary>
        public void GenerateId()
        {
            levelId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Get all placeable objects
        /// </summary>
        public List<PlaceableData> GetAllPlaceables()
        {
            var all = new List<PlaceableData>();
            all.AddRange(platforms);
            all.AddRange(traps);
            all.AddRange(collectibles);
            return all;
        }

        /// <summary>
        /// Get count of all placeables
        /// </summary>
        public int GetTotalPlaceableCount()
        {
            return platforms.Count + traps.Count + collectibles.Count;
        }
    }

}
