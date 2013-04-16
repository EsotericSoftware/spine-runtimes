using System;
using UnityEditor;
using UnityEngine;
using Spine;

[CustomEditor(typeof(SkeletonDataAsset))]
public class SkeletonDataAssetInspector : Editor {
	private SerializedProperty atlasAsset, skeletonJSON, scale, fromAnimation, toAnimation, duration;
	private bool showAnimationStateData = true;

	void OnEnable () {
		atlasAsset = serializedObject.FindProperty("atlasAsset");
		skeletonJSON = serializedObject.FindProperty("skeletonJSON");
		scale = serializedObject.FindProperty("scale");
		fromAnimation = serializedObject.FindProperty("fromAnimation");
		toAnimation = serializedObject.FindProperty("toAnimation");
		duration = serializedObject.FindProperty("duration");
	}

	override public void OnInspectorGUI () {
		serializedObject.Update();
		SkeletonDataAsset asset = (SkeletonDataAsset)target;

		EditorGUIUtility.LookLikeInspector();
		EditorGUILayout.PropertyField(atlasAsset);
		EditorGUILayout.PropertyField(skeletonJSON);
		EditorGUILayout.PropertyField(scale);
		
		SkeletonData skeletonData = asset.GetSkeletonData(true);
		if (skeletonData != null) {
			showAnimationStateData = EditorGUILayout.Foldout(showAnimationStateData, "Animation State Data");
			if (showAnimationStateData) {
				// Animation names.
				String[] animations = new String[skeletonData.Animations.Count];
				for (int i = 0; i < animations.Length; i++)
					animations[i] = skeletonData.Animations[i].Name;
			
				for (int i = 0; i < fromAnimation.arraySize; i++) {
					SerializedProperty from = fromAnimation.GetArrayElementAtIndex(i);
					SerializedProperty to = toAnimation.GetArrayElementAtIndex(i);
					SerializedProperty durationProp = duration.GetArrayElementAtIndex(i);
					EditorGUILayout.BeginHorizontal();
					from.stringValue = animations[EditorGUILayout.Popup(Math.Max(Array.IndexOf(animations, from.stringValue), 0), animations)];
					to.stringValue = animations[EditorGUILayout.Popup(Math.Max(Array.IndexOf(animations, to.stringValue), 0), animations)];
					durationProp.floatValue = EditorGUILayout.FloatField(durationProp.floatValue);
					if (GUILayout.Button("Delete")) {
						duration.DeleteArrayElementAtIndex(i);
						toAnimation.DeleteArrayElementAtIndex(i);
						fromAnimation.DeleteArrayElementAtIndex(i);
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				if (GUILayout.Button("Add Mix")) {
					duration.InsertArrayElementAtIndex(fromAnimation.arraySize);
					toAnimation.InsertArrayElementAtIndex(fromAnimation.arraySize);
					fromAnimation.InsertArrayElementAtIndex(fromAnimation.arraySize);
				}
				EditorGUILayout.Space();
				EditorGUILayout.EndHorizontal();
			}
		}
		
		if (serializedObject.ApplyModifiedProperties() ||
			(Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
		) {
			asset.Clear();
		}
	}
}