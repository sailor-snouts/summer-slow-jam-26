using JamTemplate.Menus;
using PixelCrushers.DialogueSystem;

namespace Game
{
    /// <summary>
    /// Central gate for the player's <em>gameplay</em> input - movement, character swap, and
    /// interaction. It reports "locked" while a conversation or a menu overlay (outfit menu,
    /// settings, ...) is up, so the player can't wander off or act mid-dialogue/menu. Dialogue and
    /// menu input is unaffected: option selection and buttons run through their own UI / EventSystem,
    /// and Escape runs through the pause hotkey - neither checks this. Reading one shared global
    /// keeps every input script in sync without wiring.
    /// </summary>
    public static class PlayerInput
    {
        /// <summary>True when the player's gameplay input should be ignored.</summary>
        public static bool Locked => DialogueManager.isConversationActive || MenuSceneRouter.HasOpenOverlay;
    }
}
