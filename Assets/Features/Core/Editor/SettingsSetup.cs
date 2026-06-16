#if UNITY_EDITOR
using JamTemplate.Core;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.Settings
{
    /// <summary>Editor helper that adds a Settings Manager to the open scene.</summary>
    internal static class SettingsSetup
    {
        [MenuItem("Tools/Sailor Snouts/Managers/Create Settings Manager")]
        private static void CreateInScene() => ToolRegistry.Run("Managers/Create Settings Manager", CreateDefault);

        private static void CreateDefault()
        {
            var existing = Object.FindAnyObjectByType<SettingsManager>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing);
                Debug.Log("[Settings] This scene already has a Settings Manager.", existing);
                return;
            }

            // Instantiate the same prefab the bootstrapper spawns at runtime, so
            // value tweaks can be applied back to it (a scene copy self-destructs
            // at runtime; only the prefab's values matter).
            GameObject go;
            var prefab = Resources.Load<GameObject>("Settings Manager");
            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = prefab.name;
            }
            else
            {
                go = new GameObject("Settings Manager");
                go.AddComponent<SettingsManager>();
            }

            Undo.RegisterCreatedObjectUndo(go, "Create Settings Manager");
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            Debug.Log("[Settings] Added a Settings Manager to the open scene. To change values for the auto-spawned manager, edit them here and Apply the overrides to the prefab — the scene copy self-destructs at runtime.", go);
        }
    }
}
#endif
