using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// The player's character: a <see cref="Character"/> that swaps its identity (sprite + stats)
    /// between two <see cref="CharacterData"/> on a key press. While active it is the skill-check /
    /// dialogue "player" — query <see cref="Current"/> / <see cref="CurrentData"/> to find out which
    /// character the player currently is.
    /// </summary>
    public class PlayerCharacter : Character
    {
        [Header("Swap")]
        [SerializeField] private CharacterData characterA;
        [SerializeField] private CharacterData characterB;

        [SerializeField, Tooltip("Key that swaps the active character.")]
        private Key swapKey = Key.Tab;

        private bool usingA = true;

        /// <summary>The player character currently in control (for game code and the dialogue system).</summary>
        public static PlayerCharacter Current { get; private set; }

        /// <summary>The active player's data (name, stats, portrait), or null if no player is active.</summary>
        public static CharacterData CurrentData => Current != null ? Current.Active : null;

        /// <summary>The CharacterData currently in control.</summary>
        public CharacterData Active => usingA ? characterA : characterB;

        protected override void OnEnable()
        {
            base.OnEnable();
            Current = this;
            Apply();
        }

        private void OnDisable()
        {
            if (Current == this)
                Current = null;
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;

            if (Keyboard.current != null && Keyboard.current[swapKey].wasPressedThisFrame)
                Swap();
        }

        /// <summary>Switch to the other character.</summary>
        public void Swap()
        {
            usingA = !usingA;
            Apply();
        }

        private void Apply()
        {
            CharacterData active = Active;
            if (active == null)
                return;

            SetData(active);                       // sprite + DialogueActor (inherited from Character)
            SkillCheck.DefaultCharacter = active;  // stats for skill checks / dialogue
        }
    }
}
