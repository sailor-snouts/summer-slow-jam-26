using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    // Remembers which actors have despawned this play session (by id), so they stay gone when the
    // player leaves and returns to a scene. Cleared on play start (domain reload may be disabled).
    public static class SessionDespawns
    {
        private static readonly HashSet<string> despawned = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => despawned.Clear();

        public static bool IsDespawned(string id) => !string.IsNullOrEmpty(id) && despawned.Contains(id);

        public static void MarkDespawned(string id)
        {
            if (!string.IsNullOrEmpty(id))
                despawned.Add(id);
        }
    }
}
