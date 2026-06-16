using System;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Holds the player's global UI scale (1 = 100%). Lives in Core so any canvas
    /// can read it without depending on the Settings assembly; the SettingsManager
    /// pushes the chosen value in via <see cref="Set"/>.
    /// </summary>
    public static class UIScale
    {
        /// <summary>The current UI scale multiplier (1 = 100%).</summary>
        public static float Current { get; private set; } = 1f;

        /// <summary>Raised when the scale changes so live canvases can re-apply it.</summary>
        public static event Action Changed;

        /// <summary>Sets the UI scale and notifies listeners if it changed.</summary>
        public static void Set(float scale)
        {
            scale = scale <= 0f ? 1f : scale;
            if (Current == scale)
                return;

            Current = scale;
            Changed?.Invoke();
        }
    }
}
