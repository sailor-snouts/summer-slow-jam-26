using UnityEngine;

namespace Game
{
    /// <summary>
    /// A character in the scene. Pick which character this GameObject is with the
    /// <see cref="data"/> selector (a <see cref="CharacterData"/> asset). It shows the
    /// character's profile picture on this object's <see cref="SpriteRenderer"/> — updating
    /// live in the editor — and, at runtime, copies the name + portrait onto a Pixel Crushers
    /// <c>DialogueActor</c> so dialogue uses the selected character's identity.
    /// </summary>
    // ExecuteAlways: refresh the sprite in edit mode too. DefaultExecutionOrder: set the
    // DialogueActor's name/portrait before the Dialogue System reads them.
    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Mover))]
    public class Character : MonoBehaviour
    {
        [Tooltip("Which character this GameObject is.")]
        [SerializeField] private CharacterData data;

        [Tooltip("At runtime, copy the character's name + portrait onto a DialogueActor on this object.")]
        [SerializeField] private bool applyToDialogueActor = true;

        [Tooltip("Resize the BoxCollider2D to match the sprite whenever it changes.")]
        [SerializeField] private bool fitColliderToSprite = true;

        private SpriteRenderer spriteRenderer;
        private BoxCollider2D box;

        /// <summary>The selected character definition (name, stats, portrait).</summary>
        public CharacterData Data => data;

        /// <summary>The character's name (from the selected data), or the object name if none is set.</summary>
        public string Name => data != null ? data.DisplayName : name;

        /// <summary>The character's profile picture, or null if none.</summary>
        public Sprite ProfilePicture => data != null ? data.ProfilePicture : null;

        /// <summary>Reads one of the character's stats.</summary>
        public int GetStat(Stat stat) => data != null ? data.Get(stat) : CharacterData.MinValue;

        /// <summary>Swaps which character this object is at runtime — refreshes the sprite (and DialogueActor).</summary>
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

        protected virtual void OnEnable() => RefreshSprite();

        // Fires in the editor when this component loads or its Data changes. Setting the sprite
        // here directly triggers a SendMessage (bounds-changed), which Unity forbids during
        // OnValidate — so defer the refresh to just after validation completes.
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += DeferredRefresh;
#endif
        }

#if UNITY_EDITOR
        private void DeferredRefresh()
        {
            if (this == null) // may have been destroyed between OnValidate and this callback
                return;
            RefreshSprite();
        }
#endif

        /// <summary>Shows the selected character's profile picture on this object's SpriteRenderer.</summary>
        private void RefreshSprite()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                return;

            Sprite sprite = data != null ? data.ProfilePicture : null;
            spriteRenderer.sprite = sprite;

            // Keep the collider matching the sprite, so it actually has a shape to block with.
            if (fitColliderToSprite && sprite != null)
            {
                if (box == null)
                    box = GetComponent<BoxCollider2D>();
                if (box != null)
                {
                    box.size = sprite.bounds.size;
                    box.offset = sprite.bounds.center;
                }
            }
        }

        private void ApplyToDialogueActor()
        {
            // Create the DialogueActor on demand so you don't have to add/configure it by hand —
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
