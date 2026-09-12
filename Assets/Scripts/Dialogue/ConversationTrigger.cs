using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    // Leave the trigger's "Trigger" event on its default (On Use) so it doesn't also fire on its own;
    // our interaction calls TryStart directly.
    public abstract class ConversationTrigger : DialogueSystemTrigger, IInteractable
    {
        public void Interact(Transform initiator)
        {
            if (DialogueManager.isConversationActive)
                return;

            string convo = GetConversation();
            if (string.IsNullOrEmpty(convo))
            {
                Debug.LogError($"[{GetType().Name}] No Conversation set on {SourceDescription}.", this);
                return;
            }

            // This object is the conversant (it greets first); the interacting player is the actor.
            conversation = convo;
            conversationConversant = transform;
            TryStart(initiator);
        }

        public string Conversation => GetConversation();

        protected abstract string GetConversation();

        protected virtual string SourceDescription => $"GameObject '{name}'";
    }
}
