using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
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

    public enum StatCategory
    {
        Brain,
        Brawn,
        Beauty,
    }

    public enum Facing4
    {
        Down,
        Up,
        Left,
        Right,
    }

    [CreateAssetMenu(fileName = "Character", menuName = "Game/Character")]
    public class CharacterData : ScriptableObject
    {
        public const int MinValue = 1;
        public const int MaxValue = 4;

        public const int GenderTotal = 10;

        [Tooltip("Which Dialogue System actor this character is - also used as the character's name. Falls back to the asset name if blank.")]
        [ActorPopup(true)] // (true) shows a Database field so the dropdown can list actors
        [SerializeField] private string dialogueActor;

        [Tooltip("Conversation that starts when the player interacts with this character.")]
        [ConversationPopup]
        [SerializeField] private string conversation;

        [Tooltip("Headshot / portrait shown in dialogue (the Dialogue System actor's picture).")]
        [SerializeField] private Sprite profilePicture;

        [SerializeField] private OutfitData defaultOutfit;

        [Tooltip("Outfits this character can choose from in the outfit menu. Each playable character has their own wardrobe; leave null for characters that can't change outfits.")]
        [SerializeField] private Wardrobe wardrobe;

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

        // We store only the masculine share (0..GenderTotal); feminine is the rest, so the two always
        // sum to GenderTotal.
        [SerializeField, Range(0, GenderTotal)] private int masculine = GenderTotal / 2;

        public string DisplayName => string.IsNullOrEmpty(dialogueActor) ? name : dialogueActor;

        public Sprite ProfilePicture => profilePicture;

        public Wardrobe Wardrobe => wardrobe;

        public OutfitData DefaultOutfit => defaultOutfit;

        public Sprite WorldSprite => GetSprite(Facing4.Down);

        public Sprite GetSprite(Facing4 facing)
        {
            Sprite chosen = defaultOutfit != null ? defaultOutfit.GetSprite(facing) : null;
            return chosen != null ? chosen : profilePicture;
        }

        public string Conversation => conversation;

        public int Brain => drive + willpower + observation + empathy;
        public int Brawn => vigor + endurance + agility + technique;
        public int Beauty => charm + taunt + bonhomie + hostility;

        public int Masculine => masculine;

        public int Feminine => GenderTotal - masculine;

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

        public int GetCategory(StatCategory category) => category switch
        {
            StatCategory.Brain => Brain,
            StatCategory.Brawn => Brawn,
            StatCategory.Beauty => Beauty,
            _ => throw new System.ArgumentOutOfRangeException(nameof(category), category, "Unknown category."),
        };

#if UNITY_EDITOR
        // Editing this asset doesn't fire OnValidate on the scene Characters that reference it, so
        // nudge them to re-read and update live. Clamp guards the split being edited outside the slider.
        private void OnValidate()
        {
            masculine = Mathf.Clamp(masculine, 0, GenderTotal);
            SpriteEntity.RefreshAllInEditor();
        }
#endif
    }
}
