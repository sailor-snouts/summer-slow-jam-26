using UnityEngine;

namespace Game
{
    /// <summary>
    /// A character in the scene. Pick which character this GameObject is with the
    /// <see cref="data"/> selector (a <see cref="CharacterData"/> asset). It shows the
    /// character's profile picture on this object's <see cref="SpriteRenderer"/> - updating
    /// live in the editor - and, at runtime, copies the name + portrait onto a Pixel Crushers
    /// <c>DialogueActor</c> so dialogue uses the selected character's identity.
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

        /// <summary>The selected character definition (name, stats, portrait).</summary>
        public CharacterData Data => data;

        /// <summary>The character's name (from the selected data), or the object name if none is set.</summary>
        public string Name => data != null ? data.DisplayName : name;

        /// <summary>The character's profile picture, or null if none.</summary>
        public Sprite ProfilePicture => data != null ? data.ProfilePicture : null;

        /// <summary>Reads one of the character's stats.</summary>
        public int GetStat(Stat stat) => data != null ? data.Get(stat) : CharacterData.MinValue;

        // A character's world sprite is its equipped outfit's worn look (runtime only), or its
        // portrait when nothing is equipped.
        protected override Sprite CurrentSprite
        {
            get
            {
                if (Application.isPlaying)
                {
                    Sprite worn = Outfits.WornSprite(data);
                    if (worn != null)
                        return worn;
                }
                return data != null ? data.ProfilePicture : null;
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
            // Runtime only: push identity to the DialogueActor (don't dirty it in edit mode).
            if (Application.isPlaying && applyToDialogueActor && data != null)
                ApplyToDialogueActor();
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
