using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// Throwaway harness: press a key in Play mode to roll and announce some dice, so you can
    /// watch the HUD react and check the values in the Console. Delete or disable once the
    /// dice system works.
    /// </summary>
    public class DiceRollTester : MonoBehaviour
    {
        [SerializeField] private int count = 2;
        [SerializeField] private int sides = 6;
        [SerializeField] private string label = "Test";

        [SerializeField]
        [Tooltip("Key that triggers a roll.")]
        private Key rollKey = Key.Space;

        private void Update()
        {
            if (DiceRoller.Instance == null || Keyboard.current == null)
                return;

            if (Keyboard.current[rollKey].wasPressedThisFrame)
            {
                DiceRoll roll = DiceRoller.Instance.RollAndAnnounce(count, sides, label);
                Debug.Log(roll);
            }
        }
    }
}
