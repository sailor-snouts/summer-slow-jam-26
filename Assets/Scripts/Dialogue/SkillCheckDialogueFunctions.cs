using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Exposes the skill check to the Pixel Crushers Dialogue System as a Lua function, so
    /// dialogue conditions/scripts can roll against the player. Put this on a scene object
    /// that lives alongside the Dialogue Manager.
    ///
    /// In dialogue (Conditions or [lua(...)] text):
    ///   SkillCheck("Brawn", 2, 6, 8)   -- roll 2d6 + Brawn, true if &gt;= 8
    /// It uses <see cref="SkillCheck.DefaultStats"/> (the player), and shows the roll on the HUD.
    /// </summary>
    public class SkillCheckDialogueFunctions : MonoBehaviour
    {
        private const string FunctionName = "SkillCheck";

        // Lua functions are global, so register while enabled and clean up when disabled.
        private void OnEnable()
        {
            Lua.RegisterFunction(FunctionName, this, GetType().GetMethod(nameof(LuaSkillCheck)));
        }

        private void OnDisable()
        {
            Lua.UnregisterFunction(FunctionName);
        }

        /// <summary>
        /// Lua entry point. The Dialogue System passes numbers as <see cref="double"/>, so we
        /// take doubles and the stat as a string ("Brain"/"Brawn"/"Beauty"). Returns true/false.
        /// Must be public for the Lua interpreter to call it.
        /// </summary>
        public bool LuaSkillCheck(string statName, double count, double sides, double requirement)
        {
            if (!System.Enum.TryParse(statName, ignoreCase: true, out Stat stat))
            {
                Debug.LogError($"[SkillCheck] Dialogue called SkillCheck with unknown stat '{statName}'. " +
                               "Expected Brain, Brawn, or Beauty.");
                return false;
            }

            return SkillCheck.Try(stat, (int)count, (int)sides, (int)requirement, showRoll: true);
        }
    }
}
