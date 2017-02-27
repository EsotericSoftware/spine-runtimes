/******************************************************************************
 * Spine Runtimes Software License v2.5
 *
 * Copyright (c) 2013-2016, Esoteric Software
 * All rights reserved.
 *
 * You are granted a perpetual, non-exclusive, non-sublicensable, and
 * non-transferable license to use, install, execute, and perform the Spine
 * Runtimes software and derivative works solely for personal or internal
 * use. Without the written permission of Esoteric Software (see Section 2 of
 * the Spine Software License Agreement), you may not (a) modify, translate,
 * adapt, or develop new applications using the Spine Runtimes or otherwise
 * create derivative works or improvements of the Spine Runtimes or (b) remove,
 * delete, alter, or obscure any trademarks or any copyright, trademark, patent,
 * or other intellectual property or proprietary rights notices on or in the
 * Software, including any copy thereof. Redistributions in binary or source
 * form must include this license and terms.
 *
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES, BUSINESS INTERRUPTION, OR LOSS OF
 * USE, DATA, OR PROFITS) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#define SPINE_OPTIONAL_RENDEROVERRIDE

using UnityEngine;
using System.Collections.Generic;
using Spine.Unity;

namespace Spine.Unity.Modules {
	
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
				#if SPINE_OPTIONAL_RENDEROVERRIDE
				if (skeletonRenderer != null)
					skeletonRenderer.GenerateMeshOverride -= HandleRender;
				#endif
				
				skeletonRenderer = value;
				this.enabled = false; // Disable if nulled.
			}
		}

		MeshRenderer mainMeshRenderer;
		public bool copyPropertyBlock = false;
		[Tooltip("Copies MeshRenderer flags into each parts renderer")]
		public bool copyMeshRendererFlags = false;
		public List<Spine.Unity.Modules.SkeletonPartsRenderer> partsRenderers = new List<SkeletonPartsRenderer>();

		#if UNITY_EDITOR
		void Reset () {
			if (skeletonRenderer == null)
				skeletonRenderer = GetComponent<SkeletonRenderer>();
		}
		#endif
		#endregion

		void OnEnable () {
			if (skeletonRenderer == null) return;
			if (copiedBlock == null) copiedBlock = new MaterialPropertyBlock();	
			mainMeshRenderer = skeletonRenderer.GetComponent<MeshRenderer>();

			#if SPINE_OPTIONAL_RENDEROVERRIDE
			skeletonRenderer.GenerateMeshOverride -= HandleRender;
			skeletonRenderer.GenerateMeshOverride += HandleRender;
			#endif


			#if UNITY_5_4_OR_NEWER
			if (copyMeshRendererFlags) {
				var lightProbeUsage = mainMeshRenderer.lightProbeUsage;
				bool receiveShadows = mainMeshRenderer.receiveShadows;

				for (int i = 0; i < partsRenderers.Count; i++) {
					var currentRenderer = partsRenderers[i];
					if (currentRenderer == null) continue; // skip null items.

					var mr = currentRenderer.MeshRenderer;
					mr.lightProbeUsage = lightProbeUsage;
					mr.receiveShadows = receiveShadows;
				}
			}
			#else
			if (copyMeshRendererFlags) {
				var useLightProbes = mainMeshRenderer.useLightProbes;
				bool receiveShadows = mainMeshRenderer.receiveShadows;

				for (int i = 0; i < partsRenderers.Count; i++) {
					var currentRenderer = partsRenderers[i];
					if (currentRenderer == null) continue; // skip null items.

					var mr = currentRenderer.MeshRenderer;
					mr.useLightProbes = useLightProbes;
					mr.receiveShadows = receiveShadows;
				}
			}
			#endif

		}

		void OnDisable () {
			if (skeletonRenderer == null) return;
			#if SPINE_OPTIONAL_RENDEROVERRIDE
			skeletonRenderer.GenerateMeshOverride -= HandleRender;
			#endif

			#if UNITY_EDITOR
			skeletonRenderer.LateUpdate();
			#endif

			foreach (var s in partsRenderers)
				s.ClearMesh();		
		}

		MaterialPropertyBlock copiedBlock;

		void HandleRender (SkeletonRenderer.SmartMesh.Instruction instruction) {
			int rendererCount = partsRenderers.Count;
			if (rendererCount <= 0) return;



			if (copyPropertyBlock)
				mainMeshRenderer.GetPropertyBlock(copiedBlock);

			var submeshInstructions = instruction.submeshInstructions;
			var submeshInstructionsItems = submeshInstructions.Items;
			int lastSubmeshInstruction = submeshInstructions.Count - 1;

			#if SPINE_OPTIONAL_NORMALS
			bool addNormals = skeletonRenderer.calculateNormals;
			#endif

			#if SPINE_OPTIONAL_SOLVETANGENTS
			bool addTangents = skeletonRenderer.calculateTangents;
			#endif

			bool pmaVertexColors = skeletonRenderer.pmaVertexColors;

			int rendererIndex = 0;
			var currentRenderer = partsRenderers[rendererIndex];
			for (int si = 0, start = 0; si <= lastSubmeshInstruction; si++) {
				if (submeshInstructionsItems[si].forceSeparate || si == lastSubmeshInstruction) {
					// Apply properties
					var meshGenerator = currentRenderer.MeshGenerator;
					#if SPINE_OPTIONAL_NORMALS
					meshGenerator.AddNormals = addNormals;
					#endif
					#if SPINE_OPTIONAL_SOLVETANGENTS
					meshGenerator.AddTangents = addTangents;
					#endif
					meshGenerator.PremultiplyVertexColors = pmaVertexColors;
					if (copyPropertyBlock)
						currentRenderer.SetPropertyBlock(copiedBlock);

					// Render
					currentRenderer.RenderParts(instruction.submeshInstructions, start, si + 1);

					start = si + 1;
					rendererIndex++;
					if (rendererIndex < rendererCount) {
						currentRenderer = partsRenderers[rendererIndex];
					} else {
						// Not enough renderers. Skip the rest of the instructions.
						break;
					}
				}
			}
				
			// Clear extra renderers if they exist.
			for (; rendererIndex < rendererCount; rendererIndex++) {
				partsRenderers[rendererIndex].ClearMesh();
			}

		}

	}
}
