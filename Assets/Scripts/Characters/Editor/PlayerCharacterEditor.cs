using PixelCrushers.DialogueSystem;
using UnityEditor;
using UnityEngine;

namespace Game
{
    [CustomEditor(typeof(PlayerCharacter))]
    public class PlayerCharacterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Play-mode testing", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                var player = (PlayerCharacter)target;

                if (GUILayout.Button("Swap Character"))
                    player.Swap();

                if (GUILayout.Button("Unlock Movement"))
                {
                    // A menu overlay (outfit/settings) still needs closing on its own.
                    if (DialogueManager.isConversationActive)
                        DialogueManager.StopConversation();
                    PlayerInput.ClearManualLocks();
                }
            }

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Enter Play mode to use these.", MessageType.None);
        }
    }
}
