#if UNITY_EDITOR
using System.IO;
using JamTemplate.Core;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.Saving
{
    /// <summary>
    /// Editor tool that wipes all local player data — PlayerPrefs plus every save
    /// file (manual slots, the auto-save, and high scores). Handy for testing a
    /// clean first-run. Best run outside Play mode, since a running manager keeps
    /// its data in memory until the next launch.
    /// </summary>
    internal static class SaveDataTools
    {
        [MenuItem("Tools/Sailor Snouts/Tools/Delete Save Data")]
        private static void DeleteSaveData() => ToolRegistry.Run("Tools/Delete Save Data", DeleteSaveDataDefault);

        private static void DeleteSaveDataDefault()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Save Data",
                "This permanently deletes all PlayerPrefs and every save file " +
                "(slots, auto-save, and high scores) for this project.\n\n" +
                "This cannot be undone. Continue?",
                "Delete", "Cancel");
            if (!confirmed)
                return;

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            int filesDeleted = 0;

            string savesDir = Path.Combine(Application.persistentDataPath, "Saves");
            if (Directory.Exists(savesDir))
            {
                filesDeleted += Directory.GetFiles(savesDir).Length;
                Directory.Delete(savesDir, true);
            }

            string highScores = Path.Combine(Application.persistentDataPath, "highscores.json");
            if (File.Exists(highScores))
            {
                File.Delete(highScores);
                filesDeleted++;
            }

            Debug.Log($"[Saving] Cleared all PlayerPrefs and deleted {filesDeleted} save file(s) " +
                $"from {Application.persistentDataPath}.");
        }
    }
}
#endif
