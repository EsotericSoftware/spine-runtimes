/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if UNITY_2018_3 || UNITY_2019 || UNITY_2018_3_OR_NEWER
#define NEW_PREFAB_SYSTEM
#endif
#define SPINE_OPTIONAL_RENDEROVERRIDE

using System.Collections.Generic;
using UnityEngine;

namespace Spine.Unity {

#if NEW_PREFAB_SYSTEM
	[ExecuteAlways]
#else
	[ExecuteInEditMode]
#endif
	[HelpURL("http://esotericsoftware.com/spine-unity#SkeletonRenderSeparator")]
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
				if (value == null)
					this.enabled = false;
			}
		}

		MeshRenderer mainMeshRenderer;
		public bool copyPropertyBlock = true;
		[Tooltip("Copies MeshRenderer flags into each parts renderer")]
		public bool copyMeshRendererFlags = true;
		public List<Spine.Unity.SkeletonPartsRenderer> partsRenderers = new List<SkeletonPartsRenderer>();

#if UNITY_EDITOR
		void Reset () {
			if (skeletonRenderer == null)
				skeletonRenderer = GetComponent<SkeletonRenderer>();
		}
#endif
		#endregion

		#region Callback Delegates
		/// <summary>OnMeshAndMaterialsUpdated is called at the end of LateUpdate after the Mesh and
		/// all materials have been updated.</summary>
		public event SkeletonRenderer.SkeletonRendererDelegate OnMeshAndMaterialsUpdated;
		#endregion

		#region Runtime Instantiation
		/// <summary>Adds a SkeletonRenderSeparator and child SkeletonPartsRenderer GameObjects to a given SkeletonRenderer.</summary>
		/// <returns>The to skeleton renderer.</returns>
		/// <param name="skeletonRenderer">The target SkeletonRenderer or SkeletonAnimation.</param>
		/// <param name="sortingLayerID">Sorting layer to be used for the parts renderers.</param>
		/// <param name="extraPartsRenderers">Number of additional SkeletonPartsRenderers on top of the ones determined by counting the number of separator slots.</param>
		/// <param name="sortingOrderIncrement">The integer to increment the sorting order per SkeletonPartsRenderer to separate them.</param>
		/// <param name="baseSortingOrder">The sorting order value of the first SkeletonPartsRenderer.</param>
		/// <param name="addMinimumPartsRenderers">If set to <c>true</c>, a minimum number of SkeletonPartsRenderer GameObjects (determined by separatorSlots.Count + 1) will be added.</param>
		public static SkeletonRenderSeparator AddToSkeletonRenderer (SkeletonRenderer skeletonRenderer, int sortingLayerID = 0, int extraPartsRenderers = 0, int sortingOrderIncrement = DefaultSortingOrderIncrement, int baseSortingOrder = 0, bool addMinimumPartsRenderers = true) {
			if (skeletonRenderer == null) {
				Debug.Log("Tried to add SkeletonRenderSeparator to a null SkeletonRenderer reference.");
				return null;
			}

			SkeletonRenderSeparator srs = skeletonRenderer.gameObject.AddComponent<SkeletonRenderSeparator>();
			srs.skeletonRenderer = skeletonRenderer;

			skeletonRenderer.Initialize(false);
			int count = extraPartsRenderers;
			if (addMinimumPartsRenderers)
				count = extraPartsRenderers + skeletonRenderer.separatorSlots.Count + 1;

			Transform skeletonRendererTransform = skeletonRenderer.transform;
			List<SkeletonPartsRenderer> componentRenderers = srs.partsRenderers;

			for (int i = 0; i < count; i++) {
				SkeletonPartsRenderer spr = SkeletonPartsRenderer.NewPartsRendererGameObject(skeletonRendererTransform, i.ToString());
				MeshRenderer mr = spr.MeshRenderer;
				mr.sortingLayerID = sortingLayerID;
				mr.sortingOrder = baseSortingOrder + (i * sortingOrderIncrement);
				componentRenderers.Add(spr);
			}

			srs.OnEnable();

#if UNITY_EDITOR
			// Make sure editor updates properly in edit mode.
			if (!Application.isPlaying) {
				skeletonRenderer.enabled = false;
				skeletonRenderer.enabled = true;
				skeletonRenderer.LateUpdateMesh();
			}
#endif

			return srs;
		}

		/// <summary>Add a child SkeletonPartsRenderer GameObject to this SkeletonRenderSeparator.</summary>
		public SkeletonPartsRenderer AddPartsRenderer (int sortingOrderIncrement = DefaultSortingOrderIncrement, string name = null) {
			int sortingLayerID = 0;
			int sortingOrder = 0;
			if (partsRenderers.Count > 0) {
				SkeletonPartsRenderer previous = partsRenderers[partsRenderers.Count - 1];
				MeshRenderer previousMeshRenderer = previous.MeshRenderer;
				sortingLayerID = previousMeshRenderer.sortingLayerID;
				sortingOrder = previousMeshRenderer.sortingOrder + sortingOrderIncrement;
			}

			if (string.IsNullOrEmpty(name))
				name = partsRenderers.Count.ToString();

			SkeletonPartsRenderer spr = SkeletonPartsRenderer.NewPartsRendererGameObject(skeletonRenderer.transform, name);
			partsRenderers.Add(spr);

			MeshRenderer mr = spr.MeshRenderer;
			mr.sortingLayerID = sortingLayerID;
			mr.sortingOrder = sortingOrder;

			return spr;
		}
		#endregion

		public void OnEnable () {
			if (skeletonRenderer == null) return;
			if (copiedBlock == null) copiedBlock = new MaterialPropertyBlock();
			mainMeshRenderer = skeletonRenderer.GetComponent<MeshRenderer>();

#if SPINE_OPTIONAL_RENDEROVERRIDE
			skeletonRenderer.GenerateMeshOverride -= HandleRender;
			skeletonRenderer.GenerateMeshOverride += HandleRender;
#endif

			if (copyMeshRendererFlags) {
				var lightProbeUsage = mainMeshRenderer.lightProbeUsage;
				bool receiveShadows = mainMeshRenderer.receiveShadows;
				var reflectionProbeUsage = mainMeshRenderer.reflectionProbeUsage;
				var shadowCastingMode = mainMeshRenderer.shadowCastingMode;
				var motionVectorGenerationMode = mainMeshRenderer.motionVectorGenerationMode;
				var probeAnchor = mainMeshRenderer.probeAnchor;

				for (int i = 0; i < partsRenderers.Count; i++) {
					var currentRenderer = partsRenderers[i];
					if (currentRenderer == null) continue; // skip null items.

					var mr = currentRenderer.MeshRenderer;
					mr.lightProbeUsage = lightProbeUsage;
					mr.receiveShadows = receiveShadows;
					mr.reflectionProbeUsage = reflectionProbeUsage;
					mr.shadowCastingMode = shadowCastingMode;
					mr.motionVectorGenerationMode = motionVectorGenerationMode;
					mr.probeAnchor = probeAnchor;
				}
			}

			if (skeletonRenderer.updateWhenInvisible != UpdateMode.FullUpdate)
				skeletonRenderer.LateUpdateMesh();
		}

		public void OnDisable () {
			if (skeletonRenderer == null) return;
#if SPINE_OPTIONAL_RENDEROVERRIDE
			skeletonRenderer.GenerateMeshOverride -= HandleRender;
#endif
			skeletonRenderer.LateUpdateMesh();
			ClearPartsRendererMeshes();
		}

		MaterialPropertyBlock copiedBlock;

		void HandleRender (SkeletonRendererInstruction instruction) {
			int rendererCount = partsRenderers.Count;
			if (rendererCount <= 0) return;

			if (copyPropertyBlock)
				mainMeshRenderer.GetPropertyBlock(copiedBlock);

			MeshGenerator.Settings settings = new MeshGenerator.Settings {
				addNormals = skeletonRenderer.addNormals,
				calculateTangents = skeletonRenderer.calculateTangents,
				immutableTriangles = false, // parts cannot do immutable triangles.
				pmaVertexColors = skeletonRenderer.pmaVertexColors,
				tintBlack = skeletonRenderer.tintBlack,
				useClipping = true,
				zSpacing = skeletonRenderer.zSpacing
			};

			ExposedList<SubmeshInstruction> submeshInstructions = instruction.submeshInstructions;
			SubmeshInstruction[] submeshInstructionsItems = submeshInstructions.Items;
			int lastSubmeshInstruction = submeshInstructions.Count - 1;

			int rendererIndex = 0;
			SkeletonPartsRenderer currentRenderer = partsRenderers[rendererIndex];
			for (int si = 0, start = 0; si <= lastSubmeshInstruction; si++) {
				if (currentRenderer == null)
					continue;
				if (submeshInstructionsItems[si].forceSeparate || si == lastSubmeshInstruction) {
					// Apply properties
					MeshGenerator meshGenerator = currentRenderer.MeshGenerator;
					meshGenerator.settings = settings;

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

			if (OnMeshAndMaterialsUpdated != null)
				OnMeshAndMaterialsUpdated(this.skeletonRenderer);

			// Clear extra renderers if they exist.
			for (; rendererIndex < rendererCount; rendererIndex++) {
				currentRenderer = partsRenderers[rendererIndex];
				if (currentRenderer != null)
					partsRenderers[rendererIndex].ClearMesh();
			}
		}

		protected void ClearPartsRendererMeshes () {
			foreach (SkeletonPartsRenderer partsRenderer in partsRenderers) {
				if (partsRenderer != null)
					partsRenderer.ClearMesh();
			}
		}
	}
}
