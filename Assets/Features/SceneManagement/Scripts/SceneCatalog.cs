using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace JamTemplate.SceneManagement
{
    /// <summary>Shared helpers for picking scenes that are in Build Settings.</summary>
    public static class SceneCatalog
    {
#if ODIN_INSPECTOR
        /// <summary>
        /// The enabled Build-Settings scenes as Odin dropdown items, led by a
        /// "(None)" empty entry. Use it to back a <c>[ValueDropdown]</c> string field.
        /// </summary>
        public static IEnumerable<ValueDropdownItem<string>> GetSceneNames()
        {
            yield return new ValueDropdownItem<string>("(None)", string.Empty);
#if UNITY_EDITOR
            foreach (var buildScene in UnityEditor.EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled)
                    continue;
                string name = System.IO.Path.GetFileNameWithoutExtension(buildScene.path);
                yield return new ValueDropdownItem<string>(name, name);
            }
#endif
        }
#endif
    }
}
