using UnityEngine;

namespace Game
{
    /// <summary>
    /// A skill check: roll dice and add a character's stat. <see cref="Roll"/> returns the total (to
    /// compare against a difficulty); <see cref="Try"/> does the comparison for you. Usable from code
    /// (pass the <see cref="CharacterData"/>) or dialogue (uses <see cref="DefaultCharacter"/>).
    /// Rolling goes through the scene's <see cref="DiceRoller"/>, so it shares the seeded random and
    /// can show on the dice HUD.
    /// </summary>
    public static class SkillCheck
    {
        /// <summary>
        /// The character used when a check doesn't name one (e.g. dialogue checks) — "the player".
        /// Set it once; <see cref="SkillCheckPlayer"/> is a drop-in way to do that from the player object.
        /// </summary>
        public static CharacterData DefaultCharacter { get; set; }

        /// <summary>
        /// Rolls <paramref name="count"/>d<paramref name="sides"/> and adds <paramref name="character"/>'s
        /// <paramref name="stat"/>, returning the total. Fires the dice HUD event when <paramref name="showRoll"/> is true.
        /// </summary>
        public static int Roll(CharacterData character, Stat stat, int count, int sides, bool showRoll = true)
        {
            if (character == null)
            {
                Debug.LogError("[SkillCheck] No character given and DefaultCharacter isn't set — returning 0.");
                return 0;
            }
            if (DiceRoller.Instance == null)
            {
                Debug.LogError("[SkillCheck] No DiceRoller in the scene to roll with — returning 0.");
                return 0;
            }

            DiceRoll roll = DiceRoller.Instance.Roll(count, sides);
            int statValue = character.Get(stat);
            int total = roll.Total + statValue;

            Debug.Log($"[SkillCheck] {character.DisplayName} {stat}: {roll.Total} (dice) + {statValue} ({stat}) = {total}");

            if (showRoll)
                DiceRoller.Instance.Announce(roll, $"{stat} check");

            return total;
        }

        /// <summary>Roll + stat for <see cref="DefaultCharacter"/> (the dialogue/player default).</summary>
        public static int Roll(Stat stat, int count, int sides, bool showRoll = true)
            => Roll(DefaultCharacter, stat, count, sides, showRoll);

        /// <summary>True if roll + stat meets <paramref name="requirement"/>.</summary>
        public static bool Try(CharacterData character, Stat stat, int count, int sides, int requirement, bool showRoll = true)
            => Roll(character, stat, count, sides, showRoll) >= requirement;

        /// <summary>Skill check against <see cref="DefaultCharacter"/>.</summary>
        public static bool Try(Stat stat, int count, int sides, int requirement, bool showRoll = true)
            => Roll(stat, count, sides, showRoll) >= requirement;
    }
}
