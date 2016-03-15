using UnityEngine;
using System.Collections;
using UnityEditor;
using Spine.Unity;

[CustomEditor(typeof(SkeletonRenderSeparator))]
public class SkeletonRenderSeparatorInspector : Editor {

	SkeletonRenderSeparator component;

	void OnEnable () {
		component = target as SkeletonRenderSeparator;
	}

	public override void OnInspectorGUI () {
		base.OnInspectorGUI();

		if (GUILayout.Button("Add Renderer")) {
			const int SortingOrderIncrement = 5;
			int index = component.renderers.Count;
			var smr = SkeletonRenderPart.NewSubmeshRendererGameObject(component.transform, index.ToString());
			component.renderers.Add(smr);

			// increment renderer sorting order.
			if (index != 0) {
				var prev = component.renderers[index - 1];
				if (prev != null) {
					var prevMeshRenderer = prev.GetComponent<MeshRenderer>();
					var currentMeshRenderer = smr.GetComponent<MeshRenderer>();
					if (prevMeshRenderer != null && currentMeshRenderer != null) {
						int prevSortingLayer = prevMeshRenderer.sortingLayerID;
						int prevSortingOrder = prevMeshRenderer.sortingOrder;

						currentMeshRenderer.sortingLayerID = prevSortingLayer;
						currentMeshRenderer.sortingOrder = prevSortingOrder + SortingOrderIncrement;
					}
				}
			}
		}

		if (GUILayout.Button("Destroy Renderers")) {
			if (EditorUtility.DisplayDialog("Destroy Submesh Renderers", "Do you really want to destroy all the SubmeshRenderer GameObjects in the list?", "Destroy", "Cancel")) {
				
				for (int i = 0; i < component.renderers.Count; i++) {
					Debug.LogFormat("Destroying {0}", component.renderers[i].gameObject.name);
					DestroyImmediate(component.renderers[i].gameObject);
				}

				component.renderers.Clear();
			}
		}
			
	}



}
