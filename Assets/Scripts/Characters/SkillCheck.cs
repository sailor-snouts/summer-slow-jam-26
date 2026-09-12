using UnityEngine;

namespace Game
{
    public static class SkillCheck
    {
        public static CharacterData DefaultCharacter { get; set; }

        public static int Roll(CharacterData character, Stat stat, int count, int sides, bool showRoll = true)
        {
            if (character == null)
            {
                Debug.LogError("[SkillCheck] No character given and DefaultCharacter isn't set - returning 0.");
                return 0;
            }
            return RollCore(character, Outfits.EffectiveStat(character, stat), stat.ToString(), count, sides, showRoll);
        }

        public static int Roll(CharacterData character, StatCategory category, int count, int sides, bool showRoll = true)
        {
            if (character == null)
            {
                Debug.LogError("[SkillCheck] No character given and DefaultCharacter isn't set - returning 0.");
                return 0;
            }
            return RollCore(character, Outfits.EffectiveCategory(character, category), category.ToString(), count, sides, showRoll);
        }

        public static int Roll(Stat stat, int count, int sides, bool showRoll = true)
            => Roll(DefaultCharacter, stat, count, sides, showRoll);

        public static int Roll(StatCategory category, int count, int sides, bool showRoll = true)
            => Roll(DefaultCharacter, category, count, sides, showRoll);

        public static bool Try(CharacterData character, Stat stat, int count, int sides, int requirement, bool showRoll = true)
            => Roll(character, stat, count, sides, showRoll) >= requirement;

        public static bool Try(CharacterData character, StatCategory category, int count, int sides, int requirement, bool showRoll = true)
            => Roll(character, category, count, sides, showRoll) >= requirement;

        public static bool Try(Stat stat, int count, int sides, int requirement, bool showRoll = true)
            => Roll(stat, count, sides, showRoll) >= requirement;

        public static bool Try(StatCategory category, int count, int sides, int requirement, bool showRoll = true)
            => Roll(category, count, sides, showRoll) >= requirement;

        private static int RollCore(CharacterData character, int modifier, string label, int count, int sides, bool showRoll)
        {
            if (DiceRoller.Instance == null)
            {
                Debug.LogError("[SkillCheck] No DiceRoller in the scene to roll with - returning 0.");
                return 0;
            }

            DiceRoll roll = DiceRoller.Instance.Roll(count, sides);
            int total = roll.Total + modifier;

            Debug.Log($"[SkillCheck] {character.DisplayName} {label}: {roll.Total} (dice) + {modifier} ({label}) = {total}");

            if (showRoll)
                DiceRoller.Instance.Announce(roll, $"{label} check");

            return total;
        }
    }
}
