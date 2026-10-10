using JamTemplate.Menus;
using UnityEditor;
using UnityEngine;

namespace Game
{
    // Tools > Game > Create Thank You Scene builds the end-of-story screen (and adds it to Build
    // Settings). The dialogue sends the player here with ChangeScene("ThankYou"). The objects are
    // plain scene UI, so restyle freely once it exists.
    internal static class ThankYouSceneSetup
    {
        internal const string ScenePath = "Assets/Scenes/ThankYou.unity";
        private const string TitleScene = "Title";

        [MenuItem("Tools/Game/Create Thank You Scene")]
        private static void OpenOrCreate() =>
            MenuSceneBuilder.OpenOrCreate(ScenePath, () => MenuSceneBuilder.EnsureScene(ScenePath, false, Build));

        private static void Build()
        {
            var canvas = MenuSceneBuilder.CreateCanvas("Thank You Canvas");
            MenuSceneBuilder.CreateEventSystem();
            MenuSceneBuilder.CreateBackground(canvas, MenuSceneBuilder.DarkBackground);

            var title = MenuSceneBuilder.CreateText(canvas, "Message", "Thank you for playing!", 100, FontStyle.Bold);
            Place((RectTransform)title.transform, new Vector2(0f, 180f), new Vector2(1600f, 320f));

            var column = MenuSceneBuilder.CreateButtonColumn(canvas);
            Place(column, new Vector2(0f, -160f), column.sizeDelta);

            var backToTitle = MenuSceneBuilder.CreateButton(column, "Back to Title", MenuAction.LoadScene, TitleScene);
            MenuSceneBuilder.CreateButton(column, "Quit", MenuAction.Quit, string.Empty);
            backToTitle.gameObject.AddComponent<InitialSelection>();
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
