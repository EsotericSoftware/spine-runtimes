/*****************************************************************************
 * SkeletonRagdoll added by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SkeletonRagdoll))]
public class SkeletonRagdollInspector : Editor {
	SerializedProperty startingBoneName, stopBoneNames, applyOnStart, pinStartBone, enableJointCollision, useGravity, disableIK, defaultThickness, rotationLimit, colliderLayer, mix;

	void OnEnable () {
		startingBoneName = serializedObject.FindProperty("startingBoneName");
		stopBoneNames = serializedObject.FindProperty("stopBoneNames");
		applyOnStart = serializedObject.FindProperty("applyOnStart");
		pinStartBone = serializedObject.FindProperty("pinStartBone");
		enableJointCollision = serializedObject.FindProperty("enableJointCollision");
		useGravity = serializedObject.FindProperty("useGravity");
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
		EditorGUILayout.PropertyField(useGravity);
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
