using System;
using UnityEngine;

namespace JamTemplate.SceneManagement
{
    /// <summary>Fades the overlay colour in and out. The default transition.</summary>
    [Serializable]
    public sealed class FadeTransitionEffect : SceneTransitionEffect
    {
        public override void ResetState(SceneTransitionSurface surface)
        {
            surface.Panel.localScale = Vector3.one;
            surface.Panel.anchoredPosition = Vector2.zero;
            surface.Group.alpha = 0f;
        }

        public override void Apply(SceneTransitionSurface surface, float covered)
        {
            surface.Group.alpha = Mathf.Clamp01(covered);
        }
    }
}
