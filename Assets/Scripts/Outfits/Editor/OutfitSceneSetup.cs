using JamTemplate.Core;
using JamTemplate.Menus;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// Creates the outfit menu scene from Tools > Game > Create Outfit Scene - an additive overlay
    /// with the Disco Elysium layout: stats on the left, the dressed character in the center, outfit
    /// choices on the right, and a Close button. The panels are plain scene objects, so restyle them
    /// freely; the row/button contents are rebuilt at runtime by <see cref="OutfitMenuView"/>.
    /// Also makes sure a Wardrobe asset exists and is wired in.
    /// </summary>
    internal static class OutfitSceneSetup
    {
        internal const string ScenePath = "Assets/Scenes/Outfits.unity";
        private const string WardrobeFolder = "Assets/Outfits";
        private const string WardrobePath = WardrobeFolder + "/Wardrobe.asset";

        [MenuItem("Tools/Game/Create Outfit Scene")]
        private static void OpenOrCreate() =>
            MenuSceneBuilder.OpenOrCreate(ScenePath, () => MenuSceneBuilder.EnsureScene(ScenePath, true, Build));

        private static void Build()
        {
            var canvas = MenuSceneBuilder.CreateCanvas("Outfit Canvas", 800);
            canvas.gameObject.AddComponent<EnsureCamera>();
            canvas.gameObject.AddComponent<EnsureEventSystem>();

            var background = MenuSceneBuilder.CreateBackground(canvas, MenuSceneBuilder.DarkBackground);
            background.raycastTarget = true; // blocks clicks reaching the game underneath

            var header = MenuSceneBuilder.CreateText(canvas, "Header", "OUTFITS", 64, FontStyle.Bold);
            var headerRect = (RectTransform)header.transform;
            header.alignment = TextAlignmentOptions.Left;
            SetAnchors(headerRect, new Vector2(0.03f, 0.88f), new Vector2(0.45f, 0.98f));

            // Left: stats rows are generated at runtime under this layout container.
            RectTransform statsContainer = CreatePanel(canvas, "Stats Panel",
                new Vector2(0.03f, 0.06f), new Vector2(0.28f, 0.86f));

            // Center: character preview with name above and outfit description below.
            var nameText = MenuSceneBuilder.CreateText(canvas, "Character Name", "Character", 40, FontStyle.Bold);
            SetAnchors((RectTransform)nameText.transform, new Vector2(0.32f, 0.86f), new Vector2(0.62f, 0.94f));

            var previewGo = new GameObject("Outfit Preview", typeof(Image));
            previewGo.layer = canvas.gameObject.layer;
            var previewRect = (RectTransform)previewGo.transform;
            previewRect.SetParent(canvas, false);
            SetAnchors(previewRect, new Vector2(0.34f, 0.28f), new Vector2(0.60f, 0.84f));
            var previewImage = previewGo.GetComponent<Image>();
            previewImage.preserveAspect = true;
            previewImage.raycastTarget = false;

            var description = MenuSceneBuilder.CreateText(canvas, "Outfit Description", "Nothing equipped.", 26, FontStyle.Normal);
            SetAnchors((RectTransform)description.transform, new Vector2(0.32f, 0.06f), new Vector2(0.62f, 0.26f));

            // Right: scrolling outfit list. Buttons are cloned from an inactive template at runtime.
            ScrollRect scroll = MenuSceneBuilder.CreateScrollView(canvas);
            SetAnchors((RectTransform)scroll.transform, new Vector2(0.66f, 0.16f), new Vector2(0.97f, 0.86f));

            Button template = MenuSceneBuilder.CreateActionlessButton(scroll.content, "Outfit");
            template.name = "Outfit Button Template";
            template.gameObject.SetActive(false);

            // Bottom right: Close (Escape and the I key close it too, via the overlay stack).
            RectTransform buttons = MenuSceneBuilder.CreateButtonColumn(canvas, "Bottom Buttons");
            SetAnchors(buttons, new Vector2(0.66f, 0.04f), new Vector2(0.97f, 0.14f));
            MenuSceneBuilder.CreateButton(buttons, "Close", MenuAction.CloseSelf, string.Empty);

            // Wire the view.
            var view = canvas.gameObject.AddComponent<OutfitMenuView>();
            var so = new SerializedObject(view);
            so.FindProperty("wardrobe").objectReferenceValue = EnsureWardrobe();
            so.FindProperty("statsContainer").objectReferenceValue = statsContainer;
            so.FindProperty("previewImage").objectReferenceValue = previewImage;
            so.FindProperty("previewName").objectReferenceValue = nameText;
            so.FindProperty("previewDescription").objectReferenceValue = description;
            so.FindProperty("outfitsContainer").objectReferenceValue = scroll.content;
            so.FindProperty("outfitButtonTemplate").objectReferenceValue = template;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>An anchored container with a top-aligned vertical layout for runtime-built rows.</summary>
        private static RectTransform CreatePanel(RectTransform canvas, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.layer = canvas.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(canvas, false);
            SetAnchors(rect, anchorMin, anchorMax);

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 6f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            return rect;
        }

        private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>Loads the shared Wardrobe asset, creating an empty one on first run.</summary>
        private static Wardrobe EnsureWardrobe()
        {
            var wardrobe = AssetDatabase.LoadAssetAtPath<Wardrobe>(WardrobePath);
            if (wardrobe != null)
                return wardrobe;

            if (!AssetDatabase.IsValidFolder(WardrobeFolder))
                AssetDatabase.CreateFolder("Assets", "Outfits");
            wardrobe = ScriptableObject.CreateInstance<Wardrobe>();
            AssetDatabase.CreateAsset(wardrobe, WardrobePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[OutfitSceneSetup] Created empty wardrobe at {WardrobePath} - add your Outfit assets to it.");
            return wardrobe;
        }
    }
}
