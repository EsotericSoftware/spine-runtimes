/*****************************************************************************
 * SkeletonRendererCustomMaterialsInspector created by Lost Polygon
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEditor;

namespace Spine.Unity.Editor {
	[CustomEditor(typeof(SkeletonRendererCustomMaterials))]
	public class SkeletonRendererCustomMaterialsInspector : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			SkeletonRendererCustomMaterials obj = (SkeletonRendererCustomMaterials) target;

			// Just draw the default inspector and reapply overrides on any change
			EditorGUI.BeginChangeCheck();
			{
				DrawDefaultInspector();
			}
			if (EditorGUI.EndChangeCheck()) {
				obj.RemoveCustomMaterialOverrides();
				obj.RemoveCustomSlotMaterials();
				obj.SetCustomMaterialOverrides();
				obj.SetCustomSlotMaterials();
			}
		}
	}
}