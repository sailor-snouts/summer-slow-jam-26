using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JamTemplate.SceneManagement
{
    /// <summary>
    /// Loads scenes asynchronously and reports progress. Kept separate from the
    /// visual <see cref="SceneTransition"/> so either can be used on its own.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Scene Loader")]
    [DisallowMultipleComponent]
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>Load progress from 0 to 1.</summary>
        public float Progress { get; private set; }

        /// <summary>True while a scene load is in progress.</summary>
        public bool IsLoading { get; private set; }

        /// <summary>Raised whenever the progress value changes (0 to 1).</summary>
        public event Action<float> ProgressChanged;

        /// <summary>Raised once the new scene has finished loading and activated.</summary>
        public event Action Loaded;

        /// <summary>
        /// Loads <paramref name="sceneName"/> in Single mode. The coroutine completes
        /// once the new scene is active.
        /// </summary>
        public IEnumerator LoadRoutine(string sceneName)
        {
            IsLoading = true;
            SetProgress(0f);

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (op == null)
            {
                Debug.LogError($"[SceneLoader] Could not load scene '{sceneName}'. Is it added to Build Settings?", this);
                IsLoading = false;
                yield break;
            }

            // Async load progress runs 0..0.9 while loading; the last 0.1 is activation.
            op.allowSceneActivation = false;
            while (op.progress < 0.9f)
            {
                SetProgress(op.progress / 0.9f);
                yield return null;
            }

            SetProgress(1f);
            op.allowSceneActivation = true;

            while (!op.isDone)
                yield return null;

            IsLoading = false;
            Loaded?.Invoke();
        }

        private void SetProgress(float value)
        {
            Progress = Mathf.Clamp01(value);
            ProgressChanged?.Invoke(Progress);
        }
    }
}
