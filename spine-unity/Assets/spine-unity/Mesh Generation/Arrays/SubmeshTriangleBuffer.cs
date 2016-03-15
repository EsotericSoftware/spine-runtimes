using UnityEngine;
using System.Collections;

namespace Spine.Unity.MeshGeneration {
	public class SubmeshTriangleBuffer {
		public int[] triangles;
		public int triangleCount;

		public SubmeshTriangleBuffer (int triangleCount) {
			triangles = new int[triangleCount];
			this.triangleCount = triangleCount;
		}
	}
}