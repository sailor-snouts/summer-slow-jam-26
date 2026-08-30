using UnityEngine;

namespace Game
{
    /// <summary>
    /// Base for a scene thing that shows a sprite taken from a data asset and keeps a
    /// <see cref="BoxCollider2D"/> fitted to that sprite - updating live in the editor. Subclasses
    /// supply the sprite through <see cref="CurrentSprite"/> (a <see cref="Character"/> uses its
    /// world sprite, an <see cref="InteractableObject"/> uses its object sprite), so the SpriteRenderer /
    /// collider plumbing lives here once instead of in each.
    /// </summary>
    // ExecuteAlways: refresh the sprite in edit mode too.
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class SpriteEntity : MonoBehaviour
    {
        [Tooltip("Resize the BoxCollider2D to match the sprite whenever it changes.")]
        [SerializeField] private bool fitColliderToSprite = true;

        private SpriteRenderer spriteRenderer;
        private BoxCollider2D box;

        /// <summary>The sprite to display - supplied by the subclass from its data asset.</summary>
        protected abstract Sprite CurrentSprite { get; }

        protected virtual void OnEnable() => RefreshSprite();

        // Fires in the editor when this component loads or its data changes. Setting the sprite here
        // directly triggers a SendMessage (bounds-changed), which Unity forbids during OnValidate -
        // so defer the refresh to just after validation completes.
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += DeferredRefresh;
#endif
        }

#if UNITY_EDITOR
        private void DeferredRefresh()
        {
            if (this == null) // may have been destroyed between OnValidate and this callback
                return;
            RefreshSprite();
        }
#endif

        /// <summary>Shows <see cref="CurrentSprite"/> on this object's SpriteRenderer and refits the collider.</summary>
        protected void RefreshSprite()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                return;

            Sprite sprite = CurrentSprite;
            spriteRenderer.sprite = sprite;

            // Keep the collider matching the sprite, so it actually has a shape to block/hit with.
            if (fitColliderToSprite && sprite != null)
            {
                if (box == null)
                    box = GetComponent<BoxCollider2D>();
                if (box != null)
                {
                    box.size = sprite.bounds.size;
                    box.offset = sprite.bounds.center;
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor only: re-reads the sprite on every <see cref="SpriteEntity"/> in the loaded scenes.
        /// A data ScriptableObject calls this from its OnValidate, because editing an asset (e.g.
        /// swapping its sprite) does NOT fire OnValidate on the components that reference it - so they'd
        /// otherwise stay stale until reselected. Deferred, like the per-object refresh, since setting a
        /// sprite sends bounds messages that Unity forbids mid-validation.
        /// </summary>
        public static void RefreshAllInEditor()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                SpriteEntity[] entities = FindObjectsByType<SpriteEntity>(FindObjectsInactive.Include);
                foreach (SpriteEntity entity in entities)
                    if (entity != null)
                        entity.RefreshSprite();
                // Force the Scene/Game views to redraw so the swapped sprite shows immediately.
                UnityEditor.SceneView.RepaintAll();
            };
        }
#endif
    }
}
