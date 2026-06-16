#if UNITY_EDITOR
using JamTemplate.Core;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.Audio
{
    /// <summary>Editor helper that adds an Audio Manager to the open scene.</summary>
    internal static class AudioSetup
    {
        [MenuItem("Tools/Sailor Snouts/Managers/Create Audio Manager")]
        private static void CreateInScene() => ToolRegistry.Run("Managers/Create Audio Manager", CreateDefault);

        private static void CreateDefault()
        {
            var existing = Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing);
                Debug.Log("[Audio] This scene already has an Audio Manager.", existing);
                return;
            }

            // Instantiate the same prefab the bootstrapper spawns at runtime, so
            // value tweaks can be applied back to it (a scene copy self-destructs
            // at runtime; only the prefab's values matter).
            GameObject go;
            var prefab = Resources.Load<GameObject>("Audio Manager");
            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = prefab.name;
            }
            else
            {
                go = new GameObject("Audio Manager");
                go.AddComponent<AudioManager>();
            }

            Undo.RegisterCreatedObjectUndo(go, "Create Audio Manager");
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            Debug.Log("[Audio] Added an Audio Manager to the open scene. To change values for the auto-spawned manager, edit them here and Apply the overrides to the prefab — the scene copy self-destructs at runtime.", go);
        }
    }
}
#endif
