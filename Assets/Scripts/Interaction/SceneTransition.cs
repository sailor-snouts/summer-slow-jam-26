using UnityEngine;

namespace Game
{
    /// <summary>
    /// Carries which entrance the player should appear at across a scene load. A
    /// <see cref="SceneExitTrigger"/> sets <see cref="PendingEntrance"/> just before loading; the
    /// arriving <see cref="PlayerCharacter"/> reads it, moves to the matching <see cref="SceneEntrance"/>,
    /// then clears it. Empty means "no specific entrance" - the player stays at the position the target
    /// scene authored. Static so it survives the load; reset on play start so it can't leak between
    /// sessions (works with domain reload disabled too).
    /// </summary>
    public static class SceneTransition
    {
        /// <summary>Id of the entrance the next-loaded scene should place the player at (null/empty = none).</summary>
        public static string PendingEntrance { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => PendingEntrance = null;
    }
}
