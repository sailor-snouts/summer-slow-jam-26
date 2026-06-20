using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    /// <summary>The three character stat categories.</summary>
    public enum Stat
    {
        Brain,
        Brawn,
        Beauty,
    }

    /// <summary>
    /// A character definition asset: a name, the three stats (Brain / Brawn / Beauty), and a
    /// profile picture. Make as many of these as you like (Assets ▸ Create ▸ Game ▸ Character),
    /// then point a scene <see cref="Character"/> at the one a GameObject should be.
    ///
    /// Stats are read-only at runtime — a ScriptableObject is shared by every reference, so
    /// mutating it would change the asset for everyone (and persist in the editor). If a
    /// character ever needs per-instance, changing stats, we'd add a runtime copy then.
    /// </summary>
    [CreateAssetMenu(fileName = "Character", menuName = "Game/Character")]
    public class CharacterData : ScriptableObject
    {
        /// <summary>Stat values are clamped to this inclusive range.</summary>
        public const int MinValue = 1;
        public const int MaxValue = 4;

        [Tooltip("Which Dialogue System actor this character is — also used as the character's name. Falls back to the asset name if blank.")]
        [ActorPopup(true)] // (true) shows a Database field so the dropdown can list actors
        [SerializeField] private string dialogueActor;

        [Tooltip("Conversation that starts when the player interacts with this character.")]
        [ConversationPopup]
        [SerializeField] private string conversation;

        [Tooltip("Profile picture / portrait for this character.")]
        [SerializeField] private Sprite profilePicture;

        [Header("Stats")]
        [SerializeField, Range(MinValue, MaxValue)] private int brain = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int brawn = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int beauty = MinValue;

        /// <summary>The character's name — its Dialogue System actor (falls back to the asset name if unset).</summary>
        public string DisplayName => string.IsNullOrEmpty(dialogueActor) ? name : dialogueActor;
        public Sprite ProfilePicture => profilePicture;

        /// <summary>Conversation started when the player interacts with this character.</summary>
        public string Conversation => conversation;

        public int Brain => brain;
        public int Brawn => brawn;
        public int Beauty => beauty;

        /// <summary>Reads one stat by category.</summary>
        public int Get(Stat stat) => stat switch
        {
            Stat.Brain => brain,
            Stat.Brawn => brawn,
            Stat.Beauty => beauty,
            _ => throw new System.ArgumentOutOfRangeException(nameof(stat), stat, "Unknown stat."),
        };
    }
}
