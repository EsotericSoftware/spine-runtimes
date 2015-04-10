using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(BoundingBoxFollower))]
public class BoundingBoxFollowerInspector : Editor {
	SerializedProperty skeletonRenderer, slotName;
	BoundingBoxFollower follower;
	bool needToReset = false;

	void OnEnable () {
		skeletonRenderer = serializedObject.FindProperty("skeletonRenderer");
		slotName = serializedObject.FindProperty("slotName");
		follower = (BoundingBoxFollower)target;
	}

	public override void OnInspectorGUI () {
		if (needToReset) {
			follower.HandleReset(null);
			needToReset = false;
		}
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(skeletonRenderer);
		EditorGUILayout.PropertyField(slotName, new GUIContent("Slot"));

		if (EditorGUI.EndChangeCheck()){
			serializedObject.ApplyModifiedProperties();
			needToReset = true;
		}

		bool hasBone = follower.GetComponent<BoneFollower>() != null;

		EditorGUI.BeginDisabledGroup(hasBone || follower.Slot == null);
		{
			if (GUILayout.Button(new GUIContent("Add Bone Follower", SpineEditorUtilities.Icons.bone))) {
				var boneFollower = follower.gameObject.AddComponent<BoneFollower>();
				boneFollower.boneName = follower.Slot.Data.BoneData.Name;
			}
		}
		EditorGUI.EndDisabledGroup();
		
		

		//GUILayout.Space(20);
		GUILayout.Label("Attachment Names", EditorStyles.boldLabel);
		foreach (var kp in follower.attachmentNameTable) {
			string name = kp.Value;
			var collider = follower.colliderTable[kp.Key];
			bool isPlaceholder = name != kp.Key.Name;
			collider.enabled = EditorGUILayout.ToggleLeft(new GUIContent(!isPlaceholder ? name : name + " [" + kp.Key.Name + "]", isPlaceholder ? SpineEditorUtilities.Icons.skinPlaceholder : SpineEditorUtilities.Icons.boundingBox), collider.enabled);
		}
	}
}
