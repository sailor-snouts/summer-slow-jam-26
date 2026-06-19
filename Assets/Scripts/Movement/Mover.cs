using UnityEngine;

namespace Game
{
    /// <summary>
    /// The ability to move — owns the Rigidbody2D and turns a desired direction into physics
    /// movement (so walls and other characters block it), and flips the sprite to face that
    /// direction. It does NOT decide where to go: a driver (a <see cref="PlayerController"/> for
    /// the player, an AI like <see cref="Wander"/> for an NPC) sets <see cref="MoveDirection"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class Mover : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 5f;

        [Header("Facing")]
        [SerializeField, Tooltip("Tick if the sprite art faces RIGHT by default.")]
        private bool spriteFacesRight = true;

        [SerializeField, Min(0f), Tooltip("Ignore horizontal movement smaller than this when flipping.")]
        private float facingDeadzone = 0.01f;

        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;

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

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>(); // optional — null is fine
        }

        // Sensible top-down defaults when the Rigidbody2D is first added with this component.
        private void Reset()
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;                                   // top-down: don't fall
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // don't spin on impact
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // avoid tunneling
        }

        private void FixedUpdate()
        {
            // Drive velocity (not transform) so the physics engine resolves collisions.
            body.linearVelocity = Vector2.ClampMagnitude(MoveDirection, 1f) * moveSpeed;
            UpdateFacing();
        }

        private void UpdateFacing()
        {
            if (spriteRenderer == null)
                return;

            float x = MoveDirection.x;
            if (Mathf.Abs(x) < facingDeadzone)
                return; // idle or moving straight up/down: keep the current facing

            // flipX is true when we must mirror the art from its default orientation.
            spriteRenderer.flipX = (x > 0f) != spriteFacesRight;
        }
    }
}
