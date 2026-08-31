using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    /// <summary>
    /// The twelve character stats, four per <see cref="StatCategory"/>. A category's value is the
    /// sum of its four stats (see <see cref="CharacterData.GetCategory"/>).
    /// </summary>
    public enum Stat
    {
        // Brain
        Drive,
        Willpower,
        Observation,
        Empathy,

        // Brawn
        Vigor,
        Endurance,
        Agility,
        Technique,

        // Beauty
        Charm,
        Taunt,
        Bonhomie,
        Hostility,
    }

    /// <summary>The three stat categories. A category's value is the sum of its four stats.</summary>
    public enum StatCategory
    {
        Brain,
        Brawn,
        Beauty,
    }

    /// <summary>A character's four facing directions. Down is the default (enum value 0).</summary>
    public enum Facing4
    {
        Down,
        Up,
        Left,
        Right,
    }

    /// <summary>
    /// A character definition asset: a name, the twelve stats (grouped into Brain / Brawn / Beauty),
    /// and a profile picture. Make as many of these as you like (Assets > Create > Game > Character),
    /// then point a scene <see cref="Character"/> at the one a GameObject should be.
    ///
    /// Stats are read-only at runtime - a ScriptableObject is shared by every reference, so
    /// mutating it would change the asset for everyone (and persist in the editor). If a
    /// character ever needs per-instance, changing stats, we'd add a runtime copy then.
    /// </summary>
    [CreateAssetMenu(fileName = "Character", menuName = "Game/Character")]
    public class CharacterData : ScriptableObject
    {
        /// <summary>Stat values are clamped to this inclusive range.</summary>
        public const int MinValue = 1;
        public const int MaxValue = 4;

        /// <summary>Masculine + feminine always sum to this - the split is one slider's worth of points.</summary>
        public const int GenderTotal = 10;

        [Tooltip("Which Dialogue System actor this character is - also used as the character's name. Falls back to the asset name if blank.")]
        [ActorPopup(true)] // (true) shows a Database field so the dropdown can list actors
        [SerializeField] private string dialogueActor;

        [Tooltip("Conversation that starts when the player interacts with this character.")]
        [ConversationPopup]
        [SerializeField] private string conversation;

        [Tooltip("Headshot / portrait shown in dialogue (the Dialogue System actor's picture).")]
        [SerializeField] private Sprite profilePicture;

        // World sprites by facing direction. Down is the default; the old single worldSprite migrates
        // into spriteDown. Unset directions fall back to Down, then the portrait (see GetSprite).
        [FormerlySerializedAs("worldSprite")]
        [SerializeField] private Sprite spriteDown;
        [SerializeField] private Sprite spriteUp;
        [SerializeField] private Sprite spriteLeft;
        [SerializeField] private Sprite spriteRight;

        // Brain
        [SerializeField, Range(MinValue, MaxValue)] private int drive = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int willpower = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int observation = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int empathy = MinValue;

        // Brawn
        [SerializeField, Range(MinValue, MaxValue)] private int vigor = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int endurance = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int agility = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int technique = MinValue;

        // Beauty
        [SerializeField, Range(MinValue, MaxValue)] private int charm = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int taunt = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int bonhomie = MinValue;
        [SerializeField, Range(MinValue, MaxValue)] private int hostility = MinValue;

        // Masculine / feminine split: we store only the masculine share (0..GenderTotal); feminine is
        // the rest, so the two always sum to GenderTotal. The inspector edits it as a single slider.
        [SerializeField, Range(0, GenderTotal)] private int masculine = GenderTotal / 2;

        /// <summary>The character's name - its Dialogue System actor (falls back to the asset name if unset).</summary>
        public string DisplayName => string.IsNullOrEmpty(dialogueActor) ? name : dialogueActor;

        /// <summary>Dialogue headshot / portrait.</summary>
        public Sprite ProfilePicture => profilePicture;

        /// <summary>Default in-world sprite (facing down) - used for menus and previews.</summary>
        public Sprite WorldSprite => GetSprite(Facing4.Down);

        /// <summary>
        /// The world sprite for a facing direction. Falls back to the down sprite, then the portrait,
        /// so a character always shows something even when some directions aren't authored.
        /// </summary>
        public Sprite GetSprite(Facing4 facing)
        {
            Sprite chosen = facing switch
            {
                Facing4.Up => spriteUp,
                Facing4.Left => spriteLeft,
                Facing4.Right => spriteRight,
                _ => spriteDown,
            };
            if (chosen != null)
                return chosen;
            return spriteDown != null ? spriteDown : profilePicture;
        }

        /// <summary>Conversation started when the player interacts with this character.</summary>
        public string Conversation => conversation;

        // Category totals - the sum of the four stats in each (read-only).
        public int Brain => drive + willpower + observation + empathy;
        public int Brawn => vigor + endurance + agility + technique;
        public int Beauty => charm + taunt + bonhomie + hostility;

        /// <summary>Masculine share of the <see cref="GenderTotal"/> points (0..10).</summary>
        public int Masculine => masculine;

        /// <summary>Feminine share - the rest of the <see cref="GenderTotal"/> points.</summary>
        public int Feminine => GenderTotal - masculine;

        /// <summary>Reads one of the twelve stats.</summary>
        public int Get(Stat stat) => stat switch
        {
            Stat.Drive => drive,
            Stat.Willpower => willpower,
            Stat.Observation => observation,
            Stat.Empathy => empathy,
            Stat.Vigor => vigor,
            Stat.Endurance => endurance,
            Stat.Agility => agility,
            Stat.Technique => technique,
            Stat.Charm => charm,
            Stat.Taunt => taunt,
            Stat.Bonhomie => bonhomie,
            Stat.Hostility => hostility,
            _ => throw new System.ArgumentOutOfRangeException(nameof(stat), stat, "Unknown stat."),
        };

        /// <summary>Reads a category total - the sum of its four stats.</summary>
        public int GetCategory(StatCategory category) => category switch
        {
            StatCategory.Brain => Brain,
            StatCategory.Brawn => Brawn,
            StatCategory.Beauty => Beauty,
            _ => throw new System.ArgumentOutOfRangeException(nameof(category), category, "Unknown category."),
        };

#if UNITY_EDITOR
        // Editing this asset (e.g. swapping its portrait) doesn't fire OnValidate on the scene
        // Characters that reference it, so nudge them to re-read and update live. Also keep the
        // masculine/feminine split within its point budget in case it's edited outside the slider.
        private void OnValidate()
        {
            masculine = Mathf.Clamp(masculine, 0, GenderTotal);
            SpriteEntity.RefreshAllInEditor();
        }
#endif
    }
}
