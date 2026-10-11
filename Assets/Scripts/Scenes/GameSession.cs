using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public static class GameSession
    {
        private const string TitleScene = "Title";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; // safe re-hook when domain reload is disabled
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single && scene.name == TitleScene)
                StartFresh();
        }

        public static void StartFresh()
        {
            Outfits.ClearProgress();
            PlayerCharacter.ClearSession();
            SessionDespawns.Clear();
            AutoStartConversation.ClearPlayed();
            SceneTransition.PendingEntrance = null;
            PlayerInput.ClearManualLocks();
            SkillCheck.DefaultCharacter = null;

            // The Dialogue Manager survives scene loads, so its variables (and which lines were seen) carry
            // over unless reset. This only resets the database tables; registered Lua functions stay.
            if (DialogueManager.hasInstance)
            {
                if (DialogueManager.isConversationActive)
                    DialogueManager.StopConversation();
                DialogueManager.ResetDatabase(DatabaseResetOptions.RevertToDefault);
            }

            Debug.Log("[GameSession] Fresh start: cleared progress from the previous playthrough.");
        }
    }
}
