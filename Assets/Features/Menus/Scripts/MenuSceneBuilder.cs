#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Editor helpers that assemble menu UI — a Canvas, EventSystem, background,
    /// text and Unity UI Buttons — as real, editable GameObjects, and that
    /// create the menu scenes automatically so nothing is built at runtime.
    /// </summary>
    public static class MenuSceneBuilder
    {
        public static readonly Color DarkBackground = new Color(0.06f, 0.07f, 0.10f, 1f);
        public static readonly Color ButtonColor = new Color(0.15f, 0.17f, 0.22f, 1f);

        /// <summary>
        /// Creates the scene at <paramref name="path"/> if it does not exist yet,
        /// builds it with <paramref name="populate"/>, and registers it in Build
        /// Settings. Runs without disturbing the scene currently open.
        /// </summary>
        public static void EnsureScene(string path, bool emptyScene, Action populate)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
                return;

            Scene previous = SceneManager.GetActiveScene();
            Scene openAtPath = FindOpenScene(path);
            bool reuse = openAtPath.IsValid();
            Scene scene;

            if (reuse)
            {
                // A scene is already open at this path with no asset behind it
                // (its file was deleted while open). Rebuild it in place.
                scene = openAtPath;
                foreach (GameObject root in scene.GetRootGameObjects())
                    UnityEngine.Object.DestroyImmediate(root);
                if (!emptyScene)
                    CreateDefaultObjects();
            }
            else
            {
                NewSceneSetup setup = emptyScene ? NewSceneSetup.EmptyScene : NewSceneSetup.DefaultGameObjects;
                scene = EditorSceneManager.NewScene(setup, NewSceneMode.Additive);
            }

            SceneManager.SetActiveScene(scene);
            populate();
            bool saved = EditorSceneManager.SaveScene(scene, path);

            if (previous.IsValid() && previous.isLoaded && previous != scene)
                SceneManager.SetActiveScene(previous);
            if (!reuse)
                EditorSceneManager.CloseScene(scene, true);

            if (saved)
            {
                RegisterInBuildSettings(path);
                Debug.Log($"[Sailor Snouts] Created {path}.",
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(path));
            }
            else
            {
                Debug.LogError($"[Sailor Snouts] Failed to create {path}.");
            }
        }

        /// <summary>
        /// Manual entry point for a [MenuItem]: prompts to save unsaved scenes,
        /// ensures the scene exists, then opens it.
        /// </summary>
        public static void OpenOrCreate(string path, Action ensure)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            ensure();
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
                EditorSceneManager.OpenScene(path);
        }

        private static Scene FindOpenScene(string path)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.path == path)
                    return scene;
            }
            return default;
        }

        private static void CreateDefaultObjects()
        {
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 1f, -10f);

            var light = new GameObject("Directional Light", typeof(Light));
            light.GetComponent<Light>().type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        /// <summary>Creates a screen-space Canvas with a scaler and raycaster.</summary>
        public static RectTransform CreateCanvas(string name, int sortingOrder = 0)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.layer = LayerMask.NameToLayer("UI");

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            // TextMeshPro SDF rendering needs these channels for crisp edges and
            // its outline/glow/underlay effects to work.
            canvas.additionalShaderChannels =
                AdditionalCanvasShaderChannels.TexCoord1
                | AdditionalCanvasShaderChannels.Normal
                | AdditionalCanvasShaderChannels.Tangent;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Applies the player's UI scale setting to this canvas at runtime.
            go.AddComponent<UIScaler>();

            return (RectTransform)go.transform;
        }

        /// <summary>Creates an EventSystem wired to the project's Input Actions asset.</summary>
        public static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            UIInput.Configure(go.GetComponent<InputSystemUIInputModule>());
        }

        /// <summary>Creates a full-screen solid-colour background image.</summary>
        public static Image CreateBackground(RectTransform canvas, Color color)
        {
            var go = new GameObject("Background", typeof(Image));
            go.layer = canvas.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(canvas, false);
            Stretch(rect);

            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>Creates a TextMeshPro UGUI element; the caller positions the returned RectTransform.</summary>
        public static TextMeshProUGUI CreateText(RectTransform parent, string name, string content, int fontSize, FontStyle style)
        {
            var go = new GameObject(name, typeof(TextMeshProUGUI));
            go.layer = parent.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);

            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = ConvertStyle(style);
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static FontStyles ConvertStyle(FontStyle style)
        {
            switch (style)
            {
                case FontStyle.Bold: return FontStyles.Bold;
                case FontStyle.Italic: return FontStyles.Italic;
                case FontStyle.BoldAndItalic: return FontStyles.Bold | FontStyles.Italic;
                default: return FontStyles.Normal;
            }
        }

        /// <summary>
        /// Creates a vertically stacked button container. Buttons added under it
        /// auto-arrange, and it sizes itself to fit them.
        /// </summary>
        public static RectTransform CreateButtonColumn(RectTransform parent, string name = "Buttons")
        {
            var go = new GameObject(name,
                typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.layer = parent.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 20f;

            var fitter = go.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return rect;
        }

        /// <summary>
        /// Creates a Unity UI Button under <paramref name="column"/> with a label,
        /// hover feedback (<see cref="MenuButton"/>) and a configured action.
        /// </summary>
        public static Button CreateButton(RectTransform column, string label, MenuAction action, string scene)
        {
            GameObject go = CreateButtonObject(column, label);

            var menuAction = new SerializedObject(go.AddComponent<MenuButtonAction>());
            menuAction.FindProperty("action").enumValueIndex = (int)action;
            menuAction.FindProperty("scene").stringValue = scene ?? string.Empty;
            menuAction.ApplyModifiedPropertiesWithoutUndo();

            return go.GetComponent<Button>();
        }

        /// <summary>
        /// Creates a styled Button with no MenuButtonAction — for buttons wired to
        /// their own component or onClick listener (e.g. Apply, Keep, Revert).
        /// </summary>
        public static Button CreateActionlessButton(RectTransform column, string label)
            => CreateButtonObject(column, label).GetComponent<Button>();

        private static GameObject CreateButtonObject(RectTransform column, string label)
        {
            var go = new GameObject(label + " Button", typeof(Image), typeof(Button), typeof(MenuButton));
            go.layer = column.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(column, false);
            rect.sizeDelta = new Vector2(360f, 72f);

            var image = go.GetComponent<Image>();
            image.color = ButtonColor;
            go.GetComponent<Button>().targetGraphic = image;

            var labelText = CreateText(rect, "Label", label, 32, FontStyle.Normal);
            Stretch((RectTransform)labelText.transform);
            return go;
        }

        /// <summary>
        /// Creates a vertical ScrollRect. Returns it — add content under
        /// <see cref="ScrollRect.content"/>, position the ScrollRect's transform.
        /// </summary>
        public static ScrollRect CreateScrollView(RectTransform parent)
        {
            var scrollObject = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.layer = parent.gameObject.layer;
            var scrollRect = (RectTransform)scrollObject.transform;
            scrollRect.SetParent(parent, false);

            var viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewportObject.layer = parent.gameObject.layer;
            var viewport = (RectTransform)viewportObject.transform;
            viewport.SetParent(scrollRect, false);
            Stretch(viewport);
            viewport.pivot = new Vector2(0.5f, 1f);

            var contentObject = new GameObject("Content",
                typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.layer = parent.gameObject.layer;
            var content = (RectTransform)contentObject.transform;
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            var layout = contentObject.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 32f;
            layout.padding = new RectOffset(24, 24, 24, 24);

            var fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Vertical scrollbar pinned to the right edge. AutoHideAndExpandViewport
            // below insets the viewport to make room for it when content overflows.
            GameObject scrollbarObject = DefaultControls.CreateScrollbar(UIResources());
            scrollbarObject.name = "Scrollbar Vertical";
            SetLayerRecursive(scrollbarObject, parent.gameObject.layer);
            var scrollbar = scrollbarObject.GetComponent<Scrollbar>();
            scrollbar.SetDirection(Scrollbar.Direction.BottomToTop, true);
            var scrollbarRect = (RectTransform)scrollbarObject.transform;
            scrollbarRect.SetParent(scrollRect, false);
            scrollbarRect.anchorMin = new Vector2(1f, 0f);
            scrollbarRect.anchorMax = new Vector2(1f, 1f);
            scrollbarRect.pivot = new Vector2(1f, 1f);
            scrollbarRect.sizeDelta = new Vector2(20f, 0f);
            scrollbarRect.anchoredPosition = Vector2.zero;

            var scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scroll.verticalScrollbarSpacing = -3f;
            return scroll;
        }

        /// <summary>Creates a left-aligned bold section header (e.g. "Audio") for a settings list.</summary>
        public static TextMeshProUGUI CreateSectionHeader(RectTransform parent, string text)
        {
            var header = CreateText(parent, text + " Header", text, 52, FontStyle.Bold);
            header.alignment = TextAlignmentOptions.Left;
            header.gameObject.AddComponent<LayoutElement>().minHeight = 72f;
            return header;
        }

        /// <summary>
        /// Creates a labelled row (a horizontal layout with a TMP label on the
        /// left). Add a control into the returned RectTransform.
        /// </summary>
        public static RectTransform CreateSettingRow(RectTransform parent, string label)
        {
            var go = new GameObject(label + " Row",
                typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            go.layer = parent.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);

            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 24f;
            go.GetComponent<LayoutElement>().minHeight = 64f;

            var label1 = CreateText(rect, "Label", label, 36, FontStyle.Normal);
            label1.alignment = TextAlignmentOptions.Left;
            var labelElement = label1.gameObject.AddComponent<LayoutElement>();
            labelElement.minWidth = 340f;
            labelElement.preferredWidth = 340f;

            return rect;
        }

        /// <summary>Creates a 0..1 Slider in a row.</summary>
        public static Slider CreateSlider(RectTransform row)
        {
            GameObject go = DefaultControls.CreateSlider(UIResources());
            SetLayerRecursive(go, row.gameObject.layer);
            go.transform.SetParent(row, false);

            var element = go.AddComponent<LayoutElement>();
            element.preferredWidth = 420f;
            element.minHeight = 24f;

            var slider = go.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            return slider;
        }

        /// <summary>Creates a TMP Dropdown in a row.</summary>
        public static TMP_Dropdown CreateDropdown(RectTransform row)
        {
            GameObject go = TMP_DefaultControls.CreateDropdown(TMPResources());
            SetLayerRecursive(go, row.gameObject.layer);
            go.transform.SetParent(row, false);

            var element = go.AddComponent<LayoutElement>();
            element.preferredWidth = 420f;
            element.minHeight = 56f;

            // TMP's default caption/item text is tiny; match the row labels.
            var dropdown = go.GetComponent<TMP_Dropdown>();
            if (dropdown.captionText != null)
                dropdown.captionText.fontSize = 32f;
            if (dropdown.itemText != null)
                dropdown.itemText.fontSize = 32f;
            return dropdown;
        }

        /// <summary>Creates a Toggle in a row, with its built-in legacy label removed.</summary>
        public static Toggle CreateToggle(RectTransform row)
        {
            GameObject go = DefaultControls.CreateToggle(UIResources());
            SetLayerRecursive(go, row.gameObject.layer);
            go.transform.SetParent(row, false);

            // DefaultControls adds a legacy-Text "Label" child; drop it so the
            // row's own TMP label is the only one.
            Transform builtinLabel = go.transform.Find("Label");
            if (builtinLabel != null)
                UnityEngine.Object.DestroyImmediate(builtinLabel.gameObject);

            var element = go.AddComponent<LayoutElement>();
            element.preferredWidth = 48f;
            element.minHeight = 40f;
            return go.GetComponent<Toggle>();
        }

        private static DefaultControls.Resources UIResources() => new DefaultControls.Resources
        {
            standard = BuiltinSprite("UI/Skin/UISprite.psd"),
            background = BuiltinSprite("UI/Skin/Background.psd"),
            inputField = BuiltinSprite("UI/Skin/InputFieldBackground.psd"),
            knob = BuiltinSprite("UI/Skin/Knob.psd"),
            checkmark = BuiltinSprite("UI/Skin/Checkmark.psd"),
            dropdown = BuiltinSprite("UI/Skin/DropdownArrow.psd"),
            mask = BuiltinSprite("UI/Skin/UIMask.psd"),
        };

        private static TMP_DefaultControls.Resources TMPResources() => new TMP_DefaultControls.Resources
        {
            standard = BuiltinSprite("UI/Skin/UISprite.psd"),
            background = BuiltinSprite("UI/Skin/Background.psd"),
            inputField = BuiltinSprite("UI/Skin/InputFieldBackground.psd"),
            knob = BuiltinSprite("UI/Skin/Knob.psd"),
            checkmark = BuiltinSprite("UI/Skin/Checkmark.psd"),
            dropdown = BuiltinSprite("UI/Skin/DropdownArrow.psd"),
            mask = BuiltinSprite("UI/Skin/UIMask.psd"),
        };

        private static Sprite BuiltinSprite(string path) =>
            AssetDatabase.GetBuiltinExtraResource<Sprite>(path);

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursive(child.gameObject, layer);
        }

        /// <summary>
        /// Adds a scene to Build Settings if it is not already listed. If the path
        /// is listed but its GUID no longer matches the asset (the scene was
        /// deleted and regenerated), the entry is rewritten to point at the
        /// current asset.
        /// </summary>
        public static void RegisterInBuildSettings(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            GUID assetGuid = AssetDatabase.GUIDFromAssetPath(path);

            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path != path)
                    continue;

                if (scenes[i].guid != assetGuid)
                {
                    scenes[i] = new EditorBuildSettingsScene(path, scenes[i].enabled);
                    EditorBuildSettings.scenes = scenes.ToArray();
                }
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        /// <summary>Stretches a RectTransform to fill its parent.</summary>
        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
#endif
