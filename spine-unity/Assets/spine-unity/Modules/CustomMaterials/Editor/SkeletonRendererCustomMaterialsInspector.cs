/*****************************************************************************
 * SkeletonRendererCustomMaterialsInspector created by Lost Polygon
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/
using UnityEngine;
using UnityEditor;
using Spine.Unity.Modules;

namespace Spine.Unity.Editor {
	[CustomEditor(typeof(SkeletonRendererCustomMaterials))]
	public class SkeletonRendererCustomMaterialsInspector : UnityEditor.Editor {

		#region SkeletonRenderer context menu
		[MenuItem ("CONTEXT/SkeletonRenderer/Add Basic Serialized Custom Materials")]
		static void AddSkeletonRendererCustomMaterials (MenuCommand menuCommand) {
			var skeletonRenderer = (SkeletonRenderer)menuCommand.context;
			var newComponent = skeletonRenderer.gameObject.AddComponent<SkeletonRendererCustomMaterials>();
			Undo.RegisterCreatedObjectUndo(newComponent, "Add Basic Serialized Custom Materials");
		}

		[MenuItem ("CONTEXT/SkeletonRenderer/Add Basic Serialized Custom Materials", true)]
		static bool AddSkeletonRendererCustomMaterials_Validate (MenuCommand menuCommand) {
			var skeletonRenderer = (SkeletonRenderer)menuCommand.context;
			return (skeletonRenderer.GetComponent<SkeletonRendererCustomMaterials>() == null);
		}
		#endregion

		public override void OnInspectorGUI() {
			var component = (SkeletonRendererCustomMaterials)target;
			var skeletonRenderer = component.skeletonRenderer;

			// Draw the default inspector and reapply overrides on any change
			EditorGUI.BeginChangeCheck();
			{
				DrawDefaultInspector();
			}
			if (EditorGUI.EndChangeCheck()) {
				component.ReapplyOverrides();
				if (skeletonRenderer != null)
					skeletonRenderer.LateUpdate();
			}

			if (GUILayout.Button(new GUIContent("Clear and Reapply Changes", "Removes all non-serialized overrides in the SkeletonRenderer and reapplies the overrides on this component."))) {
				if (skeletonRenderer != null) {
					skeletonRenderer.CustomMaterialOverride.Clear();
					skeletonRenderer.CustomSlotMaterials.Clear();
					component.ReapplyOverrides();
					skeletonRenderer.LateUpdate();
				}
			}
		}
	}
}