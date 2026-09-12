using UnityEngine;

namespace Game
{
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
