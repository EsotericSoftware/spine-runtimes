using UnityEditor;
using UnityEngine;
using Spine;
using Spine.Unity;
using Spine.Unity.Playables;

//[CustomPropertyDrawer(typeof(SpineAnimationStateBehaviour))]
public class SpineAnimationStateDrawer : PropertyDrawer {
	/*
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		const int fieldCount = 8;
		return fieldCount * EditorGUIUtility.singleLineHeight;
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		SerializedProperty skeletonDataAssetProp = property.FindPropertyRelative("skeletonDataAsset");
		SerializedProperty animationNameProp = property.FindPropertyRelative("animationName");
		SerializedProperty loopProp = property.FindPropertyRelative("loop");
		SerializedProperty eventProp = property.FindPropertyRelative("eventThreshold");
		SerializedProperty attachmentProp = property.FindPropertyRelative("attachmentThreshold");
		SerializedProperty drawOrderProp = property.FindPropertyRelative("drawOrderThreshold");

		Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		EditorGUI.PropertyField(singleFieldRect, skeletonDataAssetProp);

		float lineHeightWithSpacing = EditorGUIUtility.singleLineHeight + 2f;

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.PropertyField(singleFieldRect, animationNameProp);

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.PropertyField(singleFieldRect, loopProp);

		singleFieldRect.y += lineHeightWithSpacing * 0.5f;

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.LabelField(singleFieldRect, "Mixing Settings", EditorStyles.boldLabel);

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.PropertyField(singleFieldRect, eventProp);

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.PropertyField(singleFieldRect, attachmentProp);

		singleFieldRect.y += lineHeightWithSpacing;
		EditorGUI.PropertyField(singleFieldRect, drawOrderProp);
	}
	*/
}
