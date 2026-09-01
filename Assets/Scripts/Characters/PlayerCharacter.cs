using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// The player's character: a <see cref="Character"/> that swaps its identity (sprite + stats)
    /// between two <see cref="CharacterData"/> on a key press. While active it is the skill-check /
    /// dialogue "player" - query <see cref="Current"/> / <see cref="CurrentData"/> to find out which
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

        // Which character the player last chose, remembered across scene loads (static). The player is
        // a per-scene prefab instance, so this survives the swap choice between scenes. It's cleared on
        // play start (ResetSession, plus domain reload), so it lasts the play session only.
        private static CharacterData sessionCharacter;

        /// <summary>The player character currently in control (for game code and the dialogue system).</summary>
        public static PlayerCharacter Current { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            Current = null;
            sessionCharacter = null;
        }

        /// <summary>The active player's data (name, stats, portrait), or null if no player is active.</summary>
        public static CharacterData CurrentData => Current != null ? Current.Active : null;

        /// <summary>The CharacterData currently in control.</summary>
        public CharacterData Active => usingA ? characterA : characterB;

        protected override void OnEnable()
        {
            base.OnEnable();
            Current = this;

            // Restore the character chosen earlier this session so the player stays who they were
            // across scene loads. If nothing's remembered (or it doesn't match this player's options),
            // keep the serialized default.
            if (sessionCharacter == characterB)
                usingA = false;
            else if (sessionCharacter == characterA)
                usingA = true;

            Apply();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Current == this)
                Current = null;
        }

        protected override void Update()
        {
            base.Update(); // directional facing from movement

            if (!Application.isPlaying)
                return;

            if (PlayerInput.Locked)
                return; // no character swapping mid-conversation

            if (Keyboard.current != null && Keyboard.current[swapKey].wasPressedThisFrame)
                Swap();
        }

        /// <summary>Switch to the other character.</summary>
        public void Swap()
        {
            usingA = !usingA;
            Apply();
        }

        /// <summary>
        /// Sets which character the player is by actor name (e.g. from the Mirror dialogue). Matches
        /// against the two options' display names; returns true if one matched.
        /// </summary>
        public bool SetActiveByName(string actorName)
        {
            if (characterA != null && characterA.DisplayName == actorName)
            {
                usingA = true;
                Apply();
                return true;
            }
            if (characterB != null && characterB.DisplayName == actorName)
            {
                usingA = false;
                Apply();
                return true;
            }

            Debug.LogWarning(
                $"[PlayerCharacter] No player character named '{actorName}'. Options are " +
                $"'{(characterA != null ? characterA.DisplayName : "none")}' and " +
                $"'{(characterB != null ? characterB.DisplayName : "none")}'.", this);
            return false;
        }

        private void Apply()
        {
            CharacterData active = Active;
            if (active == null)
                return;

            sessionCharacter = active;             // remember for the session (survives scene loads)
            SetData(active);                       // sprite + DialogueActor (inherited from Character)
            SkillCheck.DefaultCharacter = active;  // stats for skill checks / dialogue
        }
    }
}
