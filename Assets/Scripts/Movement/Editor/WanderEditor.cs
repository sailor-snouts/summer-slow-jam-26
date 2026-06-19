using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Custom Inspector for <see cref="Wander"/>: draws each tunable as a min–max range slider
    /// followed by its distribution curve, and hides the Area Size / Center Offset fields unless
    /// Restrict Area is ticked.
    /// </summary>
    [CustomEditor(typeof(Wander))]
    public class WanderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawRangeWithCurve("moveTimeRange", "moveTimeCurve", "Move Time (s)", 0f, 10f);
            DrawRangeWithCurve("pauseTimeRange", "pauseTimeCurve", "Pause Time (s)", 0f, 10f);
            DrawRangeWithCurve("speedRange", "speedCurve", "Speed (× max)", 0f, 1f);

            EditorGUILayout.Space();

            // Draw the remaining fields (the Area restriction block). Hide the area size/offset
            // unless Restrict Area is on. Everything drawn above is excluded here.
            var handled = new List<string>
            {
                "m_Script",
                "moveTimeRange", "moveTimeCurve",
                "pauseTimeRange", "pauseTimeCurve",
                "speedRange", "speedCurve",
            };
            if (!serializedObject.FindProperty("restrictArea").boolValue)
            {
                handled.Add("areaSize");
                handled.Add("areaCenterOffset");
                handled.Add("edgeLookahead");
            }

            DrawPropertiesExcluding(serializedObject, handled.ToArray());

            serializedObject.ApplyModifiedProperties();
        }

        // Distribution curves are always normalized to a 0..1 (X) by 0..1 (Y) box.
        private static readonly Rect CurveRange = new Rect(0f, 0f, 1f, 1f);
        private static readonly Color CurveColor = new Color(0.45f, 0.9f, 0.5f);

        private void DrawRangeWithCurve(string rangeProp, string curveProp, string label, float limitMin, float limitMax)
        {
            DrawRangeSlider(rangeProp, label, limitMin, limitMax);

            SerializedProperty curve = serializedObject.FindProperty(curveProp);
            var content = new GUIContent(curve.displayName,
                "X = random 0..1 → the value's position in the range above (Y: 0 = min, 1 = max).");
            EditorGUILayout.CurveField(curve, CurveColor, CurveRange, content);
            EditorGUILayout.Space(2f);
        }

        /// <summary>Draws a Vector2 (x = min, y = max) as a min–max slider with numeric fields.</summary>
        private void DrawRangeSlider(string propertyName, string label, float limitMin, float limitMax)
        {
            SerializedProperty prop = serializedObject.FindProperty(propertyName);
            Vector2 range = prop.vector2Value;
            float min = range.x;
            float max = range.y;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
            min = EditorGUILayout.FloatField(min, GUILayout.Width(45f));
            EditorGUILayout.MinMaxSlider(ref min, ref max, limitMin, limitMax);
            max = EditorGUILayout.FloatField(max, GUILayout.Width(45f));
            EditorGUILayout.EndHorizontal();

            // Keep inside the limits, ordered, and rounded (0.01 steps for the 0..1 speed, 0.1 for seconds).
            float step = limitMax <= 1f ? 100f : 10f;
            min = Mathf.Round(Mathf.Clamp(min, limitMin, limitMax) * step) / step;
            max = Mathf.Round(Mathf.Clamp(max, limitMin, limitMax) * step) / step;
            if (min > max)
                min = max;

            prop.vector2Value = new Vector2(min, max);
        }
    }
}
