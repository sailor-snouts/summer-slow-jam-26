using JamTemplate.Menus;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Central gate for the player's <em>gameplay</em> input - movement, character swap, and
    /// interaction. It reports "locked" while a conversation or a menu overlay (outfit menu,
    /// settings, ...) is up, so the player can't wander off or act mid-dialogue/menu. Dialogue and
    /// menu input is unaffected: option selection and buttons run through their own UI / EventSystem,
    /// and Escape runs through the pause hotkey - neither checks this. Reading one shared global
    /// keeps every input script in sync without wiring.
    ///
    /// Scripted beats (e.g. the delay before an auto-started conversation) can also hold a manual lock
    /// via <see cref="Lock"/> / <see cref="Unlock"/> to cover the gap before dialogue/menu state takes over.
    /// </summary>
    public static class PlayerInput
    {
        private static int manualLocks;

        /// <summary>True when the player's gameplay input should be ignored.</summary>
        public static bool Locked =>
            manualLocks > 0 || DialogueManager.isConversationActive || MenuSceneRouter.HasOpenOverlay;

        /// <summary>Adds a manual input lock. Balance every call with <see cref="Unlock"/>.</summary>
        public static void Lock() => manualLocks++;

        /// <summary>Releases one manual lock added with <see cref="Lock"/> (never drops below zero).</summary>
        public static void Unlock() => manualLocks = Mathf.Max(0, manualLocks - 1);

        // Clear the manual lock count on play start so it can't leak across sessions (works with
        // domain reload disabled too).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => manualLocks = 0;
    }
}
