using System;
using System.Collections;
using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JamTemplate.Game
{
    /// <summary>The high-level state the game is in.</summary>
    public enum GameState
    {
        /// <summary>Normal play.</summary>
        Playing,

        /// <summary>Paused — the pause overlay is up and time is slowed or stopped.</summary>
        Paused,
    }

    /// <summary>
    /// App-level game actions and state. Tracks whether the game is playing or
    /// paused, quits the game, and drives the pause overlay — a separate scene
    /// loaded additively behind a transition fade.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Game Manager")]
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        /// <summary>The active Game Manager. Persists across scene loads.</summary>
        public static GameManager Instance { get; private set; }

        [Header("Pause")]
        [SerializeField]
#if ODIN_INSPECTOR
        [ValueDropdown(nameof(GetSceneNames))]
#endif
        [Tooltip("Scene loaded additively as the pause overlay, picked from Build Settings.")]
        private string pauseScene;

        /// <summary>The current game state.</summary>
        public GameState State { get; private set; } = GameState.Playing;

        /// <summary>True while the game is paused.</summary>
        public bool IsPaused => State == GameState.Paused;

        /// <summary>Raised whenever <see cref="State"/> changes.</summary>
        public event Action<GameState> StateChanged;

        private float playingTimeScale = 1f;
        private bool busy;
        private bool holdingTransitionLock;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single)
                return;

            // A Single load tears down any pause overlay (and the transition
            // manager resets the timescale), so the Paused state must not
            // survive into the new scene — it would block Pause() forever.
            StopAllCoroutines();
            busy = false;
            playingTimeScale = 1f;
            ReleaseTransitionLock();
            SetState(GameState.Playing);
        }

        /// <summary>
        /// Quits the game, or stops Play mode in the editor. Does nothing on WebGL
        /// (browsers don't let pages close themselves) — Menu Button Action hides
        /// Quit buttons there automatically.
        /// </summary>
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
            Debug.LogWarning("[Game] Application.Quit does nothing on WebGL.", this);
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Pauses the game: freezes time and fades in the pause overlay.
        /// <paramref name="pausedTimeScale"/> sets the paused speed — 0 stops
        /// time entirely, a small value (e.g. 0.1) gives a slow-motion pause.
        /// </summary>
        public void Pause(float pausedTimeScale = 0f)
        {
            if (busy || IsPaused)
                return;
            if (string.IsNullOrEmpty(pauseScene))
            {
                Debug.LogWarning("[Game] No pause scene assigned.", this);
                return;
            }
            if (!Application.CanStreamedLevelBeLoaded(pauseScene))
            {
                Debug.LogError($"[Game] Pause scene '{pauseScene}' is not in Build Settings; cannot pause.", this);
                return;
            }
            if (!AcquireTransitionLock())
                return; // a scene transition owns the overlay right now

            StartCoroutine(PauseRoutine(Mathf.Max(0f, pausedTimeScale)));
        }

        /// <summary>Resumes the game and unloads the pause overlay.</summary>
        public void Resume()
        {
            if (busy || !IsPaused)
                return;
            if (!AcquireTransitionLock())
                return;

            StartCoroutine(ResumeRoutine());
        }

        /// <summary>Pauses if playing, resumes if paused.</summary>
        public void TogglePause(float pausedTimeScale = 0f)
        {
            if (IsPaused)
                Resume();
            else
                Pause(pausedTimeScale);
        }

        private IEnumerator PauseRoutine(float pausedTimeScale)
        {
            busy = true;
            playingTimeScale = Time.timeScale;
            SetState(GameState.Paused);

            // Transitions animate on unscaled time, so freezing now is safe.
            Time.timeScale = pausedTimeScale;

            IEnumerator cover = PauseOverlayTransition.Cover();
            if (cover != null)
                yield return cover;

            AsyncOperation load = SceneManager.LoadSceneAsync(pauseScene, LoadSceneMode.Additive);
            if (load == null)
            {
                // Don't strand the player frozen with no overlay to resume from.
                Debug.LogError($"[Game] Pause scene '{pauseScene}' failed to load; resuming.", this);
                Time.timeScale = playingTimeScale;
                SetState(GameState.Playing);
            }
            else
            {
                yield return load;
            }

            IEnumerator reveal = PauseOverlayTransition.Reveal();
            if (reveal != null)
                yield return reveal;

            busy = false;
            ReleaseTransitionLock();
        }

        private IEnumerator ResumeRoutine()
        {
            busy = true;

            IEnumerator cover = PauseOverlayTransition.Cover();
            if (cover != null)
                yield return cover;

            if (SceneManager.GetSceneByName(pauseScene).isLoaded)
                yield return SceneManager.UnloadSceneAsync(pauseScene);

            Time.timeScale = playingTimeScale;
            SetState(GameState.Playing);

            IEnumerator reveal = PauseOverlayTransition.Reveal();
            if (reveal != null)
                yield return reveal;

            busy = false;
            ReleaseTransitionLock();
        }

        // The pause flow animates the same overlay surface the transition manager
        // owns; reserving it (through the PauseOverlayTransition extension point)
        // keeps a Load() and a Pause() from fighting over it.

        private bool AcquireTransitionLock()
        {
            if (!PauseOverlayTransition.TryBegin())
                return false;

            holdingTransitionLock = true;
            return true;
        }

        private void ReleaseTransitionLock()
        {
            if (!holdingTransitionLock)
                return;

            holdingTransitionLock = false;
            PauseOverlayTransition.End();
        }

        private void SetState(GameState state)
        {
            if (State == state)
                return;

            State = state;
            StateChanged?.Invoke(state);
        }

#if ODIN_INSPECTOR
        private static IEnumerable<ValueDropdownItem<string>> GetSceneNames()
        {
            yield return new ValueDropdownItem<string>("(None)", string.Empty);
#if UNITY_EDITOR
            foreach (var buildScene in UnityEditor.EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled)
                    continue;
                string name = System.IO.Path.GetFileNameWithoutExtension(buildScene.path);
                yield return new ValueDropdownItem<string>(name, name);
            }
#endif
        }
#endif
    }
}
