/*****************************************************************************
 * SkeletonRagdoll added by Mitch Thompson
 * Full irrevocable rights and permissions granted to Esoteric Software
*****************************************************************************/

using UnityEngine;
using UnityEditor;

namespace Spine.Unity.Modules {
	
	public class SkeletonRagdollInspector : UnityEditor.Editor {
		[CustomPropertyDrawer(typeof(SkeletonRagdoll.LayerFieldAttribute))]
		public class LayerFieldPropertyDrawer : PropertyDrawer {
			public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
				property.intValue = EditorGUI.LayerField(position, label, property.intValue);
			}
		}
	}

}
