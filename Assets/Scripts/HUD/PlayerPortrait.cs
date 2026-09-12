using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class PlayerPortrait : MonoBehaviour
    {
        [SerializeField, Tooltip("Image that shows the portrait. Defaults to an Image on this object.")]
        private Image image;

        [SerializeField, Tooltip("Hide the Image when there is no active player character.")]
        private bool hideWhenNone = true;

        private CharacterData shown;

        private void Awake()
        {
            if (image == null)
                image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            shown = null;
            Refresh();
        }

        private void Update() => Refresh();

        private void Refresh()
        {
            CharacterData current = PlayerCharacter.CurrentData;
            if (current == shown)
                return;

            shown = current;
            Sprite portrait = current != null ? current.ProfilePicture : null;

            if (image == null)
                return;
            image.sprite = portrait;
            if (hideWhenNone)
                image.enabled = portrait != null;
        }
    }
}
