using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Wardrobe", menuName = "Game/Wardrobe")]
    public class Wardrobe : ScriptableObject
    {
        [Tooltip("Outfits available in the outfit menu, in display order.")]
        [SerializeField] private List<OutfitData> outfits = new();

        public IReadOnlyList<OutfitData> Outfits => outfits;
    }
}
