using UnityEngine;
using System.Collections;

using UnityEngine.Assertions;

namespace Spine.Unity {
	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public class SubmeshRenderer : MonoBehaviour {		
		ISingleSubmeshGenerator meshGenerator;
		public ISingleSubmeshGenerator MeshGenerator {
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
			meshGenerator = new ArraysSingleSubmeshGenerator(); // swap this out with your custom ISingleSubmeshGenerator code.
			meshFilter = GetComponent<MeshFilter>();
			meshRenderer = GetComponent<MeshRenderer>();
		}

		public void ClearMesh () {
			meshFilter.sharedMesh = null;
		}

		public void RenderSubmesh (SubmeshInstruction instruction) {
			LazyIntialize();
			meshRenderer.sharedMaterial = instruction.material;
			meshFilter.sharedMesh = meshGenerator.GenerateMesh(instruction);
		}

		public void SetPropertyBlock (MaterialPropertyBlock block) {
			LazyIntialize();
			meshRenderer.SetPropertyBlock(block);
		}

		public static SubmeshRenderer NewSubmeshRendererGameObject (Transform parent, string name) {
			var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
			go.transform.SetParent(parent, false);
			var returnComponent = go.AddComponent<SubmeshRenderer>();

			return returnComponent;
		}
	}
}
