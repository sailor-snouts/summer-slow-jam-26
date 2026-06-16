using UnityEngine;
using UnityEngine.Events;

namespace JamTemplate.Saving
{
    /// <summary>
    /// Triggers an auto-save every <see cref="interval"/> seconds via
    /// <see cref="SaveManager.SaveAuto"/>, which writes to the reserved auto-save
    /// slot (never a manual one). Place it wherever auto-saving should be active —
    /// e.g. on a gameplay manager. The timer uses unscaled time, so it keeps a
    /// steady wall-clock cadence even when the game is paused.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Auto Save")]
    [DisallowMultipleComponent]
    public class AutoSave : MonoBehaviour
    {
        [SerializeField]
        [Min(1f)]
        [Tooltip("Seconds between auto-saves.")]
        private float interval = 300f;

        [SerializeField]
        [Tooltip("Also auto-save once when this component starts.")]
        private bool saveOnStart;

        [SerializeField]
        [Tooltip("Raised each time an auto-save runs — wire up an 'Auto-saving…' indicator here.")]
        private UnityEvent autoSaved;

        private float timer;

        private void OnEnable()
        {
            timer = 0f;
            if (SaveManager.Instance != null)
                SaveManager.Instance.AutoSaved += OnAutoSaved;
        }

        private void OnDisable()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.AutoSaved -= OnAutoSaved;
        }

        private void Start()
        {
            if (saveOnStart)
                Trigger();
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= interval)
                Trigger();
        }

        private void Trigger()
        {
            timer = 0f;
            if (SaveManager.Instance != null)
                SaveManager.Instance.SaveAuto();
            else
                Debug.LogWarning("[Saving] AutoSave fired but there is no Save Manager in the scene.", this);
        }

        private void OnAutoSaved() => autoSaved?.Invoke();
    }
}
