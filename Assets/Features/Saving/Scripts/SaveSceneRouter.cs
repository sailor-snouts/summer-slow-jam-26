using System;
using UnityEngine.SceneManagement;

namespace JamTemplate.Saving
{
    /// <summary>
    /// Extension point for how the save system loads the scene a save was made
    /// in (Continue / Load). Core wires the providers to the Scene Transition
    /// Manager; with nothing wired, raw SceneManager loads are used.
    /// </summary>
    public static class SaveSceneRouter
    {
        /// <summary>Set by Core: whether a scene transition is currently running.</summary>
        public static Func<bool> IsBusyProvider;

        /// <summary>Set by Core: loads a scene (Single) behind a transition.</summary>
        public static Action<string> LoadProvider;

        /// <summary>Whether a transition is running. False when nothing is wired.</summary>
        public static bool IsBusy => IsBusyProvider != null && IsBusyProvider();

        /// <summary>Loads <paramref name="scene"/>, with a raw load as fallback.</summary>
        public static void Load(string scene)
        {
            if (LoadProvider != null)
                LoadProvider(scene);
            else
                SceneManager.LoadScene(scene);
        }
    }
}
