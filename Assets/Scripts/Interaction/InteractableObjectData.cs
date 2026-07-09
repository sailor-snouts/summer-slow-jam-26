using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Definition for an interactable object - a clue, prop, door, note, etc. the player can examine.
    /// Unlike a <see cref="CharacterData"/> it has no stats and no portrait: just the world
    /// <see cref="sprite"/> to show and the <see cref="conversation"/> to start when interacted with.
    /// Create via Assets > Create > Game > Interactable Object, then point a scene
    /// <see cref="InteractableObject"/> at it.
    /// </summary>
    [CreateAssetMenu(fileName = "Interactable Object", menuName = "Game/Interactable Object")]
    public class InteractableObjectData : ScriptableObject
    {
        [Tooltip("Sprite shown for this object in the world.")]
        [SerializeField] private Sprite sprite;

        [Tooltip("Conversation that starts when the player interacts with this object.")]
        [ConversationPopup]
        [SerializeField] private string conversation;

        /// <summary>Sprite shown for this object in the world.</summary>
        public Sprite Sprite => sprite;

        /// <summary>Conversation started when the player interacts with this object.</summary>
        public string Conversation => conversation;

#if UNITY_EDITOR
        // Editing this asset (e.g. swapping its sprite) doesn't fire OnValidate on the scene
        // InteractableObjects that reference it, so nudge them to re-read and update live.
        private void OnValidate() => SpriteEntity.RefreshAllInEditor();
#endif
    }
}
