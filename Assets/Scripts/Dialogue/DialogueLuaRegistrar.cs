using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Registers the game's custom dialogue Lua functions, and auto-spawns itself at startup (and
    /// survives scene loads) so they're always available — no component to place, and no dependency
    /// on the player object being set up. Registration happens in OnEnable with 'this' (instance
    /// methods, which the Dialogue System's Lua interpreter needs) and after the system has reset
    /// its Lua environment, so the registrations stick. Values come from the static player/skill-check
    /// state, so the functions work whenever a player is present (and return 0/false otherwise).
    /// </summary>
    public class DialogueLuaRegistrar : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject("Dialogue Lua Registrar");
            DontDestroyOnLoad(go);
            go.AddComponent<DialogueLuaRegistrar>();
            Debug.Log("[DialogueLuaRegistrar] Bootstrap ran — registrar spawned.");
        }

        private void OnEnable()
        {
            Lua.RegisterFunction("IsPlayer", this, GetType().GetMethod(nameof(IsPlayer)));
            Lua.RegisterFunction("BrainCheck", this, GetType().GetMethod(nameof(BrainCheck)));
            Lua.RegisterFunction("BrawnCheck", this, GetType().GetMethod(nameof(BrawnCheck)));
            Lua.RegisterFunction("BeautyCheck", this, GetType().GetMethod(nameof(BeautyCheck)));
            Debug.Log("[DialogueLuaRegistrar] Registered IsPlayer / BrainCheck / BrawnCheck / BeautyCheck.");
        }

        private void OnDisable()
        {
            Lua.UnregisterFunction("IsPlayer");
            Lua.UnregisterFunction("BrainCheck");
            Lua.UnregisterFunction("BrawnCheck");
            Lua.UnregisterFunction("BeautyCheck");
        }

        /// <summary>Lua: true if the player is currently controlling the given actor.</summary>
        public bool IsPlayer(string actorName)
        {
            CharacterData data = PlayerCharacter.CurrentData;
            return data != null && data.DisplayName == actorName;
        }

        // Lua skill checks: roll count d sides + the player's stat, return the total to compare.
        public double BrainCheck(double count, double sides) => SkillCheck.Roll(Stat.Brain, (int)count, (int)sides, showRoll: true);
        public double BrawnCheck(double count, double sides) => SkillCheck.Roll(Stat.Brawn, (int)count, (int)sides, showRoll: true);
        public double BeautyCheck(double count, double sides) => SkillCheck.Roll(Stat.Beauty, (int)count, (int)sides, showRoll: true);
    }
}
