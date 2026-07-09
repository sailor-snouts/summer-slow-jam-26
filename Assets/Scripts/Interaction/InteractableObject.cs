using UnityEngine;

namespace Game
{
    /// <summary>
    /// A scene object the player can interact with (examine). Pick which object this GameObject is
    /// with the <see cref="data"/> selector (an <see cref="InteractableObjectData"/> asset); it shows
    /// that object's sprite and keeps the collider fitted to it. Pair it with an
    /// <see cref="ObjectDialogue"/> to start the object's conversation on interaction.
    ///
    /// It's the counterpart to a <see cref="Character"/> - but with no stats, no portrait, and no
    /// movement (objects don't move, so no <c>Mover</c> is required).
    /// </summary>
    // RequireComponent isn't inherited from SpriteEntity, so restate it here to auto-add the parts
    // RefreshSprite needs (and ExecuteAlways so the sprite shows in the editor). Also pull in
    // ObjectDialogue - the interactable half - so an object is never missing what makes it usable.
    // (ObjectDialogue requires InteractableObject too, so adding either gives you both.)
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(ObjectDialogue))]
    public class InteractableObject : SpriteEntity
    {
        [Tooltip("Which object this GameObject is.")]
        [SerializeField] private InteractableObjectData data;

        /// <summary>The selected object definition (sprite, conversation).</summary>
        public InteractableObjectData Data => data;

        protected override Sprite CurrentSprite => data != null ? data.Sprite : null;
    }
}
