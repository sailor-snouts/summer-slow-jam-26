using UnityEngine;

namespace Game
{
    public static class SceneTransition
    {
        public static string PendingEntrance { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => PendingEntrance = null;
    }
}
