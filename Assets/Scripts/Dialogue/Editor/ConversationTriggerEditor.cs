using UnityEditor;

namespace Game
{
    [CustomEditor(typeof(ConversationTrigger), editorForChildClasses: true)]
    public class ConversationTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Conversation comes from this object's data component (Character to Character Data, or " +
                "Interactable Object to Interactable Object Data). Nothing to configure here.",
                MessageType.Info);
        }
    }
}
