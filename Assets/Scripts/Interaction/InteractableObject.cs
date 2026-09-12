using UnityEngine;

namespace Game
{
    // RequireComponent isn't inherited from SpriteEntity, so restate the parts RefreshSprite needs.
    // ExecuteAlways so the sprite shows in the editor.
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(ObjectDialogue))]
    public class InteractableObject : SpriteEntity
    {
        [Tooltip("Which object this GameObject is.")]
        [SerializeField] private InteractableObjectData data;

        public InteractableObjectData Data => data;

        protected override Sprite CurrentSprite => data != null ? data.Sprite : null;
    }
}
