using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Reflection;

[CustomEditor(typeof(SkeletonUtilitySubmeshRenderer))]
public class SkeletonUtilitySubmeshRendererInspector : Editor {

	private static MethodInfo EditorGUILayoutSortingLayerField;
	protected SerializedObject rendererSerializedObject;
	protected SerializedProperty sortingLayerIDProperty;

	SkeletonUtilitySubmeshRenderer component;

	void OnEnable () {
		component = (SkeletonUtilitySubmeshRenderer)target;

		if (EditorGUILayoutSortingLayerField == null)
			EditorGUILayoutSortingLayerField = typeof(EditorGUILayout).GetMethod("SortingLayerField", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(GUIContent), typeof(SerializedProperty), typeof(GUIStyle) }, null);

		rendererSerializedObject = new SerializedObject(((SkeletonUtilitySubmeshRenderer)target).GetComponent<Renderer>());
		sortingLayerIDProperty = rendererSerializedObject.FindProperty("m_SortingLayerID");
	}

	public override void OnInspectorGUI () {
		// Sorting Layers
		{
			var renderer = component.GetComponent<Renderer>();
			if (renderer != null) {
				EditorGUI.BeginChangeCheck();

				if (EditorGUILayoutSortingLayerField != null && sortingLayerIDProperty != null) {
					EditorGUILayoutSortingLayerField.Invoke(null, new object[] { new GUIContent("Sorting Layer"), sortingLayerIDProperty, EditorStyles.popup });
				} else {
					renderer.sortingLayerID = EditorGUILayout.IntField("Sorting Layer ID", renderer.sortingLayerID);
				}

				renderer.sortingOrder = EditorGUILayout.IntField("Order in Layer", renderer.sortingOrder);

				if (EditorGUI.EndChangeCheck()) {
					rendererSerializedObject.ApplyModifiedProperties();
					EditorUtility.SetDirty(renderer);
				}
			}
		}
	}
}
