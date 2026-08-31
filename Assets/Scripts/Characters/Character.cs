using UnityEngine;

namespace Game
{
    /// <summary>
    /// A character in the scene. Pick which character this GameObject is with the
    /// <see cref="data"/> selector (a <see cref="CharacterData"/> asset). It shows the
    /// character's world sprite on this object's <see cref="SpriteRenderer"/> - updating
    /// live in the editor - and, at runtime, copies the name + portrait onto a Pixel Crushers
    /// <c>DialogueActor</c> so dialogue uses the selected character's identity. The portrait
    /// (profile picture) is separate and used only for dialogue.
    /// </summary>
    // DefaultExecutionOrder: set the DialogueActor's name/portrait before the Dialogue System reads
    // them. (Sprite refresh / collider fitting is inherited from SpriteEntity.)
    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    // RequireComponent isn't inherited from SpriteEntity, so restate the sprite/collider parts here.
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Mover))]
    public class Character : SpriteEntity
    {
        [Tooltip("Which character this GameObject is.")]
        [SerializeField] private CharacterData data;

        [Tooltip("At runtime, copy the character's name + portrait onto a DialogueActor on this object.")]
        [SerializeField] private bool applyToDialogueActor = true;

        [Tooltip("Which way the character faces to begin with - and what the editor preview shows.")]
        [SerializeField] private Facing4 defaultFacing = Facing4.Down;

        private Mover mover;
        private Facing4 currentFacing = Facing4.Down;

        /// <summary>The selected character definition (name, stats, portrait).</summary>
        public CharacterData Data => data;

        /// <summary>The character's name (from the selected data), or the object name if none is set.</summary>
        public string Name => data != null ? data.DisplayName : name;

        /// <summary>The character's profile picture, or null if none.</summary>
        public Sprite ProfilePicture => data != null ? data.ProfilePicture : null;

        /// <summary>Reads one of the character's stats.</summary>
        public int GetStat(Stat stat) => data != null ? data.Get(stat) : CharacterData.MinValue;

        // A character's world sprite is its equipped outfit's worn look (runtime only), or its own
        // directional sprite for the way it's facing. Edit mode previews the serialized defaultFacing;
        // at runtime the facing follows movement (see Update).
        protected override Sprite CurrentSprite
        {
            get
            {
                if (data == null)
                    return null;
                if (Application.isPlaying)
                {
                    Sprite worn = Outfits.WornSprite(data);
                    if (worn != null)
                        return worn;
                    return data.GetSprite(currentFacing);
                }
                return data.GetSprite(defaultFacing);
            }
        }

        /// <summary>Swaps which character this object is at runtime - refreshes the sprite (and DialogueActor).</summary>
        public void SetData(CharacterData newData)
        {
            data = newData;
            RefreshSprite();
            if (Application.isPlaying && applyToDialogueActor && data != null)
                ApplyToDialogueActor();
        }

        protected virtual void Awake()
        {
            mover = GetComponent<Mover>();
            currentFacing = defaultFacing;

            // Runtime only: push identity to the DialogueActor (don't dirty it in edit mode).
            if (Application.isPlaying && applyToDialogueActor && data != null)
                ApplyToDialogueActor();
        }

        // Runtime: face the way we're moving, swapping the directional sprite when the facing changes.
        // Idle keeps the last facing. (virtual so PlayerCharacter can extend it for swap input.)
        protected virtual void Update()
        {
            if (!Application.isPlaying || mover == null)
                return;

            Vector2 move = mover.MoveDirection;
            if (move.sqrMagnitude < 1e-6f)
                return;

            Facing4 next = FromVector(move);
            if (next != currentFacing)
            {
                currentFacing = next;
                RefreshSprite();
            }
        }

        // Pick the cardinal direction closest to a movement vector (dominant axis; ties go vertical).
        private static Facing4 FromVector(Vector2 v)
        {
            if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
                return v.x > 0f ? Facing4.Right : Facing4.Left;
            return v.y > 0f ? Facing4.Up : Facing4.Down;
        }

        // Re-pull the sprite when this character's outfit changes. Subscribed symmetrically in
        // enable/disable; the event only fires at runtime.
        protected override void OnEnable()
        {
            base.OnEnable();
            Outfits.Changed += OnOutfitChanged;
        }

        protected virtual void OnDisable()
        {
            Outfits.Changed -= OnOutfitChanged;
        }

        private void OnOutfitChanged(CharacterData changed)
        {
            if (changed == data)
                RefreshSprite();
        }

        private void ApplyToDialogueActor()
        {
            // Create the DialogueActor on demand so you don't have to add/configure it by hand -
            // its identity comes entirely from the CharacterData. Fully-qualified to bind the base
            // type (and avoid the wrapper/namespace clash).
            var dialogueActor = GetComponent<PixelCrushers.DialogueSystem.DialogueActor>();
            if (dialogueActor == null)
                dialogueActor = gameObject.AddComponent<PixelCrushers.DialogueSystem.DialogueActor>();

            dialogueActor.actor = data.DisplayName; // dialogue addresses this object as that actor
            if (data.ProfilePicture != null)
                dialogueActor.spritePortrait = data.ProfilePicture;
        }
    }
}
