#if UNITY_EDITOR
using JamTemplate.Core;
using JamTemplate.Menus;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.Saving
{
    /// <summary>
    /// Creates the High Scores scene from Tools &gt; Sailor Snouts &gt; Scenes &gt;
    /// Create High Scores Scene — an additive overlay that lists the leaderboard in
    /// descending order. The list binds to the HighScoreManager; set how many rows
    /// to show on the High Score List component (defaults to 10).
    /// </summary>
    internal static class HighScoreMenuSceneSetup
    {
        internal const string ScenePath = "Assets/Features/Core/Scenes/HighScores.unity";

        [MenuItem("Tools/Sailor Snouts/Scenes/Create High Scores Scene")]
        private static void OpenOrCreate() =>
            ToolRegistry.Run("Scenes/Create High Scores Scene", () => MenuSceneBuilder.OpenOrCreate(ScenePath, Ensure));

        internal static void Ensure() => MenuSceneBuilder.EnsureScene(ScenePath, true, Build);

        private static void Build()
        {
            // A High Score Manager so the board shows when the scene is opened on its
            // own; its Awake dedups, so a bootstrap-scene manager takes precedence.
            new GameObject("High Score Manager", typeof(HighScoreManager));

            var canvas = MenuSceneBuilder.CreateCanvas("High Scores Canvas", 500);
            canvas.gameObject.AddComponent<EnsureCamera>();
            canvas.gameObject.AddComponent<EnsureEventSystem>();

            var background = MenuSceneBuilder.CreateBackground(canvas, MenuSceneBuilder.DarkBackground);
            background.raycastTarget = true; // blocks clicks reaching the scene underneath

            var header = MenuSceneBuilder.CreateText(canvas, "Header", "High Scores", 80, FontStyle.Bold);
            var headerRect = (RectTransform)header.transform;
            headerRect.anchorMin = new Vector2(0.5f, 1f);
            headerRect.anchorMax = new Vector2(0.5f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, -50f);
            headerRect.sizeDelta = new Vector2(1400f, 120f);

            BuildList(canvas);
            BuildBottomBar(canvas);
        }

        private static void BuildList(RectTransform canvas)
        {
            ScrollRect scroll = MenuSceneBuilder.CreateScrollView(canvas);
            var scrollRect = (RectTransform)scroll.transform;
            scrollRect.anchorMin = new Vector2(0.5f, 0f);
            scrollRect.anchorMax = new Vector2(0.5f, 1f);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.sizeDelta = new Vector2(1100f, -320f);
            scrollRect.anchoredPosition = new Vector2(0f, -20f);

            RectTransform content = scroll.content;
            var list = new SerializedObject(content.gameObject.AddComponent<HighScoreList>());
            list.FindProperty("count").intValue = 10;
            list.FindProperty("content").objectReferenceValue = content;
            list.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildBottomBar(RectTransform canvas)
        {
            var bar = MenuSceneBuilder.CreateButtonColumn(canvas, "Bottom Bar");
            bar.anchorMin = new Vector2(0.5f, 0f);
            bar.anchorMax = new Vector2(0.5f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.anchoredPosition = new Vector2(0f, 50f);

            Button back = MenuSceneBuilder.CreateButton(bar, "Back", MenuAction.CloseSelf, string.Empty);
            back.gameObject.AddComponent<InitialSelection>();
        }
    }
}
#endif
