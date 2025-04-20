using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[CustomEditor(typeof(GradientMappingOverride))]
public class GradientMappingOverrideEditor : Editor
{
    private SerializedProperty settingsProperty;
    private SerializedProperty activeProperty;
    private SerializedProperty gradientProperty;
    private SerializedProperty intensityProperty;

    private void OnEnable()
    {
        settingsProperty = serializedObject.FindProperty("settings");
        activeProperty = settingsProperty.FindPropertyRelative("active");
        gradientProperty = settingsProperty.FindPropertyRelative("gradient");
        intensityProperty = settingsProperty.FindPropertyRelative("intensity");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(activeProperty);

        if (activeProperty.FindPropertyRelative("value").boolValue)
        {
            EditorGUILayout.Space();
            
            var gradientValue = gradientProperty.FindPropertyRelative("value");
            EditorGUILayout.PropertyField(gradientValue, new GUIContent("亮度映射梯度"));
            
            EditorGUILayout.PropertyField(intensityProperty);
            
            EditorGUILayout.Space();
            DrawGradientPreview();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGradientPreview()
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 20);
        rect = EditorGUI.PrefixLabel(rect, new GUIContent("梯度预览"));
        
        if (Event.current.type == EventType.Repaint)
        {
            /*
            var gradient = ((GradientParameter)gradientProperty.GetValue()).value;
            
            // 绘制渐变预览背景
            EditorGUI.DrawRect(rect, Color.black);
            
            // 绘制渐变
            for (int i = 0; i < rect.width; i++)
            {
                float t = i / rect.width;
                Color color = gradient.Evaluate(t);
                
                Rect lineRect = new Rect(rect.x + i, rect.y, 1, rect.height);
                EditorGUI.DrawRect(lineRect, color);
            }
            */
        }
    }
} 