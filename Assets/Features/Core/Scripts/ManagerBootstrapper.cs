using System;
using JamTemplate.Audio;
using JamTemplate.Game;
using JamTemplate.Saving;
using JamTemplate.SceneManagement;
using JamTemplate.Settings;
using UnityEngine;

namespace JamTemplate.Core
{
    /// <summary>
    /// Spawns every global manager once, before the first scene loads, so the full
    /// set exists no matter which scene is entered — the boot scene in a build, or a
    /// single scene opened directly in the editor. Managers are singletons that
    /// persist via <c>DontDestroyOnLoad</c>; any duplicate that happens to be placed
    /// in a scene self-destructs through its own Awake guard, so scenes never need to
    /// carry managers.
    ///
    /// To add a new manager: add one line below. Nothing else, no scene edits.
    /// Every manager loads from a prefab in Core's <c>Prefabs/Resources</c> folder,
    /// so its inspector values (and any serialized asset references, like the
    /// AudioManager's mixer) live on the prefab. <see cref="EnsureBare{T}"/> exists
    /// for managers that genuinely have no prefab.
    /// </summary>
    public static class ManagerBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
#if FMOD_PRESENT
            // FMOD backend selected at compile time: spawn its manager instead. The
            // prefab is created by the Enable FMOD editor flow once the package exists.
            EnsureFromPrefab("Fmod Audio Manager", () => FmodAudioManager.Instance != null);
#else
            EnsureFromPrefab("Audio Manager", () => AudioManager.Instance != null);
#endif
            EnsureFromPrefab("Game Manager", () => GameManager.Instance != null);
            EnsureFromPrefab("SceneTransitionManager", () => SceneTransitionManager.Instance != null);
            EnsureFromPrefab("Save Manager", () => SaveManager.Instance != null);
            EnsureFromPrefab("Settings Manager", () => SettingsManager.Instance != null);
            EnsureFromPrefab("High Score Manager", () => HighScoreManager.Instance != null);
        }

        private static void EnsureFromPrefab(string resourceName, Func<bool> alreadyExists)
        {
            if (alreadyExists())
                return;

            var prefab = Resources.Load<GameObject>(resourceName);
            if (prefab == null)
            {
                Debug.LogError(
                    $"[ManagerBootstrapper] Could not load manager prefab 'Resources/{resourceName}'. " +
                    "That manager will not exist at runtime.");
                return;
            }

            // The instance's own Awake assigns its singleton and calls DontDestroyOnLoad.
            UnityEngine.Object.Instantiate(prefab).name = prefab.name;
        }

        private static void EnsureBare<T>(string name, Func<bool> alreadyExists) where T : Component
        {
            if (alreadyExists())
                return;

            new GameObject(name).AddComponent<T>();
        }
    }
}
