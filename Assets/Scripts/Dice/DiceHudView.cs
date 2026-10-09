using System.Collections;
using System.Collections.Generic;
using JamTemplate.Audio;
using PixelCrushers.DialogueSystem;
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

        [Header("Result (checks with a target)")]
        [SerializeField, Tooltip("Dice tint when the check passes.")]
        private Color passColor = new Color(0.45f, 0.85f, 0.45f, 1f);

        [SerializeField, Tooltip("Dice tint when the check fails.")]
        private Color failColor = new Color(0.9f, 0.4f, 0.4f, 1f);

        [SerializeField, Min(0f), Tooltip("Seconds to blend the dice to the result color.")]
        private float resultColorDuration = 0.25f;

        [SerializeField, Tooltip("FMOD event played when the check passes.")]
        private AudioEvent passSound;

        [SerializeField, Tooltip("FMOD event played when the check fails.")]
        private AudioEvent failSound;

        private readonly List<TMP_Text> cells = new List<TMP_Text>();
        private readonly List<Image> cellImages = new List<Image>();
        private Coroutine showRoutine;
        private Coroutine hideRoutine;
        private bool pausedDialogue;

        private void Awake()
        {
            if (group == null)
                group = GetComponent<CanvasGroup>();
            SetVisible(false);

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        private void OnEnable() => DiceRoller.Rolled += OnRolled;

        private void OnDisable()
        {
            DiceRoller.Rolled -= OnRolled;
            ResumeDialogue(); // never leave a conversation paused if the HUD is torn down
        }

        private void OnRolled(DiceRoll roll, string label, RollOutcome outcome) => Show(roll, label, outcome);

        public void Show(DiceRoll roll, string label = null, RollOutcome outcome = RollOutcome.None)
        {
            BuildCells(roll);
            if (labelText != null)
                labelText.text = string.IsNullOrEmpty(label) ? roll.Total.ToString() : $"{label}: {roll.Total}";

            // Hold the conversation on the current line until the player closes the HUD.
            if (DialogueManager.isConversationActive && !pausedDialogue)
            {
                DialogueManager.Pause();
                pausedDialogue = true;
            }

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
                hideRoutine = null;
            }
            if (showRoutine != null)
                StopCoroutine(showRoutine);
            showRoutine = StartCoroutine(ShowRoutine(roll, outcome));
        }

        // Wired to the close button; also callable from elsewhere.
        public void Hide()
        {
            ResumeDialogue(); // closing the HUD lets the conversation continue

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
            cellImages.Clear();

            for (int i = 0; i < roll.Count; i++)
            {
                GameObject cell = Instantiate(dieCellPrefab, container);
                cells.Add(cell.GetComponentInChildren<TMP_Text>());
                cellImages.Add(cell.GetComponent<Image>()); // the die face; may be null if the prefab has none
            }
        }

        private IEnumerator ShowRoutine(DiceRoll roll, RollOutcome outcome)
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

            if (outcome != RollOutcome.None)
                yield return ShowResult(outcome == RollOutcome.Pass);

            // Stays up until the player closes it - no auto hold/fade.
            showRoutine = null;
        }

        // The dice have landed: play the pass/fail sound and blend the dice to the result color.
        private IEnumerator ShowResult(bool passed)
        {
            AudioEvent sound = passed ? passSound : failSound;
            if (sound != null)
                GameAudio.Play(sound);

            Color target = passed ? passColor : failColor;
            var start = new Color[cellImages.Count];
            for (int i = 0; i < cellImages.Count; i++)
                start[i] = cellImages[i] != null ? cellImages[i].color : target;

            float elapsed = 0f;
            while (elapsed < resultColorDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / resultColorDuration);
                for (int i = 0; i < cellImages.Count; i++)
                    if (cellImages[i] != null)
                        cellImages[i].color = Color.Lerp(start[i], target, t);
                yield return null;
            }

            foreach (Image image in cellImages)
                if (image != null)
                    image.color = target;
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

        private void ResumeDialogue()
        {
            if (!pausedDialogue)
                return;
            pausedDialogue = false;
            DialogueManager.Unpause();
        }

        private void SetVisible(bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }
    }
}
