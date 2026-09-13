using System;
using UnityEngine;

namespace Game
{
    [AddComponentMenu("Game/Dice Roller")]
    [DisallowMultipleComponent]
    public class DiceRoller : MonoBehaviour
    {
        public static DiceRoller Instance { get; private set; }

        public static event Action<DiceRoll, string> Rolled;

        [Header("Seed")]
        [SerializeField]
        [Tooltip("Tick to make every run produce the same sequence (handy for testing).")]
        private bool useFixedSeed;

        [SerializeField]
        [Tooltip("The seed used when 'Use Fixed Seed' is on.")]
        private int seed = 12345;

        [Header("HUD")]
        [SerializeField]
        [Tooltip("Dice HUD prefab shown when a roll is announced. Spawned once if the scene has none. Leave empty to not auto-spawn.")]
        private GameObject hudPrefab;

        // System.Random (not UnityEngine.Random) so the sequence is owned here, seedable, and free of global state.
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

            // Make sure a dice HUD exists to show rolls, so skill checks display everywhere the roller is.
            if (hudPrefab != null && FindAnyObjectByType<DiceHudView>() == null)
                Instantiate(hudPrefab);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

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

        public void SetSeed(int newSeed)
        {
            random = new System.Random(newSeed);
        }

        public void Announce(DiceRoll roll, string label = null)
        {
            Rolled?.Invoke(roll, label);
        }

        public DiceRoll RollAndAnnounce(int count, int sides, string label = null)
        {
            DiceRoll roll = Roll(count, sides);
            Announce(roll, label);
            return roll;
        }
    }
}
