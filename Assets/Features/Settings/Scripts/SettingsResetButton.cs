using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.Settings
{
    /// <summary>
    /// Wires a button to reset the working settings to their defaults (volumes,
    /// UI scale, graphics) — resolution, window mode and quality are kept. Like
    /// any other edit, nothing reaches the engine until Apply.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Settings Reset Button")]
    [RequireComponent(typeof(Button))]
    public class SettingsResetButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Reset);
        }

        private void Reset()
        {
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.ResetWorking();
        }
    }
}
