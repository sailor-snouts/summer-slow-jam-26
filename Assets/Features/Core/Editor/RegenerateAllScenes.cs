using System;
using System.IO;
using System.Linq;
using JamTemplate.Credits;
using JamTemplate.EndScreen;
using JamTemplate.Pause;
using JamTemplate.Saving;
using JamTemplate.Settings;
using JamTemplate.Title;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace JamTemplate.Core.Editor
{
    /// <summary>
    /// Deletes and recreates every generated menu/overlay scene from its
    /// generator, after a confirmation prompt — a one-click reset to the
    /// stock baseline (e.g. on a fresh project, or after editing a generator).
    /// Splash and gameplay scenes are left untouched.
    /// </summary>
    internal static class RegenerateAllScenes
    {
        // Each generated scene's asset path and the generator call that recreates it.
        private static readonly (string Path, Action Ensure)[] Generated =
        {
            (TitleSceneSetup.ScenePath, TitleSceneSetup.Ensure),
            (PauseSceneSetup.ScenePath, PauseSceneSetup.Ensure),
            (SettingsSceneSetup.ScenePath, SettingsSceneSetup.Ensure),
            (SaveMenuSceneSetup.ScenePath, SaveMenuSceneSetup.Ensure),
            (HighScoreMenuSceneSetup.ScenePath, HighScoreMenuSceneSetup.Ensure),
            (CreditsSceneSetup.ScenePath, CreditsSceneSetup.Ensure),
            (EndSceneSetup.WinPath, EndSceneSetup.EnsureWin),
            (EndSceneSetup.LosePath, EndSceneSetup.EnsureLose),
        };

        [MenuItem("Tools/Sailor Snouts/Scenes/Regenerate All Scenes")]
        private static void Run() => ToolRegistry.Run("Scenes/Regenerate All Scenes", RunDefault);

        private static void RunDefault()
        {
            // Save anything open first; regeneration swaps scenes in and out.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string list = string.Join("\n", Generated.Select(g => "    • " + Path.GetFileNameWithoutExtension(g.Path)));
            bool proceed = EditorUtility.DisplayDialog(
                "Regenerate All Scenes",
                "This DELETES and recreates these generated scenes from their generators:\n\n" +
                list +
                "\n\nAny manual edits to them will be lost. Splash and your gameplay scenes are not touched.\n\nContinue?",
                "Delete and Regenerate",
                "Cancel");
            if (!proceed)
                return;

            try
            {
                AssetDatabase.StartAssetEditing();
                foreach ((string path, Action _) in Generated)
                    AssetDatabase.DeleteAsset(path);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            // Each Ensure recreates its scene and re-registers it in Build Settings.
            foreach ((string _, Action ensure) in Generated)
                ensure();

            AssetDatabase.SaveAssets();
            Debug.Log($"[Sailor Snouts] Regenerated {Generated.Length} scenes in Assets/Features/Core/Scenes.");
        }
    }
}
