using UnityEngine;
using System.Collections;
using UnityEditor;
using Spine.Unity;

[CustomEditor(typeof(SpineRenderSeparator))]
public class SpineRenderSeparatorInspector : Editor {

	SpineRenderSeparator component;

	void OnEnable () {
		component = target as SpineRenderSeparator;
	}

	public override void OnInspectorGUI () {
		base.OnInspectorGUI();
		if (GUILayout.Button("Destroy Submesh Renderers")) {
			if (EditorUtility.DisplayDialog("Destroy Submesh Renderers", "Do you really want to destroy all the SubmeshRenderer GameObjects in the list?", "Destroy", "Cancel")) {
				
				for (int i = 0; i < component.submeshRenderers.Count; i++) {
					Debug.LogFormat("Destroying {0}", component.submeshRenderers[i].gameObject.name);
					DestroyImmediate(component.submeshRenderers[i].gameObject);
				}

				component.submeshRenderers.Clear();
			}
		}

		if (GUILayout.Button("Add SubmeshRenderer")) {
			int index = component.submeshRenderers.Count;
			var smr = SubmeshRenderer.NewSubmeshRendererGameObject(component.transform, index.ToString());
			component.submeshRenderers.Add(smr);
		}
	}

}
