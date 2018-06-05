#if UNITY_2017 || UNITY_2018
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

[CustomPropertyDrawer(typeof(SpineSkeletonFlipBehaviour))]
public class SpineSkeletonFlipDrawer : PropertyDrawer
{
    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
        int fieldCount = 1;
        return fieldCount * EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
		SerializedProperty flipXProp = property.FindPropertyRelative("flipX");
		SerializedProperty flipYProp = property.FindPropertyRelative("flipY");

        Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(singleFieldRect, flipXProp);

		singleFieldRect.y += EditorGUIUtility.singleLineHeight;
		EditorGUI.PropertyField(singleFieldRect, flipYProp);
    }
}
#endif