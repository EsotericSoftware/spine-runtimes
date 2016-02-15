using UnityEngine;
using System.Collections;

namespace Spine.Unity {
	public class DoubleBufferedMesh {
		readonly Mesh mesh1 = SpineMesh.NewMesh();
		readonly Mesh mesh2 = SpineMesh.NewMesh();
		bool usingMesh1;
			
		public Mesh GetNextMesh () {
			usingMesh1 = !usingMesh1;
			return usingMesh1 ? mesh1 : mesh2;
		}
	}
}
