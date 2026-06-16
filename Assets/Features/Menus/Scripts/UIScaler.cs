using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Scales this canvas to match the player's UI scale setting by dividing its
    /// CanvasScaler reference resolution (a smaller reference makes the UI larger).
    /// Reads the global <see cref="UIScale"/>, so it works with or without a
    /// SettingsManager present.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/UI Scaler")]
    [RequireComponent(typeof(CanvasScaler))]
    public class UIScaler : MonoBehaviour
    {
        private CanvasScaler scaler;
        private Vector2 baseReference;

        private void Awake()
        {
            scaler = GetComponent<CanvasScaler>();
            baseReference = scaler.referenceResolution;
        }

        private void OnEnable()
        {
            Apply();
            UIScale.Changed += Apply;
        }

        private void OnDisable()
        {
            UIScale.Changed -= Apply;
        }

        private void Apply()
        {
            scaler.referenceResolution = baseReference / Mathf.Max(0.1f, UIScale.Current);
        }
    }
}
