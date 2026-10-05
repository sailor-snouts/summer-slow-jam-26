using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    [RequireComponent(typeof(Mover))]
    [DisallowMultipleComponent]
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField, Range(0f, 5f), Tooltip("How far (world units) the cast reaches in the facing direction.")]
        private float interactRange = 1.5f;

        [SerializeField, Range(0f, 2f), Tooltip("Radius of the swept circle - a fatter sweep is more forgiving to aim.")]
        private float castRadius = 0.4f;

        [SerializeField, Tooltip("Which layers hold interactables.")]
        private LayerMask interactableLayers = ~0;

        [SerializeField, Tooltip("Key that triggers an interaction.")]
        private Key interactKey = Key.F;

        private Mover mover;
        private ContactFilter2D filter;
        private readonly RaycastHit2D[] hits = new RaycastHit2D[16];

        // The interact verb to hint right now ("talk"), or null when nothing is in reach. The key is
        // drawn as an animated icon in the HUD, so only the verb travels here. Raised when it changes.
        public static string CurrentVerb { get; private set; }
        public static event System.Action<string> HintChanged;

        private void Awake()
        {
            mover = GetComponent<Mover>();
            filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(interactableLayers);
        }

        private void Update()
        {
            UpdateHint();

            if (PlayerInput.Locked)
                return; // can't start another interaction while a conversation is up

            if (Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
                FindBest()?.Interact(transform);
        }

        private void OnDisable() => SetHint(null);

        private void UpdateHint()
        {
            IInteractable best = PlayerInput.Locked ? null : FindBest();
            SetHint(best != null ? best.InteractVerb : null);
        }

        private static void SetHint(string verb)
        {
            if (verb == CurrentVerb)
                return;
            CurrentVerb = verb;
            HintChanged?.Invoke(verb);
        }

        // Cast in the facing direction and take the first interactable the sweep reaches.
        private IInteractable FindBest()
        {
            Vector2 facing = mover.Facing;
            if (facing.sqrMagnitude < 1e-6f)
                return null; // no facing established yet
            facing.Normalize();

            int count = Physics2D.CircleCast(transform.position, castRadius, facing, filter, hits, interactRange);

            IInteractable first = null;
            float firstDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit.collider == null || hit.collider.transform == transform)
                    continue;

                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable == null)
                    continue;

                if (hit.distance < firstDistance) // first (nearest) hit along the cast
                {
                    first = interactable;
                    firstDistance = hit.distance;
                }
            }

            return first;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector2 facing = (Application.isPlaying && mover != null) ? mover.Facing : Vector2.down;
            if (facing.sqrMagnitude < 1e-6f)
                facing = Vector2.down;
            facing.Normalize();

            Vector2 start = transform.position;
            Vector2 end = start + facing * interactRange;

            bool active = Application.isPlaying && !string.IsNullOrEmpty(CurrentVerb);
            Gizmos.color = active ? Color.green : new Color(1f, 1f, 0f, 0.6f);
            float r = Mathf.Max(0.05f, castRadius);
            Vector2 side = new Vector2(-facing.y, facing.x) * r;

            Gizmos.DrawWireSphere(start, r);
            Gizmos.DrawWireSphere(end, r);
            Gizmos.DrawLine(start + side, end + side);
            Gizmos.DrawLine(start - side, end - side);
        }
#endif
    }
}
