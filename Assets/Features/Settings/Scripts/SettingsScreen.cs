using UnityEngine;

namespace JamTemplate.Settings
{
    /// <summary>
    /// Drives a settings screen: when it loads, it tells the SettingsManager to
    /// begin an edit, so the binders below it read fresh working values.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Settings Screen")]
    [DisallowMultipleComponent]
    public class SettingsScreen : MonoBehaviour
    {
        private void Awake()
        {
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.BeginEdit();
            else
                Debug.LogWarning("[Settings] No Settings Manager in the scene; controls won't apply or save.", this);
        }

        private void OnDestroy()
        {
            // Volume sliders preview live; un-applied previews shouldn't outlive the screen.
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.ApplyAudio(SettingsManager.Instance.Confirmed);
        }
    }
}
