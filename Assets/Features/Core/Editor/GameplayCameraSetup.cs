#if CINEMACHINE_3
using JamTemplate.Core;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace JamTemplate.Core.Editor
{
    /// <summary>
    /// Builds a minimal 2D gameplay camera rig in the open scene: a
    /// <see cref="CinemachineBrain"/> on the Main Camera and a
    /// <see cref="CinemachineCamera"/> that follows a placeholder target.
    /// A starting point for a jam game's camera — point the Follow at your
    /// player and delete the placeholder. Only compiled when the Cinemachine
    /// package is installed (the menu item disappears without it).
    /// </summary>
    internal static class GameplayCameraSetup
    {
        [MenuItem("Tools/Sailor Snouts/Create 2D Gameplay Camera")]
        private static void Create() => ToolRegistry.Run("Create 2D Gameplay Camera", CreateDefault);

        private static void CreateDefault()
        {
            Camera main = Camera.main;
            if (main == null)
            {
                Debug.LogWarning("[Cinemachine] No Main Camera in the open scene; add one first.");
                return;
            }

            if (main.GetComponent<CinemachineBrain>() == null)
                Undo.AddComponent<CinemachineBrain>(main.gameObject);

            var target = new GameObject("Camera Target (placeholder)");
            Undo.RegisterCreatedObjectUndo(target, "Create 2D Gameplay Camera");

            var cameraObject = new GameObject("Gameplay Camera");
            Undo.RegisterCreatedObjectUndo(cameraObject, "Create 2D Gameplay Camera");
            var vcam = cameraObject.AddComponent<CinemachineCamera>();
            // CinemachineFollow's FollowOffset defaults to (0, 0, -10) — already
            // right for a 2D orthographic camera looking down +Z.
            cameraObject.AddComponent<CinemachineFollow>();
            vcam.Follow = target.transform;

            Selection.activeGameObject = cameraObject;
            EditorGUIUtility.PingObject(cameraObject);
            Debug.Log("[Cinemachine] Added a 2D gameplay camera rig. Point the Gameplay Camera's Follow at your player and delete the placeholder.");
        }
    }
}
#endif
