#if UNITY_EDITOR
using JamTemplate.Core;
using JamTemplate.Menus;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.Settings
{
    /// <summary>
    /// Creates the Settings scene from Tools &gt; Sailor Snouts &gt; Scenes &gt; Create Settings
    /// Scene — an additive overlay (no camera of its own) with audio, display and
    /// graphics controls, an Apply button and a confirm-with-timer dialog. The
    /// controls bind to the SettingsManager, so edit/restyle the scene freely.
    /// </summary>
    internal static class SettingsSceneSetup
    {
        internal const string ScenePath = "Assets/Features/Core/Scenes/Settings.unity";

        [MenuItem("Tools/Sailor Snouts/Scenes/Create Settings Scene")]
        private static void OpenOrCreate() =>
            ToolRegistry.Run("Scenes/Create Settings Scene", () => MenuSceneBuilder.OpenOrCreate(ScenePath, Ensure));

        internal static void Ensure() => MenuSceneBuilder.EnsureScene(ScenePath, true, Build);

        private static void Build()
        {
            // A Settings Manager so the controls have something to read and write —
            // and so settings apply and persist — when the scene is opened on its
            // own. Its Awake captures the engine's current settings and dedups, so a
            // manager already living in a bootstrap scene takes precedence.
            new GameObject("Settings Manager", typeof(SettingsManager));

            var canvas = MenuSceneBuilder.CreateCanvas("Settings Canvas", 500);
            // The overlay may be opened on its own to test; ensure a camera and an
            // EventSystem exist without duplicating them when it sits over a scene.
            canvas.gameObject.AddComponent<EnsureCamera>();
            canvas.gameObject.AddComponent<EnsureEventSystem>();
            canvas.gameObject.AddComponent<SettingsScreen>();

            var background = MenuSceneBuilder.CreateBackground(canvas, MenuSceneBuilder.DarkBackground);
            background.raycastTarget = true; // blocks clicks reaching the scene underneath

            var header = MenuSceneBuilder.CreateText(canvas, "Header", "Settings", 80, FontStyle.Bold);
            var headerRect = (RectTransform)header.transform;
            headerRect.anchorMin = new Vector2(0.5f, 1f);
            headerRect.anchorMax = new Vector2(0.5f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, -50f);
            headerRect.sizeDelta = new Vector2(1400f, 120f);

            BuildControls(canvas);

            // Build the dialog before the Apply button so the button can reference it.
            SettingsConfirmDialog dialog = BuildConfirmDialog(canvas);
            BuildBottomBar(canvas, dialog);
        }

        private static void BuildControls(RectTransform canvas)
        {
            ScrollRect scroll = MenuSceneBuilder.CreateScrollView(canvas);
            var scrollRect = (RectTransform)scroll.transform;
            scrollRect.anchorMin = new Vector2(0.5f, 0f);
            scrollRect.anchorMax = new Vector2(0.5f, 1f);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.sizeDelta = new Vector2(1100f, -320f);
            scrollRect.anchoredPosition = new Vector2(0f, -20f);

            RectTransform content = scroll.content;

            MenuSceneBuilder.CreateSectionHeader(content, "Audio");
            AddVolume(content, "Master", VolumeChannel.Master);
            AddVolume(content, "SFX", VolumeChannel.Sfx);
            AddVolume(content, "Music", VolumeChannel.Music);
            AddVolume(content, "Ambiance", VolumeChannel.Ambiance);
            AddVolume(content, "Dialogue", VolumeChannel.Dialogue);
            AddVolume(content, "UI", VolumeChannel.Ui);

            MenuSceneBuilder.CreateSectionHeader(content, "Interface");
            AddDropdown(content, "UI Scale", DropdownSetting.UiScale);

            MenuSceneBuilder.CreateSectionHeader(content, "Display");
            AddDropdown(content, "Resolution", DropdownSetting.Resolution);
            AddDropdown(content, "Window Mode", DropdownSetting.WindowMode);

            MenuSceneBuilder.CreateSectionHeader(content, "Graphics");
            AddDropdown(content, "Quality", DropdownSetting.Quality);
            AddToggle(content, "VSync");
            AddDropdown(content, "Frame Rate", DropdownSetting.FrameRate);
            AddDropdown(content, "Anti-Aliasing", DropdownSetting.AntiAliasing);
        }

        private static void AddVolume(RectTransform content, string label, VolumeChannel channel)
        {
            RectTransform row = MenuSceneBuilder.CreateSettingRow(content, label);
            Slider slider = MenuSceneBuilder.CreateSlider(row);
            var binder = new SerializedObject(slider.gameObject.AddComponent<AudioVolumeSlider>());
            binder.FindProperty("channel").enumValueIndex = (int)channel;
            binder.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddDropdown(RectTransform content, string label, DropdownSetting setting)
        {
            RectTransform row = MenuSceneBuilder.CreateSettingRow(content, label);
            TMP_Dropdown dropdown = MenuSceneBuilder.CreateDropdown(row);
            var binder = new SerializedObject(dropdown.gameObject.AddComponent<SettingsDropdown>());
            binder.FindProperty("setting").enumValueIndex = (int)setting;
            binder.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddToggle(RectTransform content, string label)
        {
            RectTransform row = MenuSceneBuilder.CreateSettingRow(content, label);
            Toggle toggle = MenuSceneBuilder.CreateToggle(row);
            toggle.gameObject.AddComponent<VSyncToggle>();
        }

        private static void BuildBottomBar(RectTransform canvas, SettingsConfirmDialog dialog)
        {
            var bar = new GameObject("Bottom Bar",
                typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            bar.layer = canvas.gameObject.layer;
            var barRect = (RectTransform)bar.transform;
            barRect.SetParent(canvas, false);
            barRect.anchorMin = new Vector2(0.5f, 0f);
            barRect.anchorMax = new Vector2(0.5f, 0f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.anchoredPosition = new Vector2(0f, 50f);

            var layout = bar.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 24f;

            var fitter = bar.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Button apply = MenuSceneBuilder.CreateActionlessButton(barRect, "Apply");
            var applyBinder = new SerializedObject(apply.gameObject.AddComponent<SettingsApplyButton>());
            applyBinder.FindProperty("dialog").objectReferenceValue = dialog;
            applyBinder.ApplyModifiedPropertiesWithoutUndo();

            Button reset = MenuSceneBuilder.CreateActionlessButton(barRect, "Reset");
            reset.gameObject.AddComponent<SettingsResetButton>();

            Button back = MenuSceneBuilder.CreateButton(barRect, "Back", MenuAction.CloseSelf, string.Empty);
            apply.gameObject.AddComponent<InitialSelection>();
            _ = back;
        }

        private static SettingsConfirmDialog BuildConfirmDialog(RectTransform canvas)
        {
            int layer = canvas.gameObject.layer;

            var panel = new GameObject("Confirm Dialog", typeof(Image));
            panel.layer = layer;
            var panelRect = (RectTransform)panel.transform;
            panelRect.SetParent(canvas, false);
            MenuSceneBuilder.Stretch(panelRect);
            var dim = panel.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.7f);
            dim.raycastTarget = true;

            var box = new GameObject("Box",
                typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            box.layer = layer;
            var boxRect = (RectTransform)box.transform;
            boxRect.SetParent(panelRect, false);
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            box.GetComponent<Image>().color = MenuSceneBuilder.DarkBackground;

            var boxLayout = box.GetComponent<VerticalLayoutGroup>();
            boxLayout.childAlignment = TextAnchor.MiddleCenter;
            boxLayout.childControlWidth = false;
            boxLayout.childControlHeight = false;
            boxLayout.childForceExpandWidth = false;
            boxLayout.childForceExpandHeight = false;
            boxLayout.spacing = 32f;
            boxLayout.padding = new RectOffset(48, 48, 48, 48);

            var boxFitter = box.GetComponent<ContentSizeFitter>();
            boxFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            boxFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var message = MenuSceneBuilder.CreateText(boxRect, "Message", "Keep these settings?", 32, FontStyle.Normal);
            ((RectTransform)message.transform).sizeDelta = new Vector2(640f, 120f);

            var buttonRow = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            buttonRow.layer = layer;
            var buttonRowRect = (RectTransform)buttonRow.transform;
            buttonRowRect.SetParent(boxRect, false);
            var rowLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = false;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            rowLayout.spacing = 24f;

            Button keep = MenuSceneBuilder.CreateActionlessButton(buttonRowRect, "Keep");
            Button revert = MenuSceneBuilder.CreateActionlessButton(buttonRowRect, "Revert");

            var dialog = canvas.gameObject.AddComponent<SettingsConfirmDialog>();
            var so = new SerializedObject(dialog);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("message").objectReferenceValue = message;
            so.FindProperty("revertAfter").floatValue = 15f;
            so.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddPersistentListener(keep.onClick, dialog.Keep);
            UnityEventTools.AddPersistentListener(revert.onClick, dialog.Revert);

            // Hidden in the saved scene; SettingsConfirmDialog also hides it at runtime.
            panel.SetActive(false);
            return dialog;
        }
    }
}
#endif
