using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// The dice "object" in the scene: rolls N M-sided dice and (optionally) announces the
    /// result. Place one on a GameObject named "Dice Roller". Rolling returns the values
    /// silently; <see cref="Announce"/> raises the static <see cref="Rolled"/> event so the
    /// HUD (and later dialogue/combat) can react without holding a reference to this object.
    /// </summary>
    [AddComponentMenu("Game/Dice Roller")]
    [DisallowMultipleComponent]
    public class DiceRoller : MonoBehaviour
    {
        /// <summary>The active roller. Other scripts call DiceRoller.Instance.Roll(...).</summary>
        public static DiceRoller Instance { get; private set; }

        /// <summary>Raised by <see cref="Announce"/>: (roll, optional label). The HUD listens.</summary>
        public static event Action<DiceRoll, string> Rolled;

        [Header("Seed")]
        [SerializeField]
        [Tooltip("Tick to make every run produce the same sequence (handy for testing).")]
        private bool useFixedSeed;

        [SerializeField]
        [Tooltip("The seed used when 'Use Fixed Seed' is on.")]
        private int seed = 12345;

        // System.Random (not UnityEngine.Random) so the sequence is owned here and seedable —
        // no global state, which keeps rolls reproducible and independent of the rest of the game.
        private System.Random random;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            random = useFixedSeed ? new System.Random(seed) : new System.Random();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Rolls <paramref name="count"/> dice of <paramref name="sides"/> sides. Silent — just returns the values.</summary>
        public DiceRoll Roll(int count, int sides)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Must roll at least one die.");
            if (sides < 1)
                throw new ArgumentOutOfRangeException(nameof(sides), sides, "A die needs at least one side.");

            var values = new int[count];
            for (int i = 0; i < count; i++)
                values[i] = random.Next(1, sides + 1); // upper bound is exclusive, so +1 to include 'sides'

            DiceRoll roll = new DiceRoll(count, sides, values);
            Debug.Log($"[Dice] {roll}");
            return roll;
        }

        /// <summary>Re-seeds the roller so the next rolls are reproducible from a known point.</summary>
        public void SetSeed(int newSeed)
        {
            random = new System.Random(newSeed);
        }

        /// <summary>Raises <see cref="Rolled"/> so listeners (the HUD) can show the roll.</summary>
        public void Announce(DiceRoll roll, string label = null)
        {
            Rolled?.Invoke(roll, label);
        }

        /// <summary>Convenience for the common "roll and show it" case.</summary>
        public DiceRoll RollAndAnnounce(int count, int sides, string label = null)
        {
            DiceRoll roll = Roll(count, sides);
            Announce(roll, label);
            return roll;
        }
    }
}
