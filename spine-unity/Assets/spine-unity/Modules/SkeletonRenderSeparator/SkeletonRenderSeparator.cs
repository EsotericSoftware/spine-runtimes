using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

namespace Spine.Unity {
	
	[ExecuteInEditMode]
	[HelpURL("https://github.com/pharan/spine-unity-docs/blob/master/SkeletonRenderSeparator.md")]
	public class SkeletonRenderSeparator : MonoBehaviour {
		public const int DefaultSortingOrderIncrement = 5;

		#region Inspector
		[SerializeField]
		protected SkeletonRenderer skeletonRenderer;
		public SkeletonRenderer SkeletonRenderer {
			get { return skeletonRenderer; }
			set {
				if (skeletonRenderer != null)
					skeletonRenderer.GenerateMeshOverride -= HandleRender;
				
				skeletonRenderer = value;
				this.enabled = false; // Disable if nulled.
			}
		}

		MeshRenderer mainMeshRenderer;
		public bool copyPropertyBlock = false;
		[Tooltip("Copies MeshRenderer flags into ")]
		public bool copyMeshRendererFlags = false;
		public List<Spine.Unity.SkeletonPartsRenderer> partsRenderers = new List<SkeletonPartsRenderer>();

		#if UNITY_EDITOR
		void Reset () {
			if (skeletonRenderer == null)
				skeletonRenderer = GetComponent<SkeletonRenderer>();
		}
		#endif
		#endregion

		void OnEnable () {
			if (skeletonRenderer == null) return;
			if (block == null) block = new MaterialPropertyBlock();	
			mainMeshRenderer = skeletonRenderer.GetComponent<MeshRenderer>();

			skeletonRenderer.GenerateMeshOverride -= HandleRender;
			skeletonRenderer.GenerateMeshOverride += HandleRender;

			if (copyMeshRendererFlags) {
				bool useLightProbes = mainMeshRenderer.useLightProbes;
				bool receiveShadows = mainMeshRenderer.receiveShadows;

				for (int i = 0; i < partsRenderers.Count; i++) {
					var currentRenderer = partsRenderers[i];
					if (currentRenderer == null) continue; // skip null items.

					var mr = currentRenderer.MeshRenderer;
					mr.useLightProbes = useLightProbes;
					mr.receiveShadows = receiveShadows;
				}
			}

		}

		void OnDisable () {
			if (skeletonRenderer == null) return;
			skeletonRenderer.GenerateMeshOverride -= HandleRender;

			#if UNITY_EDITOR
			skeletonRenderer.LateUpdate();
			#endif

			foreach (var s in partsRenderers)
				s.ClearMesh();		
		}

		MaterialPropertyBlock block;

		void HandleRender (SkeletonRenderer.SmartMesh.Instruction instruction) {
			int rendererCount = partsRenderers.Count;
			if (rendererCount <= 0) return;

			int rendererIndex = 0;

			if (copyPropertyBlock)
				mainMeshRenderer.GetPropertyBlock(block);

			var submeshInstructions = instruction.submeshInstructions;
			var submeshInstructionsItems = submeshInstructions.Items;
			int lastSubmeshInstruction = submeshInstructions.Count - 1;

			var currentRenderer = partsRenderers[rendererIndex];
			bool skeletonRendererCalculateNormals = skeletonRenderer.calculateNormals;
				
			for (int i = 0, start = 0; i <= lastSubmeshInstruction; i++) {
				if (submeshInstructionsItems[i].forceSeparate) {
					currentRenderer.RenderParts(instruction.submeshInstructions, start, i + 1);
					currentRenderer.MeshGenerator.GenerateNormals = skeletonRendererCalculateNormals;
					if (copyPropertyBlock)
						currentRenderer.SetPropertyBlock(block);					

					start = i + 1;
					rendererIndex++;
					if (rendererIndex < rendererCount) {
						currentRenderer = partsRenderers[rendererIndex];
					} else {
						// Not enough renderers. Skip the rest of the instructions.
						break;
					}
				} else if (i == lastSubmeshInstruction) {
					currentRenderer.RenderParts(instruction.submeshInstructions, start, i + 1);
					currentRenderer.MeshGenerator.GenerateNormals = skeletonRendererCalculateNormals;
					if (copyPropertyBlock)
						currentRenderer.SetPropertyBlock(block);
				}
			}

			// If too many renderers. Clear the rest.
			rendererIndex++;
			if (rendererIndex < rendererCount - 1) {
				for (int i = rendererIndex; i < rendererCount; i++) {
					currentRenderer = partsRenderers[i];
					currentRenderer.ClearMesh();
				}
			}

		}



	}
}