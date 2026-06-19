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
    /// </summary>
    [RequireComponent(typeof(Mover))]
    [DisallowMultipleComponent]
    public class NpcController : MonoBehaviour
    {
        [SerializeField, Tooltip("Which movement mode the NPC starts in.")]
        private NpcWalkMode startMode = NpcWalkMode.Wander;

        /// <summary>The mode currently active.</summary>
        public NpcWalkMode CurrentMode { get; private set; }

        private void Start() => SetWalkMode(startMode);

        /// <summary>Switches the active movement mode: enables that mode's driver, disables the rest.</summary>
        public void SetWalkMode(NpcWalkMode mode)
        {
            CurrentMode = mode;

            // One line per mode — each driver is enabled only when its mode is selected.
            SetDriver<Wander>(mode == NpcWalkMode.Wander);
            // Later, e.g.:  SetDriver<Patrol>(mode == NpcWalkMode.Patrol);
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
    }
}
