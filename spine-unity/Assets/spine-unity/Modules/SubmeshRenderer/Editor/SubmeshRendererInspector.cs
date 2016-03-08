using UnityEngine;
using System.Collections;
using UnityEditor;
using Spine.Unity;

[CustomEditor(typeof(SubmeshRenderer))]
public class SubmeshRendererInspector : Editor {

	SpineInspectorUtility.SerializedSortingProperties sortingProperties;
	SubmeshRenderer component;

	void OnEnable () {
		component = target as SubmeshRenderer;
		sortingProperties = new SpineInspectorUtility.SerializedSortingProperties(component.GetComponent<MeshRenderer>());
	}

	public override void OnInspectorGUI () {
		DrawDefaultInspector();
		SpineInspectorUtility.SortingPropertyFields(sortingProperties, true);
	}

}
