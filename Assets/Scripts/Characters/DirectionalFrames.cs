using System;
using UnityEngine;

namespace Game
{
    // A list of animation frames per facing direction. No fallback between directions: an empty
    // direction means "no animation" and callers show the standing sprite instead.
    [Serializable]
    public struct DirectionalFrames
    {
        [SerializeField] private Sprite[] down;
        [SerializeField] private Sprite[] up;
        [SerializeField] private Sprite[] left;
        [SerializeField] private Sprite[] right;

        public Sprite[] Get(Facing4 facing) => facing switch
        {
            Facing4.Up => up,
            Facing4.Left => left,
            Facing4.Right => right,
            _ => down,
        };
    }
}
