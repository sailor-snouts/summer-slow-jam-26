namespace JamTemplate.Menus
{
    /// <summary>What a menu button does when pressed.</summary>
    public enum MenuAction
    {
        /// <summary>Transitions to the chosen scene.</summary>
        LoadScene,

        /// <summary>Quits the game (or stops Play mode).</summary>
        Quit,

        /// <summary>Invokes a custom UnityEvent.</summary>
        Event,

        /// <summary>Resumes the game from a paused state.</summary>
        Resume,

        /// <summary>Loads the chosen scene additively as an overlay (e.g. settings).</summary>
        OpenAdditive,

        /// <summary>Closes the additive scene this button lives in (e.g. a Back button).</summary>
        CloseSelf,

        /// <summary>Resumes the most recent save (loads its scene and applies it). Appended to keep enum values stable.</summary>
        Continue,
    }
}
