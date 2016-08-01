/*****************************************************************************
 * SkeletonAnimatorInspector created by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
using UnityEditor;
using UnityEngine;

namespace Spine.Unity.Editor {
	[CustomEditor(typeof(SkeletonAnimator))]
	public class SkeletonAnimatorInspector : SkeletonRendererInspector {
		protected SerializedProperty layerMixModes;
		protected override void OnEnable () {
			base.OnEnable();
			layerMixModes = serializedObject.FindProperty("layerMixModes");
		}

		protected override void DrawInspectorGUI () {
			base.DrawInspectorGUI();
			EditorGUILayout.PropertyField(layerMixModes, true);
			var component = (SkeletonAnimator)target;
			if (!component.valid)
				return;

			EditorGUILayout.Space();

			if (!isInspectingPrefab) {
				if (component.GetComponent<SkeletonUtility>() == null) {
					if (GUILayout.Button(new GUIContent("Add Skeleton Utility", SpineEditorUtilities.Icons.skeletonUtility), GUILayout.Height(30))) {
						component.gameObject.AddComponent<SkeletonUtility>();
					}
				}
			}
		}
	}
}
