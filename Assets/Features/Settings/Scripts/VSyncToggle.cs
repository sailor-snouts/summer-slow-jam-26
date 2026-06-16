using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.Settings
{
    /// <summary>Binds a Toggle to the working settings' VSync flag.</summary>
    [AddComponentMenu("Sailor Snouts/VSync Toggle")]
    [RequireComponent(typeof(Toggle))]
    public class VSyncToggle : MonoBehaviour
    {
        private Toggle toggle;
        private bool suppress;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();

#if UNITY_WEBGL
            // The browser forces vsync (requestAnimationFrame) on WebGL; hide the row.
            (transform.parent != null ? transform.parent.gameObject : gameObject).SetActive(false);
#else
            toggle.onValueChanged.AddListener(OnChanged);
#endif
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
            // Fall back to the engine's live VSync state when no manager is editing.
            SettingsManager manager = SettingsManager.Instance;
            SettingsState w = manager != null ? manager.Working : SettingsManager.CaptureEngine();

            suppress = true;
            toggle.isOn = w.vSync;
            suppress = false;
        }

        private void OnChanged(bool value)
        {
            if (suppress)
                return;
            SettingsManager manager = SettingsManager.Instance;
            if (manager != null)
                manager.Working.vSync = value;
        }
    }
}
