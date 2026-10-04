using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ControlHintView : MonoBehaviour
    {
        [SerializeField, Tooltip("Shows the verb text, e.g. 'to talk'.")]
        private TMP_Text label;

        [SerializeField, Tooltip("Prefix put before the verb.")]
        private string format = "to {0}";

        [Header("Key icon animation")]
        [SerializeField, Tooltip("Image that plays the key-press animation.")]
        private Image keyIcon;

        [SerializeField, Tooltip("Frames of the key animation, played in order and looped.")]
        private Sprite[] keyFrames;

        [SerializeField, Min(0f), Tooltip("Animation speed in frames per second.")]
        private float framesPerSecond = 8f;

        [Header("Fade")]
        [SerializeField, Min(0f), Tooltip("Seconds to fade in/out.")]
        private float fadeDuration = 0.12f;

        private CanvasGroup group;
        private float targetAlpha;
        private float frameTimer;
        private int frame;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false; // a hint must not intercept clicks
            Apply(PlayerInteractor.CurrentVerb);
        }

        private void OnEnable()
        {
            PlayerInteractor.HintChanged += Apply;
            Apply(PlayerInteractor.CurrentVerb);
        }

        private void OnDisable() => PlayerInteractor.HintChanged -= Apply;

        private void Apply(string verb)
        {
            bool show = !string.IsNullOrEmpty(verb);
            if (show && label != null)
                label.text = string.Format(format, verb);

            // Restart the key animation each time the hint appears.
            if (show && targetAlpha == 0f)
            {
                frame = 0;
                frameTimer = 0f;
                ShowFrame();
            }
            targetAlpha = show ? 1f : 0f;
        }

        private void Update()
        {
            if (group.alpha > 0f)
                Animate();

            if (!Mathf.Approximately(group.alpha, targetAlpha))
            {
                float step = fadeDuration > 0f ? Time.unscaledDeltaTime / fadeDuration : 1f;
                group.alpha = Mathf.MoveTowards(group.alpha, targetAlpha, step);
            }
        }

        private void Animate()
        {
            if (keyIcon == null || keyFrames == null || keyFrames.Length < 2 || framesPerSecond <= 0f)
                return;

            frameTimer += Time.unscaledDeltaTime;
            float frameLength = 1f / framesPerSecond;
            while (frameTimer >= frameLength)
            {
                frameTimer -= frameLength;
                frame = (frame + 1) % keyFrames.Length;
                ShowFrame();
            }
        }

        private void ShowFrame()
        {
            if (keyIcon != null && keyFrames != null && keyFrames.Length > 0)
                keyIcon.sprite = keyFrames[Mathf.Clamp(frame, 0, keyFrames.Length - 1)];
        }
    }
}
