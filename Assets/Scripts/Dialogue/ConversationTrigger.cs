using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Base for an interactable that starts a conversation. Because it extends
    /// <c>DialogueSystemTrigger</c> you get its full Inspector (Conversation, Conversant, Conditions,
    /// once-only, sequences, ...), and <see cref="Interact"/> starts the conversation via <c>TryStart</c>
    /// when the player's interaction sweep lands on this object. Subclasses only say where the
    /// conversation name comes from (<see cref="GetConversation"/>) - a Character, an object, etc.
    ///
    /// Leave the trigger's "Trigger" event on its default (On Use) so it doesn't also fire on its own;
    /// our interaction calls TryStart directly.
    /// </summary>
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

        /// <summary>The conversation to start - pulled from the subclass's data source (may be null/empty).</summary>
        protected abstract string GetConversation();

        /// <summary>Where the conversation should have come from, named for the "not set" error.</summary>
        protected virtual string SourceDescription => $"GameObject '{name}'";
    }
}
