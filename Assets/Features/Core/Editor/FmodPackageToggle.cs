#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.Core.Editor
{
    /// <summary>
    /// Scripted opt-in for the FMOD audio backend. The base template carries zero
    /// FMOD weight; enabling flips the project over to FMOD by editing assembly
    /// definitions in place — adding the <c>FMOD_PRESENT</c> versionDefine (which
    /// turns on every <c>#if FMOD_PRESENT</c> path) and the <c>FMODUnity</c>
    /// reference (so <see cref="JamTemplate.Audio"/> can see FMOD's runtime). It
    /// does not download anything: install the FMOD Unity package yourself first,
    /// then run Enable. Disable reverses the asmdef edits.
    /// </summary>
    internal static class FmodPackageToggle
    {
        private const string Define = "FMOD_PRESENT";
        private const string PackageName = "com.fmod.unity";
        private const string FmodAssembly = "FMODUnity";

        // asmdefs that contain #if FMOD_PRESENT code and so need the define.
        private static readonly string[] DefineTargets =
        {
            "Assets/Features/Audio/JamTemplate.Audio.asmdef",
            "Assets/Features/Core/JamTemplate.Core.asmdef",
            "Assets/Features/Core/Editor/JamTemplate.Core.Editor.asmdef",
        };

        // Only the Audio island actually compiles against FMOD's runtime types.
        private const string ReferenceTarget = "Assets/Features/Audio/JamTemplate.Audio.asmdef";

        // Assemblies the Audio island compiles against once FMOD is on: FMOD's
        // runtime, plus the Input System (FmodAudioManager watches for the first
        // user gesture to lift the browser autoplay block on WebGL).
        private static readonly string[] AudioReferences = { FmodAssembly, "Unity.InputSystem" };

        [MenuItem("Tools/Sailor Snouts/Audio/Enable FMOD")]
        private static void Enable()
        {
            if (!FmodInstalled())
            {
                EditorUtility.DisplayDialog(
                    "FMOD not found",
                    "The FMOD Unity package isn't installed in this project. Import it first " +
                    "(the '" + FmodAssembly + "' assembly must exist), then run Enable FMOD again.",
                    "OK");
                return;
            }

            ApplyEnable();
        }

        [MenuItem("Tools/Sailor Snouts/Audio/Disable FMOD")]
        private static void Disable()
        {
            if (!IsEnabled())
            {
                Debug.Log("[FMOD] Already disabled.");
                return;
            }

            ApplyDisable();
        }

        /// <summary>
        /// Headless entry point for CI (<c>-executeMethod</c>). Throws if FMOD isn't
        /// installed so batch-mode Unity exits non-zero. Re-runnable: it only adds
        /// what's missing.
        /// </summary>
        public static void EnableForCi()
        {
            if (!FmodInstalled())
                throw new InvalidOperationException(
                    "[FMOD] Enable failed: the '" + FmodAssembly + "' assembly was not found. " +
                    "Install the FMOD Unity package on the runner before enabling.");
            ApplyEnable();
        }

        /// <summary>Headless counterpart to <see cref="EnableForCi"/>.</summary>
        public static void DisableForCi() => ApplyDisable();

        // --- Apply --------------------------------------------------------------

        // Re-runnable on purpose: each Add* call no-ops when its target is already
        // present, so running Enable again picks up newly-required defines/references
        // (e.g. after pulling a change that adds one) without duplicating anything.
        private static void ApplyEnable()
        {
            foreach (string path in DefineTargets)
                AddDefine(path);
            foreach (string reference in AudioReferences)
                AddReference(ReferenceTarget, reference);

            AssetDatabase.Refresh();
            Debug.Log(
                "[FMOD] Enabled. Once Unity finishes recompiling, run " +
                "Tools ▸ Sailor Snouts ▸ Audio ▸ Create FMOD Audio Manager Prefab " +
                "to generate the Resources prefab the bootstrapper spawns.");
        }

        private static void ApplyDisable()
        {
            foreach (string path in DefineTargets)
                RemoveDefine(path);
            foreach (string reference in AudioReferences)
                RemoveReference(ReferenceTarget, reference);

            AssetDatabase.Refresh();
            Debug.Log(
                "[FMOD] Disabled. The project is back on Unity's built-in audio backend. " +
                "The Fmod Audio Manager prefab (if any) is left in place but unused; delete it manually if desired.");
        }

        // --- State --------------------------------------------------------------

        private static bool FmodInstalled() =>
            AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == FmodAssembly);

        private static bool IsEnabled()
        {
            Asmdef audio = Load(ReferenceTarget);
            return audio != null && audio.versionDefines.Any(v => v.define == Define);
        }

        // --- asmdef mutation ----------------------------------------------------

        private static void AddDefine(string path)
        {
            Asmdef a = Load(path);
            if (a == null || a.versionDefines.Any(v => v.define == Define))
                return;

            a.versionDefines.Add(new VersionDefine { name = PackageName, expression = "", define = Define });
            Save(path, a);
        }

        private static void RemoveDefine(string path)
        {
            Asmdef a = Load(path);
            if (a == null)
                return;

            int removed = a.versionDefines.RemoveAll(v => v.define == Define);
            if (removed > 0)
                Save(path, a);
        }

        private static void AddReference(string path, string reference)
        {
            Asmdef a = Load(path);
            if (a == null || a.references.Contains(reference))
                return;

            a.references.Add(reference);
            Save(path, a);
        }

        private static void RemoveReference(string path, string reference)
        {
            Asmdef a = Load(path);
            if (a == null)
                return;

            if (a.references.Remove(reference))
                Save(path, a);
        }

        private static Asmdef Load(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"[FMOD] asmdef not found: {path}");
                return null;
            }

            return JsonUtility.FromJson<Asmdef>(File.ReadAllText(path));
        }

        private static void Save(string path, Asmdef a)
        {
            // Every project asmdef shares this 12-field schema, so a full round-trip
            // through JsonUtility preserves the file; field order here matches the
            // generated layout to keep diffs minimal.
            File.WriteAllText(path, JsonUtility.ToJson(a, true) + "\n");
        }

        // Mirror of the .asmdef JSON schema used across this project.
        [Serializable]
        private class Asmdef
        {
            public string name;
            public string rootNamespace;
            public List<string> references = new List<string>();
            public List<string> includePlatforms = new List<string>();
            public List<string> excludePlatforms = new List<string>();
            public bool allowUnsafeCode;
            public bool overrideReferences;
            public List<string> precompiledReferences = new List<string>();
            public bool autoReferenced;
            public List<string> defineConstraints = new List<string>();
            public List<VersionDefine> versionDefines = new List<VersionDefine>();
            public bool noEngineReferences;
        }

        [Serializable]
        private class VersionDefine
        {
            public string name;
            public string expression;
            public string define;
        }
    }
}
#endif
