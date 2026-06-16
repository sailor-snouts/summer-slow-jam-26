using System;
using System.Collections;
using UnityEngine.SceneManagement;

namespace JamTemplate.SplashScreens
{
    /// <summary>
    /// Extension point for the fades around each splash and the final scene
    /// load. Core wires the providers to the Scene Transition Manager; with
    /// nothing wired, splashes cut hard and the next scene loads raw.
    /// </summary>
    public static class SplashTransition
    {
        /// <summary>Set by Core: snaps the screen to fully covered before the first splash.</summary>
        public static Action SnapCoveredProvider;

        /// <summary>Set by Core: covers the screen. May be null (no fade).</summary>
        public static Func<IEnumerator> CoverProvider;

        /// <summary>Set by Core: reveals the screen. May be null (no fade).</summary>
        public static Func<IEnumerator> RevealProvider;

        /// <summary>Set by Core: loads the next scene behind a transition.</summary>
        public static Action<string> LoadSceneProvider;

        /// <summary>Whether any fade surface is wired up.</summary>
        public static bool HasFader => CoverProvider != null && RevealProvider != null;

        /// <summary>Snaps to covered, if wired.</summary>
        public static void SnapCovered() => SnapCoveredProvider?.Invoke();

        /// <summary>The cover animation, or null when none is wired.</summary>
        public static IEnumerator Cover() => CoverProvider?.Invoke();

        /// <summary>The reveal animation, or null when none is wired.</summary>
        public static IEnumerator Reveal() => RevealProvider?.Invoke();

        /// <summary>Loads <paramref name="scene"/>, with a raw load as fallback.</summary>
        public static void LoadScene(string scene)
        {
            if (LoadSceneProvider != null)
                LoadSceneProvider(scene);
            else
                SceneManager.LoadScene(scene);
        }
    }
}
