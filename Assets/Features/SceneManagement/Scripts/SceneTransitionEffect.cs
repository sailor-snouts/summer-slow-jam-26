using System;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace JamTemplate.SceneManagement
{
    /// <summary>The overlay objects a transition effect animates.</summary>
    public sealed class SceneTransitionSurface
    {
        public CanvasGroup Group { get; }
        public RectTransform Panel { get; }
        public RectTransform Canvas { get; }

        public SceneTransitionSurface(CanvasGroup group, RectTransform panel, RectTransform canvas)
        {
            Group = group;
            Panel = panel;
            Canvas = canvas;
        }
    }

    public enum CurvePreset
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Smooth,
    }

    /// <summary>Ready-made easing curves for transition effects.</summary>
    public static class TransitionCurves
    {
        public static AnimationCurve Linear() => AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public static AnimationCurve EaseIn() =>
            new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 2f, 2f));

        public static AnimationCurve EaseOut() =>
            new AnimationCurve(new Keyframe(0f, 0f, 2f, 2f), new Keyframe(1f, 1f, 0f, 0f));

        public static AnimationCurve EaseInOut() => AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public static AnimationCurve Smooth() => new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(0.5f, 0.5f, 1.6f, 1.6f),
            new Keyframe(1f, 1f, 0f, 0f));

        public static AnimationCurve Get(CurvePreset preset)
        {
            switch (preset)
            {
                case CurvePreset.Linear: return Linear();
                case CurvePreset.EaseIn: return EaseIn();
                case CurvePreset.EaseOut: return EaseOut();
                case CurvePreset.Smooth: return Smooth();
                default: return EaseInOut();
            }
        }
    }

    /// <summary>
    /// Base class for scene-transition effects. Concrete effects are chosen
    /// polymorphically via [SerializeReference], so a new effect is just a new
    /// subclass.
    /// </summary>
    [Serializable]
    public abstract class SceneTransitionEffect
    {
        [Min(0f)]
        [Tooltip("Seconds to cover the screen before the next scene loads.")]
        public float coverDuration = 0.35f;

        [Min(0f)]
        [Tooltip("Seconds to reveal the new scene once it has loaded.")]
        public float revealDuration = 0.35f;

        [Tooltip("Easing applied across the transition (time 0..1, value 0..1).")]
        public AnimationCurve curve = TransitionCurves.EaseInOut();

#if ODIN_INSPECTOR
        [Button("Apply Preset Curve")]
#endif
        private void ApplyPreset(CurvePreset preset)
        {
            curve = TransitionCurves.Get(preset);
        }

        /// <summary>Snap the surface to its fully-revealed (transparent) state.</summary>
        public abstract void ResetState(SceneTransitionSurface surface);

        /// <summary>
        /// Apply the visual state for the given coverage: 0 = scene fully visible,
        /// 1 = screen fully covered.
        /// </summary>
        public abstract void Apply(SceneTransitionSurface surface, float covered);
    }
}
