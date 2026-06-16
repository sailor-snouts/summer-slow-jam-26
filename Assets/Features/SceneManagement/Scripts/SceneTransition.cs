using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.SceneManagement
{
    /// <summary>
    /// Covers and reveals the screen with a configurable transition effect.
    /// Builds its own full-screen overlay canvas at runtime. This is the visual
    /// half of the system; scene loading lives in <see cref="SceneLoader"/>.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Scene Transition")]
    [DisallowMultipleComponent]
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Colour of the covering overlay.")]
        private Color color = Color.black;

        [SerializeReference]
        [Tooltip("The transition animation. Defaults to a fade.")]
        private SceneTransitionEffect effect = new FadeTransitionEffect();

        private CanvasGroup canvasGroup;
        private RectTransform panel;
        private Image panelImage;
        private RectTransform canvasRect;
        private SceneTransitionSurface surface;
        private bool built;

        /// <summary>Root of the overlay canvas; parent custom loading UI here.</summary>
        public RectTransform OverlayRoot
        {
            get
            {
                EnsureBuilt();
                return canvasRect;
            }
        }

        public bool IsCovered { get; private set; }

        /// <summary>Duration of the cover animation, from the active effect.</summary>
        public float CoverDuration => Effect.coverDuration;

        /// <summary>Duration of the reveal animation, from the active effect.</summary>
        public float RevealDuration => Effect.revealDuration;

        private SceneTransitionEffect Effect => effect ??= new FadeTransitionEffect();

        private void Awake()
        {
            EnsureBuilt();
        }

        /// <summary>Builds the overlay canvas if it does not exist yet (idempotent).</summary>
        public void EnsureBuilt()
        {
            if (built)
                return;
            built = true;

            BuildOverlay();
            Effect.ResetState(surface);
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>Animates the overlay to fully cover the screen.</summary>
        public IEnumerator Cover()
        {
            EnsureBuilt();
            if (IsCovered)
                yield break;
            panelImage.color = color;
            canvasGroup.blocksRaycasts = true;
            yield return Animate(0f, 1f, Effect.coverDuration);
            IsCovered = true;
        }

        /// <summary>Animates the overlay away to reveal the current scene.</summary>
        public IEnumerator Reveal()
        {
            EnsureBuilt();
            if (!IsCovered)
                yield break;
            yield return Animate(1f, 0f, Effect.revealDuration);
            IsCovered = false;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>Instantly puts the overlay in its fully-covered state, no animation.</summary>
        public void SnapCovered()
        {
            EnsureBuilt();
            panelImage.color = color;
            Effect.Apply(surface, 1f);
            canvasGroup.blocksRaycasts = true;
            IsCovered = true;
        }

        private IEnumerator Animate(float from, float to, float duration)
        {
            SceneTransitionEffect e = Effect;
            if (duration <= 0f)
            {
                e.Apply(surface, to);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float linear = Mathf.Clamp01(elapsed / duration);
                float eased = e.curve != null ? e.curve.Evaluate(linear) : linear;
                e.Apply(surface, Mathf.LerpUnclamped(from, to, eased));
                yield return null;
            }

            e.Apply(surface, to);
        }

        private void BuildOverlay()
        {
            int uiLayer = LayerMask.NameToLayer("UI");

            var canvasObject = new GameObject("Scene Transition Overlay",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            canvasObject.layer = uiLayer;
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGroup = canvasObject.GetComponent<CanvasGroup>();
            canvasRect = (RectTransform)canvasObject.transform;

            var panelObject = new GameObject("Panel", typeof(Image));
            panelObject.layer = uiLayer;
            panel = (RectTransform)panelObject.transform;
            panel.SetParent(canvasObject.transform, false);
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            panel.pivot = new Vector2(0.5f, 0.5f);

            panelImage = panelObject.GetComponent<Image>();
            panelImage.color = color;
            panelImage.raycastTarget = true;

            surface = new SceneTransitionSurface(canvasGroup, panel, canvasRect);
        }
    }
}
