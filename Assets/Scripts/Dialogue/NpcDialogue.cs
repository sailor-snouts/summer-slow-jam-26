using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Character))]
    public class NpcDialogue : ConversationTrigger
    {
        protected override string DefaultVerb
        {
            get
            {
                Character character = GetComponent<Character>();
                string displayName = character != null ? character.Name : null;
                return string.IsNullOrEmpty(displayName) ? "talk" : $"talk to {displayName}";
            }
        }

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
