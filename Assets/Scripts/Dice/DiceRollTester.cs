using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
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
