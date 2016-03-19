using UnityEngine;
using System.Collections;
using Spine.Unity.MeshGeneration;

namespace Spine.Unity {
	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public class SkeletonPartsRenderer : MonoBehaviour {

		#region Properties
		ISubmeshSetMeshGenerator meshGenerator;
		public ISubmeshSetMeshGenerator MeshGenerator {
			get {
				LazyIntialize();
				return meshGenerator;
			}
		}

		MeshRenderer meshRenderer;
		public MeshRenderer MeshRenderer {
			get {
				LazyIntialize();
				return meshRenderer;
			}
		}

		MeshFilter meshFilter;
		public MeshFilter MeshFilter {
			get {
				LazyIntialize();
				return meshFilter;
			}
		}
		#endregion

		void LazyIntialize () {
			if (meshGenerator != null) return;
			meshGenerator = new ArraysSubmeshSetMeshGenerator();
			meshFilter = GetComponent<MeshFilter>();
			meshRenderer = GetComponent<MeshRenderer>();
		}

		public void ClearMesh () {
			LazyIntialize();
			meshFilter.sharedMesh = null;
		}

		public void RenderParts (ExposedList<SubmeshInstruction> instructions, int startSubmesh, int endSubmesh) {
			LazyIntialize();
			MeshAndMaterials m = meshGenerator.GenerateMesh(instructions, startSubmesh, endSubmesh);
			meshFilter.sharedMesh = m.mesh;
			meshRenderer.sharedMaterials = m.materials;
		}

		public void SetPropertyBlock (MaterialPropertyBlock block) {
			LazyIntialize();
			meshRenderer.SetPropertyBlock(block);
		}

		public static SkeletonPartsRenderer NewPartsRendererGameObject (Transform parent, string name) {
			var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
			go.transform.SetParent(parent, false);
			var returnComponent = go.AddComponent<SkeletonPartsRenderer>();

			return returnComponent;
		}
	}
}
