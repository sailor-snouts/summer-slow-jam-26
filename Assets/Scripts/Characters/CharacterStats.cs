using System;
using UnityEngine;

namespace Game
{
    /// <summary>The three character stat categories. Used to read/write a stat generically.</summary>
    public enum Stat
    {
        Brain,
        Brawn,
        Beauty,
    }

    /// <summary>
    /// A character's stats: three integer categories (Brain, Brawn, Beauty), each at least 1.
    /// Put this on a character GameObject and set the values in the Inspector. The named
    /// properties are for readable code; <see cref="Get"/>/<see cref="Set"/> let other systems
    /// (e.g. a future skill check) work with a stat chosen at runtime.
    /// </summary>
    [AddComponentMenu("Game/Character Stats")]
    [DisallowMultipleComponent]
    public class CharacterStats : MonoBehaviour
    {
        /// <summary>Stat values are clamped to this inclusive range — in the Inspector and in code.</summary>
        public const int MinValue = 1;
        public const int MaxValue = 4;

        // [Range] clamps both ends in the Inspector (shown as a slider). Script paths clamp too — see Set.
        [SerializeField, Range(MinValue, MaxValue)] private int brain = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int brawn = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int beauty = MinValue;

        public int Brain => brain;
        public int Brawn => brawn;
        public int Beauty => beauty;

        /// <summary>Reads one stat by category.</summary>
        public int Get(Stat stat) => stat switch
        {
            Stat.Brain => brain,
            Stat.Brawn => brawn,
            Stat.Beauty => beauty,
            _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, "Unknown stat."),
        };

        /// <summary>Sets one stat by category, clamped to the [MinValue, MaxValue] range.</summary>
        public void Set(Stat stat, int value)
        {
            value = Mathf.Clamp(value, MinValue, MaxValue);
            switch (stat)
            {
                case Stat.Brain: brain = value; break;
                case Stat.Brawn: brawn = value; break;
                case Stat.Beauty: beauty = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(stat), stat, "Unknown stat.");
            }
        }

        /// <summary>Adds <paramref name="delta"/> to a stat (negative to subtract), staying within range.</summary>
        public void Add(Stat stat, int delta) => Set(stat, Get(stat) + delta);
    }
}
