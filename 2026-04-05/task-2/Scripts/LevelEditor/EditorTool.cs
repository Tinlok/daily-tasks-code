namespace LevelEditor
{
    /// <summary>
    /// Available editor tools for the level editor
    /// </summary>
    public enum EditorTool
    {
        /// <summary>Selection tool - select and manipulate existing objects</summary>
        Select,

        /// <summary>Platform placement tool</summary>
        Platform,

        /// <summary>Trap placement tool</summary>
        Trap,

        /// <summary>Collectible placement tool</summary>
        Collectible,

        /// <summary>Eraser tool - remove placed objects</summary>
        Eraser
    }
}
