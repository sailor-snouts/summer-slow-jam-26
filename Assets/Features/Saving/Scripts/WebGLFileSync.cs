#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace JamTemplate.Saving
{
    /// <summary>
    /// On WebGL, <c>Application.persistentDataPath</c> is an in-memory filesystem;
    /// file writes only survive a page reload after <c>FS.syncfs</c> pushes them to
    /// the browser's IndexedDB. Call <see cref="Flush"/> after any write or delete
    /// that must persist. No-op on every other platform.
    /// </summary>
    public static class WebGLFileSync
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void JamTemplateSyncFiles();

        /// <summary>Pushes pending file writes to IndexedDB (asynchronous, fire-and-forget).</summary>
        public static void Flush() => JamTemplateSyncFiles();
#else
        /// <summary>Pushes pending file writes to IndexedDB (asynchronous, fire-and-forget).</summary>
        public static void Flush()
        {
        }
#endif
    }
}
