using UnityEngine;
using System.Collections;
using UnityEditor;
using Spine.Unity;

[CustomEditor(typeof(SkeletonRenderPart))]
public class SkeletonRenderPartInspector : Editor {
	SpineInspectorUtility.SerializedSortingProperties sortingProperties;

	void OnEnable () {
		var component = target as Component;
		sortingProperties = new SpineInspectorUtility.SerializedSortingProperties(component.GetComponent<MeshRenderer>());
	}

	public override void OnInspectorGUI () {
		DrawDefaultInspector();
		SpineInspectorUtility.SortingPropertyFields(sortingProperties, true);
	}
}
