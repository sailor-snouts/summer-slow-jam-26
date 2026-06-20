using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// A Dialogue System Trigger the player's <see cref="PlayerInteractor"/> can fire. Because it
    /// extends <c>DialogueSystemTrigger</c>, you get its full Inspector — the Conversation dropdown,
    /// Conversant, Conditions, once-only, sequences, etc. — and <see cref="Interact"/> starts it via
    /// <c>TryStart</c> when the player's interaction sweep lands on this object.
    ///
    /// Leave the trigger's "Trigger" event on its default (On Use) so it doesn't also fire on its
    /// own; our interaction calls TryStart directly.
    /// </summary>
    [RequireComponent(typeof(Character))]
    public class NpcDialogue : PixelCrushers.DialogueSystem.DialogueSystemTrigger, IInteractable
    {
        public void Interact(Transform initiator)
        {
            if (DialogueManager.isConversationActive)
                return;

            // Conversation comes from this NPC's CharacterData; the NPC is the conversant (it
            // greets first) and the interacting player is the actor.
            var character = GetComponent<Character>();
            CharacterData data = character != null ? character.Data : null;
            string convo = data != null ? data.Conversation : null;
            if (string.IsNullOrEmpty(convo))
            {
                string where = data != null
                    ? $"CharacterData '{data.name}' (GameObject '{name}')"
                    : $"GameObject '{name}'";
                Debug.LogError($"[NpcDialogue] No Conversation set on {where}.", this);
                return;
            }

            conversation = convo;
            conversationConversant = transform;
            TryStart(initiator);
        }
    }
}
