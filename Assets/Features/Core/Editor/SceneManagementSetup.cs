#if UNITY_EDITOR
using JamTemplate.Core;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.SceneManagement
{
    /// <summary>Editor helper that adds a Scene Transition Manager to the open scene.</summary>
    internal static class SceneManagementSetup
    {
        [MenuItem("Tools/Sailor Snouts/Managers/Create Scene Transition Manager")]
        private static void CreateInScene() => ToolRegistry.Run("Managers/Create Scene Transition Manager", CreateDefault);

        private static void CreateDefault()
        {
            var existing = Object.FindAnyObjectByType<SceneTransitionManager>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing);
                Debug.Log("[SceneManagement] This scene already has a Scene Transition Manager.", existing);
                return;
            }

            // Instantiate the same prefab the bootstrapper spawns at runtime, so
            // value tweaks can be applied back to it (a scene copy self-destructs
            // at runtime; only the prefab's values matter).
            GameObject go;
            var prefab = Resources.Load<GameObject>("SceneTransitionManager");
            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = prefab.name;
            }
            else
            {
                go = new GameObject("Scene Transition Manager");
                // RequireComponent pulls in SceneTransition and SceneLoader automatically.
                go.AddComponent<SceneTransitionManager>();
            }

            Undo.RegisterCreatedObjectUndo(go, "Create Scene Transition Manager");
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            Debug.Log("[SceneManagement] Added a Scene Transition Manager to the open scene. To change values for the auto-spawned manager, edit them here and Apply the overrides to the prefab — the scene copy self-destructs at runtime.", go);
        }
    }
}
#endif
