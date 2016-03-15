using UnityEngine;
using System.Collections;
using Spine.Unity.MeshGeneration;

namespace Spine.Unity {
	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public class SkeletonRenderPart : MonoBehaviour {		
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

		void LazyIntialize () {
			if (meshGenerator != null) return;
			meshGenerator = new ArraysSubmeshSetMeshGenerator();
			meshFilter = GetComponent<MeshFilter>();
			meshRenderer = GetComponent<MeshRenderer>();
		}

		public void ClearMesh () {
			meshFilter.sharedMesh = null;
		}

		public void RenderSubmesh (ExposedList<SubmeshInstruction> instructions, int startSubmesh, int endSubmesh) {
			LazyIntialize();
			MeshAndMaterials m = meshGenerator.GenerateMesh(instructions, startSubmesh, endSubmesh);
			meshFilter.sharedMesh = m.mesh;
			meshRenderer.sharedMaterials = m.materials;
		}

		public void SetPropertyBlock (MaterialPropertyBlock block) {
			LazyIntialize();
			meshRenderer.SetPropertyBlock(block);
		}

		public static SkeletonRenderPart NewSubmeshRendererGameObject (Transform parent, string name) {
			var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
			go.transform.SetParent(parent, false);
			var returnComponent = go.AddComponent<SkeletonRenderPart>();

			return returnComponent;
		}
	}
}
