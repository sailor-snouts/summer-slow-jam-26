using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Shows a dice roll on the HUD. Put this on the root of a Canvas you build in the editor,
    /// and wire the references below. It listens for <see cref="DiceRoller.Rolled"/> and, for
    /// each announced roll, spawns one cell per die, briefly flashes random faces, lands on the
    /// real values, holds, then fades out. It never rolls anything itself — it only displays.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DiceHudView : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField]
        [Tooltip("Faded in/out to show and hide the whole HUD.")]
        private CanvasGroup group;

        [SerializeField]
        [Tooltip("Parent for the die cells (give it a HorizontalLayoutGroup).")]
        private RectTransform container;

        [SerializeField]
        [Tooltip("Prefab spawned once per die. Must have a TMP_Text somewhere in it.")]
        private GameObject dieCellPrefab;

        [SerializeField]
        [Tooltip("The label / total line, e.g. 'Attack — 9'. Optional.")]
        private TMP_Text labelText;

        [Header("Timing (seconds)")]
        [SerializeField] private float settleDuration = 0.4f;
        [SerializeField] private float holdDuration = 1.5f;
        [SerializeField] private float fadeDuration = 0.4f;

        [SerializeField]
        [Tooltip("How often the faces flash while settling.")]
        private float flashInterval = 0.05f;

        // The TMP_Text on each spawned cell, so the coroutine can update faces without re-searching.
        private readonly List<TMP_Text> cells = new List<TMP_Text>();
        private Coroutine showRoutine;

        private void Awake()
        {
            if (group == null)
                group = GetComponent<CanvasGroup>();
            group.alpha = 0f; // start hidden
        }

        // Subscribe while enabled, unsubscribe when disabled. The event is static and outlives
        // this object, so skipping the unsubscribe would leak and eventually call a destroyed view.
        private void OnEnable() => DiceRoller.Rolled += OnRolled;
        private void OnDisable() => DiceRoller.Rolled -= OnRolled;

        private void OnRolled(DiceRoll roll, string label) => Show(roll, label);

        /// <summary>Displays a roll: rebuild cells, then run the reveal animation (restarting any in progress).</summary>
        public void Show(DiceRoll roll, string label = null)
        {
            BuildCells(roll);
            if (labelText != null)
                labelText.text = string.IsNullOrEmpty(label) ? roll.Total.ToString() : $"{label}: {roll.Total}";

            if (showRoutine != null)
                StopCoroutine(showRoutine);
            showRoutine = StartCoroutine(ShowRoutine(roll));
        }

        private void BuildCells(DiceRoll roll)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
            cells.Clear();

            for (int i = 0; i < roll.Count; i++)
            {
                GameObject cell = Instantiate(dieCellPrefab, container);
                cells.Add(cell.GetComponentInChildren<TMP_Text>());
            }
        }

        private IEnumerator ShowRoutine(DiceRoll roll)
        {
            group.alpha = 1f;

            // 1) Settle: flash random faces on every cell, then land on the real values.
            float elapsed = 0f;
            float nextFlash = 0f;
            while (elapsed < settleDuration)
            {
                if (elapsed >= nextFlash)
                {
                    foreach (TMP_Text cell in cells)
                        if (cell != null)
                            cell.text = UnityEngine.Random.Range(1, roll.Sides + 1).ToString(); // cosmetic only
                    nextFlash += flashInterval;
                }

                elapsed += Time.unscaledDeltaTime; // unscaled so it animates even while the game is paused
                yield return null;
            }

            for (int i = 0; i < cells.Count; i++)
                if (cells[i] != null)
                    cells[i].text = roll.Values[i].ToString();

            // 2) Hold at full opacity.
            yield return WaitUnscaled(holdDuration);

            // 3) Fade out.
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            group.alpha = 0f;
            showRoutine = null;
        }

        private static IEnumerator WaitUnscaled(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
