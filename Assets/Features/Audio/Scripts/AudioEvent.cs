using UnityEngine;
#if FMOD_PRESENT
using FMODUnity;
#endif

namespace JamTemplate.Audio
{
    /// <summary>
    /// A portable, backend-neutral sound asset. Gameplay code plays audio by
    /// referencing one of these and calling <see cref="AudioManager.Play(AudioEvent)"/>
    /// or <see cref="AudioManager.PlayMusic(AudioEvent)"/>, so the same call sites
    /// keep working when a jam swaps its audio backend. With the default Unity
    /// backend the payload is <see cref="clip"/>; when the FMOD package is installed
    /// (FMOD_PRESENT) the <c>fmodEvent</c> reference is used instead.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioEvent", menuName = "Sailor Snouts/Audio Event")]
    public class AudioEvent : ScriptableObject
    {
        [Tooltip("Which mixer category this sound routes through.")]
        public AudioCategory category = AudioCategory.Sfx;

        [Header("Playback")]
        [Range(0f, 1f)]
        [Tooltip("Volume scale applied to one-shots (0 = silent, 1 = full).")]
        public float volume = 1f;

        [Min(0f)]
        [Tooltip("Random pitch offset for one-shot Sfx (e.g. 0.1 = ±10%) so rapid repeats don't machine-gun. Ignored by looping music/ambiance.")]
        public float pitchVariation;

        [Header("Unity Audio")]
        [Tooltip("The clip played by Unity's built-in audio backend.")]
        public AudioClip clip;

#if FMOD_PRESENT
        [Header("FMOD")]
        [Tooltip("The FMOD event played when the FMOD backend is active.")]
        public EventReference fmodEvent;
#endif
    }
}
