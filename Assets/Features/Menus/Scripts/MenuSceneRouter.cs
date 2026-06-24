using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Extension point for how menu buttons change scenes. Core wires the
    /// providers to the Scene Transition Manager; with nothing wired, raw
    /// SceneManager loads keep menus functional (just without the fade).
    ///
    /// Also tracks the additive overlays it opens (e.g. a Settings menu) as a stack,
    /// so callers like the Escape key can close the topmost overlay before falling
    /// through to other behaviour (see <see cref="HasOpenOverlay"/> / <see cref="CloseTopOverlay"/>).
    /// </summary>
    public static class MenuSceneRouter
    {
        /// <summary>Set by Core: loads a scene (Single) behind a transition.</summary>
        public static Action<string> LoadProvider;

        /// <summary>Set by Core: opens a scene additively behind a transition.</summary>
        public static Action<string> OpenAdditiveProvider;

        /// <summary>Set by Core: unloads an additive overlay behind a transition.</summary>
        public static Action<string> CloseAdditiveProvider;

        // Additive overlays opened via OpenAdditive, in open order (last = topmost). The pause
        // overlay is loaded by the Game Manager, not through here, so it is deliberately not
        // tracked — letting "only the pause menu is open" be distinguished from "a sub-overlay
        // (Settings) is open over the pause menu".
        private static readonly List<string> openOverlays = new();

        /// <summary>True while any additive overlay opened via <see cref="OpenAdditive"/> is still open.</summary>
        public static bool HasOpenOverlay => openOverlays.Count > 0;

        // Reset on play (covers domain-reload-disabled) and forget overlays whenever a Single load
        // tears them down, so the stack never holds stale entries.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            openOverlays.Clear();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
                openOverlays.Clear();
        }

        /// <summary>Loads <paramref name="scene"/>, with a raw load as fallback.</summary>
        public static void Load(string scene)
        {
            if (LoadProvider != null)
                LoadProvider(scene);
            else
                SceneManager.LoadScene(scene);
        }

        /// <summary>Opens <paramref name="scene"/> additively, with a raw load as fallback.</summary>
        public static void OpenAdditive(string scene)
        {
            if (OpenAdditiveProvider != null)
                OpenAdditiveProvider(scene);
            else
                SceneManager.LoadScene(scene, LoadSceneMode.Additive);

            // Move to the top of the stack (re-opening an already-open overlay shouldn't duplicate it).
            openOverlays.Remove(scene);
            openOverlays.Add(scene);
        }

        /// <summary>Closes the additive overlay <paramref name="scene"/>, with a raw unload as fallback.</summary>
        public static void CloseAdditive(string scene)
        {
            openOverlays.Remove(scene);

            if (CloseAdditiveProvider != null)
                CloseAdditiveProvider(scene);
            else
                SceneManager.UnloadSceneAsync(scene);
        }

        /// <summary>Closes the topmost overlay opened via <see cref="OpenAdditive"/>; no-op if none are open.</summary>
        public static void CloseTopOverlay()
        {
            if (openOverlays.Count == 0)
                return;

            CloseAdditive(openOverlays[openOverlays.Count - 1]);
        }
    }
}
