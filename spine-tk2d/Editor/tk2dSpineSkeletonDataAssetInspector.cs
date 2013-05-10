using System;
using UnityEditor;
using UnityEngine;
using Spine;

/*
 */
[CustomEditor(typeof(tk2dSpineSkeletonDataAsset))]
public class tk2dSpineSkeletonDataAssetInspector : Editor {
	
	/*
	 */
	private SerializedProperty sprites;
	private SerializedProperty skeletonJSON;
	private SerializedProperty scale;
	private SerializedProperty fromAnimation;
	private SerializedProperty toAnimation;
	private SerializedProperty duration;
	private bool showAnimationStateData = true;
	
	/*
	 */
	void OnEnable () {
		sprites = serializedObject.FindProperty("sprites");
		skeletonJSON = serializedObject.FindProperty("skeletonJSON");
		scale = serializedObject.FindProperty("scale");
		fromAnimation = serializedObject.FindProperty("fromAnimation");
		toAnimation = serializedObject.FindProperty("toAnimation");
		duration = serializedObject.FindProperty("duration");
	}
	
	/*
	 */
	public override void OnInspectorGUI () {
		serializedObject.Update();
		
		tk2dSpineSkeletonDataAsset asset = target as tk2dSpineSkeletonDataAsset;
		
		EditorGUIUtility.LookLikeInspector();
		EditorGUILayout.PropertyField(sprites);
		EditorGUILayout.PropertyField(skeletonJSON);
		EditorGUILayout.PropertyField(scale);
		
		SkeletonData skeletonData = asset.GetSkeletonData();
		if(skeletonData != null) {
			showAnimationStateData = EditorGUILayout.Foldout(showAnimationStateData,"Animation State Data");
			if(showAnimationStateData) {
				
				String[] animations = new String[skeletonData.Animations.Count];
				for (int i = 0; i < animations.Length; i++) {
					animations[i] = skeletonData.Animations[i].Name;
				}
				
				for(int i = 0; i < fromAnimation.arraySize; i++) {
					SerializedProperty from = fromAnimation.GetArrayElementAtIndex(i);
					SerializedProperty to = toAnimation.GetArrayElementAtIndex(i);
					SerializedProperty durationProp = duration.GetArrayElementAtIndex(i);
					
					EditorGUILayout.BeginHorizontal();
					
					from.stringValue = animations[EditorGUILayout.Popup(Math.Max(Array.IndexOf(animations,from.stringValue),0),animations)];
					to.stringValue = animations[EditorGUILayout.Popup(Math.Max(Array.IndexOf(animations,to.stringValue),0),animations)];
					durationProp.floatValue = EditorGUILayout.FloatField(durationProp.floatValue);
					
					if(GUILayout.Button("Delete")) {
						duration.DeleteArrayElementAtIndex(i);
						toAnimation.DeleteArrayElementAtIndex(i);
						fromAnimation.DeleteArrayElementAtIndex(i);
					}
					
					EditorGUILayout.EndHorizontal();
				}
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				
				if(GUILayout.Button("Add Mix")) {
					duration.arraySize++;
					toAnimation.arraySize++;
					fromAnimation.arraySize++;
				}
				
				EditorGUILayout.Space();
				EditorGUILayout.EndHorizontal();
			}
		}
		
		serializedObject.ApplyModifiedProperties();
	}
}
