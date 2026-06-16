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

        private void Update()
        {
            if (!PausePressed())
                return;

            GameManager manager = GameManager.Instance;
            if (manager != null)
                manager.TogglePause(pausedTimeScale);
        }

        private static bool PausePressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                return true;

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.startButton.wasPressedThisFrame;
        }
    }
}
