using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace JamTemplate.Menus
{
    /// <summary>When a save-aware menu button is shown.</summary>
    public enum SaveVisibility
    {
        /// <summary>Always shown, regardless of saves.</summary>
        Always,

        /// <summary>Shown only when at least one save exists (e.g. Continue, Load Game).</summary>
        WhenSaveExists,

        /// <summary>Shown only when no save exists (e.g. a New Game prompt on a fresh install).</summary>
        WhenNoSave,
    }

    /// <summary>
    /// Gives a Unity UI Button a menu action — load a scene, quit, resume, or
    /// invoke a custom event. Wires itself to the Button's onClick at runtime,
    /// so no manual event hook-up is needed. Can also show/hide itself based on
    /// whether a save exists (see <see cref="visibleWhen"/>).
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Menu Button Action")]
    [RequireComponent(typeof(Button))]
    public class MenuButtonAction : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("What this button does when pressed.")]
        private MenuAction action = MenuAction.LoadScene;

        [SerializeField]
#if ODIN_INSPECTOR
        [ShowIf("@action == JamTemplate.Menus.MenuAction.LoadScene || action == JamTemplate.Menus.MenuAction.OpenAdditive")]
        [ValueDropdown(nameof(GetSceneNames))]
#endif
        [Tooltip("Scene to load, picked from Build Settings.")]
        private string scene;

        [SerializeField]
        [Tooltip("Show this button based on whether a save exists. 'Always' ignores saves.")]
        private SaveVisibility visibleWhen = SaveVisibility.Always;

        [SerializeField]
#if ODIN_INSPECTOR
        [ShowIf(nameof(action), MenuAction.Event)]
#endif
        [Tooltip("Invoked when the button is pressed.")]
        private UnityEvent onClick;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Activate);
        }

        private void Start()
        {
#if UNITY_WEBGL
            // Application.Quit is a no-op in the browser — don't show a dead button.
            if (action == MenuAction.Quit)
            {
                gameObject.SetActive(false);
                return;
            }
#endif

            // Save-gated buttons (Continue, Load Game…) hide themselves when their
            // save condition isn't met. Evaluated once: the title's save state is
            // fixed while it is shown.
            if (visibleWhen == SaveVisibility.Always)
                return;

            bool visible = visibleWhen == SaveVisibility.WhenSaveExists
                ? MenuSaveState.HasSave
                : !MenuSaveState.HasSave;
            if (!visible)
                gameObject.SetActive(false);
        }

        /// <summary>Runs the configured action. Wired to the Button's onClick.</summary>
        public void Activate()
        {
            switch (action)
            {
                case MenuAction.LoadScene:
                    LoadScene();
                    break;
                case MenuAction.Continue:
                    MenuSaveState.Continue();
                    break;
                case MenuAction.Quit:
                    if (GameActions.IsAvailable)
                        GameActions.Quit();
                    else
                        Debug.LogWarning("[Menu] No Game Manager available to Quit.", this);
                    break;
                case MenuAction.Resume:
                    if (GameActions.IsAvailable)
                        GameActions.Resume();
                    else
                        Debug.LogWarning("[Menu] No Game Manager available to Resume.", this);
                    break;
                case MenuAction.OpenAdditive:
                    OpenAdditive();
                    break;
                case MenuAction.CloseSelf:
                    CloseSelf();
                    break;
                case MenuAction.Event:
                    onClick?.Invoke();
                    break;
            }
        }

        private void OpenAdditive()
        {
            if (string.IsNullOrEmpty(scene))
            {
                Debug.LogWarning("[Menu] Button has no scene assigned.", this);
                return;
            }

            MenuSceneRouter.OpenAdditive(scene);
        }

        private void CloseSelf()
        {
            MenuSceneRouter.CloseAdditive(gameObject.scene.name);
        }

        private void LoadScene()
        {
            if (string.IsNullOrEmpty(scene))
            {
                Debug.LogWarning("[Menu] Button has no scene assigned.", this);
                return;
            }

            MenuSceneRouter.Load(scene);
        }

#if ODIN_INSPECTOR
        private static IEnumerable<ValueDropdownItem<string>> GetSceneNames()
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
