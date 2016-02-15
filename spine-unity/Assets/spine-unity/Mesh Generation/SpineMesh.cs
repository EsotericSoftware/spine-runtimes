using UnityEngine;
using System.Collections;

public static class SpineMesh {
	internal const HideFlags MeshHideflags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;

	public static Mesh NewMesh () {
		var m = new Mesh();
		m.name = "Skeleton Mesh";
		m.hideFlags = SpineMesh.MeshHideflags;
		return m;
	}

}
