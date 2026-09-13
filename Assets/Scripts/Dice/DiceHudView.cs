using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
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
        [Tooltip("The label / total line, e.g. 'Attack - 9'. Optional.")]
        private TMP_Text labelText;

        [SerializeField]
        [Tooltip("Button the player clicks to close the HUD.")]
        private Button closeButton;

        [Header("Timing (seconds)")]
        [SerializeField] private float settleDuration = 0.4f;
        [SerializeField] private float fadeDuration = 0.4f;

        [SerializeField]
        [Tooltip("How often the faces flash while settling.")]
        private float flashInterval = 0.05f;

        private readonly List<TMP_Text> cells = new List<TMP_Text>();
        private Coroutine showRoutine;
        private Coroutine hideRoutine;

        private void Awake()
        {
            if (group == null)
                group = GetComponent<CanvasGroup>();
            SetVisible(false);

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        private void OnEnable() => DiceRoller.Rolled += OnRolled;
        private void OnDisable() => DiceRoller.Rolled -= OnRolled;

        private void OnRolled(DiceRoll roll, string label) => Show(roll, label);

        public void Show(DiceRoll roll, string label = null)
        {
            BuildCells(roll);
            if (labelText != null)
                labelText.text = string.IsNullOrEmpty(label) ? roll.Total.ToString() : $"{label}: {roll.Total}";

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
                hideRoutine = null;
            }
            if (showRoutine != null)
                StopCoroutine(showRoutine);
            showRoutine = StartCoroutine(ShowRoutine(roll));
        }

        // Wired to the close button; also callable from elsewhere.
        public void Hide()
        {
            if (showRoutine != null)
            {
                StopCoroutine(showRoutine);
                showRoutine = null;
            }
            if (hideRoutine != null)
                StopCoroutine(hideRoutine);
            hideRoutine = StartCoroutine(HideRoutine());
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
            SetVisible(true);

            float elapsed = 0f;
            float nextFlash = 0f;
            while (elapsed < settleDuration)
            {
                if (elapsed >= nextFlash)
                {
                    foreach (TMP_Text cell in cells)
                        if (cell != null)
                            cell.text = UnityEngine.Random.Range(1, roll.Sides + 1).ToString();
                    nextFlash += flashInterval;
                }

                elapsed += Time.unscaledDeltaTime; // unscaled so it animates even while the game is paused
                yield return null;
            }

            for (int i = 0; i < cells.Count; i++)
                if (cells[i] != null)
                    cells[i].text = roll.Values[i].ToString();

            // Stays up until the player closes it - no auto hold/fade.
            showRoutine = null;
        }

        private IEnumerator HideRoutine()
        {
            group.interactable = false;
            group.blocksRaycasts = false;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            SetVisible(false);
            hideRoutine = null;
        }

        private void SetVisible(bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
