using UnityEngine;

namespace Game
{
    public static class SkillCheck
    {
        public static CharacterData DefaultCharacter { get; set; }

        public static int Roll(CharacterData character, Stat stat, int count, int sides, bool showRoll = true)
            => RollStat(character, stat, count, sides, showRoll, null);

        public static int Roll(CharacterData character, StatCategory category, int count, int sides, bool showRoll = true)
            => RollCategory(character, category, count, sides, showRoll, null);

        public static int Roll(Stat stat, int count, int sides, bool showRoll = true)
            => Roll(DefaultCharacter, stat, count, sides, showRoll);

        public static int Roll(StatCategory category, int count, int sides, bool showRoll = true)
            => Roll(DefaultCharacter, category, count, sides, showRoll);

        // Pass when dice + effective stat >= requirement. The outcome is announced with the roll so the
        // dice HUD can show pass/fail.
        public static bool Try(CharacterData character, Stat stat, int count, int sides, int requirement, bool showRoll = true)
            => RollStat(character, stat, count, sides, showRoll, requirement) >= requirement;

        public static bool Try(CharacterData character, StatCategory category, int count, int sides, int requirement, bool showRoll = true)
            => RollCategory(character, category, count, sides, showRoll, requirement) >= requirement;

        public static bool Try(Stat stat, int count, int sides, int requirement, bool showRoll = true)
            => Try(DefaultCharacter, stat, count, sides, requirement, showRoll);

        public static bool Try(StatCategory category, int count, int sides, int requirement, bool showRoll = true)
            => Try(DefaultCharacter, category, count, sides, requirement, showRoll);

        private static int RollStat(CharacterData character, Stat stat, int count, int sides, bool showRoll, int? target)
        {
            if (character == null)
            {
                Debug.LogError("[SkillCheck] No character given and DefaultCharacter isn't set - returning 0.");
                return 0;
            }
            return RollCore(character, Outfits.EffectiveStat(character, stat), stat.ToString(), count, sides, showRoll, target);
        }

        private static int RollCategory(CharacterData character, StatCategory category, int count, int sides, bool showRoll, int? target)
        {
            if (character == null)
            {
                Debug.LogError("[SkillCheck] No character given and DefaultCharacter isn't set - returning 0.");
                return 0;
            }
            return RollCore(character, Outfits.EffectiveCategory(character, category), category.ToString(), count, sides, showRoll, target);
        }

        private static int RollCore(CharacterData character, int modifier, string label, int count, int sides, bool showRoll, int? target)
        {
            if (DiceRoller.Instance == null)
            {
                Debug.LogError("[SkillCheck] No DiceRoller in the scene to roll with - returning 0.");
                return 0;
            }

            DiceRoll roll = DiceRoller.Instance.Roll(count, sides);
            int total = roll.Total + modifier;

            RollOutcome outcome = RollOutcome.None;
            if (target.HasValue)
                outcome = total >= target.Value ? RollOutcome.Pass : RollOutcome.Fail;

            string vs = target.HasValue ? $" vs {target.Value} -> {outcome}" : string.Empty;
            Debug.Log($"[SkillCheck] {character.DisplayName} {label}: {roll.Total} (dice) + {modifier} ({label}) = {total}{vs}");

            if (showRoll)
                DiceRoller.Instance.Announce(roll, $"{label} check", outcome);

            return total;
        }
    }
}
