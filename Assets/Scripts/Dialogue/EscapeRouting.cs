using JamTemplate.Game;
using JamTemplate.Menus;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Routes the Escape key for overlays the pause hotkey doesn't know about, via the template's
    /// <see cref="PauseHotkey"/> suppress seam. While a conversation or a menu overlay (e.g. the
    /// Settings menu opened over the pause menu) is open, Escape closes the topmost one instead of
    /// toggling pause; once nothing is layered on top, Escape pauses/unpauses as usual.
    ///
    /// Order matters: a dialogue is closed first, then menu overlays (Settings), then — when the
    /// router reports nothing open — the press falls through to the pause toggle, which closes the
    /// pause menu itself. Just sets static seams once at startup; no scene object needed.
    /// </summary>
    public static class EscapeRouting
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bind()
        {
            PauseHotkey.SuppressProvider = () =>
                DialogueManager.isConversationActive || MenuSceneRouter.HasOpenOverlay;

            PauseHotkey.OnSuppressedPress = () =>
            {
                if (DialogueManager.isConversationActive)
                    DialogueManager.StopConversation();
                else
                    MenuSceneRouter.CloseTopOverlay();
            };
        }
    }
}
