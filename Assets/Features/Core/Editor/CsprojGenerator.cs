using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.Core.Editor
{
    /// <summary>
    /// CI entry point: generates the solution and .csproj files so Roslyn
    /// tooling (dotnet format style/analyzers) can run outside Unity. Invoked
    /// by the style-lint workflow job through game-ci's buildMethod.
    /// </summary>
    public static class CsprojGenerator
    {
        public static void Generate()
        {
            try
            {
                // SyncVS is internal but has been the stable sync entry point
                // for years; fall back to the public code-editor API.
                Type syncVs = typeof(EditorApplication).Assembly.GetType("UnityEditor.SyncVS");
                MethodInfo syncSolution = syncVs != null
                    ? syncVs.GetMethod("SyncSolution", BindingFlags.Public | BindingFlags.Static)
                    : null;

                if (syncSolution != null)
                    syncSolution.Invoke(null, null);
                else
                    Unity.CodeEditor.CodeEditor.CurrentEditor.SyncAll();

                Debug.Log("[CsprojGenerator] Solution generated.");
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CsprojGenerator] Failed: {e}");
                EditorApplication.Exit(1);
            }
        }
    }
}
