using UnityEngine;

namespace Game
{
    // A scene collider that movers are kept inside. Put it on a GameObject with a FILLED Collider2D
    // (Box, Circle, Polygon, Capsule, or a CompositeCollider2D set to Polygons) - an EdgeCollider2D
    // has no interior and won't work. One per scene; the active one registers itself.
    //
    // The collider MUST be a trigger: it only defines the region (via ClosestPoint). A solid collider
    // here would be read as a wall by the Mover and, since movers start inside it, freeze them in place.
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class WalkableArea : MonoBehaviour
    {
        public static WalkableArea Current { get; private set; }

        private Collider2D area;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => Current = null;

        private void Awake()
        {
            area = GetComponent<Collider2D>();
            if (area != null)
                area.isTrigger = true; // never block movement; only define the region
        }

        private void Reset()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnEnable() => Current = this;
        private void OnDisable() { if (Current == this) Current = null; }

        // Nearest point inside the area to p - returns p unchanged when it's already inside.
        public Vector2 Clamp(Vector2 p) => area != null ? area.ClosestPoint(p) : p;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
                return;
            Gizmos.color = new Color(0.4f, 1f, 0.6f, 0.08f);
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = new Color(0.4f, 1f, 0.6f, 0.6f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
#endif
    }
}
