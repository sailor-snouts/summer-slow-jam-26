using JamTemplate.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    // Music carries across scenes that play the same track (the audio manager won't restart it), so a scene
    // with no PlayMusicOnStart would otherwise inherit the previous scene's music. When a scene replaces the
    // current one (a Single load) and has no music player, stop the music. Additive loads - the pause,
    // settings and outfit overlays - are ignored so music keeps playing under menus.
    public static class SceneMusicGuard
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; // safe re-hook when domain reload is disabled
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single)
                return;

            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.GetComponentInChildren<PlayMusicOnStart>(true) != null)
                    return; // this scene picks its own music

            GameAudio.StopMusic();
        }
    }
}
