using UnityEngine;
using System.Collections;
using UnityEditor;
using Spine.Unity;

namespace Spine.Unity.Editor {
	[CustomEditor(typeof(SkeletonPartsRenderer))]
	public class SkeletonRenderPartInspector : UnityEditor.Editor {
		SpineInspectorUtility.SerializedSortingProperties sortingProperties;

		void OnEnable () {			
			sortingProperties = new SpineInspectorUtility.SerializedSortingProperties((target as Component).GetComponent<MeshRenderer>());
		}

		public override void OnInspectorGUI () {
			DrawDefaultInspector();
			SpineInspectorUtility.SortingPropertyFields(sortingProperties, true);
		}
	}

}
