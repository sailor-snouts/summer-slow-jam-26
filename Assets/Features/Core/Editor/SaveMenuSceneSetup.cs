#if UNITY_EDITOR
using JamTemplate.Core;
using JamTemplate.Menus;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.Saving
{
    /// <summary>
    /// Creates the Saves scene from Tools &gt; Sailor Snouts &gt; Scenes &gt; Create
    /// Saves Scene — an additive overlay listing save slots, each with Save / Load /
    /// Delete. The rows bind to the SaveManager, so edit/restyle the scene freely.
    /// </summary>
    internal static class SaveMenuSceneSetup
    {
        internal const string ScenePath = "Assets/Features/Core/Scenes/Saves.unity";
        private const int SlotCount = 3;

        [MenuItem("Tools/Sailor Snouts/Scenes/Create Saves Scene")]
        private static void OpenOrCreate() =>
            ToolRegistry.Run("Scenes/Create Saves Scene", () => MenuSceneBuilder.OpenOrCreate(ScenePath, Ensure));

        internal static void Ensure() => MenuSceneBuilder.EnsureScene(ScenePath, true, Build);

        private static void Build()
        {
            // A Save Manager so the slots work when the scene is opened on its own;
            // its Awake dedups, so a bootstrap-scene manager still takes precedence.
            new GameObject("Save Manager", typeof(SaveManager));

            var canvas = MenuSceneBuilder.CreateCanvas("Saves Canvas", 1500);
            canvas.gameObject.AddComponent<EnsureCamera>();
            canvas.gameObject.AddComponent<EnsureEventSystem>();

            var background = MenuSceneBuilder.CreateBackground(canvas, MenuSceneBuilder.DarkBackground);
            background.raycastTarget = true; // blocks clicks reaching the scene underneath

            var header = MenuSceneBuilder.CreateText(canvas, "Header", "Saves", 80, FontStyle.Bold);
            var headerRect = (RectTransform)header.transform;
            headerRect.anchorMin = new Vector2(0.5f, 1f);
            headerRect.anchorMax = new Vector2(0.5f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, -50f);
            headerRect.sizeDelta = new Vector2(1400f, 120f);

            BuildSlots(canvas);
            BuildBottomBar(canvas);
        }

        private static void BuildSlots(RectTransform canvas)
        {
            ScrollRect scroll = MenuSceneBuilder.CreateScrollView(canvas);
            var scrollRect = (RectTransform)scroll.transform;
            scrollRect.anchorMin = new Vector2(0.5f, 0f);
            scrollRect.anchorMax = new Vector2(0.5f, 1f);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.sizeDelta = new Vector2(1200f, -320f);
            scrollRect.anchoredPosition = new Vector2(0f, -20f);

            RectTransform content = scroll.content;
            for (int i = 0; i < SlotCount; i++)
                AddSlotRow(content, i);
        }

        private static void AddSlotRow(RectTransform content, int slot)
        {
            var rowObject = new GameObject($"Slot {slot} Row",
                typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            rowObject.layer = content.gameObject.layer;
            var row = (RectTransform)rowObject.transform;
            row.SetParent(content, false);

            var layout = rowObject.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 16f;
            rowObject.GetComponent<LayoutElement>().minHeight = 80f;

            var label = MenuSceneBuilder.CreateText(row, "Label", $"Slot {slot + 1}", 32, FontStyle.Normal);
            label.alignment = TextAlignmentOptions.Left;
            var labelElement = label.gameObject.AddComponent<LayoutElement>();
            labelElement.flexibleWidth = 1f;
            labelElement.minWidth = 320f;

            Button save = SlotButton(row, "Save");
            Button load = SlotButton(row, "Load");
            Button delete = SlotButton(row, "Delete");

            var view = new SerializedObject(rowObject.AddComponent<SaveSlotView>());
            view.FindProperty("slot").intValue = slot;
            view.FindProperty("label").objectReferenceValue = label;
            view.FindProperty("saveButton").objectReferenceValue = save;
            view.FindProperty("loadButton").objectReferenceValue = load;
            view.FindProperty("deleteButton").objectReferenceValue = delete;
            view.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button SlotButton(RectTransform row, string label)
        {
            Button button = MenuSceneBuilder.CreateActionlessButton(row, label);
            var element = button.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = 150f;
            element.minHeight = 64f;
            return button;
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
