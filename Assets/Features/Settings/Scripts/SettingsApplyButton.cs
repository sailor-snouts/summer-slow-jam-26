using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.Settings
{
    /// <summary>
    /// Wires an Apply button to open the confirm dialog (which applies the
    /// working settings). Without a dialog, it applies and saves immediately.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Settings Apply Button")]
    [RequireComponent(typeof(Button))]
    public class SettingsApplyButton : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Confirm dialog shown after applying. Leave empty to apply and save immediately.")]
        private SettingsConfirmDialog dialog;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Apply);
        }

        private void Apply()
        {
            if (dialog != null)
            {
                dialog.Open();
            }
            else if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ApplyWorking();
                SettingsManager.Instance.Confirm();
            }
        }
    }
}
