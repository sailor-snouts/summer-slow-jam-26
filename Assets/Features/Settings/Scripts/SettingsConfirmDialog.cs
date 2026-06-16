using System.Collections;
using TMPro;
using UnityEngine;

namespace JamTemplate.Settings
{
    /// <summary>
    /// The apply-confirmation dialog. Open() applies the working settings and
    /// shows a countdown; Keep() saves them, while a timeout or Revert() restores
    /// the previous settings — so a change that breaks the display undoes itself.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Settings Confirm Dialog")]
    [DisallowMultipleComponent]
    public class SettingsConfirmDialog : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Root object shown while confirming. Hidden on Awake. Defaults to this GameObject.")]
        private GameObject panel;

        [SerializeField]
        [Tooltip("Message text; the countdown is appended to it.")]
        private TMP_Text message;

        [SerializeField]
        [Min(1f)]
        [Tooltip("Seconds before the change is reverted automatically.")]
        private float revertAfter = 15f;

        private Coroutine countdown;
        private string baseMessage;

        private void Awake()
        {
            if (panel == null)
                panel = gameObject;
            baseMessage = message != null && !string.IsNullOrEmpty(message.text)
                ? message.text
                : "Keep these settings?";
            panel.SetActive(false);
        }

        /// <summary>Applies the working settings and shows the confirm countdown.</summary>
        public void Open()
        {
            if (SettingsManager.Instance == null)
                return;

            SettingsManager.Instance.ApplyWorking();
            panel.SetActive(true);
            StopCountdown();
            countdown = StartCoroutine(CountdownRoutine());
        }

        /// <summary>Keeps the applied settings (saves them) and closes.</summary>
        public void Keep()
        {
            StopCountdown();
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.Confirm();
            panel.SetActive(false);
        }

        /// <summary>Reverts to the previous settings and closes.</summary>
        public void Revert()
        {
            StopCountdown();
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.Revert();
            panel.SetActive(false);
        }

        private IEnumerator CountdownRoutine()
        {
            float remaining = revertAfter;
            while (remaining > 0f)
            {
                if (message != null)
                    message.text = $"{baseMessage}\nReverting in {Mathf.CeilToInt(remaining)}…";
                // Unscaled: settings may be opened while the game is paused (timeScale 0).
                remaining -= Time.unscaledDeltaTime;
                yield return null;
            }

            Revert();
        }

        private void StopCountdown()
        {
            if (countdown != null)
            {
                StopCoroutine(countdown);
                countdown = null;
            }
        }
    }
}
