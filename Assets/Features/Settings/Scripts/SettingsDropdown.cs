using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JamTemplate.Settings
{
    /// <summary>Which setting a <see cref="SettingsDropdown"/> controls.</summary>
    public enum DropdownSetting
    {
        Resolution,
        WindowMode,
        Quality,
        FrameRate,
        AntiAliasing,
        UiScale,
    }

    /// <summary>Binds a TMP_Dropdown to one of the working settings' choice fields.</summary>
    [AddComponentMenu("Sailor Snouts/Settings Dropdown")]
    [RequireComponent(typeof(TMP_Dropdown))]
    public class SettingsDropdown : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Which setting this dropdown controls.")]
        private DropdownSetting setting;

        // No MaximizedWindow — it is macOS-only and behaves like Windowed elsewhere.
        private static readonly FullScreenMode[] WindowModes =
        {
            FullScreenMode.FullScreenWindow,
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.Windowed,
        };
        private static readonly string[] WindowLabels = { "Borderless", "Fullscreen", "Windowed" };

        private static readonly int[] FrameRates = { -1, 30, 60, 120, 144 };
        private static readonly string[] FrameLabels = { "Unlimited", "30", "60", "120", "144" };

        private static readonly int[] AaValues = { 0, 2, 4, 8 };
        private static readonly string[] AaLabels = { "Off", "2x", "4x", "8x" };

        private static readonly float[] ScaleValues = { 0.8f, 1f, 1.2f, 1.5f, 2f };
        private static readonly string[] ScaleLabels = { "80%", "100%", "120%", "150%", "200%" };

        private TMP_Dropdown dropdown;
        private readonly List<Vector2Int> resolutions = new List<Vector2Int>();
        private bool suppress;

        private void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();

#if UNITY_WEBGL
            // The browser owns the canvas size, window mode and frame pacing on
            // WebGL; hide the whole row rather than show dead controls.
            if (setting == DropdownSetting.Resolution
                || setting == DropdownSetting.WindowMode
                || setting == DropdownSetting.FrameRate)
            {
                (transform.parent != null ? transform.parent.gameObject : gameObject).SetActive(false);
                return;
            }
#endif

            Populate();
            dropdown.onValueChanged.AddListener(OnChanged);
        }

        private void OnEnable()
        {
            Sync();
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.WorkingChanged += Sync;
        }

        private void OnDisable()
        {
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.WorkingChanged -= Sync;
        }

        private void Populate()
        {
            List<string> options = new List<string>();
            switch (setting)
            {
                case DropdownSetting.Resolution:
                    foreach (Resolution r in Screen.resolutions)
                    {
                        var size = new Vector2Int(r.width, r.height);
                        if (!resolutions.Contains(size))
                        {
                            resolutions.Add(size);
                            options.Add($"{r.width} x {r.height}");
                        }
                    }
                    break;
                case DropdownSetting.WindowMode:
                    options.AddRange(WindowLabels);
                    break;
                case DropdownSetting.Quality:
                    options.AddRange(QualitySettings.names);
                    break;
                case DropdownSetting.FrameRate:
                    options.AddRange(FrameLabels);
                    break;
                case DropdownSetting.AntiAliasing:
                    options.AddRange(AaLabels);
                    break;
                case DropdownSetting.UiScale:
                    options.AddRange(ScaleLabels);
                    break;
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }

        private void Sync()
        {
            // Fall back to the engine's live values when no manager is editing, so
            // the dropdown still shows the current selection rather than a blank.
            SettingsManager manager = SettingsManager.Instance;
            SettingsState w = manager != null ? manager.Working : SettingsManager.CaptureEngine();

            suppress = true;
            dropdown.SetValueWithoutNotify(IndexFor(w));
            dropdown.RefreshShownValue();
            suppress = false;
        }

        private int IndexFor(SettingsState w)
        {
            switch (setting)
            {
                case DropdownSetting.Resolution:
                    int res = resolutions.IndexOf(new Vector2Int(w.resolutionWidth, w.resolutionHeight));
                    return res >= 0 ? res : ClosestResolution(w.resolutionWidth, w.resolutionHeight);
                case DropdownSetting.WindowMode:
                    return Mathf.Max(0, System.Array.IndexOf(WindowModes, w.fullScreenMode));
                case DropdownSetting.Quality:
                    return Mathf.Clamp(w.qualityLevel, 0, dropdown.options.Count - 1);
                case DropdownSetting.FrameRate:
                    return Mathf.Max(0, System.Array.IndexOf(FrameRates, w.targetFrameRate));
                case DropdownSetting.AntiAliasing:
                    return Mathf.Max(0, System.Array.IndexOf(AaValues, w.antiAliasing));
                case DropdownSetting.UiScale:
                    return Mathf.Max(0, System.Array.IndexOf(ScaleValues, w.uiScale));
                default:
                    return 0;
            }
        }

        private int ClosestResolution(int width, int height)
        {
            int best = 0;
            long bestDiff = long.MaxValue;
            long target = (long)width * height;
            for (int i = 0; i < resolutions.Count; i++)
            {
                long diff = System.Math.Abs((long)resolutions[i].x * resolutions[i].y - target);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    best = i;
                }
            }

            return best;
        }

        private void OnChanged(int index)
        {
            if (suppress)
                return;
            SettingsManager manager = SettingsManager.Instance;
            if (manager == null)
                return;

            SettingsState w = manager.Working;

            switch (setting)
            {
                case DropdownSetting.Resolution:
                    if (index >= 0 && index < resolutions.Count)
                    {
                        w.resolutionWidth = resolutions[index].x;
                        w.resolutionHeight = resolutions[index].y;
                    }
                    break;
                case DropdownSetting.WindowMode:
                    w.fullScreenMode = WindowModes[Mathf.Clamp(index, 0, WindowModes.Length - 1)];
                    break;
                case DropdownSetting.Quality:
                    w.qualityLevel = index;
                    break;
                case DropdownSetting.FrameRate:
                    w.targetFrameRate = FrameRates[Mathf.Clamp(index, 0, FrameRates.Length - 1)];
                    break;
                case DropdownSetting.AntiAliasing:
                    w.antiAliasing = AaValues[Mathf.Clamp(index, 0, AaValues.Length - 1)];
                    break;
                case DropdownSetting.UiScale:
                    w.uiScale = ScaleValues[Mathf.Clamp(index, 0, ScaleValues.Length - 1)];
                    break;
            }
        }
    }
}
