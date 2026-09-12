using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class Outfits
    {
        private static readonly Dictionary<CharacterData, OutfitData> equipped = new();

        public static event Action<CharacterData> Changed;

        // Clear static state on play start so nothing leaks between sessions (domain reload may be disabled).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            equipped.Clear();
            Changed = null;
        }

        public static OutfitData GetEquipped(CharacterData character)
            => character != null && equipped.TryGetValue(character, out OutfitData outfit) ? outfit : null;

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

        public static int Modifier(CharacterData character, Stat stat)
        {
            OutfitData outfit = GetEquipped(character);
            return outfit != null ? outfit.Modifier(stat) : 0;
        }

        public static int CategoryModifier(CharacterData character, StatCategory category)
        {
            OutfitData outfit = GetEquipped(character);
            return outfit != null ? outfit.CategoryModifier(category) : 0;
        }

        public static int EffectiveStat(CharacterData character, Stat stat)
            => character != null ? character.Get(stat) + Modifier(character, stat) : 0;

        public static int EffectiveCategory(CharacterData character, StatCategory category)
            => character != null ? character.GetCategory(category) + CategoryModifier(character, category) : 0;

        public static int MasculineModifier(CharacterData character)
        {
            OutfitData outfit = GetEquipped(character);
            return outfit != null ? outfit.MasculineModifier : 0;
        }

        public static int FeminineModifier(CharacterData character)
        {
            OutfitData outfit = GetEquipped(character);
            return outfit != null ? outfit.FeminineModifier : 0;
        }

        public static int EffectiveMasculine(CharacterData character)
            => character != null ? Mathf.Max(0, character.Masculine + MasculineModifier(character)) : 0;

        public static int EffectiveFeminine(CharacterData character)
            => character != null ? Mathf.Max(0, character.Feminine + FeminineModifier(character)) : 0;

        public static Sprite WornSprite(CharacterData character, Facing4 facing)
        {
            OutfitData outfit = GetEquipped(character);
            return outfit != null ? outfit.GetSprite(facing) : null;
        }
    }
}
