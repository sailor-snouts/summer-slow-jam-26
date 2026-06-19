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

        private SpriteRenderer spriteRenderer;

        /// <summary>The selected character definition (name, stats, portrait).</summary>
        public CharacterData Data => data;

        /// <summary>The character's name (from the selected data), or the object name if none is set.</summary>
        public string Name => data != null ? data.DisplayName : name;

        /// <summary>The character's profile picture, or null if none.</summary>
        public Sprite ProfilePicture => data != null ? data.ProfilePicture : null;

        /// <summary>Reads one of the character's stats.</summary>
        public int GetStat(Stat stat) => data != null ? data.Get(stat) : CharacterData.MinValue;

        private void Awake()
        {
            // Runtime only: push identity to the DialogueActor (don't dirty it in edit mode).
            if (Application.isPlaying && applyToDialogueActor && data != null)
                ApplyToDialogueActor();
        }

        private void OnEnable() => RefreshSprite();

        // Fires in the editor whenever this component loads or its Data is changed.
        private void OnValidate() => RefreshSprite();

        /// <summary>Shows the selected character's profile picture on this object's SpriteRenderer.</summary>
        private void RefreshSprite()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.sprite = data != null ? data.ProfilePicture : null;
        }

        private void ApplyToDialogueActor()
        {
            // Fully-qualified so we bind to the base type and ignore the wrapper/namespace clash.
            var dialogueActor = GetComponent<PixelCrushers.DialogueSystem.DialogueActor>();
            if (dialogueActor == null)
                return;

            dialogueActor.actor = data.DisplayName;       // dialogue addresses this object as that actor
            if (data.ProfilePicture != null)
                dialogueActor.spritePortrait = data.ProfilePicture;
        }
    }
}
