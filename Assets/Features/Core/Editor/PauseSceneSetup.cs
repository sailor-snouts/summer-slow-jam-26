#if UNITY_EDITOR
using JamTemplate.Core;
using JamTemplate.Menus;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.Pause
{
    /// <summary>
    /// Creates the pause scene from Tools &gt; Sailor Snouts &gt; Scenes &gt; Create Pause Scene
    /// — an additive overlay (no camera of its own) with a dimming backdrop, a
    /// heading and Resume/Settings/Quit buttons.
    /// </summary>
    internal static class PauseSceneSetup
    {
        internal const string ScenePath = "Assets/Features/Core/Scenes/Pause.unity";

        [MenuItem("Tools/Sailor Snouts/Scenes/Create Pause Scene")]
        private static void OpenOrCreate() =>
            ToolRegistry.Run("Scenes/Create Pause Scene", () => MenuSceneBuilder.OpenOrCreate(ScenePath, Ensure));

        internal static void Ensure() => MenuSceneBuilder.EnsureScene(ScenePath, true, Build);

        private static void Build()
        {
            var canvas = MenuSceneBuilder.CreateCanvas("Pause Canvas", 1000);
            // The pause overlay may sit over a scene with no EventSystem; ensure one.
            canvas.gameObject.AddComponent<EnsureEventSystem>();

            var backdrop = MenuSceneBuilder.CreateBackground(canvas, new Color(0.04f, 0.05f, 0.08f, 0.75f));
            backdrop.raycastTarget = true;   // blocks clicks reaching the game underneath

            var heading = MenuSceneBuilder.CreateText(canvas, "Heading", "Paused", 80, FontStyle.Bold);
            var headingRect = (RectTransform)heading.transform;
            headingRect.anchorMin = new Vector2(0.5f, 0.5f);
            headingRect.anchorMax = new Vector2(0.5f, 0.5f);
            headingRect.pivot = new Vector2(0.5f, 0.5f);
            headingRect.anchoredPosition = new Vector2(0f, 160f);
            headingRect.sizeDelta = new Vector2(1200f, 200f);

            var column = MenuSceneBuilder.CreateButtonColumn(canvas);
            column.anchorMin = new Vector2(0.5f, 0.5f);
            column.anchorMax = new Vector2(0.5f, 0.5f);
            column.pivot = new Vector2(0.5f, 0.5f);
            column.anchoredPosition = new Vector2(0f, -80f);

            var resume = MenuSceneBuilder.CreateButton(column, "Resume", MenuAction.Resume, string.Empty);
            MenuSceneBuilder.CreateButton(column, "Settings", MenuAction.OpenAdditive, "Settings");
            MenuSceneBuilder.CreateButton(column, "Quit", MenuAction.Quit, string.Empty);
            resume.gameObject.AddComponent<InitialSelection>();
        }
    }
}
#endif
