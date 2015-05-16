/*****************************************************************************
 * SkeletonRagdoll2D added by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SkeletonRagdoll2D))]
public class SkeletonRagdoll2DInspector : Editor {
	SerializedProperty startingBoneName, stopBoneNames, applyOnStart, pinStartBone, enableJointCollision, gravityScale, disableIK, defaultThickness, rotationLimit, colliderLayer, mix;

	void OnEnable () {
		startingBoneName = serializedObject.FindProperty("startingBoneName");
		stopBoneNames = serializedObject.FindProperty("stopBoneNames");
		applyOnStart = serializedObject.FindProperty("applyOnStart");
		pinStartBone = serializedObject.FindProperty("pinStartBone");
		enableJointCollision = serializedObject.FindProperty("enableJointCollision");
		gravityScale = serializedObject.FindProperty("gravityScale");
		disableIK = serializedObject.FindProperty("disableIK");
		defaultThickness = serializedObject.FindProperty("defaultThickness");
		rotationLimit = serializedObject.FindProperty("rotationLimit");
		colliderLayer = serializedObject.FindProperty("colliderLayer");
		mix = serializedObject.FindProperty("mix");
	}

	public override void OnInspectorGUI () {
		EditorGUILayout.PropertyField(startingBoneName);
		EditorGUILayout.PropertyField(stopBoneNames, true);
		EditorGUILayout.PropertyField(applyOnStart);
		EditorGUILayout.PropertyField(pinStartBone);
		EditorGUILayout.PropertyField(enableJointCollision);
		EditorGUILayout.PropertyField(gravityScale);
		EditorGUILayout.PropertyField(disableIK);
		EditorGUILayout.PropertyField(defaultThickness);
		EditorGUILayout.PropertyField(rotationLimit);
		colliderLayer.intValue = EditorGUILayout.LayerField(colliderLayer.displayName, colliderLayer.intValue);
		EditorGUILayout.PropertyField(mix);

		serializedObject.ApplyModifiedProperties();
	}

	void Header (string name) {
		GUILayout.Space(20);
		EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
	}
}
