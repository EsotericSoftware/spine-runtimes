using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

namespace Spine.Unity {
	[ExecuteInEditMode]
	public class SkeletonRenderSeparator : MonoBehaviour {
		[SerializeField]
		protected SkeletonRenderer skeletonRenderer;
		public SkeletonRenderer SkeletonRenderer {
			get { return skeletonRenderer; }
			set {
				if (skeletonRenderer != null)
					skeletonRenderer.GenerateMeshOverride -= SeparateSkeletonRender;
				skeletonRenderer = value;
			}
		}

		MeshRenderer masterMeshRenderer;

		[Header("Settings")]
		public bool propagateMaterialPropertyBlock = false;
		public bool controlMainMeshRenderer = true;

		[Space(10f)]
		public List<Spine.Unity.SkeletonRenderPart> renderers = new List<SkeletonRenderPart>();

		void Reset () {
			if (skeletonRenderer == null) {
				skeletonRenderer = GetComponent<SkeletonRenderer>();
			}
		}

		void OnEnable () {
			if (skeletonRenderer == null) return;
			if (block == null) block = new MaterialPropertyBlock();	
			masterMeshRenderer = skeletonRenderer.GetComponent<MeshRenderer>();

			if (controlMainMeshRenderer)
				masterMeshRenderer.enabled = false;

			skeletonRenderer.GenerateMeshOverride -= SeparateSkeletonRender;
			skeletonRenderer.GenerateMeshOverride += SeparateSkeletonRender;
		}

		void OnDisable () {
			if (skeletonRenderer == null) return;

			if (controlMainMeshRenderer)
				masterMeshRenderer.enabled = true;

			skeletonRenderer.GenerateMeshOverride -= SeparateSkeletonRender;

			foreach (var s in renderers)
				s.ClearMesh();		
		}

		MaterialPropertyBlock block;

		void SeparateSkeletonRender (SkeletonRenderer.SmartMesh.Instruction instruction) {
			int rendererCount = renderers.Count;
			if (rendererCount <= 0) return;

			int rendererIndex = 0;

			if (propagateMaterialPropertyBlock)
				masterMeshRenderer.GetPropertyBlock(block);

			var submeshInstructions = instruction.submeshInstructions;
			var submeshInstructionsItems = submeshInstructions.Items;
			int lastSubmeshInstruction = submeshInstructions.Count - 1;

			var currentRenderer = renderers[rendererIndex];
			for (int i = 0, start = 0; i <= lastSubmeshInstruction; i++) {
				if (submeshInstructionsItems[i].separatedBySlot) {
					//Debug.Log(submeshInstructionsItems[i].endSlot);
					currentRenderer.RenderSubmesh(instruction.submeshInstructions, start, i + 1);
					start = i + 1;

					if (propagateMaterialPropertyBlock)
						currentRenderer.SetPropertyBlock(block);					

					rendererIndex++;
					if (rendererIndex < rendererCount) {
						currentRenderer = renderers[rendererIndex];
					} else {
						break;
					}
				} else if (i == lastSubmeshInstruction) {
					//Debug.Log(submeshInstructionsItems[i].endSlot);
					currentRenderer.RenderSubmesh(instruction.submeshInstructions, start, i + 1);

					if (propagateMaterialPropertyBlock)
						currentRenderer.SetPropertyBlock(block);
				}
			}
		}



	}
}