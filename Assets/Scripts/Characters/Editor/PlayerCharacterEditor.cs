using PixelCrushers.DialogueSystem;
using UnityEditor;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Adds play-mode testing buttons to the <see cref="PlayerCharacter"/> inspector: swap between the
    /// two characters, and force-unlock gameplay input (drop manual locks and end any active
    /// conversation). These are editor conveniences for play-testing - they only do anything in Play
    /// mode, so they are disabled otherwise.
    /// </summary>
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
                    // End anything holding the lock: stop an active conversation, then drop manual
                    // locks. A menu overlay (outfit/settings) still needs closing on its own.
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
