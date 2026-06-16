using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Menu button feedback: a scale pop while highlighted (mouse hover or
    /// keyboard/controller focus) and optional select/press sounds. Drop it on
    /// any Unity UI Button.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Menu Button")]
    [RequireComponent(typeof(Button))]
    public class MenuButton : MonoBehaviour,
        ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        [Tooltip("Scale multiplier applied while the button is highlighted.")]
        private float highlightScale = 1.1f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Seconds to animate the scale change.")]
        private float scaleDuration = 0.12f;

        [SerializeField]
        [Tooltip("Optional. Played when the button becomes highlighted.")]
        private AudioClip selectSound;

        [SerializeField]
        [Tooltip("Optional. Played when the button is pressed.")]
        private AudioClip pressSound;

        private RectTransform rect;
        private Button button;
        private Vector3 baseScale;
        private Coroutine scaleRoutine;
        private bool highlighted;
        private bool pointerOver;
        private bool selected;

        private void Awake()
        {
            rect = (RectTransform)transform;
            button = GetComponent<Button>();
            baseScale = rect.localScale;
            button.onClick.AddListener(PlayPressSound);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(PlayPressSound);
        }

        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
            UpdateHighlight();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
            UpdateHighlight();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerOver = true;
            UpdateHighlight();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerOver = false;
            UpdateHighlight();
        }

        private void OnDisable()
        {
            // Selection/hover events won't pair up across a deactivation; reset.
            pointerOver = false;
            selected = false;
            highlighted = false;
            if (rect != null)
                rect.localScale = baseScale;
        }

        private void PlayPressSound() => Play(pressSound);

        // Hover and EventSystem selection highlight independently — un-hovering
        // must not visually drop a button that is still the current selection.
        private void UpdateHighlight()
        {
            bool value = pointerOver || selected;
            if (highlighted == value)
                return;
            highlighted = value;

            if (value)
                Play(selectSound);

            if (scaleRoutine != null)
                StopCoroutine(scaleRoutine);

            Vector3 target = value ? baseScale * highlightScale : baseScale;
            if (!isActiveAndEnabled)
            {
                // Deselect can arrive after deactivation; coroutines can't start then.
                rect.localScale = target;
                return;
            }

            scaleRoutine = StartCoroutine(ScaleTo(target));
        }

        private static void Play(AudioClip clip)
        {
            UiSounds.Play(clip);
        }

        private IEnumerator ScaleTo(Vector3 target)
        {
            Vector3 start = rect.localScale;
            float elapsed = 0f;
            while (elapsed < scaleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                rect.localScale = Vector3.Lerp(start, target, elapsed / scaleDuration);
                yield return null;
            }
            rect.localScale = target;
        }
    }
}
