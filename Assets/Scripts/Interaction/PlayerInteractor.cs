using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// On a key press, sweeps a circle in the player's movement/facing direction (like an Unreal
    /// sphere-sweep): the circle begins at <see cref="startDistance"/> in front of the player and
    /// travels to <see cref="maxDistance"/>. The first <see cref="IInteractable"/> it hits is used —
    /// so you must be facing the target and within the band to interact with it.
    /// </summary>
    [RequireComponent(typeof(Mover))]
    [DisallowMultipleComponent]
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField, Range(0f, 5f), Tooltip("Where the swept circle begins, in front of the player.")]
        private float startDistance = 0.25f;

        [SerializeField, Range(0f, 5f), Tooltip("Where the swept circle ends.")]
        private float maxDistance = 1.5f;

        [SerializeField, Range(0f, 2f), Tooltip("Radius of the swept circle — a fatter sweep is more forgiving to aim.")]
        private float castRadius = 0.4f;

        [SerializeField, Tooltip("Which layers hold interactables.")]
        private LayerMask interactableLayers = ~0;

        [SerializeField, Tooltip("Key that triggers an interaction.")]
        private Key interactKey = Key.F;

        private Mover mover;
        private ContactFilter2D filter;
        private readonly RaycastHit2D[] hits = new RaycastHit2D[16];

        private void Awake()
        {
            mover = GetComponent<Mover>();
            filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(interactableLayers);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
                TryInteract();
        }

        private void TryInteract()
        {
            Vector2 facing = mover.Facing;
            if (facing.sqrMagnitude < 1e-6f)
                return; // no facing established yet
            facing.Normalize();

            Vector2 castOrigin = (Vector2)transform.position + facing * startDistance;
            float castLength = Mathf.Max(0f, maxDistance - startDistance);

            int count = Physics2D.CircleCast(castOrigin, castRadius, facing, filter, hits, castLength);

            IInteractable best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit.collider == null || hit.collider.transform == transform)
                    continue; // skip ourselves

                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable == null)
                    continue;

                if (hit.distance < bestDistance)
                {
                    best = interactable;
                    bestDistance = hit.distance;
                }
            }

            best?.Interact(transform);
        }

#if UNITY_EDITOR
        // Editor-only reticle: draws the swept circle (two ends + the line it travels) in the Scene
        // view. Yellow = nothing in reach; green + marker = an interactable is hit.
        private void OnDrawGizmos()
        {
            Vector2 facing = (Application.isPlaying && mover != null) ? mover.Facing : Vector2.right;
            if (facing.sqrMagnitude < 1e-6f)
                facing = Vector2.right;
            facing.Normalize();

            Vector2 start = (Vector2)transform.position + facing * startDistance;
            float castLength = Mathf.Max(0f, maxDistance - startDistance);
            Vector2 end = start + facing * castLength;

            Vector2 marker = end;
            bool hitInteractable = false;

            if (Application.isPlaying)
            {
                var f = new ContactFilter2D { useTriggers = true };
                f.SetLayerMask(interactableLayers);
                int count = Physics2D.CircleCast(start, castRadius, facing, f, hits, castLength);
                for (int i = 0; i < count; i++)
                {
                    RaycastHit2D hit = hits[i];
                    if (hit.collider == null || hit.collider.transform == transform)
                        continue;
                    if (hit.collider.GetComponentInParent<IInteractable>() == null)
                        continue;
                    marker = start + facing * hit.distance;
                    hitInteractable = true;
                    break;
                }
            }

            Gizmos.color = hitInteractable ? Color.green : new Color(1f, 1f, 0f, 0.6f);
            float r = Mathf.Max(0.05f, castRadius);
            Vector2 side = new Vector2(-facing.y, facing.x) * r; // perpendicular, scaled to the radius

            Gizmos.DrawWireSphere(start, r);             // where the sweep begins
            Gizmos.DrawWireSphere(end, r);               // where it ends
            Gizmos.DrawLine(start + side, end + side);   // the two edges of the swept circle
            Gizmos.DrawLine(start - side, end - side);
            if (hitInteractable)
                Gizmos.DrawWireSphere(marker, r);
        }
#endif
    }
}
