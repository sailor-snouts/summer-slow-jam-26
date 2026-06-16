using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JamTemplate.Settings
{
    /// <summary>
    /// Owns the player's settings: loads them on startup, applies them to the
    /// engine (audio, display, graphics) and persists them via PlayerPrefs.
    ///
    /// Editing flow for the settings screen:
    /// BeginEdit() copies the confirmed settings into <see cref="Working"/>, which
    /// the UI mutates. ApplyWorking() pushes Working to the engine but does not
    /// save. Confirm() saves it; Revert() restores the pre-apply settings. This
    /// backs the apply-then-confirm-with-timer dialog.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Settings Manager")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)] // Initialise before the settings screen and binders read it.
    public class SettingsManager : MonoBehaviour
    {
        private const string Prefix = "settings.";

        /// <summary>The active Settings Manager. Persists across scene loads.</summary>
        public static SettingsManager Instance { get; private set; }

        /// <summary>The saved, applied settings.</summary>
        public SettingsState Confirmed { get; private set; } = new SettingsState();

        /// <summary>The in-progress edit the settings UI reads and writes.</summary>
        public SettingsState Working { get; private set; } = new SettingsState();

        /// <summary>Raised when <see cref="Working"/> is replaced (e.g. on revert) so UI can re-sync.</summary>
        public event Action WorkingChanged;

        private SettingsState revertTarget;
        private SettingsState applied;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved preferences over the engine's current settings (so a first
            // run, or any value the player never changed, follows the engine), then
            // Start() applies them — that's what persists settings across sessions.
            Confirmed = Load();
            Working = Confirmed.Clone();
        }

        private void Start()
        {
            // Apply in Start so the Audio Manager's Awake has run and its instance exists.
            Apply(Confirmed);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // --- Edit lifecycle -----------------------------------------------------

        /// <summary>Begins editing: copies the confirmed settings into <see cref="Working"/>.</summary>
        public void BeginEdit()
        {
            Working = Confirmed.Clone();
            WorkingChanged?.Invoke();
        }

        /// <summary>Applies <see cref="Working"/> to the engine without saving, keeping a revert point.</summary>
        public void ApplyWorking()
        {
            revertTarget = Confirmed.Clone();
            applied = Working.Clone();
            Apply(applied);
        }

        /// <summary>Keeps the applied settings: saves what was applied (not edits made since) as confirmed.</summary>
        public void Confirm()
        {
            Confirmed = (applied ?? Working).Clone();
            applied = null;
            Save(Confirmed);
        }

        /// <summary>Reverts to the settings from before the last <see cref="ApplyWorking"/>.</summary>
        public void Revert()
        {
            if (revertTarget == null)
                return;

            applied = null;
            Apply(revertTarget);
            Working = revertTarget.Clone();
            WorkingChanged?.Invoke();
        }

        /// <summary>
        /// Resets <see cref="Working"/> to default volumes, UI scale and graphics,
        /// keeping the current resolution, window mode and quality level. Apply still
        /// decides whether anything reaches the engine.
        /// </summary>
        public void ResetWorking()
        {
            Working = new SettingsState
            {
                resolutionWidth = Working.resolutionWidth,
                resolutionHeight = Working.resolutionHeight,
                fullScreenMode = Working.fullScreenMode,
                qualityLevel = Working.qualityLevel,
            };
            WorkingChanged?.Invoke();
        }

        // --- Apply to engine ----------------------------------------------------

        private void Apply(SettingsState s)
        {
            ApplyAudio(s);

            // SetQualityLevel resets vSync and anti-aliasing to the level's
            // defaults, so override those afterwards.
            if (s.qualityLevel >= 0 && s.qualityLevel < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(s.qualityLevel, true);

            // Under URP, MSAA comes from the pipeline asset; QualitySettings.antiAliasing is ignored.
            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
                urp.msaaSampleCount = Mathf.Max(1, s.antiAliasing);
            else
                QualitySettings.antiAliasing = s.antiAliasing;

#if !UNITY_WEBGL
            // The browser owns vsync, frame pacing and the canvas size on WebGL —
            // forcing them does nothing, and SetResolution stomps the page layout.
            QualitySettings.vSyncCount = s.vSync ? 1 : 0;
            Application.targetFrameRate = s.targetFrameRate;

            if (s.resolutionWidth > 0 && s.resolutionHeight > 0)
                Screen.SetResolution(s.resolutionWidth, s.resolutionHeight, s.fullScreenMode);
            else
                Screen.fullScreenMode = s.fullScreenMode;
#endif

            SettingsHooks.ApplyUiScale?.Invoke(s.uiScale);
        }

        /// <summary>Applies just the audio volumes — also used for live slider preview.</summary>
        public void ApplyAudio(SettingsState s)
        {
            SettingsHooks.ApplyAudio?.Invoke(s);
        }

        /// <summary>
        /// Reads the engine's (and Audio Manager's) live settings into a fresh state.
        /// Lets the settings UI show real current values even when no manager is
        /// editing — e.g. when the Settings scene is opened on its own to test.
        /// </summary>
        public static SettingsState CaptureEngine()
        {
            var s = new SettingsState
            {
                // Screen.width/height is the actual window (or WebGL canvas);
                // Screen.currentResolution is the whole monitor when windowed.
                resolutionWidth = Screen.width,
                resolutionHeight = Screen.height,
                fullScreenMode = Screen.fullScreenMode,
                qualityLevel = QualitySettings.GetQualityLevel(),
                vSync = QualitySettings.vSyncCount > 0,
                targetFrameRate = Application.targetFrameRate,
                antiAliasing = QualitySettings.antiAliasing,
            };

            s.uiScale = SettingsHooks.GetUiScale != null ? SettingsHooks.GetUiScale() : 1f;

            if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
                s.antiAliasing = urp.msaaSampleCount > 1 ? urp.msaaSampleCount : 0;

            SettingsHooks.CaptureAudio?.Invoke(s);
            return s;
        }

        // --- Persistence --------------------------------------------------------

        private SettingsState Load()
        {
            // Engine state is the baseline; saved values override it where present.
            var s = CaptureEngine();

            s.masterVolume = GetFloat("masterVolume", s.masterVolume);
            s.sfxVolume = GetFloat("sfxVolume", s.sfxVolume);
            s.musicVolume = GetFloat("musicVolume", s.musicVolume);
            s.ambianceVolume = GetFloat("ambianceVolume", s.ambianceVolume);
            s.dialogueVolume = GetFloat("dialogueVolume", s.dialogueVolume);
            s.uiVolume = GetFloat("uiVolume", s.uiVolume);

            s.resolutionWidth = GetInt("resolutionWidth", s.resolutionWidth);
            s.resolutionHeight = GetInt("resolutionHeight", s.resolutionHeight);
            s.fullScreenMode = (FullScreenMode)GetInt("fullScreenMode", (int)s.fullScreenMode);

            s.qualityLevel = GetInt("qualityLevel", s.qualityLevel);
            s.vSync = GetInt("vSync", s.vSync ? 1 : 0) != 0;
            s.targetFrameRate = GetInt("targetFrameRate", s.targetFrameRate);
            s.antiAliasing = GetInt("antiAliasing", s.antiAliasing);

            s.uiScale = GetFloat("uiScale", s.uiScale);
            return s;
        }

        private void Save(SettingsState s)
        {
            PlayerPrefs.SetFloat(Prefix + "masterVolume", s.masterVolume);
            PlayerPrefs.SetFloat(Prefix + "sfxVolume", s.sfxVolume);
            PlayerPrefs.SetFloat(Prefix + "musicVolume", s.musicVolume);
            PlayerPrefs.SetFloat(Prefix + "ambianceVolume", s.ambianceVolume);
            PlayerPrefs.SetFloat(Prefix + "dialogueVolume", s.dialogueVolume);
            PlayerPrefs.SetFloat(Prefix + "uiVolume", s.uiVolume);

            PlayerPrefs.SetInt(Prefix + "resolutionWidth", s.resolutionWidth);
            PlayerPrefs.SetInt(Prefix + "resolutionHeight", s.resolutionHeight);
            PlayerPrefs.SetInt(Prefix + "fullScreenMode", (int)s.fullScreenMode);

            PlayerPrefs.SetInt(Prefix + "qualityLevel", s.qualityLevel);
            PlayerPrefs.SetInt(Prefix + "vSync", s.vSync ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "targetFrameRate", s.targetFrameRate);
            PlayerPrefs.SetInt(Prefix + "antiAliasing", s.antiAliasing);

            PlayerPrefs.SetFloat(Prefix + "uiScale", s.uiScale);
            PlayerPrefs.Save();
        }

        private static float GetFloat(string key, float fallback) => PlayerPrefs.GetFloat(Prefix + key, fallback);

        private static int GetInt(string key, int fallback) => PlayerPrefs.GetInt(Prefix + key, fallback);
    }
}
