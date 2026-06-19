using UnityEngine;

namespace Game
{
    /// <summary>
    /// A skill check: roll dice, add a character's stat, and compare to a requirement —
    /// passes when <c>roll + stat &gt;= requirement</c>. Call it from code (pass the
    /// <see cref="CharacterStats"/>) or from the dialogue system (which uses
    /// <see cref="DefaultStats"/>). Rolling goes through the scene's <see cref="DiceRoller"/>,
    /// so it shares the same seeded random and can show on the dice HUD.
    /// </summary>
    public static class SkillCheck
    {
        /// <summary>
        /// The stats used when a check doesn't name a character (e.g. dialogue checks).
        /// Set this once to "the player" — see <see cref="SkillCheckPlayer"/> for a drop-in way.
        /// </summary>
        public static CharacterStats DefaultStats { get; set; }

        /// <summary>
        /// Rolls <paramref name="count"/>d<paramref name="sides"/>, adds <paramref name="stats"/>'s
        /// <paramref name="stat"/>, and returns whether the total meets <paramref name="requirement"/>.
        /// When <paramref name="showRoll"/> is true, fires the dice HUD event.
        /// </summary>
        public static bool Try(CharacterStats stats, Stat stat, int count, int sides, int requirement, bool showRoll = true)
        {
            if (stats == null)
            {
                Debug.LogError("[SkillCheck] No CharacterStats given and DefaultStats isn't set — failing the check.");
                return false;
            }
            if (DiceRoller.Instance == null)
            {
                Debug.LogError("[SkillCheck] No DiceRoller in the scene to roll with — failing the check.");
                return false;
            }

            DiceRoll roll = DiceRoller.Instance.Roll(count, sides);
            int statValue = stats.Get(stat);
            bool passed = roll.Total + statValue >= requirement;

            if (showRoll)
                DiceRoller.Instance.Announce(roll, $"{stat} check");

            return passed;
        }

        /// <summary>Skill check against <see cref="DefaultStats"/> (the dialogue/player default).</summary>
        public static bool Try(Stat stat, int count, int sides, int requirement, bool showRoll = true)
            => Try(DefaultStats, stat, count, sides, requirement, showRoll);
    }
}
