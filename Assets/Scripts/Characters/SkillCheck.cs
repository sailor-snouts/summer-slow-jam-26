using UnityEngine;

namespace Game
{
    /// <summary>
    /// A skill check: roll dice, add a character's stat, and compare to a requirement —
    /// passes when <c>roll + stat &gt;= requirement</c>. Call it from code (pass the
    /// <see cref="CharacterData"/>) or from the dialogue system (which uses
    /// <see cref="DefaultCharacter"/>). Rolling goes through the scene's <see cref="DiceRoller"/>,
    /// so it shares the same seeded random and can show on the dice HUD.
    /// </summary>
    public static class SkillCheck
    {
        /// <summary>
        /// The character used when a check doesn't name one (e.g. dialogue checks) — "the player".
        /// Set it once; <see cref="SkillCheckPlayer"/> is a drop-in way to do that from the player object.
        /// </summary>
        public static CharacterData DefaultCharacter { get; set; }

        /// <summary>
        /// Rolls <paramref name="count"/>d<paramref name="sides"/>, adds <paramref name="character"/>'s
        /// <paramref name="stat"/>, and returns whether the total meets <paramref name="requirement"/>.
        /// When <paramref name="showRoll"/> is true, fires the dice HUD event.
        /// </summary>
        public static bool Try(CharacterData character, Stat stat, int count, int sides, int requirement, bool showRoll = true)
        {
            if (character == null)
            {
                Debug.LogError("[SkillCheck] No character given and DefaultCharacter isn't set — failing the check.");
                return false;
            }
            if (DiceRoller.Instance == null)
            {
                Debug.LogError("[SkillCheck] No DiceRoller in the scene to roll with — failing the check.");
                return false;
            }

            DiceRoll roll = DiceRoller.Instance.Roll(count, sides);
            bool passed = roll.Total + character.Get(stat) >= requirement;

            if (showRoll)
                DiceRoller.Instance.Announce(roll, $"{stat} check");

            return passed;
        }

        /// <summary>Skill check against <see cref="DefaultCharacter"/> (the dialogue/player default).</summary>
        public static bool Try(Stat stat, int count, int sides, int requirement, bool showRoll = true)
            => Try(DefaultCharacter, stat, count, sides, requirement, showRoll);
    }
}
