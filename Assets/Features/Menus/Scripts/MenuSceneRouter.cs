using System;
using UnityEngine.SceneManagement;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Extension point for how menu buttons change scenes. Core wires the
    /// providers to the Scene Transition Manager; with nothing wired, raw
    /// SceneManager loads keep menus functional (just without the fade).
    /// </summary>
    public static class MenuSceneRouter
    {
        /// <summary>Set by Core: loads a scene (Single) behind a transition.</summary>
        public static Action<string> LoadProvider;

        /// <summary>Set by Core: opens a scene additively behind a transition.</summary>
        public static Action<string> OpenAdditiveProvider;

        /// <summary>Set by Core: unloads an additive overlay behind a transition.</summary>
        public static Action<string> CloseAdditiveProvider;

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
        }

        /// <summary>Closes the additive overlay <paramref name="scene"/>, with a raw unload as fallback.</summary>
        public static void CloseAdditive(string scene)
        {
            if (CloseAdditiveProvider != null)
                CloseAdditiveProvider(scene);
            else
                SceneManager.UnloadSceneAsync(scene);
        }
    }
}
