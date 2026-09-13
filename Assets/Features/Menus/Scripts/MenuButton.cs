using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Menu button feedback: optional select/press sounds. Drop it on any Unity UI Button.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Menu Button")]
    [RequireComponent(typeof(Button))]
    public class MenuButton : MonoBehaviour,
        ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        [Tooltip("Optional. Played when the button becomes highlighted.")]
        private AudioClip selectSound;

        [SerializeField]
        [Tooltip("Optional. Played when the button is pressed.")]
        private AudioClip pressSound;

        private Button button;
        private bool highlighted;
        private bool pointerOver;
        private bool selected;

        private void Awake()
        {
            button = GetComponent<Button>();
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
        }

        private void PlayPressSound() => Play(pressSound);

        // Hover and EventSystem selection highlight independently; play the select sound once when
        // the button first becomes highlighted by either.
        private void UpdateHighlight()
        {
            bool value = pointerOver || selected;
            if (highlighted == value)
                return;
            highlighted = value;

            if (value)
                Play(selectSound);
        }

        private static void Play(AudioClip clip)
        {
            UiSounds.Play(clip);
        }
    }
}
