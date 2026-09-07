using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// A set of world sprites, one per facing direction. An unset direction falls back to Down, so you
    /// only have to author the directions you actually have. Reused by <see cref="CharacterData"/> (a
    /// character's base look) and <see cref="OutfitData"/> (a worn look), so the facing-to-sprite
    /// selection lives in one place instead of being duplicated.
    /// </summary>
    [Serializable]
    public struct DirectionalSprites
    {
        [SerializeField] private Sprite down;
        [SerializeField] private Sprite up;
        [SerializeField] private Sprite left;
        [SerializeField] private Sprite right;

        /// <summary>The Down sprite (the default look), or null if unset.</summary>
        public Sprite DownSprite => down;

        /// <summary>
        /// The sprite for a facing direction, falling back to Down when that direction is unset. May
        /// still be null if even Down is unset, so callers can layer their own final fallback.
        /// </summary>
        public Sprite Get(Facing4 facing)
        {
            Sprite chosen = facing switch
            {
                Facing4.Up => up,
                Facing4.Left => left,
                Facing4.Right => right,
                _ => down,
            };
            return chosen != null ? chosen : down;
        }
    }
}
