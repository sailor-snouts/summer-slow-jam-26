using PixelCrushers.DialogueSystem;

namespace Game
{
    /// <summary>
    /// Central gate for the player's <em>gameplay</em> input — movement, character swap, and
    /// interaction. It reports "locked" while a conversation is up, so the player can't wander off or
    /// act mid-dialogue. Dialogue-related input is unaffected: option selection runs through the
    /// Dialogue System's own UI / EventSystem, and Escape runs through the pause hotkey — neither
    /// checks this. Reading one shared global keeps every input script in sync without wiring.
    /// </summary>
    public static class PlayerInput
    {
        /// <summary>True when the player's gameplay input should be ignored (a conversation is active).</summary>
        public static bool Locked => DialogueManager.isConversationActive;
    }
}
