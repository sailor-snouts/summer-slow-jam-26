using System;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Outfit", menuName = "Game/Outfit")]
    public class OutfitData : ScriptableObject
    {
        public const int MinModifier = -4;
        public const int MaxModifier = 4;

        public const int MinGenderModifier = -5;
        public const int MaxGenderModifier = 5;

        [Tooltip("Name shown in the outfit menu. Falls back to the asset name if blank.")]
        [SerializeField] private string displayName;

        [Tooltip("Short flavor text shown in the menu.")]
        [SerializeField, TextArea] private string description;

        [Tooltip("The outfit's worn look, per facing direction. Every character wearing it shows this.")]
        [SerializeField] private DirectionalSprites look;

        [SerializeField, Range(MinModifier, MaxModifier)] private int drive;
        [SerializeField, Range(MinModifier, MaxModifier)] private int willpower;
        [SerializeField, Range(MinModifier, MaxModifier)] private int observation;
        [SerializeField, Range(MinModifier, MaxModifier)] private int empathy;

        [SerializeField, Range(MinModifier, MaxModifier)] private int vigor;
        [SerializeField, Range(MinModifier, MaxModifier)] private int endurance;
        [SerializeField, Range(MinModifier, MaxModifier)] private int agility;
        [SerializeField, Range(MinModifier, MaxModifier)] private int technique;

        [SerializeField, Range(MinModifier, MaxModifier)] private int charm;
        [SerializeField, Range(MinModifier, MaxModifier)] private int taunt;
        [SerializeField, Range(MinModifier, MaxModifier)] private int bonhomie;
        [SerializeField, Range(MinModifier, MaxModifier)] private int hostility;

        [SerializeField, Range(MinGenderModifier, MaxGenderModifier)] private int masculine;
        [SerializeField, Range(MinGenderModifier, MaxGenderModifier)] private int feminine;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;

        public string Description => description;

        public int MasculineModifier => masculine;

        public int FeminineModifier => feminine;

        public Sprite GetSprite(Facing4 facing) => look.Get(facing);

        public int Modifier(Stat stat) => stat switch
        {
            Stat.Drive => drive,
            Stat.Willpower => willpower,
            Stat.Observation => observation,
            Stat.Empathy => empathy,
            Stat.Vigor => vigor,
            Stat.Endurance => endurance,
            Stat.Agility => agility,
            Stat.Technique => technique,
            Stat.Charm => charm,
            Stat.Taunt => taunt,
            Stat.Bonhomie => bonhomie,
            Stat.Hostility => hostility,
            _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, "Unknown stat."),
        };

        public int CategoryModifier(StatCategory category) => category switch
        {
            StatCategory.Brain => drive + willpower + observation + empathy,
            StatCategory.Brawn => vigor + endurance + agility + technique,
            StatCategory.Beauty => charm + taunt + bonhomie + hostility,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown category."),
        };
    }
}
