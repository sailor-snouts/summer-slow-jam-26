using System.Collections;
using JamTemplate.Menus;
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
            Lua.RegisterFunction("ChangeScene", this, GetType().GetMethod(nameof(ChangeScene)));
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
            Lua.UnregisterFunction("ChangeScene");
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

        // ChangeScene("ThankYou") - loads that scene (with the normal fade) once this conversation closes,
        // so the line that called it is still shown first.
        public void ChangeScene(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"[DialogueLuaRegistrar] ChangeScene: '{sceneName}' isn't in Build Settings.");
                return;
            }
            StartCoroutine(LoadWhenConversationEnds(sceneName));
        }

        private IEnumerator LoadWhenConversationEnds(string sceneName)
        {
            while (DialogueManager.isConversationActive)
                yield return null;
            MenuSceneRouter.Load(sceneName);
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

        // DriveCheck(2, 6, true, 12): roll 2d6 + effective Drive, true when the total >= 12. showUi pops the
        // dice HUD. One method per stat and per category so each is its own Conditions dropdown entry.
        public bool DriveCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(Stat.Drive, (int)count, (int)sides, (int)target, showUi);
        public bool WillpowerCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(Stat.Willpower, (int)count, (int)sides, (int)target, showUi);
        public bool ObservationCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(Stat.Observation, (int)count, (int)sides, (int)target, showUi);
        public bool EmpathyCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(Stat.Empathy, (int)count, (int)sides, (int)target, showUi);

        public bool VigorCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(Stat.Vigor, (int)count, (int)sides, (int)target, showUi);
        public bool EnduranceCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(Stat.Endurance, (int)count, (int)sides, (int)target, showUi);
        public bool AgilityCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(Stat.Agility, (int)count, (int)sides, (int)target, showUi);
        public bool TechniqueCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(Stat.Technique, (int)count, (int)sides, (int)target, showUi);

        public bool CharmCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(Stat.Charm, (int)count, (int)sides, (int)target, showUi);
        public bool TauntCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(Stat.Taunt, (int)count, (int)sides, (int)target, showUi);
        public bool BonhomieCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(Stat.Bonhomie, (int)count, (int)sides, (int)target, showUi);
        public bool HostilityCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(Stat.Hostility, (int)count, (int)sides, (int)target, showUi);

        public bool BrainCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(StatCategory.Brain, (int)count, (int)sides, (int)target, showUi);
        public bool BrawnCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(StatCategory.Brawn, (int)count, (int)sides, (int)target, showUi);
        public bool BeautyCheck(double count, double sides, bool showUi, double target) => SkillCheck.Try(StatCategory.Beauty, (int)count, (int)sides, (int)target, showUi);
    }
}
