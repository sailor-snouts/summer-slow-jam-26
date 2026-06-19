using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// Throwaway harness: press a key in Play mode to run a skill check against the default
    /// (player) character and log the result. Watch the dice HUD pop the roll. Delete or
    /// disable once you've confirmed the pipeline works.
    /// </summary>
    public class SkillCheckTester : MonoBehaviour
    {
        [SerializeField] private Stat stat = Stat.Brawn;
        [SerializeField] private int count = 2;
        [SerializeField] private int sides = 6;
        [SerializeField] private int requirement = 8;

        [SerializeField, Tooltip("Key that runs the check.")]
        private Key key = Key.C;

        private void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current[key].wasPressedThisFrame)
            {
                bool passed = SkillCheck.Try(stat, count, sides, requirement, showRoll: true);
                Debug.Log($"{stat} check vs {requirement}: {(passed ? "PASS" : "FAIL")}");
            }
        }
    }
}
