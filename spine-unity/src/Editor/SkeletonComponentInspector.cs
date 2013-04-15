using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkeletonComponent))]
public class SkeletonComponentInspector : Editor {
	private SerializedProperty skeletonDataAsset, animationName, loop, timeScale;

	void OnEnable () {
		skeletonDataAsset = serializedObject.FindProperty("skeletonDataAsset");
		animationName = serializedObject.FindProperty("animationName");
		loop = serializedObject.FindProperty("loop");
		timeScale = serializedObject.FindProperty("timeScale");
	}

	override public void OnInspectorGUI () {
		serializedObject.Update();
		SkeletonComponent component = (SkeletonComponent)target;

		EditorGUIUtility.LookLikeInspector();
		EditorGUILayout.PropertyField(skeletonDataAsset);
		
		if (component.skeleton != null) {
			// Animation name.
			String[] animations = new String[component.skeleton.Data.Animations.Count + 1];
			animations[0] = "<None>";
			int animationIndex = 0;
			for (int i = 0; i < animations.Length - 1; i++) {
				String name = component.skeleton.Data.Animations[i].Name;
				animations[i + 1] = name;
				if (name == animationName.stringValue)
					animationIndex = i + 1;
			}
		
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Animation");
			EditorGUIUtility.LookLikeControls();
			animationIndex = EditorGUILayout.Popup(animationIndex, animations);
			EditorGUIUtility.LookLikeInspector();
			EditorGUILayout.EndHorizontal();
		
			animationName.stringValue = animationIndex == 0 ? null : animations[animationIndex];
		}

		// Animation loop.
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Loop");
		loop.boolValue = EditorGUILayout.Toggle(loop.boolValue);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.PropertyField(timeScale);

		if (serializedObject.ApplyModifiedProperties() ||
			(Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
		) {
			component.Clear();
		}
	}
}