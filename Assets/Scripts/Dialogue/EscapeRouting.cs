using JamTemplate.Game;
using JamTemplate.Menus;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    public static class EscapeRouting
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bind()
        {
            // Suppress the pause toggle while a conversation or menu overlay is up, so Escape doesn't
            // open the pause menu on top of them.
            PauseHotkey.SuppressProvider = () =>
                DialogueManager.isConversationActive || MenuSceneRouter.HasOpenOverlay;

            // Intentionally does NOT end a conversation - one with nothing layered over it ignores Escape.
            PauseHotkey.OnSuppressedPress = () =>
            {
                if (MenuSceneRouter.HasOpenOverlay)
                    MenuSceneRouter.CloseTopOverlay();
            };
        }
    }
}
