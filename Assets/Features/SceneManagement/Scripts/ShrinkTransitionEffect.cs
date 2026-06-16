using System;
using UnityEngine;

namespace JamTemplate.SceneManagement
{
    /// <summary>Scales the overlay panel up to cover the screen and down to reveal it.</summary>
    [Serializable]
    public sealed class ShrinkTransitionEffect : SceneTransitionEffect
    {
        public override void ResetState(SceneTransitionSurface surface)
        {
            surface.Panel.anchoredPosition = Vector2.zero;
            surface.Group.alpha = 1f;
            surface.Panel.localScale = Vector3.zero;
        }

        public override void Apply(SceneTransitionSurface surface, float covered)
        {
            float scale = Mathf.Max(0f, covered);
            surface.Panel.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
