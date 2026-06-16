using UnityEngine;

namespace JamTemplate.Menus
{
    /// <summary>
    /// Guarantees a camera exists to render this scene, creating a plain 2D camera
    /// only if none is present. Put it on overlay scenes (loaded additively) that
    /// may be opened on their own to test, without risking a second camera — or a
    /// duplicate AudioListener — when one already exists in the scene underneath.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Ensure Camera")]
    public class EnsureCamera : MonoBehaviour
    {
        private void Awake()
        {
            if (Object.FindAnyObjectByType<Camera>() != null)
                return;

            var go = new GameObject("Camera", typeof(Camera));
            go.tag = "MainCamera";

            var camera = go.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;

            // A scene with no camera usually has no listener either; add one so
            // standalone play doesn't warn, but never a second one.
            if (Object.FindAnyObjectByType<AudioListener>() == null)
                go.AddComponent<AudioListener>();
        }
    }
}
