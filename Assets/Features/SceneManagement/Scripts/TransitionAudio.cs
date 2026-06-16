using System;

namespace JamTemplate.SceneManagement
{
    /// <summary>
    /// Decoupling seam for the audio fade that accompanies scene transitions, so
    /// the SceneManagement assembly doesn't depend on the Audio assembly. The
    /// Audio feature sets the providers (the AudioManager does this in its Awake).
    /// With nothing wired up, transitions simply run without an audio fade.
    /// </summary>
    public static class TransitionAudio
    {
        /// <summary>Set by the Audio feature: fades all audio out over a duration in seconds.</summary>
        public static Action<float> FadeOutProvider;

        /// <summary>Set by the Audio feature: fades all audio back in over a duration in seconds.</summary>
        public static Action<float> FadeInProvider;

        /// <summary>Whether an audio fade is available (an AudioManager exists).</summary>
        public static bool IsAvailable => FadeOutProvider != null && FadeInProvider != null;

        /// <summary>Fades all audio out, if a provider is wired up.</summary>
        public static void FadeOut(float duration) => FadeOutProvider?.Invoke(duration);

        /// <summary>Fades all audio back in, if a provider is wired up.</summary>
        public static void FadeIn(float duration) => FadeInProvider?.Invoke(duration);
    }
}
