using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace JamTemplate.Core.Editor
{
    /// <summary>
    /// Headless build entry points for CI (self-hosted runner). Invoke with
    /// <c>Unity.exe -batchmode -nographics -quit -executeMethod
    /// JamTemplate.Core.Editor.BatchBuild.BuildWebGL</c>. Builds every enabled
    /// Build-Settings scene and exits non-zero on failure so the job fails.
    /// </summary>
    public static class BatchBuild
    {
        public static void BuildWebGL() => Build(BuildTarget.WebGL, "build/WebGL");

        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "build/Windows/JamTemplate.exe");

        private static void Build(BuildTarget target, string outputPath)
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[BatchBuild] No enabled scenes in Build Settings.");
                EditorApplication.Exit(1);
                return;
            }

            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
            if (!BuildPipeline.IsBuildTargetSupported(group, target))
            {
                Debug.LogError($"[BatchBuild] {target} build support is not installed in this editor. Add it via Unity Hub ▸ Installs ▸ (gear on the version) ▸ Add Modules ▸ {target} Build Support.");
                EditorApplication.Exit(1);
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log($"[BatchBuild] {target}: {summary.result}, {summary.totalSize} bytes, {summary.totalTime}.");
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
