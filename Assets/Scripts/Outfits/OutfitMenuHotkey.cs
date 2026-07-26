using JamTemplate.Game;
using JamTemplate.Menus;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game
{
    /// <summary>
    /// Toggles the outfit menu with a key (I). The menu is the "Outfits" scene opened additively via
    /// the menu router, so it joins the overlay stack: Escape closes it, gameplay input locks while it
    /// is open, and the pause menu still works around it. Auto-spawns at startup so there is nothing
    /// to place in a scene.
    /// </summary>
    public class OutfitMenuHotkey : MonoBehaviour
    {
        /// <summary>Scene name of the outfit menu overlay (create it via Tools > Game > Create Outfit Scene).</summary>
        public const string SceneName = "Outfits";

        private const Key ToggleKey = Key.I;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject("Outfit Menu Hotkey");
            DontDestroyOnLoad(go);
            go.AddComponent<OutfitMenuHotkey>();
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current[ToggleKey].wasPressedThisFrame)
                return;

            // Already open: the same key closes it (Escape works too, via the overlay stack).
            if (SceneManager.GetSceneByName(SceneName).isLoaded)
            {
                MenuSceneRouter.CloseAdditive(SceneName);
                return;
            }

            // Only open from plain gameplay - not during dialogue, pause, or another overlay.
            if (DialogueManager.isConversationActive || MenuSceneRouter.HasOpenOverlay)
                return;
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
                return;

            if (!Application.CanStreamedLevelBeLoaded(SceneName))
            {
                Debug.LogError(
                    $"[OutfitMenuHotkey] Scene '{SceneName}' is not in Build Settings. " +
                    "Create it via Tools > Game > Create Outfit Scene.", this);
                return;
            }

            MenuSceneRouter.OpenAdditive(SceneName);
        }
    }
}
