#if UNITY_EDITOR
using JamTemplate.Core;
using JamTemplate.Menus;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.EndScreen
{
    /// <summary>
    /// Creates the Win and Lose scenes from Tools &gt; Sailor Snouts &gt; Scenes &gt; Create
    /// Win/Lose Scene — real, editable Unity UI with a background, a result
    /// message and Play Again / Quit buttons.
    /// </summary>
    internal static class EndSceneSetup
    {
        internal const string WinPath = "Assets/Features/Core/Scenes/Win.unity";
        internal const string LosePath = "Assets/Features/Core/Scenes/Lose.unity";

        [MenuItem("Tools/Sailor Snouts/Scenes/Create Win Scene")]
        private static void OpenOrCreateWin() =>
            ToolRegistry.Run("Scenes/Create Win Scene", () => MenuSceneBuilder.OpenOrCreate(WinPath, EnsureWin));

        [MenuItem("Tools/Sailor Snouts/Scenes/Create Lose Scene")]
        private static void OpenOrCreateLose() =>
            ToolRegistry.Run("Scenes/Create Lose Scene", () => MenuSceneBuilder.OpenOrCreate(LosePath, EnsureLose));

        internal static void EnsureWin() =>
            MenuSceneBuilder.EnsureScene(WinPath, false, () => Build("You Win!"));

        internal static void EnsureLose() =>
            MenuSceneBuilder.EnsureScene(LosePath, false, () => Build("Game Over"));

        private static void Build(string message)
        {
            var canvas = MenuSceneBuilder.CreateCanvas("End Screen Canvas");
            MenuSceneBuilder.CreateEventSystem();
            MenuSceneBuilder.CreateBackground(canvas, MenuSceneBuilder.DarkBackground);

            var text = MenuSceneBuilder.CreateText(canvas, "Message", message, 100, FontStyle.Bold);
            var textRect = (RectTransform)text.transform;
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0f, 180f);
            textRect.sizeDelta = new Vector2(1500f, 320f);

            var column = MenuSceneBuilder.CreateButtonColumn(canvas);
            column.anchorMin = new Vector2(0.5f, 0.5f);
            column.anchorMax = new Vector2(0.5f, 0.5f);
            column.pivot = new Vector2(0.5f, 0.5f);
            column.anchoredPosition = new Vector2(0f, -120f);

            var playAgain = MenuSceneBuilder.CreateButton(column, "Play Again", MenuAction.LoadScene, string.Empty);
            MenuSceneBuilder.CreateButton(column, "Quit", MenuAction.Quit, string.Empty);
            playAgain.gameObject.AddComponent<InitialSelection>();
        }
    }
}
#endif
