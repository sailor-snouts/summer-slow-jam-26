using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    [RequireComponent(typeof(Mover))]
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        private Mover mover;
        private InputAction moveAction;

        private void Awake()
        {
            mover = GetComponent<Mover>();

            moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            moveAction.AddBinding("<Gamepad>/leftStick");
        }

        private void OnEnable() => moveAction.Enable();
        private void OnDisable() => moveAction.Disable();
        private void OnDestroy() => moveAction.Dispose();

        private void Update()
        {
            // Locked during conversations: stop, and don't read movement (dialogue owns input then).
            if (PlayerInput.Locked)
            {
                mover.MoveDirection = Vector2.zero;
                return;
            }

            // Read in Update, apply in the Mover's FixedUpdate - the standard input/physics split.
            mover.MoveDirection = moveAction.ReadValue<Vector2>();
        }
    }
}
