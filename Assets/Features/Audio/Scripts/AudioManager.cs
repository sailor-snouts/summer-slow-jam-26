using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace JamTemplate.Audio
{
    /// <summary>Mixer routing for one audio category.</summary>
    [System.Serializable]
    public class AudioBus
    {
        [Tooltip("Mixer group this category's audio sources route through.")]
        public AudioMixerGroup group;

        [Tooltip("Name of the exposed mixer parameter that controls this category's volume (in dB).")]
        public string volumeParameter;
    }

    /// <summary>
    /// Central audio service. Splits audio across five categories routed through a
    /// Unity AudioMixer, plays one-shots and looping music/ambiance with fades and
    /// crossfades, and controls per-category volume and mute. Persists across scene
    /// loads, so music carries through scene transitions.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Audio Manager")]
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        /// <summary>The active Audio Manager. Persists across scene loads.</summary>
        public static AudioManager Instance { get; private set; }

        [Header("Mixer")]
        [SerializeField]
        [Tooltip("The AudioMixer the category groups belong to.")]
        private AudioMixer mixer;

        [Header("Category Routing")]
        [SerializeField] private AudioBus sfx = new AudioBus { volumeParameter = "SfxVolume" };
        [SerializeField] private AudioBus music = new AudioBus { volumeParameter = "MusicVolume" };
        [SerializeField] private AudioBus ambiance = new AudioBus { volumeParameter = "AmbianceVolume" };
        [SerializeField] private AudioBus dialogue = new AudioBus { volumeParameter = "DialogueVolume" };
        [SerializeField] private AudioBus ui = new AudioBus { volumeParameter = "UiVolume" };

        [Header("Defaults")]
        [SerializeField]
        [Min(0f)]
        [Tooltip("Fade/crossfade duration used by Play and Stop calls that do not specify one.")]
        private float defaultFadeDuration = 1f;

        private LoopingChannel musicChannel;
        private LoopingChannel ambianceChannel;
        private AudioSource sfxSource;
        private AudioSource dialogueSource;
        private AudioSource uiSource;

        private const string MasterParameter = "MasterVolume";

        private readonly float[] volumes = { 1f, 1f, 1f, 1f, 1f };
        private readonly bool[] muted = new bool[5];
        private float masterVolume = 1f;
        private float globalFade = 1f;
        private bool hasMasterParameter;
        private Coroutine globalFadeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicChannel = new LoopingChannel(this, "Music", music.group);
            ambianceChannel = new LoopingChannel(this, "Ambiance", ambiance.group);
            sfxSource = CreateSource("SFX", sfx.group, false);
            dialogueSource = CreateSource("Dialogue", dialogue.group, false);
            uiSource = CreateSource("UI", ui.group, false);

            ValidateSetup();
            ApplyAll();
        }

        private void Start()
        {
            // Mixer values written during Awake (which runs before the first scene
            // when bootstrapped) can be reset as the mixer initialises; re-apply
            // once it is live.
            ApplyAll();
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            Instance = null;
        }

        // --- Music & ambiance ---------------------------------------------------

        /// <summary>Crossfades to <paramref name="clip"/> as looping music.</summary>
        public void PlayMusic(AudioClip clip) => musicChannel.Play(clip, defaultFadeDuration);

        /// <inheritdoc cref="PlayMusic(AudioClip)"/>
        public void PlayMusic(AudioClip clip, float fadeDuration) => musicChannel.Play(clip, fadeDuration);

        /// <summary>Fades out the current music.</summary>
        public void StopMusic() => musicChannel.Stop(defaultFadeDuration);

        /// <inheritdoc cref="StopMusic()"/>
        public void StopMusic(float fadeDuration) => musicChannel.Stop(fadeDuration);

        /// <summary>Crossfades to <paramref name="clip"/> as a looping ambiance bed.</summary>
        public void PlayAmbiance(AudioClip clip) => ambianceChannel.Play(clip, defaultFadeDuration);

        /// <inheritdoc cref="PlayAmbiance(AudioClip)"/>
        public void PlayAmbiance(AudioClip clip, float fadeDuration) => ambianceChannel.Play(clip, fadeDuration);

        /// <summary>Fades out the current ambiance.</summary>
        public void StopAmbiance() => ambianceChannel.Stop(defaultFadeDuration);

        /// <inheritdoc cref="StopAmbiance()"/>
        public void StopAmbiance(float fadeDuration) => ambianceChannel.Stop(fadeDuration);

        // --- One-shots ----------------------------------------------------------

        /// <summary>Plays a one-shot sound effect.</summary>
        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
                return;

            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        /// <summary>
        /// Plays a one-shot sound effect with a random pitch offset (e.g. 0.1 = ±10%)
        /// so rapid repeats of the same clip don't machine-gun.
        /// </summary>
        public void PlaySfx(AudioClip clip, float volume, float pitchVariation)
        {
            if (clip == null)
                return;

            sfxSource.pitch = 1f + Random.Range(-Mathf.Abs(pitchVariation), Mathf.Abs(pitchVariation));
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        /// <summary>Plays a one-shot UI sound.</summary>
        public void PlayUi(AudioClip clip, float volume = 1f)
        {
            if (clip != null)
                uiSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        /// <summary>Plays a dialogue line, replacing any line currently playing.</summary>
        public void PlayDialogue(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
                return;

            dialogueSource.Stop();
            dialogueSource.clip = clip;
            dialogueSource.volume = Mathf.Clamp01(volume);
            dialogueSource.Play();
        }

        /// <summary>Stops the current dialogue line.</summary>
        public void StopDialogue() => dialogueSource.Stop();

        // --- AudioEvent (portable, backend-neutral) -----------------------------

        /// <summary>
        /// Plays an <see cref="AudioEvent"/>, dispatched by its category: Music and
        /// Ambiance events loop with a crossfade (as <see cref="PlayMusic(AudioEvent)"/>
        /// / <see cref="PlayAmbiance(AudioEvent)"/>); everything else is a one-shot.
        /// This is the call jam code uses so swapping the audio backend never touches
        /// the call site.
        /// </summary>
        public void Play(AudioEvent audioEvent)
        {
            if (audioEvent == null)
                return;

            switch (audioEvent.category)
            {
                case AudioCategory.Music:
                    PlayMusic(audioEvent);
                    break;
                case AudioCategory.Ambiance:
                    PlayAmbiance(audioEvent);
                    break;
                case AudioCategory.Dialogue:
                    PlayDialogue(audioEvent.clip, audioEvent.volume);
                    break;
                case AudioCategory.Ui:
                    PlayUi(audioEvent.clip, audioEvent.volume);
                    break;
                default:
                    PlaySfx(audioEvent.clip, audioEvent.volume, audioEvent.pitchVariation);
                    break;
            }
        }

        /// <summary>Crossfades to an <see cref="AudioEvent"/>'s clip as looping music.</summary>
        public void PlayMusic(AudioEvent audioEvent)
        {
            if (audioEvent != null)
                musicChannel.Play(audioEvent.clip, defaultFadeDuration);
        }

        /// <summary>Crossfades to an <see cref="AudioEvent"/>'s clip as a looping ambiance bed.</summary>
        public void PlayAmbiance(AudioEvent audioEvent)
        {
            if (audioEvent != null)
                ambianceChannel.Play(audioEvent.clip, defaultFadeDuration);
        }

        // --- Volume & mute ------------------------------------------------------

        /// <summary>Sets a category's volume (0 = silent, 1 = full).</summary>
        public void SetVolume(AudioCategory category, float volume01)
        {
            volumes[(int)category] = Mathf.Clamp01(volume01);
            Apply(category);
        }

        /// <summary>Returns a category's volume (0 to 1).</summary>
        public float GetVolume(AudioCategory category) => volumes[(int)category];

        /// <summary>Sets the master volume that scales every category (0 = silent, 1 = full).</summary>
        public void SetMasterVolume(float volume01)
        {
            masterVolume = Mathf.Clamp01(volume01);
            ApplyAll();
        }

        /// <summary>Returns the master volume (0 to 1).</summary>
        public float GetMasterVolume() => masterVolume;

        /// <summary>Mutes or unmutes a category without losing its volume setting.</summary>
        public void SetMute(AudioCategory category, bool mute)
        {
            muted[(int)category] = mute;
            Apply(category);
        }

        /// <summary>Returns whether a category is muted.</summary>
        public bool IsMuted(AudioCategory category) => muted[(int)category];

        /// <summary>Flips a category's mute state.</summary>
        public void ToggleMute(AudioCategory category) => SetMute(category, !muted[(int)category]);

        // --- Global fade --------------------------------------------------------

        /// <summary>Fades all audio out over <paramref name="duration"/> seconds.</summary>
        public void FadeOut(float duration) => FadeGlobalTo(0f, duration);

        /// <summary>Fades all audio back in over <paramref name="duration"/> seconds.</summary>
        public void FadeIn(float duration) => FadeGlobalTo(1f, duration);

        private void FadeGlobalTo(float target, float duration)
        {
            if (globalFadeRoutine != null)
                StopCoroutine(globalFadeRoutine);
            globalFadeRoutine = StartCoroutine(FadeGlobalRoutine(target, duration));
        }

        private IEnumerator FadeGlobalRoutine(float target, float duration)
        {
            float start = globalFade;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                globalFade = Mathf.Lerp(start, target, elapsed / duration);
                ApplyAll();
                yield return null;
            }

            globalFade = target;
            ApplyAll();
            globalFadeRoutine = null;
        }

        private void Apply(AudioCategory category)
        {
            if (mixer == null)
                return;

            string parameter = BusFor(category).volumeParameter;
            if (string.IsNullOrEmpty(parameter))
                return;

            float linear = muted[(int)category] ? 0f : volumes[(int)category];
            // Without an exposed master parameter, fold master and the global
            // fade into every category instead.
            if (!hasMasterParameter)
                linear *= masterVolume * globalFade;
            mixer.SetFloat(parameter, LinearToDecibels(linear));
        }

        private void ApplyAll()
        {
            if (mixer != null && hasMasterParameter)
                mixer.SetFloat(MasterParameter, LinearToDecibels(masterVolume * globalFade));
            for (int i = 0; i < volumes.Length; i++)
                Apply((AudioCategory)i);
        }

        private AudioBus BusFor(AudioCategory category)
        {
            switch (category)
            {
                case AudioCategory.Music: return music;
                case AudioCategory.Ambiance: return ambiance;
                case AudioCategory.Dialogue: return dialogue;
                case AudioCategory.Ui: return ui;
                default: return sfx;
            }
        }

        private static float LinearToDecibels(float linear)
        {
            // -80 dB is the AudioMixer's effective silence floor.
            return linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
        }

        private void ValidateSetup()
        {
            if (mixer == null)
            {
                Debug.LogWarning("[Audio] No AudioMixer assigned — per-category volume and mute are disabled.", this);
                return;
            }

            hasMasterParameter = mixer.GetFloat(MasterParameter, out _);

            for (int i = 0; i < volumes.Length; i++)
            {
                string parameter = BusFor((AudioCategory)i).volumeParameter;
                if (!string.IsNullOrEmpty(parameter) && !mixer.GetFloat(parameter, out _))
                    Debug.LogWarning($"[Audio] Mixer parameter '{parameter}' is not exposed on '{mixer.name}'.", this);
            }
        }

        private AudioSource CreateSource(string label, AudioMixerGroup group, bool loop)
        {
            var child = new GameObject(label);
            child.transform.SetParent(transform, false);

            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.outputAudioMixerGroup = group;
            return source;
        }

        /// <summary>A looping audio channel that crossfades between clips.</summary>
        private sealed class LoopingChannel
        {
            private readonly AudioManager owner;
            private readonly AudioSource a;
            private readonly AudioSource b;
            private bool activeIsA;
            private bool stopping;
            private Coroutine fade;

            public LoopingChannel(AudioManager owner, string label, AudioMixerGroup group)
            {
                this.owner = owner;
                a = owner.CreateSource(label + " A", group, true);
                b = owner.CreateSource(label + " B", group, true);
            }

            private AudioSource Active => activeIsA ? a : b;
            private AudioSource Standby => activeIsA ? b : a;

            public void Play(AudioClip clip, float fadeDuration)
            {
                if (clip == null)
                {
                    Stop(fadeDuration);
                    return;
                }
                if (Active.clip == clip && Active.isPlaying)
                {
                    // Mid-stop fade: cancel the stop and bring the track back
                    // instead of swallowing the call and fading to silence.
                    if (stopping)
                    {
                        stopping = false;
                        Restart(FadeTo(Active, 1f, fadeDuration));
                    }
                    return;
                }

                stopping = false;
                AudioSource outgoing = Active;
                AudioSource incoming = Standby;
                incoming.clip = clip;
                incoming.volume = 0f;
                incoming.Play();
                activeIsA = !activeIsA;

                Restart(Crossfade(outgoing, incoming, fadeDuration));
            }

            public void Stop(float fadeDuration)
            {
                stopping = true;
                Restart(FadeOutAndStop(fadeDuration));
            }

            private void Restart(IEnumerator routine)
            {
                if (fade != null)
                    owner.StopCoroutine(fade);
                fade = owner.StartCoroutine(routine);
            }

            private IEnumerator Crossfade(AudioSource from, AudioSource to, float duration)
            {
                float fromStart = from.volume;
                if (duration > 0f)
                {
                    float elapsed = 0f;
                    while (elapsed < duration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        float t = Mathf.Clamp01(elapsed / duration);
                        to.volume = t;
                        from.volume = fromStart * (1f - t);
                        yield return null;
                    }
                }

                to.volume = 1f;
                from.volume = 0f;
                from.Stop();
                from.clip = null;
                fade = null;
            }

            private IEnumerator FadeOutAndStop(float duration)
            {
                // Fade both sources — a Stop during a crossfade would otherwise
                // leave the outgoing source frozen mid-volume, then cut it.
                float startA = a.volume;
                float startB = b.volume;
                if (duration > 0f)
                {
                    float elapsed = 0f;
                    while (elapsed < duration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        float t = Mathf.Clamp01(elapsed / duration);
                        a.volume = startA * (1f - t);
                        b.volume = startB * (1f - t);
                        yield return null;
                    }
                }

                a.Stop();
                b.Stop();
                stopping = false;
                fade = null;
            }

            private IEnumerator FadeTo(AudioSource source, float target, float duration)
            {
                float start = source.volume;
                if (duration > 0f)
                {
                    float elapsed = 0f;
                    while (elapsed < duration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        source.volume = Mathf.Lerp(start, target, elapsed / duration);
                        yield return null;
                    }
                }

                source.volume = target;
                fade = null;
            }
        }
    }
}
