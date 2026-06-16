#if FMOD_PRESENT
using ActiveAudioManager = JamTemplate.Audio.FmodAudioManager;
#else
using ActiveAudioManager = JamTemplate.Audio.AudioManager;
#endif

namespace JamTemplate.Audio
{
    /// <summary>
    /// Backend-neutral entry point for gameplay audio. Jam code plays sounds with
    /// <c>GameAudio.Play(audioEvent)</c> / <c>GameAudio.PlayMusic(audioEvent)</c> and
    /// never changes when the project swaps its audio backend (Unity built-in ↔ FMOD):
    /// the call routes to whichever manager the FMOD_PRESENT define selected at
    /// compile time. Calls made before the manager exists are silently ignored, so
    /// gameplay never has to null-check the audio system.
    ///
    /// Cross-cutting audio (menu sounds, transition fades, volume sliders) does NOT
    /// go through here — it uses the decoupling seams wired in FeatureWiring. This
    /// facade is for gameplay code that deliberately depends on the Audio assembly.
    /// (Named <c>GameAudio</c>, not <c>Audio</c>: a type called <c>Audio</c> inside
    /// the <c>JamTemplate.Audio</c> namespace is shadowed by that namespace segment
    /// when referenced from other <c>JamTemplate.*</c> code.)
    /// </summary>
    public static class GameAudio
    {
        private static ActiveAudioManager Manager => ActiveAudioManager.Instance;

        /// <summary>Plays an <see cref="AudioEvent"/>, dispatched by its category.</summary>
        public static void Play(AudioEvent audioEvent)
        {
            ActiveAudioManager m = Manager;
            if (m != null)
                m.Play(audioEvent);
        }

        /// <summary>Switches looping music to an <see cref="AudioEvent"/>.</summary>
        public static void PlayMusic(AudioEvent audioEvent)
        {
            ActiveAudioManager m = Manager;
            if (m != null)
                m.PlayMusic(audioEvent);
        }

        /// <summary>Switches the looping ambiance bed to an <see cref="AudioEvent"/>.</summary>
        public static void PlayAmbiance(AudioEvent audioEvent)
        {
            ActiveAudioManager m = Manager;
            if (m != null)
                m.PlayAmbiance(audioEvent);
        }

        /// <summary>Fades out the current music.</summary>
        public static void StopMusic()
        {
            ActiveAudioManager m = Manager;
            if (m != null)
                m.StopMusic();
        }

        /// <summary>Fades out the current ambiance.</summary>
        public static void StopAmbiance()
        {
            ActiveAudioManager m = Manager;
            if (m != null)
                m.StopAmbiance();
        }

        /// <summary>
        /// Fades all audio out over <paramref name="duration"/> seconds — for deaths,
        /// cutscenes, or pauses. Pair with <see cref="FadeIn"/> to bring it back.
        /// (Scene-transition fades are handled separately by SceneManagement; per-category
        /// volume is owned by Settings, so it isn't exposed here.)
        /// </summary>
        public static void FadeOut(float duration)
        {
            ActiveAudioManager m = Manager;
            if (m != null)
                m.FadeOut(duration);
        }

        /// <summary>Fades all audio back in over <paramref name="duration"/> seconds.</summary>
        public static void FadeIn(float duration)
        {
            ActiveAudioManager m = Manager;
            if (m != null)
                m.FadeIn(duration);
        }
    }
}
