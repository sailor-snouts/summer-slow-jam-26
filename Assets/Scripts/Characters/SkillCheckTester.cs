using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class SkillCheckTester : MonoBehaviour
    {
        [SerializeField] private Stat stat = Stat.Vigor;
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
