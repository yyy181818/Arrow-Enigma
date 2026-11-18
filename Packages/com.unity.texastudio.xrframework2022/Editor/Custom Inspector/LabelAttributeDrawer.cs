
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LabelAttribute labelAttribute = (LabelAttribute)attribute;
        if(labelAttribute.Label.Length > 0)
        {
            label.text = labelAttribute.Label;
        }
        EditorGUI.PropertyField(position, property, label);
    }
}