#if FMOD_PRESENT
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace JamTemplate.Audio
{
    /// <summary>
    /// FMOD-backed audio service. A drop-in alternative to <see cref="AudioManager"/>
    /// selected at compile time by the FMOD_PRESENT define: it satisfies the same
    /// cross-cutting seams (UI one-shots, transition fades, per-category volume) and
    /// the same portable <see cref="AudioEvent"/> gameplay API, so Menus/Settings/
    /// SceneManagement and jam code never change when a project swaps backend.
    ///
    /// Built WebGL-first, because browsers impose three constraints the desktop
    /// backend never hits:
    ///   * Banks load asynchronously over several frames — playback must wait until
    ///     <see cref="banksReady"/>; calls made before then are queued (music/ambiance)
    ///     or dropped (transient one-shots) rather than thrown away as exceptions.
    ///   * The audio context stays suspended until the first real user gesture, so
    ///     <see cref="Update"/> watches for the first key/click/tap/button press and
    ///     then resumes the mixer (see <see cref="NotifyUserGesture"/>).
    ///   * Focus/visibility changes must suspend and resume the mixer.
    /// Author banks non-streaming / loaded to RAM so the readiness wait is bounded.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/FMOD Audio Manager")]
    [DisallowMultipleComponent]
    public class FmodAudioManager : MonoBehaviour
    {
        /// <summary>The active FMOD Audio Manager. Persists across scene loads.</summary>
        public static FmodAudioManager Instance { get; private set; }

        [Header("Banks")]
        [SerializeField]
        [Tooltip("Bank files to load at startup (without the .bank extension). The " +
                 "Master bank and its .strings bank are required; add more as needed.")]
        private string[] banks = { "Master", "Master.strings" };

        [Header("Mixer Routing")]
        [SerializeField]
        [Tooltip("FMOD path of the master bus — folds in master volume and the global fade.")]
        private string masterBusPath = "bus:/";

        [SerializeField] private string sfxVca = "vca:/Sfx";
        [SerializeField] private string musicVca = "vca:/Music";
        [SerializeField] private string ambianceVca = "vca:/Ambiance";
        [SerializeField] private string dialogueVca = "vca:/Dialogue";
        [SerializeField] private string uiVca = "vca:/UI";

        [Header("UI")]
        [SerializeField]
        [Tooltip("Event played for menu button select/press feedback. The clip passed " +
                 "through the UI-sound seam is ignored under FMOD; this one event stands " +
                 "in for menu clicks.")]
        private EventReference uiClickEvent;

        // Per-category linear volumes / mutes, indexed by AudioCategory. FMOD VCA and
        // bus volumes are linear gains (1 = unity), so no decibel conversion is needed.
        private readonly float[] volumes = { 1f, 1f, 1f, 1f, 1f };
        private readonly bool[] muted = new bool[5];
        private float masterVolume = 1f;
        private float globalFade = 1f;
        private Coroutine globalFadeRoutine;

        private Bus masterBus;
        private readonly VCA[] vcas = new VCA[5];
        private bool busesResolved;
        private bool banksReady;
        private bool resumed;

        // Looping channels held as event instances.
        private EventInstance musicInstance;
        private EventInstance ambianceInstance;

        // Music/ambiance requested before banks finish loading, replayed once ready.
        private AudioEvent pendingMusic;
        private AudioEvent pendingAmbiance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(LoadBanksThenInit());
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            ReleaseInstance(ref musicInstance);
            ReleaseInstance(ref ambianceInstance);
            Instance = null;
        }

        // --- WebGL-safe bank loading -------------------------------------------

        private IEnumerator LoadBanksThenInit()
        {
            foreach (string bank in banks)
            {
                try
                {
                    RuntimeManager.LoadBank(bank, loadSamples: true);
                }
                catch (BankLoadException e)
                {
                    Debug.LogError($"[FmodAudio] Failed to load bank '{bank}': {e.Message}", this);
                }
            }

            // On WebGL banks stream in over several frames; wait for all of them and
            // their sample data before resolving buses or playing anything.
            while (!RuntimeManager.HaveAllBanksLoaded)
                yield return null;
            while (RuntimeManager.AnySampleDataLoading())
                yield return null;

            ResolveBuses();
            banksReady = true;
            ApplyAll();

            // Honour anything requested while banks were still loading.
            if (pendingMusic != null) { AudioEvent e = pendingMusic; pendingMusic = null; PlayMusic(e); }
            if (pendingAmbiance != null) { AudioEvent e = pendingAmbiance; pendingAmbiance = null; PlayAmbiance(e); }
        }

        private void ResolveBuses()
        {
            masterBus = RuntimeManager.GetBus(masterBusPath);
            vcas[(int)AudioCategory.Sfx] = RuntimeManager.GetVCA(sfxVca);
            vcas[(int)AudioCategory.Music] = RuntimeManager.GetVCA(musicVca);
            vcas[(int)AudioCategory.Ambiance] = RuntimeManager.GetVCA(ambianceVca);
            vcas[(int)AudioCategory.Dialogue] = RuntimeManager.GetVCA(dialogueVca);
            vcas[(int)AudioCategory.Ui] = RuntimeManager.GetVCA(uiVca);
            busesResolved = true;
        }

        // --- Autoplay / focus (WebGL) ------------------------------------------

        private void Update()
        {
            // Browsers keep the audio context suspended until the first real user
            // gesture — the #1 cause of a silent web build. Watch for one directly
            // rather than piggybacking on UI sounds, which only fire when a menu
            // button has a clip assigned (it won't under FMOD) and can mis-fire on
            // programmatic auto-selection before the player has actually interacted.
            if (!resumed && DetectFirstGesture())
                NotifyUserGesture();
        }

        private static bool DetectFirstGesture()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
                return true;

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                return true;

            Touchscreen touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
                return true;

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && (gamepad.buttonSouth.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame))
                return true;

            return false;
