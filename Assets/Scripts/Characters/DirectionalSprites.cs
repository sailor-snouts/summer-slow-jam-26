using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public struct DirectionalSprites
    {
        [SerializeField] private Sprite down;
        [SerializeField] private Sprite up;
        [SerializeField] private Sprite left;
        [SerializeField] private Sprite right;

        public Sprite DownSprite => down;

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
