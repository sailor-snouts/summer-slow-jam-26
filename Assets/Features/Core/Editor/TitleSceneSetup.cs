#if UNITY_EDITOR
using JamTemplate.Core;
using JamTemplate.Menus;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.Title
{
    /// <summary>
    /// Creates the title scene from Tools &gt; Sailor Snouts &gt; Scenes &gt; Create Title Scene
    /// — real, editable Unity UI with a background, title and a column of menu
    /// buttons. Re-run the command (after deleting the scene) to regenerate it.
    /// </summary>
    internal static class TitleSceneSetup
    {
        internal const string ScenePath = "Assets/Features/Core/Scenes/Title.unity";

        [MenuItem("Tools/Sailor Snouts/Scenes/Create Title Scene")]
        private static void OpenOrCreate() =>
            ToolRegistry.Run("Scenes/Create Title Scene", () => MenuSceneBuilder.OpenOrCreate(ScenePath, Ensure));

        internal static void Ensure() => MenuSceneBuilder.EnsureScene(ScenePath, false, Build);

        private static void Build()
        {
            var canvas = MenuSceneBuilder.CreateCanvas("Title Canvas");
            MenuSceneBuilder.CreateEventSystem();
            MenuSceneBuilder.CreateBackground(canvas, MenuSceneBuilder.DarkBackground);

            var title = MenuSceneBuilder.CreateText(canvas, "Title", "Game Title", 96, FontStyle.Bold);
            var titleRect = (RectTransform)title.transform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -160f);
            titleRect.sizeDelta = new Vector2(1400f, 220f);

            var column = MenuSceneBuilder.CreateButtonColumn(canvas);
            column.anchorMin = new Vector2(0.5f, 0.5f);
            column.anchorMax = new Vector2(0.5f, 0.5f);
            column.pivot = new Vector2(0.5f, 0.5f);
            column.anchoredPosition = new Vector2(0f, -120f);

            // New Game always shows; set its scene (your first gameplay scene) in
            // the inspector. Continue and Load Game appear only when a save exists.
            var newGame = MenuSceneBuilder.CreateButton(column, "New Game", MenuAction.LoadScene, string.Empty);

            var continueGame = MenuSceneBuilder.CreateButton(column, "Continue", MenuAction.Continue, string.Empty);
            SetVisibility(continueGame.gameObject, SaveVisibility.WhenSaveExists);

            var loadGame = MenuSceneBuilder.CreateButton(column, "Load Game", MenuAction.OpenAdditive, "Saves");
            SetVisibility(loadGame.gameObject, SaveVisibility.WhenSaveExists);

            MenuSceneBuilder.CreateButton(column, "Settings", MenuAction.OpenAdditive, "Settings");
            MenuSceneBuilder.CreateButton(column, "Credits", MenuAction.LoadScene, "Credits");
            MenuSceneBuilder.CreateButton(column, "Quit", MenuAction.Quit, string.Empty);
            newGame.gameObject.AddComponent<InitialSelection>();
        }

        private static void SetVisibility(GameObject button, SaveVisibility visibility)
        {
            var binder = new SerializedObject(button.GetComponent<MenuButtonAction>());
            binder.FindProperty("visibleWhen").enumValueIndex = (int)visibility;
            binder.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