#else
            return Input.anyKeyDown
                || Input.GetMouseButtonDown(0)
                || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
#endif
        }

        /// <summary>
        /// Resumes the audio mixer on the first real user gesture (called from
        /// <see cref="Update"/>, and available to call manually). Idempotent — only
        /// the first call resumes; later calls no-op.
        /// </summary>
        public void NotifyUserGesture()
        {
            if (resumed)
                return;
            resumed = true;
            RuntimeManager.CoreSystem.mixerResume();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Don't pre-empt the first-gesture resume; the context is still suspended.
            if (!resumed)
                return;
            if (hasFocus)
                RuntimeManager.CoreSystem.mixerResume();
            else
                RuntimeManager.CoreSystem.mixerSuspend();
        }

        private void OnApplicationPause(bool paused)
        {
            if (!resumed)
                return;
            if (paused)
                RuntimeManager.CoreSystem.mixerSuspend();
            else
                RuntimeManager.CoreSystem.mixerResume();
        }

        // --- AudioEvent (portable, backend-neutral) -----------------------------

        /// <summary>Plays an <see cref="AudioEvent"/>, dispatched by its category.</summary>
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
                default:
                    PlayOneShot(audioEvent.fmodEvent, audioEvent.volume);
                    break;
            }
        }

        /// <summary>Switches looping music to an <see cref="AudioEvent"/>'s FMOD event.</summary>
        public void PlayMusic(AudioEvent audioEvent)
        {
            if (audioEvent == null) { StopMusic(); return; }
            if (!banksReady) { pendingMusic = audioEvent; return; }

            StopInstance(ref musicInstance);
            musicInstance = StartLooping(audioEvent.fmodEvent, audioEvent.volume);
        }

        /// <summary>Fades out the current music.</summary>
        public void StopMusic()
        {
            pendingMusic = null;
            StopInstance(ref musicInstance);
        }

        /// <summary>Switches the looping ambiance bed to an <see cref="AudioEvent"/>'s FMOD event.</summary>
        public void PlayAmbiance(AudioEvent audioEvent)
        {
            if (audioEvent == null) { StopAmbiance(); return; }
            if (!banksReady) { pendingAmbiance = audioEvent; return; }

            StopInstance(ref ambianceInstance);
            ambianceInstance = StartLooping(audioEvent.fmodEvent, audioEvent.volume);
        }

        /// <summary>Fades out the current ambiance.</summary>
        public void StopAmbiance()
        {
            pendingAmbiance = null;
            StopInstance(ref ambianceInstance);
        }

        // --- One-shots ----------------------------------------------------------

        /// <summary>
        /// Plays the configured UI feedback event. The <paramref name="clip"/> from the
        /// seam is ignored — FMOD plays its own authored event. (Resuming a suspended
        /// browser audio context is handled centrally in <see cref="Update"/>, not here,
        /// because menu buttons often carry no clip under FMOD and so never call this.)
        /// </summary>
        public void PlayUi(AudioClip clip, float volume = 1f)
        {
            PlayOneShot(uiClickEvent, volume);
        }

        private void PlayOneShot(EventReference reference, float volume)
        {
            // Transient one-shots can't be meaningfully deferred, so drop them until
            // banks are ready rather than queueing stale fire-and-forget sounds.
            if (reference.IsNull || !banksReady)
                return;

            EventInstance instance = RuntimeManager.CreateInstance(reference);
            instance.setVolume(Mathf.Clamp01(volume));
            instance.start();
            instance.release(); // frees the instance once it finishes playing
        }

        private EventInstance StartLooping(EventReference reference, float volume)
        {
            if (reference.IsNull)
                return default;

            EventInstance instance = RuntimeManager.CreateInstance(reference);
            instance.setVolume(Mathf.Clamp01(volume));
            instance.start();
            return instance;
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
            ApplyMaster();
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
                ApplyMaster();
                yield return null;
            }

            globalFade = target;
            ApplyMaster();
            globalFadeRoutine = null;
        }

        // --- Apply to FMOD ------------------------------------------------------

        private void Apply(AudioCategory category)
        {
            if (!busesResolved)
                return;

            VCA vca = vcas[(int)category];
            if (vca.isValid())
                vca.setVolume(muted[(int)category] ? 0f : volumes[(int)category]);
        }

        private void ApplyMaster()
        {
            if (busesResolved && masterBus.isValid())
                masterBus.setVolume(masterVolume * globalFade);
        }

        private void ApplyAll()
        {
            ApplyMaster();
            for (int i = 0; i < volumes.Length; i++)
                Apply((AudioCategory)i);
        }

        // --- Instance helpers ---------------------------------------------------

        private static void StopInstance(ref EventInstance instance)
        {
            if (!instance.isValid())
                return;

            instance.stop(STOP_MODE.ALLOWFADEOUT);
            instance.release();
            instance = default;
        }

        private static void ReleaseInstance(ref EventInstance instance)
        {
            if (!instance.isValid())
                return;

            instance.stop(STOP_MODE.IMMEDIATE);
            instance.release();
            instance = default;
        }
    }
}
#endif
