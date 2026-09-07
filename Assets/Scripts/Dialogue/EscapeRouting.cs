using JamTemplate.Game;
using JamTemplate.Menus;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Routes the Escape key for overlays the pause hotkey doesn't know about, via the template's
    /// <see cref="PauseHotkey"/> suppress seam. While a conversation or a menu overlay (e.g. the
    /// Settings menu opened over the pause menu) is open, Escape does not toggle pause: it closes the
    /// topmost menu overlay, and does nothing during a conversation - dialogue is dismissed by playing
    /// through it, not by Escape. Once nothing is layered on top, Escape pauses/unpauses as usual.
    /// Just sets static seams once at startup; no scene object needed.
    /// </summary>
    public static class EscapeRouting
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bind()
        {
            // Suppress the pause toggle while a conversation or a menu overlay is up, so Escape doesn't
            // open the pause menu on top of them.
            PauseHotkey.SuppressProvider = () =>
                DialogueManager.isConversationActive || MenuSceneRouter.HasOpenOverlay;

            // Escape closes a menu overlay (e.g. Settings) if one is on top. It intentionally does NOT
            // end a conversation - a conversation with nothing layered over it just ignores Escape.
            PauseHotkey.OnSuppressedPress = () =>
            {
                if (MenuSceneRouter.HasOpenOverlay)
                    MenuSceneRouter.CloseTopOverlay();
            };
        }
    }
}
