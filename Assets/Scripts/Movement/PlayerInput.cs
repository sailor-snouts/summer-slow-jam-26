using JamTemplate.Menus;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    public static class PlayerInput
    {
        private static int manualLocks;

        public static bool Locked =>
            manualLocks > 0 || DialogueManager.isConversationActive || MenuSceneRouter.HasOpenOverlay;

        public static void Lock() => manualLocks++;

        public static void Unlock() => manualLocks = Mathf.Max(0, manualLocks - 1);

        public static void ClearManualLocks() => manualLocks = 0;

        // Reset on play start so the count can't leak across sessions when domain reload is disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => manualLocks = 0;
    }
}
