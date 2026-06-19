using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// Reads movement input (WASD / arrows / left stick) and feeds it to this object's
    /// <see cref="Mover"/>. This is the only piece that knows about input — swap it for an AI
    /// driver on an NPC and the same <see cref="Mover"/> moves the same way.
    /// </summary>
    [RequireComponent(typeof(Mover))]
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        private Mover mover;
        private InputAction moveAction;

        private void Awake()
        {
            mover = GetComponent<Mover>();

            // Defined in code so it's self-contained (no .inputactions asset wiring needed).
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
            // Read in Update, apply in the Mover's FixedUpdate — the standard input/physics split.
            mover.MoveDirection = moveAction.ReadValue<Vector2>();
        }
    }
}
