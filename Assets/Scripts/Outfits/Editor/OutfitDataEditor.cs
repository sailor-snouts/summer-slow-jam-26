using UnityEditor;

namespace Game
{
    /// <summary>
    /// Custom Inspector for <see cref="OutfitData"/>: draws the twelve stat modifiers grouped under
    /// their category headings (Brain / Brawn / Beauty), each group followed by a read-only total -
    /// the sum of its four modifiers - matching the Character inspector's layout.
    /// </summary>
    [CustomEditor(typeof(OutfitData))]
    public class OutfitDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sprite"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("characterSprites"), true);

            DrawCategory("Brain", "drive", "willpower", "observation", "empathy");
            DrawCategory("Brawn", "vigor", "endurance", "agility", "technique");
            DrawCategory("Beauty", "charm", "taunt", "bonhomie", "hostility");

            DrawGender();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>Masculine and feminine modifiers - two independent -5..5 sliders.</summary>
        private void DrawGender()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gender modifiers", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("masculine"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("feminine"));
        }

        /// <summary>Draws a category heading, its four modifier sliders, and a disabled total field.</summary>
        private void DrawCategory(string category, params string[] modifierFields)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{category} modifiers", EditorStyles.boldLabel);

            int total = 0;
            foreach (string field in modifierFields)
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
