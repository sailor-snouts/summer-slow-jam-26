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
        /// <summary>
        /// Every stat and category check, by method name — one Lua function each, so the dialogue
        /// editor lists them all in the Conditions dropdown. The names match the public methods below.
        /// </summary>
        private static readonly string[] CheckFunctions =
        {
            // Brain stats
            nameof(DriveCheck), nameof(WillpowerCheck), nameof(ObservationCheck), nameof(EmpathyCheck),
            // Brawn stats
            nameof(VigorCheck), nameof(EnduranceCheck), nameof(AgilityCheck), nameof(TechniqueCheck),
            // Beauty stats
            nameof(CharmCheck), nameof(TauntCheck), nameof(BonhomieCheck), nameof(HostilityCheck),
            // Category totals
            nameof(BrainCheck), nameof(BrawnCheck), nameof(BeautyCheck),
        };

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
            foreach (string functionName in CheckFunctions)
                Lua.RegisterFunction(functionName, this, GetType().GetMethod(functionName));
            Debug.Log($"[DialogueLuaRegistrar] Registered IsPlayer + {CheckFunctions.Length} stat/category checks.");
        }

        private void OnDisable()
        {
            Lua.UnregisterFunction("IsPlayer");
            foreach (string functionName in CheckFunctions)
                Lua.UnregisterFunction(functionName);
        }

        /// <summary>Lua: true if the player is currently controlling the given actor.</summary>
        public bool IsPlayer(string actorName)
        {
            CharacterData data = PlayerCharacter.CurrentData;
            return data != null && data.DisplayName == actorName;
        }

        // Lua skill checks: roll count d sides + the player's stat (or category total), returning the
        // total to compare. One method per stat and per category so each is its own dropdown entry.

        // Brain stats
        public double DriveCheck(double count, double sides) => SkillCheck.Roll(Stat.Drive, (int)count, (int)sides, showRoll: true);
        public double WillpowerCheck(double count, double sides) => SkillCheck.Roll(Stat.Willpower, (int)count, (int)sides, showRoll: true);
        public double ObservationCheck(double count, double sides) => SkillCheck.Roll(Stat.Observation, (int)count, (int)sides, showRoll: true);
        public double EmpathyCheck(double count, double sides) => SkillCheck.Roll(Stat.Empathy, (int)count, (int)sides, showRoll: true);

        // Brawn stats
        public double VigorCheck(double count, double sides) => SkillCheck.Roll(Stat.Vigor, (int)count, (int)sides, showRoll: true);
        public double EnduranceCheck(double count, double sides) => SkillCheck.Roll(Stat.Endurance, (int)count, (int)sides, showRoll: true);
        public double AgilityCheck(double count, double sides) => SkillCheck.Roll(Stat.Agility, (int)count, (int)sides, showRoll: true);
        public double TechniqueCheck(double count, double sides) => SkillCheck.Roll(Stat.Technique, (int)count, (int)sides, showRoll: true);

        // Beauty stats
        public double CharmCheck(double count, double sides) => SkillCheck.Roll(Stat.Charm, (int)count, (int)sides, showRoll: true);
        public double TauntCheck(double count, double sides) => SkillCheck.Roll(Stat.Taunt, (int)count, (int)sides, showRoll: true);
        public double BonhomieCheck(double count, double sides) => SkillCheck.Roll(Stat.Bonhomie, (int)count, (int)sides, showRoll: true);
        public double HostilityCheck(double count, double sides) => SkillCheck.Roll(Stat.Hostility, (int)count, (int)sides, showRoll: true);

        // Category totals (sum of the four stats)
        public double BrainCheck(double count, double sides) => SkillCheck.Roll(StatCategory.Brain, (int)count, (int)sides, showRoll: true);
        public double BrawnCheck(double count, double sides) => SkillCheck.Roll(StatCategory.Brawn, (int)count, (int)sides, showRoll: true);
        public double BeautyCheck(double count, double sides) => SkillCheck.Roll(StatCategory.Beauty, (int)count, (int)sides, showRoll: true);
    }
}
