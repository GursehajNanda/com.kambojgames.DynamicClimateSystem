using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxFloat))]
public class MinMaxFloatDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get the min and max fields
        SerializedProperty minProp = property.FindPropertyRelative("min");
        SerializedProperty maxProp = property.FindPropertyRelative("max");


        RangeAttribute range = fieldInfo.GetCustomAttribute<RangeAttribute>();
        if (range == null)
        {
            return; 
        }

        float rangeMin = range.min;
        float rangeMax = range.max;

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        float min = minProp.floatValue;
        float max = maxProp.floatValue;

        // Calculate control rectangles
        float fieldWidth = 40f;
        float spacing = 4f;
        float sliderWidth = position.width - fieldWidth * 2 - spacing * 2;

        Rect minRect = new Rect(position.x, position.y, fieldWidth, position.height);
        Rect sliderRect = new Rect(minRect.xMax + spacing, position.y, sliderWidth, position.height);
        Rect maxRect = new Rect(sliderRect.xMax + spacing, position.y, fieldWidth, position.height);

        // Draw controls
        min = EditorGUI.FloatField(minRect, min);
        max = EditorGUI.FloatField(maxRect, max);

        EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, rangeMin, rangeMax);

        // Clamp and assign
        minProp.floatValue = Mathf.Clamp(min, rangeMin, max);
        maxProp.floatValue = Mathf.Clamp(max, min, rangeMax);

        EditorGUI.EndProperty();
    }
}