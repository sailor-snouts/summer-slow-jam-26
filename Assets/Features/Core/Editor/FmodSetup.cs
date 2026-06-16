#if UNITY_EDITOR && FMOD_PRESENT
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.Audio
{
    /// <summary>
    /// Editor helper that creates the <c>Fmod Audio Manager</c> Resources prefab the
    /// bootstrapper spawns when the FMOD backend is enabled. It only exists once
    /// FMOD_PRESENT is defined (after Enable FMOD recompiles), which is why the
    /// prefab isn't shipped with the base template — authoring it while FMOD is
    /// absent would leave a missing-script asset behind.
    /// </summary>
    internal static class FmodSetup
    {
        private const string ResourcesDir = "Assets/Features/Core/Prefabs/Resources";
        private const string PrefabName = "Fmod Audio Manager";
        private static string PrefabPath => $"{ResourcesDir}/{PrefabName}.prefab";

        [MenuItem("Tools/Sailor Snouts/Audio/Create FMOD Audio Manager Prefab")]
        private static void CreatePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
            {
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                Debug.Log($"[FMOD] '{PrefabName}' prefab already exists.", existing);
                return;
            }

            if (!Directory.Exists(ResourcesDir))
                Directory.CreateDirectory(ResourcesDir);

            var temp = new GameObject(PrefabName);
            temp.AddComponent<FmodAudioManager>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, PrefabPath);
            Object.DestroyImmediate(temp);

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log(
                $"[FMOD] Created '{PrefabPath}'. Assign its bank names, VCA paths and the UI " +
                "click event on the prefab — those values are what the runtime manager uses.",
                prefab);
        }
    }
}
#endif
