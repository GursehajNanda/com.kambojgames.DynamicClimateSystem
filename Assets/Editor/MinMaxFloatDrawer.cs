using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxFloat))]
public class MinMaxFloatDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        SerializedProperty minProp = property.FindPropertyRelative("min");
        SerializedProperty maxProp = property.FindPropertyRelative("max");


        float min = minProp.floatValue;
        float max = maxProp.floatValue;


        RangeAttribute range = fieldInfo.GetCustomAttribute<RangeAttribute>();


        if (range == null)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            float fieldWidth = 60f;
            float spacing = 4f;
            float sliderWidth = position.width - fieldWidth * 2 - spacing * 2;

            Rect minRect = new Rect(position.x, position.y, fieldWidth, position.height);
            Rect maxRect = new Rect(minRect.xMax + sliderWidth + spacing * 2, position.y, fieldWidth, position.height);

            min = EditorGUI.FloatField(minRect, min);
            max = EditorGUI.FloatField(maxRect, max);

            minProp.floatValue = min;
            maxProp.floatValue = max;

            EditorGUI.EndProperty();
            return;
        }


        float rangeMin = range?.min ?? min;
        float rangeMax = range?.max ?? max;
        

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        float fw = 40f;
        float space = 4f;
        float sw = position.width - fw * 2 - space * 2;

        Rect minR = new Rect(position.x, position.y, fw, position.height);
        Rect sliderR = new Rect(minR.xMax + space, position.y, sw, position.height);
        Rect maxR = new Rect(sliderR.xMax + space, position.y, fw, position.height);


        min = EditorGUI.FloatField(minR, min);
        max = EditorGUI.FloatField(maxR, max);


        EditorGUI.MinMaxSlider(sliderR, ref min, ref max, rangeMin, rangeMax);
        min = Mathf.Clamp(min, rangeMin, max);
        max = Mathf.Clamp(max, min, rangeMax);

        minProp.floatValue = min;
        maxProp.floatValue = max;

        EditorGUI.EndProperty();

    }
}