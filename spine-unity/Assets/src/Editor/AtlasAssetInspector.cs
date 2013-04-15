using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AtlasAsset))]
public class AtlasAssetInspector : Editor {
	private SerializedProperty atlasFile, material;

	void OnEnable () {
		atlasFile = serializedObject.FindProperty("atlasFile");
		material = serializedObject.FindProperty("material");
	}

	override public void OnInspectorGUI () {
		serializedObject.Update();
		AtlasAsset asset = (AtlasAsset)target;

		EditorGUIUtility.LookLikeInspector();
		EditorGUILayout.PropertyField(atlasFile);
		EditorGUILayout.PropertyField(material);
		
		if (serializedObject.ApplyModifiedProperties() ||
			(Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
		) {
			asset.Clear();
		}
	}
}