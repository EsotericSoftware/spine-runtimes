

/*****************************************************************************
 * SkeletonAnimatorInspector created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
using System;
using UnityEditor;
using UnityEngine;
using Spine;

[CustomEditor(typeof(SkeletonAnimator))]
public class SkeletonAnimatorInspector : SkeletonRendererInspector {
	protected SerializedProperty layerMixModes;
	protected bool isPrefab;
	protected override void OnEnable () {
		base.OnEnable();
		layerMixModes = serializedObject.FindProperty("layerMixModes");

		if (PrefabUtility.GetPrefabType(this.target) == PrefabType.Prefab)
			isPrefab = true;
	}

	protected override void gui () {
		base.gui();

		EditorGUILayout.PropertyField(layerMixModes, true);

		SkeletonAnimator component = (SkeletonAnimator)target;
		if (!component.valid)
			return;

		EditorGUILayout.Space();

		if (!isPrefab) {
			if (component.GetComponent<SkeletonUtility>() == null) {
				if (GUILayout.Button(new GUIContent("Add Skeleton Utility", SpineEditorUtilities.Icons.skeletonUtility), GUILayout.Height(30))) {
					component.gameObject.AddComponent<SkeletonUtility>();
				}
			}
		}
	}
}