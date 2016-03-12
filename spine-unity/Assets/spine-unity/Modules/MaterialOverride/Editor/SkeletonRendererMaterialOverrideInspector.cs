using UnityEditor;

[CustomEditor(typeof(SkeletonRendererMaterialOverride))]
public class SkeletonRendererMaterialOverrideInspector : Editor {
	public override void OnInspectorGUI() {
		SkeletonRendererMaterialOverride obj = (SkeletonRendererMaterialOverride) target;

		EditorGUI.BeginChangeCheck();
		{
			base.OnInspectorGUI();
		}
		if (EditorGUI.EndChangeCheck()) {
			obj.RemoveAtlasMaterialOverrides();
			obj.RemoveSlotMaterialOverrides();
			obj.SetAtlasMaterialOverrides();
			obj.SetSlotMaterialOverrides();
		}
	}
}