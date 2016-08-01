using UnityEngine;

namespace Spine.Unity {
	public static class SpineMesh {
#if UNITY_5
		internal const HideFlags MeshHideflags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
#else
		internal const HideFlags MeshHideflags = HideFlags.DontSave; 
#endif
		/// <summary>Factory method for creating a new mesh for use in Spine components. This can be called in field initializers.</summary>
		public static Mesh NewMesh () {
			var m = new Mesh();
			m.MarkDynamic();
			m.name = "Skeleton Mesh";
			m.hideFlags = SpineMesh.MeshHideflags;
			return m;
		}
	}
}
