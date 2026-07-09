using UnityEditor;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Custom Inspector for <see cref="CharacterData"/>: draws the twelve stats grouped under their
    /// category headings (Brain / Brawn / Beauty), each group followed by a read-only total - the
    /// sum of its four stats - plus the masculine/feminine split as a single 10-point slider.
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

            DrawGenderSlider();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// One slider for the masculine/feminine split of <see cref="CharacterData.GenderTotal"/> points:
        /// far left = all masculine (10/0), far right = all feminine (0/10), middle = 5/5. We store the
        /// masculine share, but drive the slider by the feminine share so left-to-right reads
        /// masculine to feminine.
        /// </summary>
        private void DrawGenderSlider()
        {
            const int total = CharacterData.GenderTotal;
            SerializedProperty masculine = serializedObject.FindProperty("masculine");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Gender ({total} pts)", EditorStyles.boldLabel);

            int feminine = total - masculine.intValue;
            var label = new GUIContent(
                $"Masc {masculine.intValue} / Fem {feminine}",
                "Slide left for masculine (10/0), right for feminine (0/10). The two always total 10.");
            int newFeminine = EditorGUILayout.IntSlider(label, feminine, 0, total);
            masculine.intValue = total - newFeminine;
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
