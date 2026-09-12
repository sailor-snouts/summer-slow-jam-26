using UnityEngine;

namespace Game
{
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

        protected abstract Sprite CurrentSprite { get; }

        protected virtual void OnEnable() => RefreshSprite();

        // Setting the sprite triggers a SendMessage (bounds-changed) that Unity forbids during
        // OnValidate - defer the refresh to just after validation completes.
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

        protected void RefreshSprite()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                return;

            spriteRenderer.sprite = CurrentSprite;
        }

        // Only writes sortingOrder when it changes, so it does not dirty the scene every frame. Sorts
        // within this renderer's sorting layer - keep ground/background art on a lower layer so it
        // stays behind.
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
