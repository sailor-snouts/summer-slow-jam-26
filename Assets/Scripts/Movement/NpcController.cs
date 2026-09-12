using UnityEngine;

namespace Game
{
    public enum NpcWalkMode
    {
        Wander,

        None,
    }

    [RequireComponent(typeof(Mover))]
    [DisallowMultipleComponent]
    public class NpcController : MonoBehaviour
    {
        [SerializeField, Tooltip("Which movement mode the NPC starts in.")]
        private NpcWalkMode startMode = NpcWalkMode.Wander;

        public NpcWalkMode CurrentMode { get; private set; }

        public bool IsFrozen { get; private set; }

        private Mover mover;

        private void Awake() => mover = GetComponent<Mover>();

        private void Start() => SetWalkMode(startMode);

        public void SetWalkMode(NpcWalkMode mode)
        {
            CurrentMode = mode;
            ApplyDrivers();
        }

        public void SetFrozen(bool value)
        {
            IsFrozen = value;
            ApplyDrivers();
        }

        private void ApplyDrivers()
        {
            bool wander = !IsFrozen && CurrentMode == NpcWalkMode.Wander;
            SetDriver<Wander>(wander);

            // With no driver feeding it, stop the Mover dead so it doesn't coast on its last heading.
            if (mover != null && !wander)
                mover.MoveDirection = Vector2.zero;
        }

        private void SetDriver<T>(bool active) where T : MonoBehaviour
        {
            var driver = GetComponent<T>();
            if (driver != null)
                driver.enabled = active;
            else if (active)
                Debug.LogWarning(
                    $"[NpcController] '{name}' is set to a mode that needs a {typeof(T).Name} " +
                    "component, but none is attached.", this);
        }

        // Invoked by the Dialogue System via SendMessage on conversation participants, so the names must match exactly.
        private void OnConversationStart(Transform actor) => SetFrozen(true);
        private void OnConversationEnd(Transform actor) => SetFrozen(false);
    }
}
