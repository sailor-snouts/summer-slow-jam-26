using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace JamTemplate.Menus
{
    /// <summary>Shared wiring for the Input System UI module.</summary>
    public static class UIInput
    {
        /// <summary>
        /// Points an <see cref="InputSystemUIInputModule"/> at the project's Input
        /// Actions asset and binds the UI actions. Avoids AssignDefaultActions,
        /// which throws in recent Input System versions because the actions it
        /// creates programmatically aren't part of an InputActionAsset.
        /// </summary>
        public static void Configure(InputSystemUIInputModule module)
        {
            if (module == null)
                return;

            // A background click would deselect the current button and kill
            // keyboard/gamepad navigation with nothing left to restore it.
            module.deselectOnBackgroundClick = false;

            InputActionAsset actions = InputSystem.actions;
            if (actions == null)
            {
                Debug.LogWarning("[Sailor Snouts] No project Input Actions configured. " +
                    "Set one in Project Settings > Input System Package.", module);
                return;
            }

            module.actionsAsset = actions;
            Bind(actions, "UI/Point", r => module.point = r);
            Bind(actions, "UI/Click", r => module.leftClick = r);
            Bind(actions, "UI/MiddleClick", r => module.middleClick = r);
            Bind(actions, "UI/RightClick", r => module.rightClick = r);
            Bind(actions, "UI/ScrollWheel", r => module.scrollWheel = r);
            Bind(actions, "UI/Navigate", r => module.move = r);
            Bind(actions, "UI/Submit", r => module.submit = r);
            Bind(actions, "UI/Cancel", r => module.cancel = r);
            Bind(actions, "UI/TrackedDevicePosition", r => module.trackedDevicePosition = r);
            Bind(actions, "UI/TrackedDeviceOrientation", r => module.trackedDeviceOrientation = r);
        }

        private static void Bind(InputActionAsset asset, string actionPath, Action<InputActionReference> setter)
        {
            InputAction action = asset.FindAction(actionPath);
            if (action != null)
                setter(InputActionReference.Create(action));
        }
    }
}
