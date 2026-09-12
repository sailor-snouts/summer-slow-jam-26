#if ODIN_INSPECTOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
#endif
using JamTemplate.Menus;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class SceneExitTrigger : MonoBehaviour
    {
        [SerializeField]
#if ODIN_INSPECTOR
        [ValueDropdown(nameof(GetSceneNames))]
#endif
        [Tooltip("Scene to load when the player enters, picked from Build Settings.")]
        private string scene;

        [SerializeField, Tooltip("Which entrance (SceneEntrance id) in the target scene to place the player at. Leave blank to use that scene's authored player position.")]
        private string entranceId;

        private bool triggered;

        // Newly added colliders start as triggers - a doorway shouldn't block the player.
        private void Reset()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered)
                return;
            if (other.GetComponentInParent<PlayerCharacter>() == null)
                return; // only the player changes the scene

            if (string.IsNullOrEmpty(scene))
            {
                Debug.LogError($"[SceneExitTrigger] No scene set on '{name}'.", this);
                return;
            }
            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                Debug.LogError($"[SceneExitTrigger] Scene '{scene}' is not in Build Settings, so it can't be loaded.", this);
                return;
            }

            triggered = true; // guard against re-entering the trigger before the load completes
            SceneTransition.PendingEntrance = entranceId;
            MenuSceneRouter.Load(scene);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
                return;
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.2f);
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.8f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
#endif

#if ODIN_INSPECTOR
        private static IEnumerable<ValueDropdownItem<string>> GetSceneNames()
        {
            yield return new ValueDropdownItem<string>("(None)", string.Empty);
#if UNITY_EDITOR
            foreach (var buildScene in UnityEditor.EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled)
                    continue;
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(buildScene.path);
                yield return new ValueDropdownItem<string>(sceneName, sceneName);
            }
#endif
        }
#endif
    }
}
