using UnityEditor;

namespace Game
{
    /// <summary>
    /// Slim Inspector for every <see cref="ConversationTrigger"/> (NPC or object): hides all the
    /// inherited Dialogue System Trigger fields. The conversation comes from the object's data
    /// component (Character → CharacterData, or Interactable Object → InteractableObjectData); the
    /// rest run on their defaults (Trigger = On Use, no conditions; conversant set in code).
    /// </summary>
    [CustomEditor(typeof(ConversationTrigger), editorForChildClasses: true)]
    public class ConversationTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Conversation comes from this object's data component (Character → Character Data, or " +
                "Interactable Object → Interactable Object Data). Nothing to configure here.",
                MessageType.Info);
        }
    }
}
