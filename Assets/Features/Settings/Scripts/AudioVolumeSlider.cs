using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.Settings
{
    /// <summary>Which volume a <see cref="AudioVolumeSlider"/> controls.</summary>
    public enum VolumeChannel
    {
        Master,
        Sfx,
        Music,
        Ambiance,
        Dialogue,
        Ui,
    }

    /// <summary>Binds a Slider to one of the working settings' volume channels.</summary>
    [AddComponentMenu("Sailor Snouts/Audio Volume Slider")]
    [RequireComponent(typeof(Slider))]
    public class AudioVolumeSlider : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Which volume this slider controls.")]
        private VolumeChannel channel;

        private Slider slider;
        private bool suppress;

        private void Awake()
        {
            slider = GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.onValueChanged.AddListener(OnChanged);
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

        private void Sync()
        {
            // Fall back to the live audio values when no manager is editing, so the
            // slider reflects the current volume rather than sitting at zero.
            SettingsManager manager = SettingsManager.Instance;
            SettingsState w = manager != null ? manager.Working : SettingsManager.CaptureEngine();

            suppress = true;
            slider.value = Read(w);
            suppress = false;
        }

        private void OnChanged(float value)
        {
            if (suppress)
                return;
            SettingsManager manager = SettingsManager.Instance;
            if (manager == null)
                return;

            SettingsState w = manager.Working;

            switch (channel)
            {
                case VolumeChannel.Master: w.masterVolume = value; break;
                case VolumeChannel.Sfx: w.sfxVolume = value; break;
                case VolumeChannel.Music: w.musicVolume = value; break;
                case VolumeChannel.Ambiance: w.ambianceVolume = value; break;
                case VolumeChannel.Dialogue: w.dialogueVolume = value; break;
                case VolumeChannel.Ui: w.uiVolume = value; break;
            }

            // Preview volume changes live (they can't break the display); the
            // settings screen restores the confirmed volumes if never applied.
            SettingsManager.Instance.ApplyAudio(w);
        }

        private float Read(SettingsState w)
        {
            switch (channel)
            {
                case VolumeChannel.Master: return w.masterVolume;
                case VolumeChannel.Sfx: return w.sfxVolume;
                case VolumeChannel.Music: return w.musicVolume;
                case VolumeChannel.Ambiance: return w.ambianceVolume;
                case VolumeChannel.Dialogue: return w.dialogueVolume;
                case VolumeChannel.Ui: return w.uiVolume;
                default: return 1f;
            }
        }
    }
}
