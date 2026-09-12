using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class Mover : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 5f;

        [Header("Collision")]
        [SerializeField, Tooltip("Layers that block movement - walls and other characters.")]
        private LayerMask blockingLayers = ~0;

        // Small gap kept from surfaces so the cast doesn't start already overlapping.
        private const float Skin = 0.02f;

        private Rigidbody2D body;
        private ContactFilter2D filter;
        private readonly RaycastHit2D[] hits = new RaycastHit2D[8];

        public Vector2 MoveDirection { get; set; }

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        public Vector2 Facing { get; private set; } = Vector2.down;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic; // we move it ourselves; no physics push

            filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(blockingLayers);
        }

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

        // Two passes so the leftover can slide into a corner instead of sticking.
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
            if (MoveDirection.sqrMagnitude > 1e-6f)
                Facing = MoveDirection.normalized;
        }
    }
}
