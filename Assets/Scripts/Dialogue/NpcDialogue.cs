using UnityEngine;

namespace Game
{
    /// <summary>
    /// A <see cref="ConversationTrigger"/> for an NPC: its conversation comes from the object's
    /// <see cref="Character"/> to <see cref="CharacterData"/>. The NPC is the conversant (it greets
    /// first) and the interacting player is the actor.
    /// </summary>
    [RequireComponent(typeof(Character))]
    public class NpcDialogue : ConversationTrigger
    {
        protected override string GetConversation()
        {
            Character character = GetComponent<Character>();
            return character != null && character.Data != null ? character.Data.Conversation : null;
        }

        protected override string SourceDescription
        {
            get
            {
                CharacterData data = GetComponent<Character>()?.Data;
                return data != null
                    ? $"CharacterData '{data.name}' (GameObject '{name}')"
                    : $"GameObject '{name}'";
            }
        }
    }
}
