using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.Tests
{
    /// <summary>
    /// Cheap whole-project invariants. Running these in CI also forces every
    /// JamTemplate assembly to compile, which is half the point.
    /// </summary>
    public class SmokeTests
    {
        private static readonly (string resource, Type component)[] Managers =
        {
            ("Audio Manager", typeof(JamTemplate.Audio.AudioManager)),
            ("Game Manager", typeof(JamTemplate.Game.GameManager)),
            ("SceneTransitionManager", typeof(JamTemplate.SceneManagement.SceneTransitionManager)),
            ("Save Manager", typeof(JamTemplate.Saving.SaveManager)),
            ("Settings Manager", typeof(JamTemplate.Settings.SettingsManager)),
            ("High Score Manager", typeof(JamTemplate.Saving.HighScoreManager)),
        };

        [Test]
        public void EveryManagerPrefabLoadsAndCarriesItsComponent()
        {
            foreach ((string resource, Type component) in Managers)
            {
                var prefab = Resources.Load<GameObject>(resource);
                Assert.IsNotNull(prefab, $"Resources/{resource}.prefab failed to load — the bootstrapper would not spawn this manager.");
                Assert.IsNotNull(prefab.GetComponent(component), $"'{resource}' prefab has no {component.Name} component.");
            }
        }

        [Test]
        public void FmodManagerPrefabIsValidWhenSetUp()
        {
            // The FMOD backend is an opt-in (docs/AudioFmod.md). When it's set up the
            // bootstrapper spawns this prefab, so verify it loads and still carries its
            // manager component — a missing script would otherwise only surface at runtime.
            // Matched by type name to avoid a compile dependency on the #if FMOD_PRESENT
            // FmodAudioManager type from this default-backend test assembly.
            var prefab = Resources.Load<GameObject>("Fmod Audio Manager");
            if (prefab == null)
                Assert.Ignore("FMOD backend not set up (no 'Fmod Audio Manager' prefab) — skipping.");

            bool hasManager = false;
            foreach (MonoBehaviour component in prefab.GetComponents<MonoBehaviour>())
            {
                if (component != null && component.GetType().Name == "FmodAudioManager")
                {
                    hasManager = true;
                    break;
                }
            }

            Assert.IsTrue(hasManager,
                "'Fmod Audio Manager' prefab has no FmodAudioManager component (missing script?) — " +
                "re-run Tools ▸ Sailor Snouts ▸ Audio ▸ Create FMOD Audio Manager Prefab.");
        }

        [Test]
        public void BuildSettingsScenesExistWithCurrentGuids()
        {
            Assert.IsNotEmpty(EditorBuildSettings.scenes, "No scenes registered in Build Settings.");

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                Assert.IsTrue(File.Exists(scene.path), $"Build Settings lists '{scene.path}' but the file does not exist.");
                GUID actual = AssetDatabase.GUIDFromAssetPath(scene.path);
                Assert.AreEqual(actual, scene.guid,
                    $"Build Settings entry for '{scene.path}' has a stale GUID — re-register the scene (a regenerated scene keeps its path but gets a new GUID).");
            }
        }

        [Test]
        public void FeatureAssembliesReferenceNoOtherFeature()
        {
            string featuresRoot = Path.Combine(Application.dataPath, "Features");
            foreach (string asmdefPath in Directory.GetFiles(featuresRoot, "*.asmdef", SearchOption.AllDirectories))
            {
                var asmdef = JsonUtility.FromJson<AsmdefShape>(File.ReadAllText(asmdefPath));
                if (asmdef.name == "JamTemplate.Core" || asmdef.name == "JamTemplate.Core.Editor")
                    continue;

                var offending = new List<string>();
                foreach (string reference in asmdef.references ?? Array.Empty<string>())
                {
                    if (reference.StartsWith("JamTemplate", StringComparison.Ordinal))
                        offending.Add(reference);
                }

                Assert.IsEmpty(offending,
                    $"{asmdef.name} references [{string.Join(", ", offending)}] — features must be pure islands; add an extension point and wire it in Core's FeatureWiring instead.");
            }
        }

        [Serializable]
        private class AsmdefShape
        {
            public string name;
            public string[] references;
        }
    }
}
