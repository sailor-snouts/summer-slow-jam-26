#if UNITY_EDITOR
using JamTemplate.Core;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.Saving
{
    /// <summary>Editor helper that adds a High Score Manager to the open scene.</summary>
    internal static class HighScoreSetup
    {
        [MenuItem("Tools/Sailor Snouts/Managers/Create High Score Manager")]
        private static void CreateInScene() => ToolRegistry.Run("Managers/Create High Score Manager", CreateDefault);

        private static void CreateDefault()
        {
            var existing = Object.FindAnyObjectByType<HighScoreManager>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing);
                Debug.Log("[Saving] This scene already has a High Score Manager.", existing);
                return;
            }

            // Instantiate the same prefab the bootstrapper spawns at runtime, so
            // value tweaks can be applied back to it (a scene copy self-destructs
            // at runtime; only the prefab's values matter).
            GameObject go;
            var prefab = Resources.Load<GameObject>("High Score Manager");
            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = prefab.name;
            }
            else
            {
                go = new GameObject("High Score Manager");
                go.AddComponent<HighScoreManager>();
            }

            Undo.RegisterCreatedObjectUndo(go, "Create High Score Manager");
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            Debug.Log("[Saving] Added a High Score Manager to the open scene. To change values for the auto-spawned manager, edit them here and Apply the overrides to the prefab — the scene copy self-destructs at runtime.", go);
        }
    }
}
#endif
