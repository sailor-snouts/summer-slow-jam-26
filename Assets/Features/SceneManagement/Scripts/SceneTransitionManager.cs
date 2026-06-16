using System.Collections;
using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JamTemplate.SceneManagement
{
    /// <summary>
    /// Drives the cover -> load -> reveal sequence. Spawned automatically at
    /// startup by the Core feature and survives scene loads. Trigger from
    /// anywhere with SceneTransitionManager.Instance.Load("SceneName").
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Scene Transition Manager")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SceneTransition), typeof(SceneLoader))]
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Loading Progress")]
        [SerializeField]
        [Tooltip("Show a loading progress display on the overlay while the next scene loads.")]
        private bool showProgress = true;

        [SerializeField]
#if ODIN_INSPECTOR
        [ShowIf(nameof(showProgress))]
#endif
        [Tooltip("Show the load percentage as text.")]
        private bool showText = true;

        [SerializeField]
#if ODIN_INSPECTOR
        [ShowIf(nameof(showProgress))]
#endif
        [Tooltip("Show a horizontal fill bar.")]
        private bool showBar = true;

        [SerializeField]
#if ODIN_INSPECTOR
        [ShowIf(nameof(showProgress))]
#endif
        [Tooltip("Colour of the progress text and bar fill.")]
        private Color progressColor = Color.white;

        [Header("Audio")]
        [SerializeField]
        [Tooltip("Fade all audio out as the screen covers, and back in as it reveals (uses the Audio Manager if one exists).")]
        private bool fadeAudio = true;

        private SceneTransition transition;
        private SceneLoader loader;
        private GameObject progressRoot;
        private Text progressText;
        private RectTransform progressBarFill;
        private bool isTransitioning;

        /// <summary>True while a cover -> load -> reveal sequence is running.</summary>
        public bool IsTransitioning => isTransitioning;

        /// <summary>The visual transition component this manager drives.</summary>
        public SceneTransition Transition =>
            transition != null ? transition : transition = GetComponent<SceneTransition>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            transition = GetComponent<SceneTransition>();
            loader = GetComponent<SceneLoader>();
            transition.EnsureBuilt();
            BuildProgressUI();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Transitions to <paramref name="sceneName"/>: cover, load, reveal.</summary>
        public void Load(string sceneName)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] A transition is already in progress.", this);
                return;
            }
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[SceneTransitionManager] Load called with no scene name.", this);
                return;
            }

            StartCoroutine(LoadRoutine(sceneName));
        }

        /// <summary>
        /// Reserves the transition overlay for an external cover/reveal sequence
        /// (e.g. the pause flow), so <see cref="Load"/> and the additive calls
        /// won't animate the same surface concurrently. Returns false if a
        /// transition is already running; pair with
        /// <see cref="EndExternalTransition"/>.
        /// </summary>
        public bool TryBeginExternalTransition()
        {
            if (isTransitioning)
                return false;

            isTransitioning = true;
            return true;
        }

        /// <summary>Releases the reservation taken by <see cref="TryBeginExternalTransition"/>.</summary>
        public void EndExternalTransition()
        {
            isTransitioning = false;
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            isTransitioning = true;

            // A fresh scene always starts at normal speed, even if the previous
            // one left time slowed or stopped (e.g. quitting from a pause menu).
            Time.timeScale = 1f;

            // Any overlay bookkeeping dies with the outgoing scenes.
            mutedGroups.Clear();
            selectionBeforeOverlay = null;

            bool fadingAudio = fadeAudio && TransitionAudio.IsAvailable;

            if (fadingAudio && !transition.IsCovered)
                TransitionAudio.FadeOut(transition.CoverDuration);
            yield return transition.Cover();

            SetProgressValue(0f);
            ShowProgress(true);

            loader.ProgressChanged += SetProgressValue;
            yield return loader.LoadRoutine(sceneName);
            loader.ProgressChanged -= SetProgressValue;

            ShowProgress(false);

            if (fadingAudio)
                TransitionAudio.FadeIn(transition.RevealDuration);
            yield return transition.Reveal();

            isTransitioning = false;
        }

        /// <summary>
        /// Loads <paramref name="sceneName"/> additively as an overlay, behind a
        /// cover/reveal fade. The current scene stays loaded underneath. Use it
        /// for menus like settings that sit on top of whatever is open.
        /// </summary>
        public void OpenAdditive(string sceneName)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] A transition is already in progress.", this);
                return;
            }
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[SceneTransitionManager] OpenAdditive called with no scene name.", this);
                return;
            }
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
                return;

            StartCoroutine(OpenAdditiveRoutine(sceneName));
        }

        /// <summary>Unloads an additive overlay <paramref name="sceneName"/>, behind a cover/reveal fade.</summary>
        public void CloseAdditive(string sceneName)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] A transition is already in progress.", this);
                return;
            }
            if (string.IsNullOrEmpty(sceneName) || !SceneManager.GetSceneByName(sceneName).isLoaded)
                return;

            StartCoroutine(CloseAdditiveRoutine(sceneName));
        }

        private IEnumerator OpenAdditiveRoutine(string sceneName)
        {
            isTransitioning = true;

            yield return transition.Cover();
            // The overlay should own input while it is up: remember the selection
            // (so closing can restore gamepad/keyboard focus) and disable
            // interaction on everything already loaded — Unity's automatic
            // navigation would otherwise wander onto the covered menu's buttons.
            CaptureUnderlyingUI();
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return transition.Reveal();

            isTransitioning = false;
        }

        private IEnumerator CloseAdditiveRoutine(string sceneName)
        {
            isTransitioning = true;

            yield return transition.Cover();
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
                yield return SceneManager.UnloadSceneAsync(sceneName);
            RestoreUnderlyingUI();
            yield return transition.Reveal();

            isTransitioning = false;
        }

        // --- Underlying-UI capture for additive overlays --------------------------

        private readonly List<(CanvasGroup group, bool added, bool wasInteractable)> mutedGroups
            = new List<(CanvasGroup, bool, bool)>();
        private GameObject selectionBeforeOverlay;

        private void CaptureUnderlyingUI()
        {
            selectionBeforeOverlay = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject
                : null;

            foreach (Canvas canvas in FindObjectsByType<Canvas>())
            {
                if (canvas.transform.parent != null || canvas.gameObject.scene == gameObject.scene)
                    continue; // only root canvases of regular scenes (not nested, not DontDestroyOnLoad)

                CanvasGroup group = canvas.GetComponent<CanvasGroup>();
                bool added = group == null;
                if (added)
                    group = canvas.gameObject.AddComponent<CanvasGroup>();

                mutedGroups.Add((group, added, group.interactable));
                group.interactable = false;
            }
        }

        private void RestoreUnderlyingUI()
        {
            foreach ((CanvasGroup group, bool added, bool wasInteractable) in mutedGroups)
            {
                if (group == null)
                    continue;
                if (added)
                    Destroy(group);
                else
                    group.interactable = wasInteractable;
            }
            mutedGroups.Clear();

            if (EventSystem.current != null && selectionBeforeOverlay != null)
                EventSystem.current.SetSelectedGameObject(selectionBeforeOverlay);
            selectionBeforeOverlay = null;
        }

        private void ShowProgress(bool visible)
        {
            if (progressRoot != null)
                progressRoot.SetActive(visible);
        }

        private void SetProgressValue(float value)
        {
            value = Mathf.Clamp01(value);
            if (progressText != null)
                progressText.text = Mathf.RoundToInt(value * 100f) + "%";
            if (progressBarFill != null)
                progressBarFill.anchorMax = new Vector2(value, 1f);
        }

        private void BuildProgressUI()
        {
            if (!showProgress)
                return;

            int uiLayer = LayerMask.NameToLayer("UI");

            progressRoot = new GameObject("Loading Progress", typeof(RectTransform));
            progressRoot.layer = uiLayer;
            var rootRect = (RectTransform)progressRoot.transform;
            rootRect.SetParent(transition.OverlayRoot, false);
            rootRect.anchorMin = new Vector2(0.5f, 0f);
            rootRect.anchorMax = new Vector2(0.5f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.anchoredPosition = new Vector2(0f, 120f);
            rootRect.sizeDelta = new Vector2(600f, 80f);

            if (showText)
            {
                var textObject = new GameObject("Percent", typeof(Text));
                textObject.layer = uiLayer;
                var textRect = (RectTransform)textObject.transform;
                textRect.SetParent(rootRect, false);
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(0f, 24f);
                textRect.offsetMax = Vector2.zero;

                progressText = textObject.GetComponent<Text>();
                progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                progressText.fontSize = 32;
                progressText.alignment = TextAnchor.MiddleCenter;
                progressText.color = progressColor;
                progressText.raycastTarget = false;
                progressText.text = "0%";
            }

            if (showBar)
            {
                var barObject = new GameObject("Bar", typeof(Image));
                barObject.layer = uiLayer;
                var barRect = (RectTransform)barObject.transform;
                barRect.SetParent(rootRect, false);
                barRect.anchorMin = new Vector2(0f, 0f);
                barRect.anchorMax = new Vector2(1f, 0f);
                barRect.pivot = new Vector2(0.5f, 0f);
                barRect.sizeDelta = new Vector2(0f, 12f);
                barRect.anchoredPosition = Vector2.zero;

                var barImage = barObject.GetComponent<Image>();
                barImage.color = new Color(progressColor.r, progressColor.g, progressColor.b, 0.2f);
                barImage.raycastTarget = false;

                var fillObject = new GameObject("Fill", typeof(Image));
                fillObject.layer = uiLayer;
                progressBarFill = (RectTransform)fillObject.transform;
                progressBarFill.SetParent(barRect, false);
                progressBarFill.anchorMin = Vector2.zero;
                progressBarFill.anchorMax = new Vector2(0f, 1f);
                progressBarFill.offsetMin = Vector2.zero;
                progressBarFill.offsetMax = Vector2.zero;
                progressBarFill.pivot = new Vector2(0f, 0.5f);

                var fillImage = fillObject.GetComponent<Image>();
                fillImage.color = progressColor;
                fillImage.raycastTarget = false;
            }

            progressRoot.SetActive(false);
        }
    }
}
