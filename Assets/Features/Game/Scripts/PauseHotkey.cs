using UnityEngine;
using UnityEngine.InputSystem;

namespace JamTemplate.Game
{
    /// <summary>
    /// Toggles pause from a hotkey — Escape on keyboard or Start on a gamepad.
    /// Lives on the Game Manager prefab so the hotkey works in every scene.
    /// </summary>
    [AddComponentMenu("Sailor Snouts/Pause Hotkey")]
    [DisallowMultipleComponent]
    public class PauseHotkey : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Time scale while paused — 0 stops time entirely, a small value gives slow motion.")]
        private float pausedTimeScale;

        /// <summary>
        /// Optional seams set by game content. When <see cref="SuppressProvider"/> returns true the
        /// hotkey does NOT pause (e.g. a dialogue is open); instead it invokes <see cref="OnSuppressedPress"/>
        /// (e.g. close the dialogue). Lets this feature stay dialogue-aware without referencing the
        /// dialogue system.
        /// </summary>
        public static System.Func<bool> SuppressProvider;
        public static System.Action OnSuppressedPress;

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            bool escape = keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
            bool start = gamepad != null && gamepad.startButton.wasPressedThisFrame;
            if (!escape && !start)
                return;

#if UNITY_EDITOR
            // Shift+Escape exits Play mode — a play-testing shortcut for stopping the editor,
            // separate from the in-game pause (plain Escape still pauses).
            if (escape && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed))
            {
                UnityEditor.EditorApplication.isPlaying = false;
                return;
            }
#endif

            // While something owns the key (e.g. an open dialogue), close that instead of pausing.
            // The next press, with nothing to suppress, pauses normally.
            if (SuppressProvider != null && SuppressProvider())
            {
                OnSuppressedPress?.Invoke();
                return;
            }

            GameManager manager = GameManager.Instance;
            if (manager != null)
                manager.TogglePause(pausedTimeScale);
        }
    }
}
