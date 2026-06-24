using UnityEditor;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Custom Inspector for <see cref="CharacterData"/>: draws the twelve stats grouped under their
    /// category headings (Brain / Brawn / Beauty), each group followed by a read-only total — the
    /// sum of its four stats.
    /// </summary>
    [CustomEditor(typeof(CharacterData))]
    public class CharacterDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueActor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conversation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("profilePicture"));

            DrawCategory("Brain", "drive", "willpower", "observation", "empathy");
            DrawCategory("Brawn", "vigor", "endurance", "agility", "technique");
            DrawCategory("Beauty", "charm", "taunt", "bonhomie", "hostility");

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>Draws a category heading, its four stat sliders, and a disabled total field.</summary>
        private void DrawCategory(string category, params string[] statFields)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(category, EditorStyles.boldLabel);

            int total = 0;
            foreach (string field in statFields)
            {
                SerializedProperty property = serializedObject.FindProperty(field);
                EditorGUILayout.PropertyField(property);
                total += property.intValue;
            }

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.IntField($"{category} (total)", total);
        }
    }
}
