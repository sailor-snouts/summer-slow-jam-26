using UnityEditor;
using UnityEngine;

namespace Game
{
    [CustomEditor(typeof(CharacterData))]
    public class CharacterDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueActor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conversation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("profilePicture"));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultOutfit"), new GUIContent("Default Outfit"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wardrobe"));

            DrawCategory("Brain", "drive", "willpower", "observation", "empathy");
            DrawCategory("Brawn", "vigor", "endurance", "agility", "technique");
            DrawCategory("Beauty", "charm", "taunt", "bonhomie", "hostility");

            DrawGenderSlider();

            serializedObject.ApplyModifiedProperties();
        }

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
