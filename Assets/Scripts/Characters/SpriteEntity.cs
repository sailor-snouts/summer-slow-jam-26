using UnityEngine;

namespace Game
{
    /// <summary>
    /// Base for a scene thing that shows a sprite taken from a data asset, updating live in the editor.
    /// Subclasses supply the sprite through <see cref="CurrentSprite"/> (a <see cref="Character"/> uses
    /// its world sprite, an <see cref="InteractableObject"/> uses its object sprite), so the
    /// SpriteRenderer plumbing lives here once instead of in each.
    /// </summary>
    // ExecuteAlways: refresh the sprite in edit mode too.
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class SpriteEntity : MonoBehaviour
    {
        [Header("Depth sorting")]
        [Tooltip("Draw a lower world Y on top of a higher one (pseudo top-down depth), by setting sortingOrder from Y.")]
        [SerializeField] private bool sortByYPosition = true;

        [Tooltip("Y sorting granularity: sortingOrder = round(-y * this). Higher gives finer steps.")]
        [SerializeField] private int sortPrecision = 100;

        private SpriteRenderer spriteRenderer;

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

        /// <summary>Shows <see cref="CurrentSprite"/> on this object's SpriteRenderer.</summary>
        protected void RefreshSprite()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                return;

            spriteRenderer.sprite = CurrentSprite;
        }

        // Pseudo top-down depth: a lower world Y draws on top of a higher one. Runs after movement
        // (LateUpdate) and in the editor (ExecuteAlways); only writes sortingOrder when it changes, so
        // it does not dirty the scene every frame. Sorts within this renderer's sorting layer - keep
        // ground/background art on a lower layer (or a very low Order in Layer) so it stays behind.
        private void LateUpdate()
        {
            if (!sortByYPosition)
                return;
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                return;

            int order = Mathf.RoundToInt(-transform.position.y * sortPrecision);
            if (spriteRenderer.sortingOrder != order)
                spriteRenderer.sortingOrder = order;
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
