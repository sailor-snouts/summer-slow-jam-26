using UnityEngine;

namespace Game
{
    /// <summary>
    /// The ability to move. Drives a kinematic Rigidbody2D by casting its collider ahead each
    /// physics step and stopping at (and sliding along) anything on the blocking layers — walls
    /// and other characters. Because it never applies forces, nothing gets pushed: characters
    /// block each other and the walls, but can't shove anything. Also flips the sprite to face
    /// the move direction. A driver (<see cref="PlayerController"/> or an AI like
    /// <see cref="Wander"/>) sets <see cref="MoveDirection"/>; this decides nothing about where to go.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class Mover : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 5f;

        [Header("Collision")]
        [SerializeField, Tooltip("Layers that block movement — walls and other characters.")]
        private LayerMask blockingLayers = ~0;

        [Header("Facing")]
        [SerializeField, Tooltip("Tick if the sprite art faces RIGHT by default.")]
        private bool spriteFacesRight = true;

        [SerializeField, Min(0f), Tooltip("Ignore horizontal movement smaller than this when flipping.")]
        private float facingDeadzone = 0.01f;

        // Small gap kept from surfaces so the cast doesn't start already overlapping.
        private const float Skin = 0.02f;

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private ContactFilter2D filter;
        private readonly RaycastHit2D[] hits = new RaycastHit2D[8];

        /// <summary>
        /// Desired movement direction, set by a driver (input or AI). Magnitude is clamped to 1,
        /// so a value straight from a stick or a normalized direction both work; (0,0) = stop.
        /// </summary>
        public Vector2 MoveDirection { get; set; }

        /// <summary>Units per second at full input.</summary>
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        /// <summary>The last non-zero move direction (normalized), kept while idle — i.e. which way it's facing.</summary>
        public Vector2 Facing { get; private set; } = Vector2.right;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            body.bodyType = RigidbodyType2D.Kinematic; // we move it ourselves; no physics push

            filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(blockingLayers);
        }

        // Top-down defaults when the Rigidbody2D is first added with this component.
        private void Reset()
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void FixedUpdate()
        {
            Vector2 velocity = Vector2.ClampMagnitude(MoveDirection, 1f) * moveSpeed;
            Vector2 delta = velocity * Time.fixedDeltaTime;
            if (delta != Vector2.zero)
                body.MovePosition(body.position + CollideAndSlide(delta));

            UpdateFacing();
        }

        // Move by delta, but stop at any blocking collider and slide the leftover along its
        // surface. Two passes handles sliding into a corner. Never applies force.
        private Vector2 CollideAndSlide(Vector2 delta)
        {
            Vector2 moved = Vector2.zero;
            Vector2 remaining = delta;

            for (int pass = 0; pass < 2 && remaining.sqrMagnitude > 1e-8f; pass++)
            {
                float distance = remaining.magnitude;
                Vector2 dir = remaining / distance;

                int count = body.Cast(dir, filter, hits, distance + Skin);
                if (count == 0)
                {
                    moved += remaining;
                    break;
                }

                RaycastHit2D nearest = hits[0];
                for (int h = 1; h < count; h++)
                    if (hits[h].distance < nearest.distance)
                        nearest = hits[h];

                float allowed = Mathf.Max(0f, nearest.distance - Skin);
                moved += dir * allowed;

                // Project the leftover onto the surface so we slide instead of sticking.
                Vector2 leftover = remaining - dir * allowed;
                remaining = leftover - Vector2.Dot(leftover, nearest.normal) * nearest.normal;
            }

            return moved;
        }

        private void UpdateFacing()
        {
            // Remember the last real heading so we keep facing it while idle.
            if (MoveDirection.sqrMagnitude > 1e-6f)
                Facing = MoveDirection.normalized;

            if (spriteRenderer == null)
                return;

            float x = MoveDirection.x;
            if (Mathf.Abs(x) < facingDeadzone)
                return; // moving straight up/down: keep the current left/right flip

            spriteRenderer.flipX = (x > 0f) != spriteFacesRight;
        }
    }
}
