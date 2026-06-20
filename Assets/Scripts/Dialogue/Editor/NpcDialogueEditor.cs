using UnityEditor;

namespace Game
{
    /// <summary>
    /// Slim Inspector for <see cref="NpcDialogue"/>: hides all the inherited Dialogue System Trigger
    /// fields. The conversation, actor, and portrait all come from the object's Character → Character
    /// Data; the rest run on their defaults (Trigger = On Use, no conditions; conversant set in code).
    /// </summary>
    [CustomEditor(typeof(NpcDialogue))]
    public class NpcDialogueEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Conversation, actor, and portrait come from this object's Character → Character Data. " +
                "Nothing to configure here.", MessageType.Info);
        }
    }
}
