using System;
using UnityEngine;

namespace JamTemplate.SceneManagement
{
    public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down,
    }

    /// <summary>Slides the overlay panel across the screen from one edge.</summary>
    [Serializable]
    public sealed class SlideTransitionEffect : SceneTransitionEffect
    {
        [Tooltip("Edge the covering panel slides in from.")]
        public SlideDirection direction = SlideDirection.Left;

        public override void ResetState(SceneTransitionSurface surface)
        {
            surface.Panel.localScale = Vector3.one;
            surface.Group.alpha = 1f;
            Apply(surface, 0f);
        }

        public override void Apply(SceneTransitionSurface surface, float covered)
        {
            Vector2 size = surface.Canvas.rect.size;
            Vector2 hidden;
            switch (direction)
            {
                case SlideDirection.Right: hidden = new Vector2(size.x, 0f); break;
                case SlideDirection.Up: hidden = new Vector2(0f, size.y); break;
                case SlideDirection.Down: hidden = new Vector2(0f, -size.y); break;
                default: hidden = new Vector2(-size.x, 0f); break;
            }

            surface.Panel.anchoredPosition = Vector2.LerpUnclamped(hidden, Vector2.zero, covered);
        }
    }
}
