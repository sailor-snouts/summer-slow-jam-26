using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.Saving
{
    /// <summary>
    /// Fills <see cref="content"/> with the top high scores, highest first, at
    /// runtime (the board length is dynamic). Set how many to show with
    /// <see cref="count"/>. Rebuilds whenever the board changes.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/High Score List")]
    [DisallowMultipleComponent]
    public class HighScoreList : MonoBehaviour
    {
        [SerializeField]
        [Min(1)]
        [Tooltip("How many scores to display.")]
        private int count = 10;

        [SerializeField]
        [Tooltip("Container the rows are added under (expects a vertical layout group).")]
        private RectTransform content;

        // Only rows this component spawned — hand-placed children of content survive rebuilds.
        private readonly List<GameObject> rows = new List<GameObject>();

        private void OnEnable()
        {
            Rebuild();
            if (HighScoreManager.Instance != null)
                HighScoreManager.Instance.Changed += Rebuild;
        }

        private void OnDisable()
        {
            if (HighScoreManager.Instance != null)
                HighScoreManager.Instance.Changed -= Rebuild;
        }

        private void Rebuild()
        {
            if (content == null)
                return;

            foreach (GameObject row in rows)
            {
                if (row != null)
                    Destroy(row);
            }
            rows.Clear();

            IReadOnlyList<HighScore> scores = HighScoreManager.Instance != null
                ? HighScoreManager.Instance.Top(count)
                : Array.Empty<HighScore>();

            if (scores.Count == 0)
            {
                AddRow("—", "No scores yet");
                return;
            }

            for (int i = 0; i < scores.Count; i++)
                AddRow($"{i + 1}. {scores[i].name}", scores[i].score.ToString());
        }

        private void AddRow(string left, string right)
        {
            var rowObject = new GameObject("Score Row",
                typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            rowObject.layer = gameObject.layer;
            var row = (RectTransform)rowObject.transform;
            row.SetParent(content, false);
            rows.Add(rowObject);

            var layout = rowObject.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 16f;
            rowObject.GetComponent<LayoutElement>().minHeight = 56f;

            TextMeshProUGUI nameText = MakeText(row, left, TextAlignmentOptions.Left);
            nameText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            TextMeshProUGUI scoreText = MakeText(row, right, TextAlignmentOptions.Right);
            scoreText.gameObject.AddComponent<LayoutElement>().minWidth = 220f;
        }

        private TextMeshProUGUI MakeText(RectTransform parent, string value, TextAlignmentOptions alignment)
        {
            var go = new GameObject("Text", typeof(TextMeshProUGUI));
            go.layer = gameObject.layer;
            ((RectTransform)go.transform).SetParent(parent, false);

            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 32;
            text.color = Color.white;
            text.alignment = alignment;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }
    }
}
