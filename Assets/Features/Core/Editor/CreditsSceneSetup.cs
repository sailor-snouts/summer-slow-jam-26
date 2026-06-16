#if UNITY_EDITOR
using JamTemplate.Core;
using JamTemplate.Menus;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace JamTemplate.Credits
{
    /// <summary>
    /// Creates the credits scene from Tools &gt; Sailor Snouts &gt; Scenes &gt; Create Credits
    /// Scene — a Canvas with a heading, scroll view and Back button, plus a
    /// <see cref="CreditsBuilder"/> wired to a <see cref="CreditsData"/> asset
    /// and the role/names TMP text prefabs.
    /// </summary>
    internal static class CreditsSceneSetup
    {
        internal const string ScenePath = "Assets/Features/Core/Scenes/Credits.unity";
        private const string DataPath = "Assets/Features/Core/ScriptableObjects/Credits.asset";
        private const string RolePrefabPath = "Assets/Features/Core/Prefabs/Role Text.prefab";
        private const string NamesPrefabPath = "Assets/Features/Core/Prefabs/Names Text.prefab";

        [MenuItem("Tools/Sailor Snouts/Scenes/Create Credits Scene")]
        private static void OpenOrCreate() => ToolRegistry.Run("Scenes/Create Credits Scene", OpenOrCreateDefault);

        private static void OpenOrCreateDefault()
        {
            // Prompt before EnsureAssets so the user handles real unsaved work
            // first; any dirty state left by the temp prefab GameObject is
            // discarded when OpenScene loads the credits scene in Single mode.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EnsureAssets();
            Ensure();
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                EditorSceneManager.OpenScene(ScenePath);
        }

        internal static void Ensure() => MenuSceneBuilder.EnsureScene(ScenePath, false, Build);

        private static void Build()
        {
            var canvas = MenuSceneBuilder.CreateCanvas("Credits Canvas");
            MenuSceneBuilder.CreateEventSystem();
            MenuSceneBuilder.CreateBackground(canvas, MenuSceneBuilder.DarkBackground);

            var header = MenuSceneBuilder.CreateText(canvas, "Header", "Credits", 80, FontStyle.Bold);
            var headerRect = (RectTransform)header.transform;
            headerRect.anchorMin = new Vector2(0.5f, 1f);
            headerRect.anchorMax = new Vector2(0.5f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0f, -60f);
            headerRect.sizeDelta = new Vector2(1400f, 150f);

            var scroll = MenuSceneBuilder.CreateScrollView(canvas);
            var scrollRect = (RectTransform)scroll.transform;
            scrollRect.anchorMin = new Vector2(0.5f, 0f);
            scrollRect.anchorMax = new Vector2(0.5f, 1f);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.sizeDelta = new Vector2(960f, -370f);
            scrollRect.anchoredPosition = new Vector2(0f, -35f);

            CreditsData data = AssetDatabase.LoadAssetAtPath<CreditsData>(DataPath);
            CreditsBuilder builder = canvas.gameObject.AddComponent<CreditsBuilder>();
            var builderObject = new SerializedObject(builder);
            builderObject.FindProperty("data").objectReferenceValue = data;
            builderObject.FindProperty("content").objectReferenceValue = scroll.content;
            builderObject.ApplyModifiedPropertiesWithoutUndo();

            var column = MenuSceneBuilder.CreateButtonColumn(canvas);
            column.anchorMin = new Vector2(0.5f, 0f);
            column.anchorMax = new Vector2(0.5f, 0f);
            column.pivot = new Vector2(0.5f, 0f);
            column.anchoredPosition = new Vector2(0f, 70f);

            var back = MenuSceneBuilder.CreateButton(column, "Back", MenuAction.LoadScene, "Title");
            back.gameObject.AddComponent<InitialSelection>();
        }

        private static void EnsureAssets()
        {
            TMP_Text rolePrefab = EnsureRolePrefab();
            TMP_Text namesPrefab = EnsureNamesPrefab();
            EnsureData(rolePrefab, namesPrefab);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureData(TMP_Text rolePrefab, TMP_Text namesPrefab)
        {
            CreditsData existing = AssetDatabase.LoadAssetAtPath<CreditsData>(DataPath);
            if (existing != null)
            {
                bool dirty = false;
                if (existing.rolePrefab == null) { existing.rolePrefab = rolePrefab; dirty = true; }
                if (existing.namesPrefab == null) { existing.namesPrefab = namesPrefab; dirty = true; }
                if (dirty) EditorUtility.SetDirty(existing);
                return;
            }

            CreditsData data = ScriptableObject.CreateInstance<CreditsData>();
            data.rolePrefab = rolePrefab;
            data.namesPrefab = namesPrefab;
            AssetDatabase.CreateAsset(data, DataPath);
        }

        private static TMP_Text EnsureRolePrefab()
            => EnsureTextPrefab(RolePrefabPath, "Role Text", 46, FontStyles.Bold);

        private static TMP_Text EnsureNamesPrefab()
            => EnsureTextPrefab(NamesPrefabPath, "Names Text", 30, FontStyles.Normal);

        private static TMP_Text EnsureTextPrefab(string path, string name, int fontSize, FontStyles style)
        {
            TMP_Text existing = LoadPrefabComponent(path);
            if (existing != null)
                return existing;

            // HideInHierarchy (without DontSave) keeps the temp GameObject
            // invisible while letting SaveAsPrefabAsset succeed.
            var go = EditorUtility.CreateGameObjectWithHideFlags(name,
                HideFlags.HideInHierarchy, typeof(TextMeshProUGUI));
            try
            {
                var text = go.GetComponent<TextMeshProUGUI>();
                text.font = TMP_Settings.defaultFontAsset;
                text.text = name;
                text.fontSize = fontSize;
                text.fontStyle = style;
                text.alignment = TextAlignmentOptions.Top;
                text.textWrappingMode = TextWrappingModes.Normal;
                text.color = Color.white;
                text.raycastTarget = false;

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                return prefab.GetComponent<TMP_Text>();
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private static TMP_Text LoadPrefabComponent(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return prefab != null ? prefab.GetComponent<TMP_Text>() : null;
        }
    }
}
#endif
