using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JamTemplate.Audio;
using JamTemplate.Core;
using JamTemplate.Game;
using JamTemplate.Menus;
using JamTemplate.Saving;
using JamTemplate.SceneManagement;
using JamTemplate.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JamTemplate.Core.Editor
{
    /// <summary>
    /// Editor utility that strips manager objects out of every Build-Settings scene.
    /// Managers are now spawned by <c>ManagerBootstrapper</c> at startup, so any copy
    /// placed in a scene is a redundant duplicate that self-destructs at runtime.
    /// This removes them so the scenes stay clean.
    /// </summary>
    public static class RemoveRedundantManagers
    {
        private static readonly Type[] ManagerTypes =
        {
            typeof(AudioManager),
            typeof(GameManager),
            typeof(SceneTransitionManager),
            typeof(SaveManager),
            typeof(SettingsManager),
            typeof(HighScoreManager),
        };

        // Components a manager drags in via RequireComponent — part of the
        // manager, not user content.
        private static readonly Type[] ManagerSiblingTypes =
        {
            typeof(SceneTransition),
            typeof(SceneLoader),
        };

        private static bool IsManagerSibling(Component component) =>
            ManagerSiblingTypes.Any(t => t.IsInstanceOfType(component));

        [MenuItem("Tools/Sailor Snouts/Tools/Remove Redundant Managers From Build Scenes")]
        public static void Run() => ToolRegistry.Run("Tools/Remove Redundant Managers From Build Scenes", RunDefault);

        private static void RunDefault()
        {
            // Let the user save anything open before we start swapping scenes.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string restorePath = EditorSceneManager.GetActiveScene().path;
            var report = new StringBuilder();
            int totalRemoved = 0;

            try
            {
                foreach (var entry in EditorBuildSettings.scenes)
                {
                    if (!entry.enabled || string.IsNullOrEmpty(entry.path))
                        continue;

                    var scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
                    var removedHere = RemoveFromScene(scene);
                    if (removedHere.Count == 0)
                        continue;

                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    totalRemoved += removedHere.Count;
                    report.AppendLine($"  {scene.name}: {string.Join(", ", removedHere)}");
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(restorePath))
                    EditorSceneManager.OpenScene(restorePath, OpenSceneMode.Single);
            }

            if (totalRemoved == 0)
                Debug.Log("[RemoveRedundantManagers] No manager objects found in build scenes — nothing to remove.");
            else
                Debug.Log($"[RemoveRedundantManagers] Removed {totalRemoved} manager object(s):\n{report}");
        }

        private static List<string> RemoveFromScene(Scene scene)
        {
            var removed = new List<string>();

            foreach (var type in ManagerTypes)
            {
                // Gather components of this manager type that live in THIS scene.
                var components = UnityEngine.Object
                    .FindObjectsByType(type, FindObjectsInactive.Include)
                    .Cast<Component>()
                    .Where(c => c != null && c.gameObject.scene == scene)
                    .ToList();

                foreach (var component in components)
                {
                    GameObject go = component.gameObject;

                    // Only delete the whole GameObject when the manager (plus its
                    // own RequireComponent siblings) is all it carries — a dev may
                    // have attached one to an object that also holds other
                    // components or children.
                    bool hasOtherContent = go.transform.childCount > 0
                        || go.GetComponents<Component>().Any(c =>
                            c != component && !(c is Transform) && !IsManagerSibling(c));

                    if (!hasOtherContent)
                    {
                        removed.Add($"{type.Name} ({go.name})");
                        UnityEngine.Object.DestroyImmediate(go);
                        continue;
                    }

                    if (PrefabUtility.IsPartOfPrefabInstance(component))
                    {
                        Debug.LogWarning(
                            $"[RemoveRedundantManagers] '{go.name}' in {scene.name} carries a {type.Name} inside a prefab instance alongside other content — remove it by editing the prefab.",
                            go);
                        continue;
                    }

                    removed.Add($"{type.Name} (component only — kept '{go.name}')");
                    UnityEngine.Object.DestroyImmediate(component);
                    foreach (var siblingType in ManagerSiblingTypes)
                    {
                        if (go.TryGetComponent(siblingType, out Component sibling))
                            UnityEngine.Object.DestroyImmediate(sibling);
                    }
                }
            }

            return removed;
        }
    }
}
