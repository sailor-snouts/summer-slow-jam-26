using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// The shared pool of outfits the player can choose from - the outfit menu lists these in order.
    /// One wardrobe is shared by both playable characters; each character remembers which outfit it
    /// has equipped (see <see cref="Outfits"/>). Create via Assets > Create > Game > Wardrobe.
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
