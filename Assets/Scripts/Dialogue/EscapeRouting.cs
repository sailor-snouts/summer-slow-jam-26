using JamTemplate.Game;
using JamTemplate.Menus;
using UnityEngine;

namespace Game
{
    public static class EscapeRouting
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bind()
        {
            // Only a menu overlay (e.g. Settings) suppresses the pause toggle - Escape closes it first.
            // A conversation does NOT suppress, so Escape opens the pause menu over it, like in gameplay.
            PauseHotkey.SuppressProvider = () => MenuSceneRouter.HasOpenOverlay;

            PauseHotkey.OnSuppressedPress = () =>
            {
                if (MenuSceneRouter.HasOpenOverlay)
                    MenuSceneRouter.CloseTopOverlay();
            };
        }
    }
}
