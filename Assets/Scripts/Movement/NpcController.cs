using UnityEngine;

namespace Game
{
    /// <summary>The NPC's available movement modes. Add more as we build them (Patrol, Follow, …).</summary>
    public enum NpcWalkMode
    {
        Wander,
    }

    /// <summary>
    /// Drives an NPC by choosing which movement "brain" is active. Pick the starting mode in the
    /// Inspector; the controller enables that mode's driver component and disables the others, so
    /// exactly one is feeding the <see cref="Mover"/>. Call <see cref="SetWalkMode"/> to switch
    /// at runtime. Add the driver components you want to use (e.g. <see cref="Wander"/>) alongside this.
    ///
    /// It also stops the NPC while the player is in conversation with it (see
    /// <see cref="SetFrozen"/> / the OnConversation* hooks) so an NPC never wanders off mid-dialogue.
    /// </summary>
    [RequireComponent(typeof(Mover))]
    [DisallowMultipleComponent]
    public class NpcController : MonoBehaviour
    {
        [SerializeField, Tooltip("Which movement mode the NPC starts in.")]
        private NpcWalkMode startMode = NpcWalkMode.Wander;

        /// <summary>The mode currently active.</summary>
        public NpcWalkMode CurrentMode { get; private set; }

        /// <summary>True while movement is frozen (e.g. during a conversation with this NPC).</summary>
        public bool IsFrozen { get; private set; }

        private Mover mover;

        private void Awake() => mover = GetComponent<Mover>();

        private void Start() => SetWalkMode(startMode);

        /// <summary>Switches the active movement mode: enables that mode's driver, disables the rest.</summary>
        public void SetWalkMode(NpcWalkMode mode)
        {
            CurrentMode = mode;
            ApplyDrivers();
        }

        /// <summary>
        /// Freezes or resumes movement. While frozen, every driver is disabled and the Mover is
        /// stopped this frame (not just left coasting on its last direction). Resuming restores the
        /// current <see cref="CurrentMode"/>'s driver.
        /// </summary>
        public void SetFrozen(bool value)
        {
            IsFrozen = value;
            ApplyDrivers();
            if (IsFrozen && mover != null)
                mover.MoveDirection = Vector2.zero; // stop dead, don't coast on the last heading
        }

        // Enable a driver only when it's the current mode AND we're not frozen.
        private void ApplyDrivers()
        {
            SetDriver<Wander>(!IsFrozen && CurrentMode == NpcWalkMode.Wander);
            // Later, e.g.:  SetDriver<Patrol>(!IsFrozen && CurrentMode == NpcWalkMode.Patrol);
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

        // The Dialogue System sends these to a conversation's participants (via SendMessage). The NPC
        // is the conversant, so it gets them when the player starts/ends talking to it — freeze while
        // the conversation is up, resume when it ends or is exited. Plain named methods: no Dialogue
        // System reference needed.
        private void OnConversationStart(Transform actor) => SetFrozen(true);
        private void OnConversationEnd(Transform actor) => SetFrozen(false);
    }
}
