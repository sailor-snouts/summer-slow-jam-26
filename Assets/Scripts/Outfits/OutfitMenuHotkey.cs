using JamTemplate.Game;
using JamTemplate.Menus;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game
{
    public class OutfitMenuHotkey : MonoBehaviour
    {
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

            if (SceneManager.GetSceneByName(SceneName).isLoaded)
            {
                MenuSceneRouter.CloseAdditive(SceneName);
                return;
            }

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
