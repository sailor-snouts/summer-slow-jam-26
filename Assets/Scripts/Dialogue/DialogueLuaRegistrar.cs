using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    public class DialogueLuaRegistrar : MonoBehaviour
    {
        private static readonly string[] CheckFunctions =
        {
            nameof(DriveCheck), nameof(WillpowerCheck), nameof(ObservationCheck), nameof(EmpathyCheck),
            nameof(VigorCheck), nameof(EnduranceCheck), nameof(AgilityCheck), nameof(TechniqueCheck),
            nameof(CharmCheck), nameof(TauntCheck), nameof(BonhomieCheck), nameof(HostilityCheck),
            nameof(BrainCheck), nameof(BrawnCheck), nameof(BeautyCheck),
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject("Dialogue Lua Registrar");
            DontDestroyOnLoad(go);
            go.AddComponent<DialogueLuaRegistrar>();
            Debug.Log("[DialogueLuaRegistrar] Bootstrap ran - registrar spawned.");
        }

        private void OnEnable()
        {
            Lua.RegisterFunction("IsPlayer", this, GetType().GetMethod(nameof(IsPlayer)));
            Lua.RegisterFunction("SetPlayer", this, GetType().GetMethod(nameof(SetPlayer)));
            Lua.RegisterFunction("UnlockOutfit", this, GetType().GetMethod(nameof(UnlockOutfit)));
            Lua.RegisterFunction("NpcWalk", this, GetType().GetMethod(nameof(NpcWalk)));
            Lua.RegisterFunction("Masculine", this, GetType().GetMethod(nameof(Masculine)));
            Lua.RegisterFunction("Feminine", this, GetType().GetMethod(nameof(Feminine)));
            foreach (string functionName in CheckFunctions)
                Lua.RegisterFunction(functionName, this, GetType().GetMethod(functionName));
            Debug.Log($"[DialogueLuaRegistrar] Registered IsPlayer + SetPlayer + Masculine/Feminine + {CheckFunctions.Length} stat/category checks.");
        }

        private void OnDisable()
        {
            Lua.UnregisterFunction("IsPlayer");
            Lua.UnregisterFunction("SetPlayer");
            Lua.UnregisterFunction("UnlockOutfit");
            Lua.UnregisterFunction("NpcWalk");
            Lua.UnregisterFunction("Masculine");
            Lua.UnregisterFunction("Feminine");
            foreach (string functionName in CheckFunctions)
                Lua.UnregisterFunction(functionName);
        }

        public bool IsPlayer(string actorName)
        {
            CharacterData data = PlayerCharacter.CurrentData;
            return data != null && data.DisplayName == actorName;
        }

        public void SetPlayer(string actorName)
        {
            if (PlayerCharacter.Current != null)
                PlayerCharacter.Current.SetActiveByName(actorName);
            else
                Debug.LogWarning($"[DialogueLuaRegistrar] SetPlayer('{actorName}') called but there's no active PlayerCharacter.");
        }

        // UnlockOutfit("Erin Quennell", "Overcoat") - makes that outfit selectable in the outfit menu.
        public void UnlockOutfit(string characterName, string outfitName)
        {
            PlayerCharacter player = PlayerCharacter.Current;
            if (player == null)
            {
                Debug.LogWarning($"[DialogueLuaRegistrar] UnlockOutfit('{characterName}', '{outfitName}') called but there's no active PlayerCharacter.");
                return;
            }

            CharacterData character = player.OptionByName(characterName);
            if (character == null)
            {
                Debug.LogWarning($"[DialogueLuaRegistrar] UnlockOutfit: no player character named '{characterName}'.");
                return;
            }

            OutfitData outfit = FindOutfit(character, outfitName);
            if (outfit == null)
            {
                Debug.LogWarning($"[DialogueLuaRegistrar] UnlockOutfit: no outfit named '{outfitName}' in {character.DisplayName}'s wardrobe.");
                return;
            }

            Outfits.Unlock(character, outfit);
        }

        // NpcWalk("Bouncer") - arms that NPC's path walk to run once this conversation ends.
        public void NpcWalk(string npcName)
        {
            NpcWalkAfterConversation[] walkers = FindObjectsByType<NpcWalkAfterConversation>(FindObjectsInactive.Include);
            foreach (NpcWalkAfterConversation walker in walkers)
            {
                Character character = walker.GetComponent<Character>();
                if (character != null && character.Name == npcName)
                {
                    walker.Arm();
                    return;
                }
            }
            Debug.LogWarning($"[DialogueLuaRegistrar] NpcWalk: no NpcWalkAfterConversation on a character named '{npcName}'.");
        }

        private static OutfitData FindOutfit(CharacterData character, string outfitName)
        {
            Wardrobe wardrobe = character.Wardrobe;
            if (wardrobe == null)
                return null;
            foreach (OutfitData outfit in wardrobe.Outfits)
                if (outfit != null && (outfit.DisplayName == outfitName || outfit.name == outfitName))
                    return outfit;
            return null;
        }

        // Effective values in 0..10 to compare, not dice rolls - e.g. Feminine() >= 7.
        public double Masculine() => Outfits.EffectiveMasculine(PlayerCharacter.CurrentData);
        public double Feminine() => Outfits.EffectiveFeminine(PlayerCharacter.CurrentData);

        // One method per stat and per category so each is its own Conditions dropdown entry.
        public double DriveCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(Stat.Drive, (int)count, (int)sides, showRoll: showUi);
        public double WillpowerCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(Stat.Willpower, (int)count, (int)sides, showRoll: showUi);
        public double ObservationCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(Stat.Observation, (int)count, (int)sides, showRoll: showUi);
        public double EmpathyCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(Stat.Empathy, (int)count, (int)sides, showRoll: showUi);

        public double VigorCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(Stat.Vigor, (int)count, (int)sides, showRoll: showUi);
        public double EnduranceCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(Stat.Endurance, (int)count, (int)sides, showRoll: showUi);
        public double AgilityCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(Stat.Agility, (int)count, (int)sides, showRoll: showUi);
        public double TechniqueCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(Stat.Technique, (int)count, (int)sides, showRoll: showUi);

        public double CharmCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(Stat.Charm, (int)count, (int)sides, showRoll: showUi);
        public double TauntCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(Stat.Taunt, (int)count, (int)sides, showRoll: showUi);
        public double BonhomieCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(Stat.Bonhomie, (int)count, (int)sides, showRoll: showUi);
        public double HostilityCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(Stat.Hostility, (int)count, (int)sides, showRoll: showUi);

        public double BrainCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(StatCategory.Brain, (int)count, (int)sides, showRoll: showUi);
        public double BrawnCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(StatCategory.Brawn, (int)count, (int)sides, showRoll: showUi);
        public double BeautyCheck(double count, double sides, bool showUi = false) => SkillCheck.Roll(StatCategory.Beauty, (int)count, (int)sides, showRoll: showUi);
    }
}
