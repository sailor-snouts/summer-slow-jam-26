using System;

namespace JamTemplate.Settings
{
    /// <summary>
    /// Extension points the Settings feature pushes through instead of
    /// referencing other assemblies. Core wires them to the Audio Manager and
    /// the UI scale; with nothing wired, the corresponding settings are simply
    /// not applied (everything else still works and persists).
    /// </summary>
    public static class SettingsHooks
    {
        /// <summary>Set by Core: pushes the state's volumes to the audio system.</summary>
        public static Action<SettingsState> ApplyAudio;

        /// <summary>Set by Core: reads the audio system's current volumes into the state.</summary>
        public static Action<SettingsState> CaptureAudio;

        /// <summary>Set by Core: applies the chosen UI scale (1 = 100%).</summary>
        public static Action<float> ApplyUiScale;

        /// <summary>Set by Core: reads the current UI scale.</summary>
        public static Func<float> GetUiScale;
    }
}
