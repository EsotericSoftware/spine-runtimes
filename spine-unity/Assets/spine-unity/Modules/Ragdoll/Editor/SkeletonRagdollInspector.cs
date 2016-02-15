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
	SerializedProperty startingBoneName, stopBoneNames, applyOnStart, pinStartBone, enableJointCollision, useGravity, disableIK, thickness, rotationLimit, colliderLayer, mix, rootMass, massFalloffFactor;

	void OnEnable () {
		startingBoneName = serializedObject.FindProperty("startingBoneName");
		stopBoneNames = serializedObject.FindProperty("stopBoneNames");
		applyOnStart = serializedObject.FindProperty("applyOnStart");
		pinStartBone = serializedObject.FindProperty("pinStartBone");
		enableJointCollision = serializedObject.FindProperty("enableJointCollision");
		useGravity = serializedObject.FindProperty("useGravity");
		disableIK = serializedObject.FindProperty("disableIK");
		thickness = serializedObject.FindProperty("thickness");
		rotationLimit = serializedObject.FindProperty("rotationLimit");
		colliderLayer = serializedObject.FindProperty("colliderLayer");
		mix = serializedObject.FindProperty("mix");
		rootMass = serializedObject.FindProperty("rootMass");
		massFalloffFactor = serializedObject.FindProperty("massFalloffFactor");
	}

	public override void OnInspectorGUI () {
		EditorGUILayout.PropertyField(startingBoneName);
		EditorGUILayout.PropertyField(stopBoneNames, true);
		EditorGUILayout.PropertyField(applyOnStart);
		EditorGUILayout.PropertyField(pinStartBone);
		EditorGUILayout.PropertyField(enableJointCollision);
		EditorGUILayout.PropertyField(useGravity);
		EditorGUILayout.PropertyField(disableIK);
		EditorGUILayout.PropertyField(thickness);
		EditorGUILayout.PropertyField(rotationLimit);
		EditorGUILayout.PropertyField(rootMass);
		EditorGUILayout.PropertyField(massFalloffFactor);
		colliderLayer.intValue = EditorGUILayout.LayerField(colliderLayer.displayName, colliderLayer.intValue);
		EditorGUILayout.PropertyField(mix);

		serializedObject.ApplyModifiedProperties();
	}

	void Header (string name) {
		GUILayout.Space(20);
		EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
	}
}
