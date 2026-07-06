using UnityEngine;

namespace Game
{
    /// <summary>
    /// A <see cref="ConversationTrigger"/> for an interactable object (a clue, prop, note, …): its
    /// conversation comes from the object's <see cref="InteractableObject"/> →
    /// <see cref="InteractableObjectData"/>. Unlike an NPC there are no stats or portrait — just the
    /// object's conversation.
    /// </summary>
    [RequireComponent(typeof(InteractableObject))]
    public class ObjectDialogue : ConversationTrigger
    {
        protected override string GetConversation()
        {
            InteractableObject obj = GetComponent<InteractableObject>();
            return obj != null && obj.Data != null ? obj.Data.Conversation : null;
        }

        protected override string SourceDescription
        {
            get
            {
                InteractableObjectData data = GetComponent<InteractableObject>()?.Data;
                return data != null
                    ? $"InteractableObjectData '{data.name}' (GameObject '{name}')"
                    : $"GameObject '{name}'";
            }
        }
    }
}
