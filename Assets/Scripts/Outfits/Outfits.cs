using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Runtime outfit state: which <see cref="OutfitData"/> each character has equipped. Kept outside
    /// <see cref="CharacterData"/> because that is a shared asset - equipping is per-run state, not
    /// data. Also the one place that combines base stats with outfit modifiers: skill checks and the
    /// outfit menu both read <see cref="EffectiveStat"/> / <see cref="EffectiveCategory"/>.
    /// </summary>
    public static class Outfits
    {
        private static readonly Dictionary<CharacterData, OutfitData> equipped = new();

        /// <summary>Raised when a character's outfit changes (equip or unequip).</summary>
        public static event Action<CharacterData> Changed;

        // Clear static state on play start so nothing leaks between sessions (works with domain
        // reload disabled too).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            equipped.Clear();
            Changed = null;
        }

        /// <summary>The outfit a character has on, or null when unequipped.</summary>
        public static OutfitData GetEquipped(CharacterData character)
            => character != null && equipped.TryGetValue(character, out OutfitData outfit) ? outfit : null;

        /// <summary>Puts an outfit on a character (null takes it off). Raises <see cref="Changed"/>.</summary>
        public static void Equip(CharacterData character, OutfitData outfit)
        {
            if (character == null || GetEquipped(character) == outfit)
                return;

            if (outfit == null)
                equipped.Remove(character);
            else
                equipped[character] = outfit;

            Debug.Log($"[Outfits] {character.DisplayName} equipped {(outfit != null ? outfit.DisplayName : "nothing")}.");
            Changed?.Invoke(character);
        }

        /// <summary>The equipped outfit's modifier for one stat (0 when nothing is equipped).</summary>
        public static int Modifier(CharacterData character, Stat stat)
        {
            OutfitData outfit = GetEquipped(character);
            return outfit != null ? outfit.Modifier(stat) : 0;
        }

        /// <summary>The equipped outfit's total modifier for a category (0 when nothing is equipped).</summary>
        public static int CategoryModifier(CharacterData character, StatCategory category)
        {
            OutfitData outfit = GetEquipped(character);
            return outfit != null ? outfit.CategoryModifier(category) : 0;
        }

        /// <summary>Base stat plus the equipped outfit's modifier - what skill checks use.</summary>
        public static int EffectiveStat(CharacterData character, Stat stat)
            => character != null ? character.Get(stat) + Modifier(character, stat) : 0;

        /// <summary>Base category total plus the equipped outfit's modifiers - what skill checks use.</summary>
        public static int EffectiveCategory(CharacterData character, StatCategory category)
            => character != null ? character.GetCategory(category) + CategoryModifier(character, category) : 0;

        /// <summary>The equipped outfit's masculine modifier (0 when nothing is equipped).</summary>
        public static int MasculineModifier(CharacterData character)
        {
            OutfitData outfit = GetEquipped(character);
            return outfit != null ? outfit.MasculineModifier : 0;
        }

        /// <summary>The equipped outfit's feminine modifier (0 when nothing is equipped).</summary>
        public static int FeminineModifier(CharacterData character)
        {
            OutfitData outfit = GetEquipped(character);
            return outfit != null ? outfit.FeminineModifier : 0;
        }

        /// <summary>The character's masculine value plus the equipped outfit's modifier (never below 0).</summary>
        public static int EffectiveMasculine(CharacterData character)
            => character != null ? Mathf.Max(0, character.Masculine + MasculineModifier(character)) : 0;

        /// <summary>The character's feminine value plus the equipped outfit's modifier (never below 0).</summary>
        public static int EffectiveFeminine(CharacterData character)
            => character != null ? Mathf.Max(0, character.Feminine + FeminineModifier(character)) : 0;

        /// <summary>The character's worn world sprite, or null when nothing is equipped.</summary>
        public static Sprite WornSprite(CharacterData character)
        {
            OutfitData outfit = GetEquipped(character);
            return outfit != null ? outfit.GetSprite(character) : null;
        }
    }
}
