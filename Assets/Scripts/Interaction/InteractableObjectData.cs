using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Interactable Object", menuName = "Game/Interactable Object")]
    public class InteractableObjectData : ScriptableObject
    {
        [Tooltip("Sprite shown for this object in the world.")]
        [SerializeField] private Sprite sprite;

        [Tooltip("Conversation that starts when the player interacts with this object.")]
        [ConversationPopup]
        [SerializeField] private string conversation;

        public Sprite Sprite => sprite;

        public string Conversation => conversation;

#if UNITY_EDITOR
        // Editing this asset (e.g. swapping its sprite) doesn't fire OnValidate on the scene
        // InteractableObjects that reference it, so nudge them to re-read and update live.
        private void OnValidate() => SpriteEntity.RefreshAllInEditor();
#endif
    }
}
