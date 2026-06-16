using System;
using UnityEngine;

namespace JamTemplate.Settings
{
    /// <summary>
    /// A full snapshot of the player's settings. Plain data — the
    /// <see cref="SettingsManager"/> applies it to the engine and persists it.
    /// </summary>
    [Serializable]
    public class SettingsState
    {
        // Audio (0..1).
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float musicVolume = 1f;
        public float ambianceVolume = 1f;
        public float dialogueVolume = 1f;
        public float uiVolume = 1f;

        // Display.
        public int resolutionWidth;
        public int resolutionHeight;
        public FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow;

        // Graphics.
        public int qualityLevel;
        public bool vSync = true;
        public int targetFrameRate = -1; // -1 = unlimited
        public int antiAliasing;          // 0, 2, 4 or 8

        // Interface.
        public float uiScale = 1f; // 1 = 100%

        /// <summary>Returns an independent copy. All fields are value types, so this is a deep copy.</summary>
        public SettingsState Clone() => (SettingsState)MemberwiseClone();
    }
}
