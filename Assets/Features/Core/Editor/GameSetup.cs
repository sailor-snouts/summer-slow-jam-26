#if UNITY_EDITOR
using JamTemplate.Core;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.Game
{
    /// <summary>Editor helper that adds a Game Manager to the open scene.</summary>
    internal static class GameSetup
    {
        [MenuItem("Tools/Sailor Snouts/Managers/Create Game Manager")]
        private static void CreateInScene() => ToolRegistry.Run("Managers/Create Game Manager", CreateDefault);

        private static void CreateDefault()
        {
            var existing = Object.FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing);
                Debug.Log("[Game] This scene already has a Game Manager.", existing);
                return;
            }

            // Instantiate the same prefab the bootstrapper spawns at runtime, so
            // value tweaks can be applied back to it (a scene copy self-destructs
            // at runtime; only the prefab's values matter).
            GameObject go;
            var prefab = Resources.Load<GameObject>("Game Manager");
            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = prefab.name;
            }
            else
            {
                go = new GameObject("Game Manager");
                go.AddComponent<GameManager>();
            }

            Undo.RegisterCreatedObjectUndo(go, "Create Game Manager");
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            Debug.Log("[Game] Added a Game Manager to the open scene. To change values for the auto-spawned manager, edit them here and Apply the overrides to the prefab — the scene copy self-destructs at runtime.", go);
        }
    }
}
#endif
