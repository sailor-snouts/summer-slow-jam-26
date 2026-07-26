using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// An outfit the player can wear: a worn look (world sprite) plus flat modifiers to the twelve
    /// stats, Disco Elysium style (+1 Charm, -1 Hostility, ...). Create via
    /// Assets > Create > Game > Outfit and add it to the <see cref="Wardrobe"/> so it shows in the
    /// outfit menu. Equipping is runtime state - see <see cref="Outfits"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "Outfit", menuName = "Game/Outfit")]
    public class OutfitData : ScriptableObject
    {
        /// <summary>Stat modifiers are clamped to this inclusive range.</summary>
        public const int MinModifier = -4;
        public const int MaxModifier = 4;

        /// <summary>Masculine / feminine modifiers are clamped to this inclusive range.</summary>
        public const int MinGenderModifier = -5;
        public const int MaxGenderModifier = 5;

        /// <summary>A worn look for one specific character (overrides the default sprite).</summary>
        [Serializable]
        public struct CharacterSprite
        {
            [Tooltip("Character this look is for.")]
            public CharacterData character;

            [Tooltip("How that character looks wearing this outfit.")]
            public Sprite sprite;
        }

        [Tooltip("Name shown in the outfit menu. Falls back to the asset name if blank.")]
        [SerializeField] private string displayName;

        [Tooltip("Short flavor text shown in the menu.")]
        [SerializeField, TextArea] private string description;

        [Tooltip("Default worn look - used for any character without a specific look below.")]
        [SerializeField] private Sprite sprite;

        [Tooltip("Optional per-character worn looks. Characters not listed use the default sprite.")]
        [SerializeField] private List<CharacterSprite> characterSprites = new();

        // Brain
        [SerializeField, Range(MinModifier, MaxModifier)] private int drive;
        [SerializeField, Range(MinModifier, MaxModifier)] private int willpower;
        [SerializeField, Range(MinModifier, MaxModifier)] private int observation;
        [SerializeField, Range(MinModifier, MaxModifier)] private int empathy;

        // Brawn
        [SerializeField, Range(MinModifier, MaxModifier)] private int vigor;
        [SerializeField, Range(MinModifier, MaxModifier)] private int endurance;
        [SerializeField, Range(MinModifier, MaxModifier)] private int agility;
        [SerializeField, Range(MinModifier, MaxModifier)] private int technique;

        // Beauty
        [SerializeField, Range(MinModifier, MaxModifier)] private int charm;
        [SerializeField, Range(MinModifier, MaxModifier)] private int taunt;
        [SerializeField, Range(MinModifier, MaxModifier)] private int bonhomie;
        [SerializeField, Range(MinModifier, MaxModifier)] private int hostility;

        // Independent masculine / feminine modifiers this outfit applies to the wearer's presentation.
        // Each ranges -5..5 (0 = no change) and is added on top of the character's own value - see
        // Outfits.EffectiveMasculine / EffectiveFeminine.
        [SerializeField, Range(MinGenderModifier, MaxGenderModifier)] private int masculine;
        [SerializeField, Range(MinGenderModifier, MaxGenderModifier)] private int feminine;

        /// <summary>Name shown in the menu (falls back to the asset name).</summary>
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;

        /// <summary>Short flavor text shown in the menu.</summary>
        public string Description => description;

        /// <summary>This outfit's masculine modifier (added to the wearer's masculine value).</summary>
        public int MasculineModifier => masculine;

        /// <summary>This outfit's feminine modifier (added to the wearer's feminine value).</summary>
        public int FeminineModifier => feminine;

        /// <summary>The worn look for a character - their specific sprite if listed, else the default.</summary>
        public Sprite GetSprite(CharacterData character)
        {
            foreach (CharacterSprite entry in characterSprites)
                if (entry.character == character && entry.sprite != null)
                    return entry.sprite;
            return sprite;
        }

        /// <summary>This outfit's modifier for one stat (0 if unmodified).</summary>
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

        /// <summary>This outfit's total modifier for a category (the sum of its four stats' modifiers).</summary>
        public int CategoryModifier(StatCategory category) => category switch
        {
            StatCategory.Brain => drive + willpower + observation + empathy,
            StatCategory.Brawn => vigor + endurance + agility + technique,
            StatCategory.Beauty => charm + taunt + bonhomie + hostility,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown category."),
        };
    }
}
