using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// A pool of outfits a character can choose from - the outfit menu lists these in order. Each
    /// playable character points at their own wardrobe (see <see cref="CharacterData.Wardrobe"/>), and
    /// remembers which outfit they have equipped (see <see cref="Outfits"/>). Create via
    /// Assets > Create > Game > Wardrobe.
    /// </summary>
    [CreateAssetMenu(fileName = "Wardrobe", menuName = "Game/Wardrobe")]
    public class Wardrobe : ScriptableObject
    {
        [Tooltip("Outfits available in the outfit menu, in display order.")]
        [SerializeField] private List<OutfitData> outfits = new();

        /// <summary>The outfits available to the player, in menu order.</summary>
        public IReadOnlyList<OutfitData> Outfits => outfits;
    }
}
