using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.Saving
{
    /// <summary>
    /// Drives one row of the save menu: shows the slot's timestamp (or "empty") and
    /// wires its Save / Load / Delete buttons to the <see cref="SaveManager"/>. Load
    /// and Delete are disabled while the slot is empty, and the row refreshes itself
    /// whenever its slot is saved or deleted.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Save Slot View")]
    [DisallowMultipleComponent]
    public class SaveSlotView : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Which save slot this row controls.")]
        private int slot;

        [SerializeField]
        [Tooltip("Label showing the slot number and its timestamp.")]
        private TMP_Text label;

        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;

        private void OnEnable()
        {
            if (saveButton != null) saveButton.onClick.AddListener(Save);
            if (loadButton != null) loadButton.onClick.AddListener(Load);
            if (deleteButton != null) deleteButton.onClick.AddListener(Delete);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Saved += OnSlotChanged;
                SaveManager.Instance.Deleted += OnSlotChanged;
                SaveManager.Instance.SaveFailed += OnSaveFailed;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (saveButton != null) saveButton.onClick.RemoveListener(Save);
            if (loadButton != null) loadButton.onClick.RemoveListener(Load);
            if (deleteButton != null) deleteButton.onClick.RemoveListener(Delete);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Saved -= OnSlotChanged;
                SaveManager.Instance.Deleted -= OnSlotChanged;
                SaveManager.Instance.SaveFailed -= OnSaveFailed;
            }
        }

        private void Save()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Save(slot);
        }

        private void Load()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Load(slot);
        }

        private void Delete()
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.Delete(slot);
        }

        private void OnSlotChanged(int changed)
        {
            if (changed == slot)
                Refresh();
        }

        private void OnSaveFailed(int failed)
        {
            if (failed == slot && label != null)
                label.text = $"Slot {slot + 1}    — save failed —";
        }

        private void Refresh()
        {
            SaveData info = SaveManager.Instance != null ? SaveManager.Instance.Peek(slot) : null;
            bool has = info != null;

            if (label != null)
                label.text = has
                    ? $"Slot {slot + 1}    {FormatTime(info.SavedAt)}"
                    : $"Slot {slot + 1}    — empty —";

            if (loadButton != null) loadButton.interactable = has;
            if (deleteButton != null) deleteButton.interactable = has;
        }

        private static string FormatTime(string iso)
        {
            return DateTime.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime utc)
                ? utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                : iso;
        }
    }
}
