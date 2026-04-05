namespace LevelEditor
{
    /// <summary>
    /// Types of placeable objects in the level editor
    /// </summary>
    public enum PlaceableType
    {
        /// <summary>Standard platform - player can stand on it</summary>
        Platform,

        /// <summary>Moving platform - moves between waypoints</summary>
        MovingPlatform,

        /// <summary>One-way platform - player can jump up through</summary>
        OneWayPlatform,

        /// <summary>Spike trap - damages player on contact</summary>
        SpikeTrap,

        /// <summary>Saw trap - moving circular blade</summary>
        SawTrap,

        /// <summary>Coin - collectible for score</summary>
        Coin,

        /// <summary>Gem - special collectible</summary>
        Gem,

        /// <summary>Heart - restores health</summary>
        Heart,

        /// <summary>Key - unlocks doors</summary>
        Key
    }
}
