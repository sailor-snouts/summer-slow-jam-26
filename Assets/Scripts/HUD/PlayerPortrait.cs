using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// Keeps a HUD Image showing the current player character's portrait. The active player can change
    /// during a session - dialogue swaps which character you are, and each scene loads its own player
    /// instance - so this watches <see cref="PlayerCharacter.CurrentData"/> and updates the Image only
    /// when it actually changes. Put it on the HUD portrait object (or anywhere and assign the Image).
    /// </summary>
    public class PlayerPortrait : MonoBehaviour
    {
        [SerializeField, Tooltip("Image that shows the portrait. Defaults to an Image on this object.")]
        private Image image;

        [SerializeField, Tooltip("Hide the Image when there is no active player character.")]
        private bool hideWhenNone = true;

        // The character whose portrait is currently shown, so we skip work when nothing changed.
        private CharacterData shown;

        private void Awake()
        {
            if (image == null)
                image = GetComponent<Image>();
        }

        // Force a refresh whenever we're enabled (e.g. after a scene load re-shows the HUD).
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
                return; // same character as last frame - nothing to do

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
